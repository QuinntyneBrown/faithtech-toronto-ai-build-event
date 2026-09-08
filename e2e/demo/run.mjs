import { mkdir, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { environment } from './environment.mjs';
import { host } from './host.mjs';
import { admin } from './admin.mjs';
import { gallery } from './gallery.mjs';
import { client } from './client.mjs';
import { api } from './api.mjs';
import { provisioning } from './provisioning.mjs';

const stories = { provisioning, admin, client, api, 'design-system': gallery };
const selected = process.argv.length > 2 ? process.argv.slice(2) : Object.keys(stories);
if (selected.some(s => !stories[s])) throw new Error('Choose provisioning, admin, client, api, or design-system');
const directory = resolve('artifacts/demo-runs', `take-${Date.now()}`);
await mkdir(directory, { recursive: true });
console.log(`Staging: ${directory}`);
const results = [];
for (const slug of selected) {
  let local;
  try {
    console.log(`Preparing ${slug}`);
    if (slug === 'design-system') {
      local = await host({ mounts: [{ prefix: '/design-system/', root: 'design-system' }] });
      results.push(await gallery(directory, local.url));
    } else {
      const resources = resolve(directory, `${slug}-resources`); await mkdir(resources);
      local = await environment(resources);
      results.push(await stories[slug](directory, local));
    }
    console.log(`${slug}: assertions and encoded timeline passed`);
  } catch (error) {
    results.push({ slug, status: 'blocked', error: error.message });
    await writeFile(resolve(directory, `${slug}-error.txt`), error.stack);
    console.error(`${slug}: ${error.message}`); process.exitCode = 1;
  } finally {
    try { await local?.close(); }
    catch (error) { results.push({ slug, status: 'cleanup-failed', error: error.message }); console.error(error.message); process.exitCode = 1; }
  }
}
await writeFile(resolve(directory, 'results.json'), JSON.stringify(results, null, 2));
