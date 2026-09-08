import { resolve } from 'node:path';
import { host } from './host.mjs';
import { gallery } from './gallery.mjs';
const server = await host({ mounts: [{ prefix: '/design-system/', root: 'design-system' }] });
try { console.log(await gallery(resolve('artifacts/demo-runs', `gallery-${Date.now()}`), server.url)); }
finally { await server.close(); }
