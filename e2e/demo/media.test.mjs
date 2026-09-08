import { test } from 'node:test';
import assert from 'node:assert/strict';
import { captions, run } from './media.mjs';
import { locateCues } from './timeline.mjs';

test('Given timed narration, when captions are exported, then words retain their playback times', () => {
  assert.equal(captions([{ start: 1.25, end: 3.5, text: 'Welcome to FaithTech.' }]),
    'WEBVTT\n\n00:00:01.250 --> 00:00:03.500\nWelcome to FaithTech.\n');
});
test('Given a shifted video clock, when encoded markers are read, then narration follows actual frames', () => {
  const pixels = Buffer.alloc(100 * 12);
  for (let i = 25; i < 75; i++) { pixels[i * 12] = 20; pixels[i * 12 + 1] = 10; pixels[i * 12 + 2] = 200; }
  assert.equal(locateCues(pixels, [{ start: 12, duration: 1.8, chapter: 'Opening' }])[0].start, 1);
  assert.throws(() => locateCues(pixels, [{ duration: 4, chapter: 'Too short' }]), /compressed/);
});
test('Given a failed encoder, when it exits, then the run rejects instead of publishing', async () => {
  await assert.rejects(run(process.execPath, ['-e', 'process.exit(7)']), /exit 7/);
});
test('Given a stalled helper, when its deadline expires, then it is terminated', async () => {
  await assert.rejects(run(process.execPath, ['-e', 'setInterval(()=>{},1000)'], { timeout: 100 }), /deadline/);
});
