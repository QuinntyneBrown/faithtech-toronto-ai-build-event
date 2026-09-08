import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdir, mkdtemp, writeFile, rm } from 'node:fs/promises';
import { resolve, sep } from 'node:path';
import { host } from './host.mjs';

test('Given local video and SPA files, when served, then ranges and deep links work within the mount', async () => {
  const root = resolve('artifacts/demo-host-tests'); await mkdir(root, { recursive: true });
  const directory = await mkdtemp(resolve(root, 'run-')); let server;
  try {
    await writeFile(resolve(directory, 'index.html'), '<p>Demo client</p>');
    await writeFile(resolve(directory, 'clip.webm'), Buffer.from('0123456789'));
    server = await host({ mounts: [{ prefix: '/demo/', root: directory }], fallback: true });
    assert.equal(await (await fetch(server.url + '/demo/events/123')).text(), '<p>Demo client</p>');
    const range = await fetch(server.url + '/demo/clip.webm', { headers: { Range: 'bytes=2-5' } });
    assert.equal(range.status, 206); assert.equal(await range.text(), '2345');
    assert.equal(range.headers.get('content-range'), 'bytes 2-5/10');
    assert.equal((await fetch(server.url + '/demo/clip.webm', { headers: { Range: 'bytes=20-' } })).status, 416);
    assert.equal((await fetch(server.url + '/demo/%2e%2e%2foutside.txt')).status, 404);
  } finally {
    await server?.close();
    if (!resolve(directory).startsWith(root + sep)) throw new Error('Unsafe cleanup target');
    await rm(directory, { recursive: true, force: true });
  }
});
