import { promote } from './delivery.mjs';
for (const folder of process.argv.slice(2)) console.log(`Promoted reviewed ${await promote(folder)}`);
