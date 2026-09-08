import { chromium, expect } from '@playwright/test';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname, basename } from 'node:path';
import { createHash } from 'node:crypto';
import { host } from './host.mjs';
import { run } from './media.mjs';

// Plays the encoded deliverable at normal speed; produces frames for visual review.
for (const input of process.argv.slice(2)) {
  const videoPath = resolve(input), slug = basename(videoPath, '.webm'), folder = dirname(videoPath);
  const metadata = JSON.parse(await readFile(resolve(folder, `${slug}-chapters.json`)));
  const qa = resolve(folder, 'review'); await mkdir(qa, { recursive: true });
  const server = await host({ mounts: [{ prefix: '/', root: folder }] });
  const browser = await chromium.launch({ args: ['--autoplay-policy=no-user-gesture-required'] });
  try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
    await page.setContent(`<body style="margin:0;background:#111"><video width="1280" height="720" controls src="${server.url}/${slug}.webm"></video></body>`);
    await expect.poll(() => page.locator('video').evaluate(v => v.readyState), { timeout: 30000 }).toBeGreaterThanOrEqual(2);
    const actual = await page.locator('video').evaluate(v => ({ duration: v.duration, width: v.videoWidth, height: v.videoHeight }));
    expect(actual.width).toBe(1280); expect(actual.height).toBe(720);
    expect(Math.abs(actual.duration - metadata.duration)).toBeLessThan(0.2);
    await page.locator('video').evaluate(v => { v.controls = false; v.playbackRate = 1; return v.play(); });
    const start = Date.now(); let next = 0, progress = 0;
    while (true) {
      const state = await page.locator('video').evaluate(v => ({ time: v.currentTime, ended: v.ended, error: v.error?.message, frames: v.getVideoPlaybackQuality().totalVideoFrames }));
      if (state.error) throw new Error(state.error);
      if (next < metadata.cues.length && state.time >= metadata.cues[next].start + 2) {
        await page.screenshot({ path: resolve(qa, `chapter-${String(next).padStart(2, '0')}.png`) }); next++;
      }
      if (state.time >= progress) { console.log(`${slug}: normal-speed playback ${state.time.toFixed(0)} / ${actual.duration.toFixed(0)} seconds`); progress += 20; }
      if (state.ended) { expect(state.frames).toBeGreaterThan(25); break; }
      if (Date.now() - start > (actual.duration + 45) * 1000) throw new Error('Playback stalled');
      await page.waitForTimeout(200);
    }
    expect(next).toBe(metadata.cues.length);
    await page.screenshot({ path: resolve(qa, 'ending.png') });
    const posterIndex = ({ client: 3, admin: 3 })[slug] ?? 2;
    const posterTime = metadata.cues[Math.min(posterIndex, metadata.cues.length - 1)].start + 2;
    await run(process.env.FFMPEG || 'ffmpeg', ['-v', 'error', '-y', '-ss', String(posterTime), '-i', videoPath, '-frames:v', '1', resolve(folder, `${slug}-poster.png`)]);
    await run(process.env.FFMPEG || 'ffmpeg', ['-v', 'error', '-y', '-i', videoPath, '-vn', '-c:a', 'libmp3lame', '-b:a', '96k', resolve(qa, 'narration.mp3')]);
    const levels = await run(process.env.FFMPEG || 'ffmpeg', ['-hide_banner', '-i', videoPath, '-vn', '-af', 'volumedetect', '-f', 'null', '-']);
    const meanDb = Number(/mean_volume: ([-\d.]+) dB/.exec(levels)?.[1]);
    const maxDb = Number(/max_volume: ([-\d.]+) dB/.exec(levels)?.[1]);
    expect(meanDb).toBeGreaterThan(-45); expect(maxDb).toBeLessThan(0);
    const sha256 = createHash('sha256').update(await readFile(videoPath)).digest('hex');
    await writeFile(resolve(qa, 'playback.json'), JSON.stringify({ ...actual, sha256, audio: { meanDb, maxDb },
      elapsed: (Date.now() - start) / 1000, chapters: next, decodedToEnd: true }, null, 2));
    console.log(`${slug}: decoded to end; poster and narration available for inspection`);
  } finally { await browser.close(); await server.close(); }
}
