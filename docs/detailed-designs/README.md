# FaithTech Toronto software design

These designs refine the [L1 scope](../specs/L1.md) and [60 L2 requirements](../specs/L2.md). The requirements remain normative, including every acceptance criterion. Quoted requirement excerpts retain their original wording.

Feature pages distinguish proposed components from inspected source behavior. The operator CLI designs identify existing provisioning, event, schedule, and access code alongside their proposed extensions. The standalone `design-system/` and static `docs/mocks/` provide design evidence; neither establishes production acceptance. Feature designs below share the following contracts.

## Architecture decisions

The API uses .NET 10, EF Core 10, SQL Server, and MediatR **12.5.0**. Microsoft.Extensions dependency injection, Options validation, Configuration, logging, and hosted services provide application composition. The deployment uses HTTPS and same-origin application/API hosting.

`backend/src/FaithTechTorontoAiBuildEvent.Domain` contains entities and domain policies without project or framework dependencies. `Application` contains feature commands, queries, handlers, validators, and infrastructure ports. `Infrastructure` implements SQL persistence, credentials, time, logo decoding, and notification delivery. `Api` composes these dependencies. Each type occupies a matching file and namespace; controllers reside in `FaithTechTorontoAiBuildEvent.Api.Controllers` and only bind, dispatch through MediatR, and return. A provisioning CLI resides under `backend/src` and uses the same application services. API acceptance tests reside in `backend/tests`.

Features use direct, feature-specific EF Core queries through application ports. A generic repository or separate microservice per subsystem adds no required behavior. SQL Server stores authoritative state. Redis transports SignalR notifications between API instances; it holds no sole copy of business state.

`frontend/projects/admin` and `client` own routed pages, guards, and dialogs. Sibling `domain` components and stores inject `api` contracts through tokens. Sibling `components` contains presentational inputs/outputs and imports no other project. Every Angular component has separate TypeScript, HTML, and stylesheet files.

Every consumed service has separate interface, token, and implementation files in `api`: for example `event-service.contract.ts` declares `IEventService`, `event-service.token.ts` exports `EVENT_SERVICE`, and `event.service.ts` declares `EventService`. Consumers call `inject(EVENT_SERVICE)` and never import implementations. Production composition binds HTTP adapters; Playwright composition binds mocks. HTTP, SignalR event conversion, and observable-to-signal conversion remain inside adapters. Domain state uses signals without HTTP dependencies. No frontend state container becomes an authorization boundary.

## HTTP and persistence contracts

Participant routes begin `/api/events/{eventId}`; administrator routes begin `/api/admin/events/{eventId}`. Authentication routes are explicitly identified in their feature designs. All identifiers are opaque UUIDs; route identity never grants access. Responses contain only fields authorized for that actor. Date/time instants use ISO 8601 UTC; event-local displays use the saved timezone and explicit dates. Scores and raffle identity use stable participant IDs, never names.

Each mutating intent supplies an `Idempotency-Key` UUID. Command descriptions use `OperationId` for that bound header value; clients do not supply conflicting body identities. Platform-level operations such as event creation use a nullable event scope. Authentication uses completion-only metadata after credential verification, never reusable secret-bearing receipts. `OperationReceipt` has `ActorId`, `EventId`, `OperationId`, `Target`, `PayloadHash`, `Result`, and `CommittedAtUtc`. A unique actor/event/operation key prevents duplicate commits. A canonical payload includes the route, method, target IDs, normalized submitted values, and expected version. Reuse with different content returns `409 operation-key-reused`. Authenticated receipt lookup precedes temporal rejection, so an authorized identical retry returns its committed result after closure. A failed validation or authorization creates no success receipt.

Ordinary receipts contain safe response data or identifiers for an authorized projection. Secret-revealing credential issuance is the exception: the receipt records issuance identity and completion only. A lost one-time code response requires explicit code replacement; no endpoint or retry recovers the code. Passwords, entry codes, session secrets, and CSRF tokens never enter receipts or logs.

Edits of existing mutable resources send `If-Match` with an opaque base64 SQL `rowversion`; missing versions return `428`. Version mismatch returns `409 stale-version` with the currently authorized value and version. The client retains the proposed draft for deliberate reapplication. EF maps concurrency tokens as infrastructure shadow properties, preserving the dependency-free domain. SQL uniqueness and transaction isolation enforce invariants spanning rows. Shared event/phase guards serialize only operations that change their invariants; unrelated answer rows do not share a global edit version.

