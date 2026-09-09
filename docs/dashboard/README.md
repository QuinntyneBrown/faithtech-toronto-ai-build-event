# Project completion dashboard

A disposable local dashboard for the September 9 event companion, visually
inspired by [Open MCT](https://nasa.github.io/openmct/). Plain HTML, CSS and browser
JavaScript; a small Node built-in HTTP server. No install, build or database.
The user approved standalone code and a dark theme for this dashboard only. Its
audit counts only MVP requirements; requirements explicitly marked NON-MVP in
`docs/specs/L2.md` are excluded from all dashboard totals and estimates.

From the repository root:

```powershell
node docs/dashboard/serve.mjs
```

Open http://127.0.0.1:4317 in Chrome. Keep that process running; Ctrl+C stops it.
The server binds only to loopback and serves dashboard assets, read-only Git
activity and the audit's explicitly listed evidence files.

Click a requirement to inspect its finding and source evidence. Select a review
status and add notes. **Accepted** needs a nonblank evidence note and means all
criteria were verified by the reviewer. Saving updates totals and the chart;
reopening work adds it back. Search and workstream/status filters leave the
overall denominator unchanged. The target is zero remaining requirements.

Reviews persist in localStorage for this browser and this exact URL. Export
progress regularly; importing a valid export replaces the current browser's
reviews and history. Invalid imports preserve current reviews. Other tabs on the
same origin receive saved progress changes. Clearing browser data removes local
reviews; exports are the backup. There is no cloud sync or server-side database.

## What the numbers mean

The large headline is **estimated development completion**, initially **66%**.
It averages equal-weight requirements using these simple estimates: not started
0%, in progress 50%, implementation awaiting verification 90%, accepted 100%.
It updates with saved review statuses and ignores view filters. Only an entirely
accepted scope displays 100%; other rounded values are capped at 99%. This is a
rough status-based estimate, not a measurement of effort, hours or time remaining.

`audit.json` is a dated review of all 34 active event L2 requirements, with a source
reference and next action for each. Baseline: 17 need verification, 14 have known
implementation gaps, 3 have verification work not started, and 0 are certified
accepted. **Zero accepted does not mean zero code implemented.** The build passed,
but whole-requirement acceptance was not established.

The separate acceptance burndown uses equal-weight requirements, not hours or
story points. It starts when this browser first opens the dashboard and records
actual acceptance/reopening decisions. There is no fabricated historical curve
or estimated completion date. Git activity refreshes every 30 seconds but does
not change acceptance statuses or rerun tests. Audit findings/checks remain dated;
review source changes and record a new assessment explicitly. Update the JSON
when a new repository audit is performed, and reload the dashboard.

The dashboard is outside the event's four screens and does not count toward event
completion. Acceptance criteria are in [dashboard.md](../specs/dashboard.md).

## Verification

```powershell
node --test docs/dashboard/progress.test.mjs
cd e2e
npm ci
npm exec -- playwright test --config dashboard.config.ts
```

The Chrome tests use a dedicated configuration and disposable browser contexts.
They do not call the event API or alter your ordinary Chrome reviews. The regular
event Playwright configuration excludes dashboard tests.
