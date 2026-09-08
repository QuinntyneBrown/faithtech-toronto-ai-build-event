import { spawn } from 'node:child_process';
import { randomBytes, randomUUID } from 'node:crypto';
import { readFile, writeFile, unlink } from 'node:fs/promises';
import { resolve, basename, dirname } from 'node:path';
import { request } from '@playwright/test';
import { host } from './host.mjs';
import { run } from './media.mjs';

export async function environment(directory) {
  const database = `FaithTechDemo_${randomUUID().replaceAll('-', '')}`;
  const dotnet = process.env.FAITHTECH_DEMO_DOTNET || resolve(process.env.LOCALAPPDATA, 'FaithTech/dotnet/dotnet.exe');
  const sqlServer = process.env.FAITHTECH_DEMO_SQL_SERVER || '.\\SQLEXPRESS';
  const password = `Demo9!${randomBytes(16).toString('hex')}`;
  const pfxPath = resolve(directory, 'localhost.pfx'), passphrase = randomBytes(24).toString('base64');
  const env = { ...process.env, ConnectionStrings__EventDatabase: `Server=${sqlServer};Database=${database};Integrated Security=true;TrustServerCertificate=true`,
    Security__DigestKey: randomBytes(32).toString('base64'), ASPNETCORE_ENVIRONMENT: 'Development',
    Logging__LogLevel__Default: 'Warning', ASPNETCORE_Kestrel__Certificates__Default__Path: pfxPath,
    ASPNETCORE_Kestrel__Certificates__Default__Password: passphrase, FAITHTECH_DEMO_CERT_PASSWORD: passphrase };
  const sql = query => run('sqlcmd', ['-S', sqlServer, '-E', '-C', '-b', '-l', '10', '-t', '30', '-Q', query]);
  const cliPath = resolve('backend/src/FaithTechTorontoAiBuildEvent.Provisioning/bin/Debug/net10.0/FaithTechTorontoAiBuildEvent.Provisioning.dll');
  const cli = (args, onOutput) => run(dotnet, [basename(cliPath), ...args], { cwd: dirname(cliPath), env, input: password + '\n', onOutput, timeout: 60000 });
  let api, server, apiClient, logs = '', closed = false;
  const close = async () => {
    if (closed) return; closed = true;
    const failures = [];
    for (const clean of [() => apiClient?.dispose(), () => server?.close(), async () => {
      if (api && api.exitCode === null) { api.kill(); await new Promise((ok, fail) => {
        const deadline = setTimeout(() => fail(new Error(`API process ${api.pid} did not exit`)), 10000);
        api.once('close', () => { clearTimeout(deadline); ok(); });
      }); }
    }, async () => {
      if (!/^FaithTechDemo_[a-f0-9]{32}$/.test(database)) throw new Error('Unsafe database name');
      await sql(`IF DB_ID(N'${database}') IS NOT NULL BEGIN ALTER DATABASE [${database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${database}]; END`);
    }, () => unlink(pfxPath).catch(e => { if (e.code !== 'ENOENT') throw e; })]) {
      try { await clean(); } catch (error) { failures.push(error.message); }
    }
    await writeFile(resolve(directory, 'api.log'), logs);
    await writeFile(resolve(directory, 'cleanup.json'), JSON.stringify({ database, failures }));
    if (failures.length) throw new Error(`Cleanup failed: ${failures.join('; ')}`);
  };
  try {
    await writeFile(resolve(directory, 'resources.json'), JSON.stringify({ database }));
    await run('powershell.exe', ['-NoProfile', '-File', resolve('e2e/demo/certificate.ps1'), pfxPath], { env, timeout: 30000 });
    await cli(['migrate']); await cli(['create-admin', 'demo-host']);
    const reservation = await host({ mounts: [] }); const apiURL = reservation.url.replace('http:', 'https:'); await reservation.close();
    api = spawn(dotnet, [resolve('backend/src/FaithTechTorontoAiBuildEvent.Api/bin/Debug/net10.0/FaithTechTorontoAiBuildEvent.Api.dll'), '--urls', apiURL], { env, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'] });
    api.stdout.on('data', d => { logs += d; }); api.stderr.on('data', d => { logs += d; });
    let startupError; api.on('error', e => { startupError = e; });
    apiClient = await request.newContext({ baseURL: apiURL, ignoreHTTPSErrors: true, timeout: 5000 });
    const deadline = Date.now() + 60000;
    while (true) {
      if (startupError || api.exitCode !== null) throw startupError || new Error('API exited: ' + logs);
      if (await apiClient.get('/api/admin/antiforgery').then(r => r.ok()).catch(() => false)) break;
      if (Date.now() > deadline) throw new Error('API readiness deadline: ' + logs);
      await new Promise(ok => setTimeout(ok, 250));
    }
    server = await host({ tls: { pfx: await readFile(pfxPath), passphrase }, upstream: apiURL, fallback: true,
      mounts: [{ prefix: '/admin/', root: 'frontend/dist/admin/browser' }, { prefix: '/', root: 'frontend/dist/client/browser' }] });
    return { url: server.url, password, cli, close, sql, database, apiURL };
  } catch (error) { await close(); throw error; }
}
