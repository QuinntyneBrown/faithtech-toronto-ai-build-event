import { cp, mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
const base = fileURLToPath(new URL('../', import.meta.url));
const bundle = process.argv.includes('--mocks');
const destination = resolve(base, bundle ? 'dist/bundle/design-system' : 'dist/site');
await mkdir(destination, { recursive: true });
for (const name of ['index.html', 'tokens.css', 'theme.css', 'patterns.css', 'gallery.js', 'gallery-patterns.js', 'assets', 'LICENSE.cornerstone']) {
  await cp(resolve(base, name), resolve(destination, name), { recursive: true });
}
if (bundle) await cp(resolve(base, '../docs/mocks'), resolve(base, 'dist/bundle/docs/mocks'), { recursive: true });
console.log(`Built ${bundle ? 'complete mock bundle' : 'standalone design system'}: ${destination}`);
