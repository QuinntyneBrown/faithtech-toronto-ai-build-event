# Throwaway project dashboard

This developer aid is separate from the event companion and is not a fifth event
screen. Its scope is the 30 MVP requirements in L2.md; legacy features, the four
wholly NON-MVP requirements (L2-039, L2-041, L2-042, and L2-051), and the dashboard
itself do not count toward event completion.

## DB-L1-001: Understand remaining work and track completion

Show an evidence-based repository audit, actionable remaining work, and a burndown
that records actual completion decisions. Keep operation local and simple.

## DB-L2-001: Inspect the baseline

Traces to: DB-L1-001.

1. Given the audited baseline, when opened, then every active event requirement
   appears with a status, source evidence, and an explicit next action.
2. Given filters or a search, when changed, then matching requirements appear and
   overall completion remains based on the full scope; no matches has an empty state.
3. Given an unverified implementation, when totals are shown, then code presence
   does not count as accepted completion; blocked checks are identified honestly.

## DB-L2-002: Record progress through completion

Traces to: DB-L1-001.

1. Given a requirement and a review note, when its status is changed, then totals
   and burndown update and survive reload; Done requires a nonblank evidence note.
2. Given a completed requirement, when reopened, then remaining work increases;
   completing the whole scope shows zero remaining without a divide-by-zero error.
3. Given recorded progress, when exported and imported in a fresh browser, then
   statuses, notes, and history return. Invalid data leaves existing progress intact.
4. Given unavailable or corrupt browser storage, when loaded or saved, then a clear
   warning appears and the baseline remains usable without silently deleting data.
5. Given no historical completion observations, when the chart opens, then it starts
   at the audit baseline and does not fabricate historical velocity or a finish date.

## DB-L2-003: Use the local dashboard

Traces to: DB-L1-001.

1. Given desktop or 375px mobile Chrome, when inspecting and editing requirements,
   then content and actions remain reachable without page-level horizontal overflow.
2. Given keyboard input, when opening and closing the requirement inspector, then
   controls are labeled and focus returns to the opening control.
3. Given the checkout and Node.js, when the documented local launch command runs,
   then the dashboard opens without a build, database, account, or external service.

The baseline is a dated code review, not an automatic acceptance certificate.
Status changes are explicit operator assessments. Export provides a portable backup.

## DB-L2-004: One prominent development completion estimate

Traces to: DB-L1-001.

1. Given audited statuses, when the dashboard opens, then one dominant number shows
   estimated development completion, with equal requirement weights: not started
   0%, in progress 50%, needs verification 90%, accepted 100%. Wholly NON-MVP
   requirements are excluded before calculating the baseline, which is 64%.
2. Given a status changes or saved reviews are restored, when rendered, then the
   estimate updates from the whole scope regardless of search or workstream filters.
3. Given no requirements or none started, when estimated, then display 0%; show 100%
   only when all requirements are accepted. Round other estimates to whole percent,
   capped at 99%, and explain that this is a rough status estimate, not measured effort.
