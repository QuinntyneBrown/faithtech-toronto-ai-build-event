import { readFile, writeFile } from 'node:fs/promises';
const apps = [
  ['admin', 'Host event setup, schedule and roster', 'frontend/projects/admin/src/main.ts', 'HTTPS /admin/; provisioned administrator; shared API + SQL'],
  ['client', 'Participant entry and sessions', 'frontend/projects/client/src/main.ts', 'HTTPS /events/{id}; email + entry code; shared API + SQL'],
  ['api', 'Shared event and access service', 'backend/src/FaithTechTorontoAiBuildEvent.Api/Program.cs', 'HTTPS /api; cookies + CSRF; SQL'],
  ['provisioning', 'Operator schema and account administration', 'backend/src/FaithTechTorontoAiBuildEvent.Provisioning/Program.cs', 'CLI subprocess; Windows SQL identity; SQL'],
  ['design-system', 'Independent foundations and controls for designers and builders', 'design-system/index.html', 'Local HTTP /design-system/; public; no API dependency'],
];
const stamp = seconds => {
  const value = Math.floor(seconds); return `${String(Math.floor(value / 60)).padStart(2, '0')}:${String(value % 60).padStart(2, '0')}`;
};
const rows = ['| Recording | Purpose | Measured media | Status |', '|---|---|---|---|'];
const chapters = [];
for (const [slug, purpose] of apps) {
  let metadata;
  try { metadata = JSON.parse(await readFile(`docs/demo/${slug}-chapters.json`)); }
  catch (error) { if (error.code !== 'ENOENT') throw error; }
  if (!metadata) { rows.push(`| ${slug} | ${purpose} | — | Not recorded |`); continue; }
  const verification = JSON.parse(await readFile(`docs/demo/${slug}-verification.json`));
  rows.push(`| [${slug}](${slug}.webm) · [poster](${slug}-poster.png) | ${purpose} | ${metadata.duration.toFixed(2)} s · 1280 × 720 · ${(metadata.size / 1048576).toFixed(2)} MiB · VP8/Opus | Reviewed |`);
  chapters.push(`### ${slug}\n\n[Captions and transcript](${slug}.vtt) · [Chapter metadata](${slug}-chapters.json) · [Verification](${slug}-verification.json)\n\n` +
    metadata.cues.map(c => `- ${stamp(c.start)} — ${c.chapter}`).join('\n') +
    `\n\nNarration levels: mean ${verification.audio.meanDb} dBFS; peak ${verification.audio.maxDb} dBFS.`);
}
const surfaces = apps.map(([slug, , entrypoint, surface]) => `- **${slug}:** [entrypoint](../../${entrypoint}); ${surface}. Reproduce with \`node --experimental-transform-types e2e/demo/run.mjs ${slug}\`.`).join('\n');
const content = rows.join('\n') + '\n\n' + surfaces + '\n\n' + chapters.join('\n\n');
const path = 'docs/demo/README.md', readme = await readFile(path, 'utf8');
const begin = '<!-- generated-demo-catalog:start -->', end = '<!-- generated-demo-catalog:end -->';
if (!readme.includes(begin) || !readme.includes(end)) throw new Error('Missing catalog boundaries; preserve the manual README');
const start = readme.indexOf(begin) + begin.length, stop = readme.indexOf(end, start);
await writeFile(path, readme.slice(0, start) + '\n' + content + '\n' + readme.slice(stop));
console.log('Updated the reviewed demo catalog; manual documentation preserved.');
