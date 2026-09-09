# Implement the Toronto AI Build Event companion

Implement the 18 detailed designs and all 34 active L2 requirements as small, acceptance-test-led vertical slices. Create the .NET Clean Architecture solution under `backend/` and the Angular workspace under `frontend/`; never restore the retired multi-event implementation.

## Delivery order

1. Public event state, Countdown, fresh schema, readiness, and seeded RTR project.
2. Server time and countdown synchronization.
3. Direct SQL credential replacement and inline administrator authentication. Security hardening, session-protection guarantees, and durable abuse budgets are NON-MVP.
4. Email entry, protected private-session recovery, optional profiles, and Countdown roster administration.
5. Presenter-controlled screen transitions, projects, first-time random team formation, manual member moves, and project assignments.
6. Raffle eligibility, random durable draws, retained results, and synchronized Cornerstone presentation. Concurrency/race/idempotency guarantees are NON-MVP.
7. SignalR notification recovery, CLI installation/passcode rotation, accessible responsive frontend completion, release automation, and operational runbook/load/restore evidence.

## Rules for every slice

- Start from an L2 Given–When–Then criterion and a failing behavioral test. Acceptance tests include the required `Acceptance Test`, `Traces to`, and `Description` comments.
- Use real API/SQL/SignalR acceptance tests for backend behavior. Use Playwright page objects—one per public screen—with mocked API contracts for frontend behavior.
- Keep all UI in published `@quinntyne/cornerstone`. Verify `0.2.0` first; implement missing capabilities upstream, publish them, and consume the released version.
- Use the simplest sequential persistence and SignalR behavior that satisfies the MVP requirements. Security hardening, privacy enforcement, abuse protection, auditing, and concurrency/race/idempotency/stale-version guarantees are NON-MVP and must not be added to the remaining implementation.
- Build, test, type-check, lint, inspect the diff, and commit every verified behavior before starting the next. Do not add architecture or specification-parsing tests.

## Completion evidence

Completion requires all MVP L2 criteria, clean npm-only Cornerstone consumption, installed Windows CLI/direct SQL verification, full API/browser suites, production build/package checks, measured realtime evidence, and isolated database-restore evidence. NON-MVP security, privacy, abuse protection, auditing, and concurrency guarantees are excluded. The runbook must document setup, readiness, backups every 15 minutes during the event, and restore steps.
