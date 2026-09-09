# Implementation evidence

The operator seed-import increment adds explicit target inspection, protected
preview/apply, atomic event/schedule imports, database-principal attribution, and
receipt/history reconciliation. Migration callers now use the reviewed flow.
See [the operator runbook](operator-event-import.md). This does not implement the
remaining general event/roster/SQL maintenance CLI commands or participant features.

Requirements in [specs/L2.md](specs/L2.md) remain the acceptance authority. Detailed designs
and mocks supply implementation and visual inputs. Completion requires all 48
requirements, including their cross-cutting criteria.

## Current completion boundary

The full implementation plan is **incomplete**. Administrator functionality
covers provisioned access, session management, draft creation/listing/copying,
editing titles, venue details, coordinates, waiting/closing content, directions,
timezone and dated event endpoints, validated venue logos, the optional Use
Liturgy setting, scheduled stages/content, independent selection and demo
presentation windows, applying the editable September 9 reference configuration,
and the full roster lifecycle (add, rename, deactivate, replace entry code,
reactivate). The participant application now authenticates by email and entry
code, holds a 24-hour absolute session across refresh/history/restored tabs,
signs out, and offers core navigation (current event, schedule, teams, people,
messages, quiz, raffle, showcase — the last seven as an explicit stub, not a
broken link) behind a session guard that re-verifies the server on every entry.
A protected deep link resumes after authentication via a server-validated
return target; switching between two events in one browser session cannot
show one event's private content under the other's URL.

Explicit publication and the remaining activity configuration (schedule
countdown/live-stage participant screens, teams/projects, profiles/messaging,
quizzes, raffle, demos/showcase content) remain unimplemented — the "current
event" and "showcase" destinations above are routing placeholders, not the
built screens L2-005 through L2-028 require. Participant companion-link
filtering and project URL storage remain unimplemented. Passing tests below
establish only the implemented behaviors, not completion of any entire
cross-cutting requirement.

Latest verification on 8 September 2026: **127 API acceptance cases** against
isolated SQL Server databases, **90 Playwright cases** with injected mock
adapters across both the administrator and participant applications, and
successful builds of all .NET projects and all five Angular projects. Browser:
Chrome, Windows ARM64. No performance/load acceptance is claimed.

Continue with schedule, countdown, transitions and synchronization
(L2-005/006/007/044) as the next participant-facing slice, then teams and
projects. Continue through the entire ordered delivery queue below. Remaining
shared work includes distributed notifications/outbox, authentication
completion receipts, production key/proxy configuration, complete error/state
matrices, and operational recovery/load verification. Preserve the separate
Azure deployment runbook being maintained alongside implementation.

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
| Roster lifecycle | Rename, deactivate, replace-code and reactivate all use the roster's existing version/receipt/audit pattern. Deactivation and code replacement revoke every live participant session for the registration in the same transaction; reactivation never revives a revoked session and is rejected with a field-specific error when the entry's retained email now belongs to a different active entry, until an admin clears that binding via code replacement. Browser acceptance covers the confirmation dialogs, the already-present "replace the entry code" action, and focus handling when a freshly issued code renders into an already-open dialog. |
| Event copying | `POST /api/admin/events/{id}/copy` clones content, venue and schedule into a new draft, shifting every dated interval by the elapsed duration between the source's resolved start and the supplied new start; stage identities are remapped, registrations/credentials are never copied, Use Liturgy is forced off, and a dated-interval copy with no resolved source start returns a configuration error instead of guessing. |
| Participant authentication | `POST /api/events/{id}/session` atomically binds a trimmed, case/whitespace-insensitive email to an event-scoped entry code on first use, or resumes an existing binding; same-code races share the administrator authentication budget's rolling-window lock, so exactly one of two concurrent different-email attempts against one code wins. Malformed email returns a field-specific 422; every other rejection (unpublished event, inactive entry, wrong-event code, mismatched or duplicate email) returns a generic 401 that discloses nothing about other participants. A second cookie scheme, scoped to `/api/events/{id}` and independently validated against the request's own event-id path segment on every request, keeps one event's session from being honored on another's route even if replayed manually. |
| Participant session and navigation | `GET`/`DELETE /api/events/{id}/session` read and revoke the current session only (24-hour absolute expiry, no idle extension). The client's event shell, behind a guard that re-verifies the server on initial navigation, browser history and a restored tab, offers current-event/schedule/teams/people/messages/quiz/raffle/showcase navigation and sign-out; unbuilt activities render a shared stub, never a broken link. A protected deep link carries a `returnTo` through the access screen and lands there after authentication only if the server's `ReturnTargetPolicy` accepts it as a same-event, recognized-route request; an external URL, a protocol-relative one, or another event's route falls back to a computed default. The session panel and shell track the current event reactively (not a one-time route snapshot) so that reusing the shell component across an in-app switch between two events cannot show the prior event's content under the new one. |

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

