import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { spawn } from 'node:child_process';

for (const firstStatus of [503, 401]) {
  test(`Given sign-in returns ${firstStatus}, smoke ${firstStatus === 503 ? 'retries recovery' : 'rejects credentials immediately'}`, async () => {
    let attempts = 0;
    let authenticated = false;
    const revision = 'a'.repeat(40);
    const server = createServer((request, response) => {
      request.resume();
      let status = 200;
      let body = '<base href="/">';
      if (request.url === '/api/admin/antiforgery') body = JSON.stringify({ requestToken: 'synthetic' });
      else if (request.url === '/api/admin/session' && request.method === 'POST') {
        status = ++attempts === 1 ? firstStatus : 204;
        authenticated = status === 204;
      } else if (request.url === '/api/admin/session' && request.method === 'DELETE') {
        authenticated = false; status = 204;
      } else if (request.url === '/api/admin/readiness') {
        status = authenticated ? 200 : 401;
        body = JSON.stringify({ ready: true, revision, instance: 'synthetic' });
      } else if (request.url.includes('missing')) status = 404;
      response.writeHead(status, { 'Content-Type': 'text/html' });
      response.end(body);
    });
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    try {
      const child = spawn('pwsh', ['-NoProfile', '-File', 'eng/scripts/smoke-release.ps1',
        '-Url', `http://127.0.0.1:${server.address().port}`, '-Revision', revision],
      { env: { ...process.env, SMOKE_USERNAME: 'synthetic', SMOKE_PASSWORD: 'synthetic' }, timeout: 20000 });
      let error = '';
      child.stdout.resume();
      child.stderr.on('data', chunk => { error += chunk; });
      const code = await new Promise((resolve, reject) => { child.on('error', reject); child.on('close', resolve); });
      assert.equal(code, firstStatus === 503 ? 0 : 1, error);
      assert.equal(attempts, firstStatus === 503 ? 2 : 1);
      if (firstStatus === 401) assert.match(error, /Unexpected HTTP status: 401/);
      assert.equal(authenticated, false);
    } finally {
      server.closeAllConnections();
      await new Promise(resolve => server.close(resolve));
    }
  });
}