`IEventStore` is the application transaction/query port implemented by `SqlEventStore` using `EventDbContext`. Its feature methods load authorized aggregates, check versions, and commit changes, receipts, audit records, and `OutboxMessage` rows together. Class diagrams show only each slice's contract members; shared interfaces combine these members, with concrete typed load/query methods rather than return-type-only overloads. Each feature describes additional unique keys and locks. A transaction returns success only after durable commit. Unique-key races map to the same conflict or original receipt as the corresponding serial execution.

`ProblemDetails` responses use stable `code`, `correlationId`, and field-error paths. Invalid JSON/syntax returns `400`, authentication failure `401`, forbidden authorized roles `403`, and concealed inaccessible resources `404`. Domain validation returns `422`, stale or conflicting state `409`, throttling `429` with positive `Retry-After`, and temporary infrastructure failure `503`. A timeout after an attempted commit is an unknown outcome: the UI reconciles with the same operation identity before offering a new intent.

Input handling trims surrounding whitespace, normalizes line endings to LF, and counts Unicode scalar values. Plain text remains literal text in every renderer. Common limits are names/options 1–200 characters, prose 5,000, messages 1–2,000, and HTTPS URLs 2,048. Tags contain at most 20 distinct normalized values across the three fields, each at most 40 characters. Optional fields permit empty values; draft exceptions follow L2-001 and L2-017. HTTPS URLs reject user information and are never fetched by the API. Accepted logo files decode as PNG, JPEG, or WebP, at most 2 MiB and 4,096 pixels per dimension; SVG and mismatched executable content fail validation.

## Time, closure, and connected updates

`IServerClock` obtains SQL Server UTC time. `QuizAdmissionBehavior` records `ReceivedAtUtc` durably at the API's authorized admission point, before queued answer processing. Its admission fence orders receipt against closure; final scores wait for all earlier admissions to resolve. The [quiz answer design](quizzes/answer-quiz/README.md) defines this protocol. Time-sensitive transactions sample current authoritative time for other actions. `EventSnapshot` includes server time, event endpoints, current stage ID/content, window states, visible activities, and authorized versions. Browser countdowns interpolate from the latest server sample using a monotonic timer, not the device wall clock.

`WindowClosure` stores event/window identity, effective closing instant, and closure reason. Before schedule edits or protected actions, `WindowPolicy` materializes every closure implied by the previously committed schedule and authoritative time. This works without a connected browser or timely background job. Once closed, quiz and selection windows remain closed despite later edits. Event completion likewise prevents participant link editing and timed activities from reopening; it does not add a time gate to profile, messaging, or safety actions. A hosted boundary worker records due closures and notifications; request-time checks remain authoritative if that worker is late. Administrator corrections are restricted to the explicit exceptions in each requirement.

`OutboxMessage` contains an ID, event ID, audience, affected resource ID/version, creation time, and dispatch lease. It carries invalidation metadata, never private message bodies or profile fields. `OutboxDispatcher` leases committed rows, publishes through `EventHub`, and marks delivery attempts complete after successful publication. Expired leases retry; duplicate notifications are harmless. A failed transaction emits nothing. Notification delivery is at least once and is not a substitute for durable state.

`EventHub` assigns groups from validated server-side sessions: event-wide public data and participant-specific private audiences. It accepts no arbitrary client-selected group membership. Multiple API instances use the SignalR Redis backplane and load-balancer session affinity. Redis outage leaves SQL state intact and triggers reconnecting/stale UI; delivery resumes through the outbox. Clients refetch authorized resources after invalidation and on focus, pageshow, or reconnection. Out-of-order responses cannot overwrite a newer resource version; uncertain ordering triggers another authoritative read. A periodic five-second reconciliation read detects missed notifications; scheduled boundaries use server-sampled local timers plus immediate refetch. The two-second healthy connected deadline is measured, not inferred from the five-second safety poll.

Reconnect performs session validation before private rendering, then obtains a complete authorized snapshot. It skips missed animations. The client renders within two seconds after successful synchronization; pending/failed synchronization remains visibly unavailable. A non-event activity retains its view and in-memory draft when the current stage changes, and announces the new stage with a navigation action. The main event page switches automatically.

## Authentication and privacy

Participant and administrator authentication use separate Secure, HttpOnly, SameSite cookies containing protected references to SQL session records. Anonymous `GET /api/events/{eventId}/antiforgery` and `GET /api/admin/antiforgery` issue same-origin antiforgery tokens and the corresponding antiforgery cookie. Request tokens stay in memory for unsafe-method headers, including authentication requests. Cookie-authenticated mutations reject missing/invalid antiforgery tokens. Secrets never appear in URLs, analytics, browser persistent storage, or logs. Database backups and the ASP.NET Data Protection key ring require protected operator storage.

