// Acceptance Test: L2-049/AC2 — failed packaging must not replace the installed tool.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, copyFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

for (const failure of ['restore', 'build', 'pack']) {
  test(`Given ${failure} fails, when installing the operator, then the installed tool is untouched`, async () => {
    const root = await mkdtemp(join(tmpdir(), 'faithtech-install-'));
    try {
      await mkdir(join(root, 'eng/scripts'), { recursive: true });
      const script = join(root, 'eng/scripts/Install-OperatorTool.ps1');
      await copyFile('eng/scripts/Install-OperatorTool.ps1', script);
      const result = spawnSync('pwsh', ['-NoProfile', '-Command', `
        function global:dotnet {
          Write-Output "INVOKED:$($args[0])"
          $global:LASTEXITCODE = if ($args[0] -eq $env:TEST_FAILURE) { 1 } else { 0 }
        }
        & $env:TEST_INSTALL_SCRIPT
      `], { encoding: 'utf8', timeout: 15000, env: { ...process.env, LOCALAPPDATA: root,
        TEST_FAILURE: failure, TEST_INSTALL_SCRIPT: script } });
      assert.notEqual(result.status, 0);
      assert.doesNotMatch(result.stdout, /INVOKED:tool/);
    } finally {
      assert.ok(resolve(root).startsWith(resolve(tmpdir())));
      await rm(root, { recursive: true, force: true });
    }
  });
}
