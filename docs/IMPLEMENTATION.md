# Implementation evidence

Requirements in [specs/L2.md](specs/L2.md) remain the acceptance authority. Detailed designs
and mocks supply implementation and visual inputs. Completion requires all 48
requirements, including their cross-cutting criteria.

## Current completion boundary

The full implementation plan is **incomplete**. The participant application's
route table is empty. Administrator functionality currently covers provisioned
access, session management, draft creation/listing, and editing titles, venue
details, coordinates, waiting/closing content, directions, timezone and dated
event endpoints, validated venue logos, the optional Use Liturgy setting, scheduled
stages/content, independent selection and demo presentation windows, and applying
the editable September 9 reference configuration.
Publication, copying, roster management and the remaining activity configuration remain
unimplemented. Participant companion-link filtering and project URL storage remain
unimplemented. Passing tests below establish only
the implemented behaviors, not completion of any entire cross-cutting requirement.

Latest verification on 8 September 2026: **83 API acceptance cases** against
isolated SQL Server databases, **70 Playwright cases** with injected mock adapters,
and successful builds of all .NET projects and all five Angular projects. Browser:
Chrome **152.0.7977.76**, Windows ARM64. No performance/load acceptance is claimed.

Continue with remaining schedule acceptance and roster management,
then participant access and explicit publication, followed by event copying.
Continue through the entire ordered delivery queue below. Remaining shared work
includes distributed notifications/outbox,
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
| Event editor API | Protected detail reads and versioned saves retain independent event values. Invalid supplied links preserve the draft; stale saves return current authorized values; retries return the original committed outcome after later edits. |
| Concurrent edits and audit | Simultaneous saves from one version produce exactly one update, receipt and attributable audit. Missing authentication, antiforgery or expected version commits nothing. |
| Event dates | Nine API cases cover overnight dates, browser minute precision, both valid offsets for repeated local times, missing/mismatched offsets, nonexistent times, invalid zones and nonpositive intervals. Local dates and resolved UTC instants survive rereading and appear in event listings. |
| Editor browser recovery | Opening, editing, saving and reopening; explicit unsaved discard; retained field errors; deliberate conflict reapplication; lost-response retry producing one save; client Unicode scalar limits and normalization. |
| Editor layout | Rendered desktop inspected against the settings mock. Populated and validation states pass Axe and horizontal-overflow checks separately at all ten required widths. This does not establish the full loading/error/zoom/browser matrix or complete mock parity. |
| Venue logo API | 24 additional API cases cover PNG/JPEG/WebP, exact 2 MiB and 4,096-pixel boundaries, malformed/mismatched uploads, all eight EXIF orientations, private serving, concurrent retry, stale replacement, authentication and antiforgery. Canonical PNG bytes, event version, receipt and audit commit together in SQL. |
| Venue logo editor | Injected service upload and private preview, retained invalid selections, explicit stale recovery, lost-response retry, selection clearing, unsaved navigation, saved text/logo coexistence, and preview failure/retry pass browser acceptance. Branding is grouped with venue information. |
| Optional companion setting | New drafts default off. Administrator enable/disable/re-enable survives API rereads and browser refreshes; another event keeps its default. Unauthorized writes fail. This does not establish participant link filtering, URL retention or connected-update behavior, which require the remaining project/participant implementation. |
| Schedule persistence | Dated overnight stages retain stable identities through edits and deletion. Adjacent intervals are accepted; overlap is rejected. Schedule changes use the event version, transactional receipts and audit records; ordinary event edits cannot invalidate saved intervals. |
| Schedule editor | Stage/content editing and independent selection/presentation windows survive save and refresh. Browser acceptance covers lost-response retry, explicit stale reapplication, unsaved navigation, dialog focus, and empty/populated/validation/overlay accessibility at ten widths. |
| Schedule closure | SQL time determines elapsed published windows and events without a worker. Disabling an open or elapsed selection window preserves closure, and completed events cannot be extended into the future. Tests seed publication as their Given; no publication endpoint is delivered. |
| Schedule validation feedback | Rejected event/window/stage fields retain entered values and receive an announced, focused summary with links and accessible descriptions. Stage links open the affected dialog field and follow stable identities after other rows are removed. Removing a stage or disabling a window clears obsolete field feedback; stage removal restores focus to its heading. Twelve additional API cases verify field-specific rejection without changing persisted configuration. |
| September 9 reference | The protected reference action atomically applies all 21 distinct entries, phase boundaries, combined timed content, the 20:00 reminder, independent activity windows, Stone Church and Use Liturgy off. Blank drafts retain absent address/coordinates/logo and are not published. API acceptance verifies editable content, fresh identities across events, access/CSRF/version protection, and old-operation retry after later edits without another audit. Browser acceptance verifies explicit replacement confirmation, cancellation, refresh, lost-response retry, stale reconfirmation, retained rejected drafts, and the reference dialog at all ten widths. Participant transitions and activity behavior remain dependent on later slices. |

## Event editor continuation

`GET/PUT /api/admin/events/{id}` now reads and saves draft details. PUT requires an
`Idempotency-Key` UUID and `If-Match` version. `EventInput.start` and `.end` each
carry a dated `local` value and optional `offsetMinutes`; the API resolves offsets
against the supplied timezone, rejecting invalid or unresolved ambiguous times.
Drafts may retain incomplete publication fields. These endpoints do not publish.

The Angular editor consumes `EVENT_SERVICE`; the production adapter sends HTTPS
same-origin API requests and the Playwright composition uses an injected adapter.
Browser fixtures retain event state outside the page so refreshes and controlled
lost responses can be exercised without reaching the production adapter.

`POST /api/admin/events/{id}/logo` accepts multipart `file`, the same expected-version
header and a fresh operation UUID. The authenticated GET at that route serves the
stored PNG with no-store caching and nosniff. Uploads are decoded with SkiaSharp
4.151.2 and re-encoded with orientation applied; original metadata and trailing
content are not served. NuGet's vulnerability audit, including transitive packages,
reported no known vulnerable packages on 8 September 2026. MediatR remains 12.5.0.

`GET/PUT /api/admin/events/{id}/schedule` reads and atomically saves event timing,
stages and independent activity windows, using the same expected-version and
operation headers as event edits. The Angular schedule page consumes
`SCHEDULE_SERVICE` and owns stage and unsaved-change dialogs.

`POST /api/admin/events/{id}/reference-schedule` applies the September 9 template
with the same expected-version and operation headers. It replaces the schedule,
sets the venue name and companion setting, and retains other event details.
The browser describes those changes before confirmation. Template content comes
from L2-008 and the supplied presentation; it does not provision project choices,
registrations, credentials, quizzes or prizes.

The next acceptance increments must complete quiz references and closure, participant
stage transitions, and participant-access checks. Publication field errors and
valid empty-schedule publication also remain. Publication must not be enabled until its
complete configuration and participant-access checks exist. Distributed
invalidation, restart/restore verification and the remaining delivery groups are
still required; no complete L2 requirement is declared satisfied by these slices.

Implementation references: [EF Core transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions),
[Angular route guards](https://angular.dev/guide/routing/route-guards),
[invalid local times](https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.isinvalidtime?view=net-10.0)
and [ambiguous offsets](https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.getambiguoustimeoffsets?view=net-10.0).

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
