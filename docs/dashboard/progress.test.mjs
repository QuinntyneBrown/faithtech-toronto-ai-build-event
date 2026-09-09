// Acceptance Test
// Traces to: DB-L2-001, DB-L2-002
// Description: Completion reflects explicit evidence and preserves a truthful history.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { initialProgress, updateProgress, summarize, parseProgress, estimateDevelopment } from './progress.mjs';

const items = [{ id: 'L2-001', status: 'review' }, { id: 'L2-002', status: 'progress' }];
const baseline = () => initialProgress(items, '2026-09-09T18:00:00Z');

test('Given code awaiting review, totals do not count it as accepted', () => {
  assert.deepEqual(summarize(items, baseline()), { total: 2, done: 0, remaining: 2, percent: 0, review: 1, progress: 1, todo: 0 });
});
test('Given a completed review, evidence and a new burndown observation survive export/import', () => {
  const next = updateProgress(items, baseline(), 'L2-001', 'done', 'API and browser cases passed', '2026-09-09T19:00:00Z');
  const restored = parseProgress(JSON.stringify(next), items);
  assert.equal(summarize(items, restored).remaining, 1);
  assert.equal(restored.history.at(-1).remaining, 1);
  assert.equal(restored.entries['L2-001'].note, 'API and browser cases passed');
  assert.equal(baseline().history.length, 1);
});
test('Given missing evidence, completion is rejected', () => {
  assert.throws(() => updateProgress(items, baseline(), 'L2-001', 'done', '  '), /evidence/i);
});
test('Given all work accepted then reopened, totals reach zero then increase', () => {
  let state = baseline();
  for (const item of items) state = updateProgress(items, state, item.id, 'done', 'Verified');
  assert.equal(summarize(items, state).percent, 100);
  assert.equal(state.history.at(-1).remaining, 0);
  state = updateProgress(items, state, 'L2-001', 'progress', 'Regression found');
  assert.equal(state.history.at(-1).remaining, 1);
});
test('Given invalid imported data, reject without modifying existing progress', () => {
  const state = baseline();
  for (const invalid of ['{', '{}', JSON.stringify({ ...state, entries: { 'L2-999': { status: 'done', note: 'x' } } }), JSON.stringify({ ...state, history: [] })]) {
    assert.throws(() => parseProgress(invalid, items));
  }
  assert.equal(state.history[0].remaining, 2);
});
test('Given fabricated or inconsistent history, reject the import', () => {
  const state = baseline();
  state.history[0].remaining = 0;
  assert.throws(() => parseProgress(JSON.stringify(state), items));
});

// Traces to: DB-L2-004
test('Given mixed development statuses, estimate progress separately from acceptance', () => {
  assert.equal(estimateDevelopment(items, baseline()), 70);
  const next = updateProgress(items, baseline(), 'L2-002', 'review', 'Implementation ready');
  assert.equal(estimateDevelopment(items, next), 90);
  assert.equal(summarize(items, next).done, 0);
});
test('Given empty, unstarted or accepted scope, estimate the boundaries safely', () => {
  assert.equal(estimateDevelopment([], initialProgress([])), 0);
  assert.equal(estimateDevelopment([{ id: 'a', status: 'todo' }], { entries: {} }), 0);
  assert.equal(estimateDevelopment([{ id: 'a', status: 'done' }], { entries: {} }), 100);
  const almostDone = Array.from({ length: 100 }, (_, id) => ({ id: String(id), status: id ? 'done' : 'review' }));
  assert.equal(estimateDevelopment(almostDone, { entries: {} }), 99);
});