`SessionAuthorizationBehavior` validates active registration, session revocation, event scope, and ownership on every protected request, including receipt retrieval. SignalR connection establishment and client invocations perform the same validation. Participant sessions expire absolutely after 24 hours. Administrator sessions expire after 30 minutes of explicit user inactivity or eight hours absolutely; polling, hub traffic, and background refresh do not extend idle expiry. A dedicated interaction endpoint records authenticated deliberate activity. Deactivation and credential replacement revoke sessions transactionally and emit private invalidations. Sign-out revokes only the current session.

Private HTTP responses use `Cache-Control: no-store`. The client conceals private views during restored-tab validation and clears private signals and drafts on sign-out, known expiry, or revocation. History navigation and the back-forward cache follow the same guard. An offline browser cannot detect an unknown remote revocation; known local expiry still clears content. Sensitive recipient data is never included in a broadly broadcast notification.

Authentication failure counters use SQL-backed rolling windows under a short transaction lock. Limits are ten failed attempts per source in five minutes and five per submitted participant code or administrator account in five minutes. Keyed hashes represent attempted identities without storing secrets; nonexistent identities receive the same treatment. Trusted proxy configuration determines source addresses. Throttled attempts do not test credentials or extend the window. Chat limits count only committed new sends: 30 per sender per minute, excluding an identical committed retry.

## Verification and deployment

Each feature identifies acceptance scenarios for API integration tests and Playwright page objects. Tests state behavior; selectors reside in one page object per screen. No architecture or specification-parsing tests form part of the design. Mock-backed browser tests prove UI behavior; separate real-API integration, connected-client, restart, and load checks establish production claims.

The UI matrix includes 320, 575, 576, 767, 768, 991, 992, 1199, 1200, and 1440 CSS-pixel widths; loading, empty, error, and populated states; keyboard use; and 200% zoom. Latest stable Chrome validation records the actual version, operating system, motion setting, and GPU/audio capability. The existing mock evidence does not transfer these claims to future Angular components.

Deployment configuration supplies SQL, Redis for multiple instances, protected key storage, allowed origins, map-tile provider settings, and operator credentials through validated Options. Validated logo bytes reside in SQL beside event configuration. Startup fails with redacted actionable configuration errors. Private health endpoints expose SQL readiness and outbox lag to operators. Audit records contain actor/event/action/time/outcome and stable IDs, excluding credentials, emails, messages, biographies, tags, and report text. Logs use correlation IDs and parameter redaction.

Load acceptance uses 200 participants and two administrators, a two-minute warm-up and ten-minute measurement repeated ten times. Reads occur every five seconds, participant mutations every 30 seconds, and administrator mutations every minute, including a transition and raffle. Real-API p95 is at most one second, network failures at most 1%, and connected updates at most two seconds. A separate quiz burst submits 200 answers in one second. Reports retain environment, configuration, raw timings, and failure counts; these are acceptance targets, not measurements supplied by this design.

SQL backups include all authoritative entities, receipts, and outbox rows. Operators restore into an isolated environment with outbound notifications disabled, verify identities and retained state, then enable traffic deliberately. Redis is rebuilt rather than restored as authoritative data. Schema migrations run once before application rollout; rollback uses a compatible application build or an explicitly rehearsed database restore. The operations feature defines the recovery procedure in detail.

## Local Super admin CLI

The seven operator feature designs cover L2-049 through L2-060. The installed command is `faithtech-admin`, packaged from the existing Provisioning project. It connects directly to an explicitly selected database with a separate operator identity. It does not use browser authentication or add a SQL HTTP endpoint. The API retains its existing role and event boundaries.

The [target design](operations/connect-operator-target/README.md) defines protected profiles and a database-operator actor registry. The [operation protocol](operations/review-and-reconcile-operations/README.md) owns preview/apply, ActorKind-compatible receipt changes, atomic unit-of-work behavior, migrations, output, journals, and recovery. These operator rules refine the application-only GUID actor description earlier in this index; existing application actors remain distinct.

The [event maintenance design](operations/manage-event-data/README.md) defines shared noncommitting mutation helpers used by API and operator transaction wrappers. [Seed import](operations/import-event-seed/README.md) commits event settings and schedule together. Validated writes reuse application validation and the proposed platform outbox; raw [SQL repairs](operations/execute-data-repair/README.md) preserve operator transaction control and do not promise application-invariant enforcement or automatic invalidation.

The CLI's L2-060 performance scenario uses two ten-minute intervals after warm-up. Existing broader platform measurement designs remain separate. The documents define implementation and behavioral acceptance obligations; diagram rendering does not demonstrate a successful deployment, database mutation, or load test.

## Technical sources

