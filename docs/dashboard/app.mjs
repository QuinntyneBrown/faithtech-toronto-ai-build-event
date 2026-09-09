import { initialProgress, updateProgress, summarize, parseProgress, statuses } from './progress.mjs';
import { renderChart } from './chart.mjs';

const $ = id => document.getElementById(id);
const escape = value => String(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const storageKey = 'faithtech-completion-v1';
let audit, state, area = 'all', selected;
const notice = message => { $('notice').textContent = message; $('notice').hidden = !message; };
const statusOf = item => state.entries[item.id]?.status ?? item.status;
const badge = (status, text = statuses[status] ?? status) => `<span class="badge ${escape(status)}">${escape(text)}</span>`;

function render() {
  const summary = summarize(audit.items, state);
  const metrics = [
    ['REMAINING TO ACCEPT', summary.remaining, `/ ${summary.total}`, 'Full requirement scope · equal weight', 'cyan-border', 'remaining'],
    ['NEEDS VERIFICATION', summary.review, '', 'Implementation present; acceptance pending', '', 'review'],
    ['IN PROGRESS', summary.progress, '', `${summary.todo} more requirements not started`, 'amber-border', 'progress'],
    ['ACCEPTED', `${summary.percent}%`, `${summary.done} / ${summary.total}`, 'Explicit reviews backed by evidence', 'green-border', 'accepted'],
  ];
  $('metrics').innerHTML = metrics.map(([label, value, suffix, description, color, id]) => `<article class="metric ${color}"><div class="metric-label">${label}</div><div class="metric-value"><span data-testid="${id}">${value}</span><small>${suffix}</small></div><p class="metric-description">${description}</p><div class="mini-bar"><span style="width:${id === 'remaining' ? 100 - summary.percent : id === 'accepted' ? summary.percent : Number(value) / summary.total * 100}%"></span></div></article>`).join('');
  $('areas').innerHTML = [...new Set(audit.items.map(item => item.area))].map(name => `<button class="area-button ${area === name ? 'active' : ''}" data-area="${escape(name)}"><span>${escape(name)}</span><span>${audit.items.filter(item => item.area === name && statusOf(item) !== 'done').length}</span></button>`).join('');
  $('overview').classList.toggle('active', area === 'all');
  $('active-area').textContent = area === 'all' ? 'ALL WORKSTREAMS' : area.toUpperCase();
  renderList(); renderChart($('chart'), state, summary.total);
  $('history-summary').textContent = `${state.history.length} recorded observation${state.history.length === 1 ? '' : 's'} · ${summary.done} accepted`;
}

function renderList() {
  const query = $('search').value.trim().toLowerCase(), filter = $('filter').value;
  const items = audit.items.filter(item => (area === 'all' || item.area === area) && (filter === 'all' || statusOf(item) === filter) && `${item.id} ${item.title} ${item.area} ${item.note} ${state.entries[item.id]?.note ?? ''}`.toLowerCase().includes(query));
  $('result-count').textContent = `${items.length} / ${audit.items.length}`;
  $('requirements').innerHTML = items.length ? items.map(item => `<button class="requirement" data-id="${item.id}"><span class="requirement-main"><span class="requirement-title"><span class="requirement-id">${item.id}</span>${escape(item.title)}</span><span class="requirement-note">${escape(state.entries[item.id]?.note || item.note)}</span></span>${badge(statusOf(item))}</button>`).join('') : '<p class="empty">No requirements match these filters.</p>';
}

function inspect(id) {
  selected = audit.items.find(item => item.id === id);
  if (!selected) return;
  $('inspector-id').textContent = selected.id;
  $('inspector-title').textContent = selected.title;
  $('inspector-area').textContent = selected.area;
  $('finding').textContent = selected.note;
  $('source').href = `/source?path=${encodeURIComponent(selected.evidence)}`;
  $('source').title = selected.evidence;
  $('review-status').value = statusOf(selected);
  $('review-note').value = state.entries[id]?.note ?? '';
  $('review-error').hidden = true;
  $('inspector').showModal();
}

async function repository() {
  try {
    const response = await fetch('/repository'); if (!response.ok) throw new Error();
    const data = await response.json();
    $('repo-status').textContent = `HEAD ${data.head}`;
    $('commits').innerHTML = data.commits.map(commit => `<article class="commit"><p class="commit-title">${escape(commit.title)}</p><p class="commit-meta"><span>${escape(commit.hash)}</span>${escape(new Date(commit.at).toLocaleString())}</p></article>`).join('');
  } catch { $('repo-status').textContent = 'UNAVAILABLE'; $('commits').textContent = 'Could not read local Git activity. Retrying in 30 seconds.'; }
}

async function start() {
  try {
    const response = await fetch('/audit.json'); if (!response.ok) throw new Error('The audit could not be loaded. Refresh after checking the local server.');
    audit = await response.json(); state = initialProgress(audit.items);
    try {
      const saved = localStorage.getItem(storageKey);
      if (saved !== null) state = parseProgress(saved, audit.items);
      else localStorage.setItem(storageKey, JSON.stringify(state));
    } catch { notice('Saved progress is unreadable or browser storage is unavailable. Showing the audit baseline; existing storage has not been cleared. Export your reviews to keep a backup.'); }
    $('checks').innerHTML = audit.checks.map(check => `<div class="check"><details><summary>${escape(check.name)}</summary><p>${escape(check.detail)}</p></details>${badge(check.status, check.status.toUpperCase())}</div>`).join('');
    $('audit-label').textContent = `Audit: ${new Date(audit.auditedAt).toLocaleString()} · baseline ${audit.commit} · manual acceptance tracking`;
    render(); void repository(); setInterval(repository, 30_000);
  } catch (error) { notice(error.message); $('metrics').textContent = 'Dashboard data unavailable. Refresh to retry.'; }
}

$('search').addEventListener('input', renderList);
$('filter').addEventListener('change', renderList);
$('areas').addEventListener('click', event => { const button = event.target.closest('[data-area]'); if (button) { area = button.dataset.area; render(); } });
$('overview').addEventListener('click', () => { area = 'all'; $('search').value = ''; $('filter').value = 'all'; render(); });
$('requirements').addEventListener('click', event => { const button = event.target.closest('[data-id]'); if (button) inspect(button.dataset.id); });
$('close').addEventListener('click', () => $('inspector').close());
$('inspector').addEventListener('close', () => { (document.querySelector(`[data-id="${selected.id}"]`) ?? $('search')).focus(); });

function saveState(next) {
  state = next;
  try { localStorage.setItem(storageKey, JSON.stringify(state)); notice(''); }
  catch { notice('Browser storage could not save this review. It is available for this session only. Export progress now to keep it.'); }
  render();
}

$('review-form').addEventListener('submit', event => {
  event.preventDefault();
  try {
    const next = updateProgress(audit.items, state, selected.id, $('review-status').value, $('review-note').value);
    saveState(next); $('inspector').close();
  } catch (error) { $('review-error').textContent = error.message; $('review-error').hidden = false; }
});

$('export').addEventListener('click', () => {
  if (!state) return;
  const blob = new Blob([JSON.stringify(state, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob), link = document.createElement('a');
  link.href = url; link.download = `faithtech-progress-${new Date().toISOString().slice(0, 10)}.json`;
  link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
});

$('import').addEventListener('change', async event => {
  const file = event.target.files[0]; if (!file || !audit) return;
  try {
    if (file.size > 2_000_000) throw new Error('Progress file is too large (maximum 2 MB).');
    const next = parseProgress(await file.text(), audit.items);
    saveState(next);
  } catch (error) { notice(`Import rejected. ${error.message} Your current reviews have not changed.`); }
  event.target.value = '';
});

window.addEventListener('storage', event => {
  if (event.key !== storageKey || !audit) return;
  try { state = event.newValue ? parseProgress(event.newValue, audit.items) : initialProgress(audit.items); render(); }
  catch { notice('Another tab saved unreadable progress. Keeping this tab’s current review state.'); }
});
void start();
