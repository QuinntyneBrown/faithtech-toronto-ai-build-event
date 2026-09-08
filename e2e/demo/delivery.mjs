import { readFile, writeFile, copyFile, mkdir, unlink } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { resolve } from 'node:path';

export async function promote(folder, destination = 'docs/demo', copy = copyFile) {
  const approval = JSON.parse(await readFile(resolve(folder, 'review/approved.json')));
  const playback = JSON.parse(await readFile(resolve(folder, 'review/playback.json')));
  const slug = approval.slug;
  if (!['admin', 'client', 'api', 'provisioning', 'design-system'].includes(slug)) throw new Error('Unknown demo');
  const video = await readFile(resolve(folder, `${slug}.webm`));
  const sha256 = createHash('sha256').update(video).digest('hex');
  if (!playback.decodedToEnd || approval.sha256 !== sha256 || playback.sha256 !== sha256)
    throw new Error('Review does not match the current video');
  const names = [`${slug}.webm`, `${slug}-poster.png`, `${slug}.vtt`, `${slug}-chapters.json`];
  const verification = `${slug}-verification.json`;
  await writeFile(resolve(folder, verification), JSON.stringify({ ...playback, visualReview: approval.notes }, null, 2));
  names.push(verification);
  // Preflight the complete set before replacing anything.
  for (const name of names) await readFile(resolve(folder, name));
  await mkdir(destination, { recursive: true });
  const backup = resolve(folder, `backup-${Date.now()}`); await mkdir(backup);
  const previous = new Set(), replaced = [];
  for (const name of names) {
    try { await copyFile(resolve(destination, name), resolve(backup, name)); previous.add(name); }
    catch (error) { if (error.code !== 'ENOENT') throw error; }
  }
  try {
    for (const name of names) { replaced.push(name); await copy(resolve(folder, name), resolve(destination, name)); }
  } catch (error) {
    for (const name of replaced) {
      if (previous.has(name)) await copyFile(resolve(backup, name), resolve(destination, name));
      else await unlink(resolve(destination, name)).catch(e => { if (e.code !== 'ENOENT') throw e; });
    }
    throw error;
  }
  return slug;
}
