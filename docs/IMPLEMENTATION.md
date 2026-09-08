# Implementation evidence

Requirements in `specs/L2.md` remain the acceptance authority. Detailed designs
and mocks supply implementation and visual inputs. Completion requires all 48
requirements, including their cross-cutting criteria.

## Current completion boundary

The full implementation plan is **incomplete**. The participant application's
route table is empty. Administrator functionality currently covers provisioned
access, session management, and creation/listing of event drafts with a title.
There is no event editing or publication yet. Passing tests below establish only
the implemented behaviors, not completion of any entire cross-cutting requirement.

Latest verification on 7 September 2026: **19 API acceptance cases** against
isolated SQL Server databases, **19 Playwright cases** with injected mock adapters,
and successful builds of all .NET projects and all five Angular projects. Browser:
Chrome **152.0.7977.76**, Windows ARM64. No performance/load acceptance is claimed.

Continue with event configuration/editing/publication, then the ordered delivery
queue below. Remaining shared work includes distributed notifications/outbox,
authentication completion receipts, production key/proxy configuration, complete
error/state matrices, and operational recovery/load verification. Preserve the
separate Azure deployment runbook being maintained alongside implementation.

## Verified increments

| Increment | Evidence |
|---|---|
| Backend build foundation | .NET 10.0.400 solution build: zero warnings/errors; no product behavior claimed. |
| Administrator access and sessions | 10 API acceptance cases pass against isolated, migrated SQL Server databases: access denial, CSRF, secure sign-in, unknown account, expiry, interaction, sign-out and account disabling. |
| Operator CLI | Migrate, create-admin and disable-admin executed against a disposable SQL database; disabled state verified in SQL. |
| Authentication abuse budgets | 14 total API cases pass. Account/source limits, normalized nonexistent identities, expiry and concurrent failures use SQL-persisted HMAC keys and transaction locks. |
| Administrator browser access | 17 browser scenarios pass: access, session restoration, expiry, failed sign-out, keyboard navigation, ten viewport widths and event draft creation. Desktop rendering inspected against the input mock. |
| Event drafts | SQL-backed creation and listing, normalized title validation, durable identical retries, changed-operation conflicts and transactional creation audit records. |
| Same-origin hosting | 18 API cases pass. Published HTTPS process verified with isolated SQL: Angular shell, provisioned sign-in, draft creation/retry and sign-out. |
| Review fixes | 19 API / 19 browser cases pass. Database Options reject missing configuration at startup; explicit administrator interaction renews idle expiry; discard confirmation restores keyboard focus. |

## Delivery queue

1. Administrator provisioning, authentication, expiry and sign-out (L2-038/041/042).
2. Event configuration, copying, preset and roster (L2-001/002/008/047).
3. Participant access, continuity and navigation (L2-003/004/046).
4. Schedule, countdown, transitions and synchronization (L2-005/006/007/044).
5. Teams and projects (L2-009–012).
6. Profiles, discovery, messaging and safety (L2-013–016/048).
7. Quizzes and final results (L2-017–019).
8. Prizes and raffle (L2-020–022).
9. Demos, build links, showcases and Liturgy (L2-023–028).
10. Gallery, visual fidelity, browser matrix and operations (L2-029–045).

Each group is split into acceptance-driven increments. Security, accessibility,
input validation and recovery accompany each relevant behavior. Passing static
mock tests does not establish production acceptance.
