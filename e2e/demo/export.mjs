import { readFile, writeFile } from 'node:fs/promises';
import { resolve, dirname, basename } from 'node:path';
import { timeline } from './timeline.mjs';
import { mux } from './media.mjs';

// Re-encode the same successful continuous take; never combines separate takes.
for (const input of process.argv.slice(2)) {
  const target = resolve(input), folder = dirname(target), slug = basename(target, '.webm');
  const manifest = resolve(folder, `${slug}-chapters.json`);
  const metadata = JSON.parse(await readFile(manifest));
  const raw = resolve(folder, 'raw.webm');
  const located = await timeline(raw, metadata.cues.map((cue, index) => ({ ...cue, path: resolve(folder, `${cue.speechIndex ?? index}.wav`) })));
  const offset = located[0].start;
  const cues = located.map(c => ({ ...c, start: c.start - offset, end: c.end - offset, visibleEnd: c.visibleEnd - offset }));
  const measured = await mux(raw, cues, target, offset);
  metadata.cues = cues.map(({ path, ...cue }) => cue);
  metadata.duration = Number(measured.format.duration); metadata.size = Number(measured.format.size);
  await writeFile(manifest, JSON.stringify(metadata, null, 2));
  console.log(`Exported ${slug}: ${metadata.duration.toFixed(2)} seconds`);
}
