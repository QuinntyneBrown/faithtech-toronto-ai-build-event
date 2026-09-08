import { resolve } from 'node:path';
import { host } from './host.mjs';
import { gallery } from './gallery.mjs';
const server = await host({ mounts: [{ prefix: '/design-system/', root: 'design-system' }] });
const directory = resolve('artifacts/demo-runs', `gallery-${Date.now()}`);
console.log(`Staging: ${directory}`);
try { console.log(await gallery(directory, server.url)); }
finally { await server.close(); }
