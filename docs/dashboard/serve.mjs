import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const directory = fileURLToPath(new URL('.', import.meta.url));
const root = resolve(directory, '../..');
const run = promisify(execFile);
const assets = new Map([['/', 'index.html'], ...['app.mjs', 'chart.mjs', 'progress.mjs', 'style.css', 'audit.json'].map(name => [`/${name}`, name])]);
const types = { html: 'text/html', mjs: 'text/javascript', css: 'text/css', json: 'application/json' };
const server = createServer(async (request, response) => {
  response.setHeader('Cache-Control', 'no-store');
  response.setHeader('X-Content-Type-Options', 'nosniff');
  try {
    if (request.method !== 'GET') { response.writeHead(405); response.end('Read-only server'); return; }
    const url = new URL(request.url, 'http://127.0.0.1:4317');
    if (url.pathname === '/repository') {
      const options = { cwd: root, windowsHide: true, timeout: 5000, maxBuffer: 100000 };
      const [{ stdout: head }, { stdout: log }] = await Promise.all([
        run('git', ['rev-parse', '--short', 'HEAD'], options),
        run('git', ['log', '-5', '--format=%h%x09%cI%x09%s'], options),
      ]);
      response.setHeader('Content-Type', 'application/json');
      response.end(JSON.stringify({ head: head.trim(), commits: log.trim().split('\n').map(line => { const [hash, at, ...title] = line.split('\t'); return { hash, at, title: title.join('\t') }; }) }));
      return;
    }
    if (url.pathname === '/source') {
      const audit = JSON.parse(await readFile(resolve(directory, 'audit.json'), 'utf8'));
      const path = url.searchParams.get('path');
      if (![...audit.items.map(item => item.evidence), 'docs/specs/L2.md'].includes(path)) { response.writeHead(404); response.end('Unknown evidence'); return; }
      response.setHeader('Content-Type', 'text/plain; charset=utf-8');
      response.end(await readFile(resolve(root, path))); return;
    }
    const asset = assets.get(url.pathname);
    if (!asset) { response.writeHead(404); response.end('Not found'); return; }
    response.setHeader('Content-Type', `${types[asset.split('.').at(-1)]}; charset=utf-8`);
    response.end(await readFile(resolve(directory, asset)));
  } catch { response.writeHead(503); response.end('Local data is unavailable. Check the dashboard server and retry.'); }
});
server.on('error', error => { console.error(error.message); process.exitCode = 1; });
server.listen(4317, '127.0.0.1', () => console.log('Project dashboard: http://127.0.0.1:4317'));
