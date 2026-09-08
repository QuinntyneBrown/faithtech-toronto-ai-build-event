import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';

test('Given a modified release package, when deployment starts, then it fails before contacting Azure', async () => {
  const directory = await mkdtemp(join(tmpdir(), 'faithtech-release-'));
  try {
    await writeFile(join(directory, 'web.zip'), 'tampered package');
    await writeFile(join(directory, 'release.json'), JSON.stringify({ revision: 'a'.repeat(40), sha256: 'b'.repeat(64) }));
    const result = spawnSync('pwsh', ['-NoProfile', '-File', 'eng/scripts/deploy-release.ps1', '-PackageDirectory', directory],
      { encoding: 'utf8', timeout: 15000 });
    assert.equal(result.status, 1);
    assert.match(result.stderr, /Release package hash mismatch/);
  } finally { await rm(directory, { recursive: true, force: true }); }
});
