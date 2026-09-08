import { spawnSync } from 'node:child_process';

for (const project of ['api', 'components', 'domain', 'admin', 'client']) {
  const result = spawnSync(process.execPath, ['node_modules/@angular/cli/bin/ng.js', 'build', project], { stdio: 'inherit' });
  if (result.status !== 0) process.exit(result.status ?? 1);
}