Publication field errors and valid empty-schedule publication also remain. Publication
must not be enabled until its complete configuration and participant-access checks
exist. Distributed invalidation, restart/restore verification and the remaining
delivery groups are still required; no complete L2 requirement is declared satisfied
by these slices.

## Roster and participant access continuation

`SqlRosterStore` gained `Rename`, `Deactivate`, `ReplaceCode` and `Reactivate`,
each following `Add`'s existing `sp_getapplock`/receipt/audit shape. Deactivate
and ReplaceCode call into `IParticipantStore.RevokeSessionsForRegistration`
within the same transaction (both stores share the request's `EventDbContext`
instance, so the revoke is atomic with the roster mutation, not a follow-up
write). Reactivate checks the entry's retained `NormalizedEmail` against every
other *active* registration before flipping `Active`, since the partial unique
index that enforces "one active entry per email" only ever covered active rows
— a different entry can freely bind an email while the original sits inactive.

`POST /api/admin/events/{id}/copy` reuses `ScheduleMapping.Input`/`Apply` and
`ScheduleValidator.Normalize` to shift and re-validate a schedule rather than
re-implementing interval arithmetic; only content that already exists (no
teams, quizzes or prizes yet) is copied.

`AuthenticationBudget.Verify` was generalized to take a caller-supplied scope
key instead of assuming an administrator username, so participant sign-in
shares its SQL-persisted rolling-window throttle (keyed by event and entry
code) with administrator sign-in rather than a second implementation.
`ParticipantCookieEvents` mirrors `AdministratorCookieEvents` but additionally
compares the session's bound event claim against the request path's `eventId`
segment on every validation — cookie path scoping alone is not a security
boundary against a non-browser client replaying the cookie value cross-path.
An anonymous antiforgery-token fetch made after a participant signs in still
observed an unauthenticated principal (the "Participant" scheme, unlike the
default admin scheme, is authenticated only when something explicitly
requests it), so `EventAntiforgeryController` now authenticates that scheme
itself before generating tokens, keeping generation and later validation
consistent.

The client's `ParticipantSessionPanel` and `ClientShell` initially captured
`:eventId` once via `ActivatedRoute` snapshot; because the shell's route
config is identical for every event, Angular reuses the component instance
across an in-app switch between two events, so a snapshot would leave the nav
links and the session check pinned to whichever event loaded first. Both now
track `eventId` reactively (a `paramMap`-derived signal on the shell; an
`effect` that tears down and re-verifies on the panel) instead.
`GetAuthorizedInitialRoute` computes a same-event landing route from the
event's resolved end time (a completed event resolves to `/showcase`;
everything else to the shell's index, since dedicated countdown/current-stage
screens don't exist yet) and validates any client-supplied `returnTo` through
`ReturnTargetPolicy` before honoring it.

The next acceptance increments must build the actual schedule/countdown/current-stage
participant screens that `GetAuthorizedInitialRoute` and the shell's stub routes
currently stand in for, then teams and projects. Publication remains blocked on
its own complete configuration and participant-access checks. Distributed
invalidation, restart/restore verification and the remaining delivery groups are
still required; no complete L2 requirement is declared satisfied by these slices.

Implementation references: [EF Core transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions),
[Angular route guards](https://angular.dev/guide/routing/route-guards),
[Angular signal effects](https://angular.dev/guide/signals#effects),
[invalid local times](https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.isinvalidtime?view=net-10.0)
and [ambiguous offsets](https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.getambiguoustimeoffsets?view=net-10.0).

## Delivery queue

1. Administrator provisioning, authentication, expiry and sign-out (L2-038/041/042). **Done.**
2. Event configuration, copying, preset and roster (L2-001/002/008/047). **Done.**
3. Participant access, continuity and navigation (L2-003/004/046). **Done**, except the actual
   countdown/current-stage/recap screens the shell's stub routes and computed initial route
   currently stand in for — those depend on group 4.
4. Schedule, countdown, transitions and synchronization (L2-005/006/007/044). **Next.**
5. Teams and projects (L2-009–012).
6. Profiles, discovery, messaging and safety (L2-013–016/048).
7. Quizzes and final results (L2-017–019).
8. Prizes and raffle (L2-020–022).
9. Demos, build links, showcases and Liturgy (L2-023–028).
10. Gallery, visual fidelity, browser matrix and operations (L2-029–045).

Each group is split into acceptance-driven increments. Security, accessibility,
input validation and recovery accompany each relevant behavior. Passing static
mock tests does not establish production acceptance.
