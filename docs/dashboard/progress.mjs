export const statuses = { todo: 'Not started', progress: 'In progress', review: 'Needs verification', done: 'Accepted' };

export function summarize(items, state) {
  const counts = { total: items.length, done: 0, remaining: 0, percent: 0, review: 0, progress: 0, todo: 0 };
  for (const item of items) counts[state.entries[item.id]?.status ?? item.status]++;
  counts.remaining = counts.total - counts.done;
  counts.percent = counts.total ? Math.round(counts.done / counts.total * 100) : 0;
  return counts;
}

export function initialProgress(items, at = new Date().toISOString()) {
  const state = { version: 1, entries: {}, history: [] };
  state.history.push({ at, remaining: summarize(items, state).remaining, total: items.length });
  return state;
}

export function updateProgress(items, state, id, status, note, at = new Date().toISOString()) {
  if (!items.some(item => item.id === id) || !Object.hasOwn(statuses, status)) throw new Error('Unknown requirement or status.');
  if (typeof note !== 'string' || note.length > 10000) throw new Error('Keep evidence notes below 10,000 characters.');
  if (status === 'done' && !note.trim()) throw new Error('Add evidence before accepting this requirement.');
  const next = structuredClone(state);
  next.entries[id] = { status, note: note.trim() };
  const remaining = summarize(items, next).remaining;
  // Keep note-only edits out of the burndown; reopening work adds it back.
  if (remaining !== next.history.at(-1).remaining) next.history.push({ at, remaining, total: items.length });
  return next;
}

export function parseProgress(text, items) {
  if (text.length > 2_000_000) throw new Error('Progress file is too large (maximum 2 MB).');
  let value;
  try { value = JSON.parse(text); } catch { throw new Error('This is not a valid JSON progress file.'); }
  const invalid = () => { throw new Error('Progress file is invalid or belongs to a different requirement scope.'); };
  if (!value || value.version !== 1 || !value.entries || Array.isArray(value.entries) || !Array.isArray(value.history) || !value.history.length) invalid();
  const clean = { version: 1, entries: {}, history: [] };
  for (const [id, entry] of Object.entries(value.entries)) {
    if (!items.some(item => item.id === id) || !entry || !Object.hasOwn(statuses, entry.status) || typeof entry.note !== 'string' || entry.note.length > 10000 || (entry.status === 'done' && !entry.note.trim())) invalid();
    clean.entries[id] = { status: entry.status, note: entry.note };
  }
  let previous = -Infinity;
  for (const point of value.history) {
    if (!point || typeof point.at !== 'string' || !Number.isFinite(Date.parse(point.at)) || Date.parse(point.at) < previous || point.total !== items.length || !Number.isInteger(point.remaining) || point.remaining < 0 || point.remaining > items.length) invalid();
    previous = Date.parse(point.at);
    clean.history.push({ at: point.at, remaining: point.remaining, total: point.total });
  }
  if (clean.history.at(-1).remaining !== summarize(items, clean).remaining) invalid();
  return clean;
}
