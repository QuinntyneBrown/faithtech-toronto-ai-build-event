import { test } from 'node:test';
import assert from 'node:assert/strict';
import { captions, run } from './media.mjs';

test('Given timed narration, when captions are exported, then words retain their playback times', () => {
  assert.equal(captions([{ start: 1.25, end: 3.5, text: 'Welcome to FaithTech.' }]),
    'WEBVTT\n\n00:00:01.250 --> 00:00:03.500\nWelcome to FaithTech.\n');
});
test('Given a failed encoder, when it exits, then the run rejects instead of publishing', async () => {
  await assert.rejects(run(process.execPath, ['-e', 'process.exit(7)']), /exit 7/);
});
test('Given a stalled helper, when its deadline expires, then it is terminated', async () => {
  await assert.rejects(run(process.execPath, ['-e', 'setInterval(()=>{},1000)'], { timeout: 100 }), /deadline/);
});
