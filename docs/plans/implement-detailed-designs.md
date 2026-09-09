# Implement the Toronto AI Build Event companion

Implement the 18 detailed designs and all 34 active L2 requirements as small, acceptance-test-led vertical slices. Create the .NET Clean Architecture solution under `backend/` and the Angular workspace under `frontend/`; never restore the retired multi-event implementation.

## Delivery order

1. Public event state, Countdown, fresh schema, readiness, and seeded RTR project.
2. Server time and countdown synchronization.
3. Direct SQL credential replacement, inline administrator authentication, session expiry, request protection, and durable budgets.
4. Email entry, protected private-session recovery, optional profiles, and Countdown roster administration.
5. Presenter-controlled screen transitions, projects, first-time random team formation, manual member moves, and project assignments.
6. Raffle eligibility, atomic cryptographic draws, retained-result privacy, and synchronized Cornerstone presentation.
7. SignalR notification recovery, CLI installation/passcode rotation, accessible responsive frontend completion, release automation, and operational runbook/load/restore evidence.

## Rules for every slice

- Start from an L2 Given–When–Then criterion and a failing behavioral test. Acceptance tests include the required `Acceptance Test`, `Traces to`, and `Description` comments.
- Use real API/SQL/SignalR acceptance tests for backend behavior. Use Playwright page objects—one per public screen—with mocked API contracts for frontend behavior.
- Keep all UI in published `@quinntyne/cornerstone`. Verify `0.2.0` first; implement missing capabilities upstream, publish them, and consume the released version.
- Use decimal-string event versions, operation IDs, authorized snapshot projections, event-lock serialization, and content-free SignalR invalidations as specified in the detailed designs.
- Build, test, type-check, lint, inspect the diff, and commit every verified behavior before starting the next. Do not add architecture or specification-parsing tests.

## Completion evidence

Completion requires all active L2 criteria, clean npm-only Cornerstone consumption, installed Windows CLI/direct SQL verification, full API/browser suites, production build/package checks, measured 200-viewer realtime evidence, and isolated database-restore evidence. The runbook must document setup, readiness, redacted diagnostics, backups every 15 minutes during the event, and session/receipt invalidation during restore.
