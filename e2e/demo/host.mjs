import http from 'node:http';
import https from 'node:https';
import { readFile, stat } from 'node:fs/promises';
import { resolve, sep, extname } from 'node:path';

export async function host({ mounts, tls, upstream, fallback } = {}) {
  const types = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.svg': 'image/svg+xml',
    '.png': 'image/png', '.webm': 'video/webm', '.wav': 'audio/wav', '.vtt': 'text/vtt', '.json': 'application/json' };
  const sockets = new Set();
  const handler = async (req, res) => {
    if (upstream && req.url.startsWith('/api/')) {
      const proxy = https.request(new URL(req.url, upstream), { method: req.method, headers: { ...req.headers, host: new URL(upstream).host }, rejectUnauthorized: false }, response => {
        res.writeHead(response.statusCode, response.headers); response.pipe(res);
      });
      proxy.setTimeout(30000, () => proxy.destroy(new Error('Upstream timeout')));
      proxy.on('error', () => { if (!res.headersSent) res.writeHead(502); res.end(); });
      req.pipe(proxy); return;
    }
    try {
      const path = decodeURIComponent(new URL(req.url, 'http://localhost').pathname);
      const mount = mounts.find(m => path.startsWith(m.prefix));
      if (!mount) { res.writeHead(404); res.end(); return; }
      const root = resolve(mount.root);
      let file = resolve(root, path.slice(mount.prefix.length));
      if (file !== root && !file.startsWith(root + sep)) throw new Error('Outside mount');
      try { if ((await stat(file)).isDirectory()) file = resolve(file, 'index.html'); }
      catch { if (fallback && !extname(path)) file = resolve(root, 'index.html'); else throw new Error('Missing file'); }
      const body = await readFile(file);
      const headers = { 'Content-Type': types[extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-store', 'Accept-Ranges': 'bytes' };
      const range = /^bytes=(\d+)-(\d*)$/.exec(req.headers.range || '');
      if (range) {
        const start = Number(range[1]), end = Math.min(Number(range[2] || body.length - 1), body.length - 1);
        if (start > end) { res.writeHead(416); res.end(); return; }
        res.writeHead(206, { ...headers, 'Content-Range': `bytes ${start}-${end}/${body.length}`, 'Content-Length': end - start + 1 });
        res.end(body.subarray(start, end + 1));
      } else { res.writeHead(200, { ...headers, 'Content-Length': body.length }); res.end(body); }
    } catch { res.writeHead(404); res.end('Not found'); }
  };
  const server = tls ? https.createServer(tls, handler) : http.createServer(handler);
  server.on('connection', socket => { sockets.add(socket); socket.on('close', () => sockets.delete(socket)); });
  await new Promise((ok, fail) => { server.once('error', fail); server.listen(0, '127.0.0.1', ok); });
  return { url: `${tls ? 'https' : 'http'}://127.0.0.1:${server.address().port}`,
    close: () => new Promise(ok => { server.close(ok); for (const socket of sockets) socket.destroy(); }) };
}