.NET 10 is the selected supported LTS baseline ([Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)). SQL concurrency uses EF Core concurrency tokens and conflict handling ([EF Core concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)); transactional persistence follows [EF Core transaction guidance](https://learn.microsoft.com/en-us/ef/core/saving/transactions), with MARS disabled for savepoint compatibility. SignalR deployment follows the [ASP.NET Core scale guidance](https://learn.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-10.0). Outbox durability and resynchronization are application design decisions supporting L2-044.

Cookie authority is revalidated on each request using the framework's supported validation hooks ([cookie authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)). Same-origin request-token handling follows [ASP.NET Core antiforgery guidance](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0). Redis fanout uses the [SignalR backplane](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane?view=aspnetcore-10.0); durable replay and full-state reconciliation remain application responsibilities.

## Feature index

All 33 feature designs own primary coverage for the 60 L2 requirements. Each includes the three C4 levels, a typed structure diagram, and behavior sequences with rendered PNG siblings. Shared constraints apply across the tree. The [verification record](REVIEW.md) records scope and asset checks.

| Subsystem | Feature design | Primary requirements |
|---|---|---|
| event-administration | [Configure events](event-administration/configure-events/README.md) | L2-001, L2-047 |
| event-administration | [Manage the event roster](event-administration/manage-roster/README.md) | L2-002 |
| access | [Enter an event](access/enter-event/README.md) | L2-003, L2-046 |
| access | [Manage participant and administrator sessions](access/manage-sessions/README.md) | L2-004, L2-038 |
| event-flow | [Follow the live event](event-flow/follow-live-event/README.md) | L2-005, L2-007 |
| event-flow | [Configure the event schedule](event-flow/configure-schedule/README.md) | L2-006, L2-008 |
| teams-projects | [Assign participants to teams](teams-projects/assign-teams/README.md) | L2-009, L2-010 |
| teams-projects | [Choose an event project](teams-projects/choose-project/README.md) | L2-011, L2-012 |
| community | [Share a participant profile](community/share-profile/README.md) | L2-013 |
| community | [Find people and explain matches](community/find-people/README.md) | L2-014, L2-048 |
| community | [Exchange private messages](community/exchange-messages/README.md) | L2-015 |
| community | [Block, report and resolve conversation abuse](community/moderate-conversations/README.md) | L2-016 |
| quizzes | [Configure a quiz lifecycle](quizzes/configure-quiz/README.md) | L2-017 |
| quizzes | [Answer a quiz and read final results](quizzes/answer-quiz/README.md) | L2-018, L2-019 |
| raffles | [Configure prizes and inspect eligibility](raffles/configure-prizes/README.md) | L2-020 |
| raffles | [Draw and present a raffle winner](raffles/draw-winner/README.md) | L2-021, L2-022 |
| showcase | [Schedule team demonstrations](showcase/schedule-demos/README.md) | L2-023 |
| showcase | [Publish team build links and recap](showcase/publish-team-build/README.md) | L2-024, L2-025 |
| companion-links | [Expose optional Liturgy links](companion-links/connect-liturgy/README.md) | L2-026, L2-027, L2-028 |
| design-system | [Publish the standalone design gallery](design-system/publish-gallery/README.md) | L2-029, L2-030 |
| design-system | [Match the recorded Cornerstone light reference](design-system/match-light-reference/README.md) | L2-031, L2-032, L2-033 |
| browser-experience | [Use the responsive accessible client](browser-experience/use-accessible-client/README.md) | L2-034, L2-035, L2-036, L2-037 |
| platform-security | [Protect event access and private state](platform-security/protect-access/README.md) | L2-039, L2-041 |
| platform-security | [Validate input and enforce abuse limits](platform-security/validate-requests/README.md) | L2-040, L2-042 |
| operations | [Operate and measure the event platform](operations/operate-event/README.md) | L2-043, L2-045 |
| operations | [Recover committed state after interruption](operations/recover-state/README.md) | L2-044 |
| operations | [Install and update the operator tool](operations/install-operator-tool/README.md) | L2-049 |
| operations | [Connect to an explicit operator target](operations/connect-operator-target/README.md) | L2-050, L2-051 |
| operations | [Manage event data through validated commands](operations/manage-event-data/README.md) | L2-052 |
| operations | [Manage roster credentials and administrator access](operations/manage-operator-access/README.md) | L2-053 |
| operations | [Import an event seed without losing event history](operations/import-event-seed/README.md) | L2-054, L2-055 |
| operations | [Execute explicitly reviewed SQL data repairs](operations/execute-data-repair/README.md) | L2-056 |
| operations | [Review, apply and reconcile operator operations](operations/review-and-reconcile-operations/README.md) | L2-057, L2-058, L2-059, L2-060 |
