import { readFile } from 'node:fs/promises';
import { run } from './media.mjs';

export function locateCues(pixels, cues, fps = 25) {
  const located = cues.map((cue, index) => {
    const frames = [];
    for (let i = 0; i < pixels.length; i += 12) {
      if (Math.abs(pixels[i] - (20 + index * 12)) < 6 && pixels[i + 1] < 25 && pixels[i + 2] > 185)
        frames.push(i / 12);
    }
    if (!frames.length) throw new Error(`No encoded chapter marker: ${cue.chapter}`);
    const start = frames[0] / fps, visibleEnd = (frames.at(-1) + 1) / fps;
    return { ...cue, start, end: start + cue.duration, visibleEnd };
  });
  return located.map((cue, index) => {
    // Lossy colour at a transition may resemble the preceding marker for a frame.
    const visibleEnd = Math.min(cue.visibleEnd, located[index + 1]?.start ?? Infinity);
    if (visibleEnd - cue.start < cue.duration - 0.2) throw new Error(`Video compressed narration interval: ${cue.chapter}`);
    return { ...cue, visibleEnd };
  });
}
export async function timeline(raw, cues) {
  const path = raw.replace('.webm', '.rgb');
  await run(process.env.FFMPEG || 'ffmpeg', ['-y', '-v', 'error', '-i', raw, '-vf', 'fps=25,crop=2:2:6:6,format=rgb24', '-f', 'rawvideo', path]);
  return locateCues(await readFile(path), cues);
}
