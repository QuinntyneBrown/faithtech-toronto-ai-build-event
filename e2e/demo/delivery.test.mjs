import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdir, mkdtemp, writeFile, readFile, copyFile, rm } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { resolve, sep } from 'node:path';
import { promote } from './delivery.mjs';

test('Given an interrupted promotion, when copying fails, then the previous recording set is restored', async () => {
  const root = resolve('artifacts/demo-delivery-tests'); await mkdir(root, { recursive: true });
  const run = await mkdtemp(resolve(root, 'run-'));
  const staged = resolve(run, 'staged'), final = resolve(run, 'final');
  try {
    await mkdir(resolve(staged, 'review'), { recursive: true }); await mkdir(final);
    const sha256 = createHash('sha256').update('new video').digest('hex');
    await writeFile(resolve(staged, 'review/approved.json'), JSON.stringify({ slug: 'api', sha256 }));
    await writeFile(resolve(staged, 'review/playback.json'), JSON.stringify({ sha256, decodedToEnd: true }));
    for (const [name, content] of Object.entries({ 'api.webm': 'new video', 'api-poster.png': 'new poster', 'api.vtt': 'captions', 'api-chapters.json': '{}' })) {
      await writeFile(resolve(staged, name), content); await writeFile(resolve(final, name), 'old ' + name);
    }
    let calls = 0;
    await assert.rejects(promote(staged, final, async (from, to) => {
      if (++calls === 2) throw new Error('Copy interrupted'); await copyFile(from, to);
    }), /interrupted/);
    assert.equal(await readFile(resolve(final, 'api.webm'), 'utf8'), 'old api.webm');
    assert.equal(await readFile(resolve(final, 'api-poster.png'), 'utf8'), 'old api-poster.png');
    await writeFile(resolve(staged, 'api.webm'), 'changed after review');
    await assert.rejects(promote(staged, final), /Review does not match/);
  } finally {
    if (!resolve(run).startsWith(root + sep)) throw new Error('Unsafe cleanup target');
    await rm(run, { recursive: true, force: true });
  }
});
