# FaithTech event companion — detailed design

## Scope and design authority

This design describes the dedicated September 9, 2026 Toronto build-event companion. The [L1](../specs/L1.md) and [L2](../specs/L2.md) specifications define behavior. The approved [Angular mock](../mocks/README.md) defines the visual and interaction reference. Exactly four screens exist: Countdown, Projects, Team selection, and Raffle. Administrator tools appear on those screens. PowerPoint remains independently operated.

The target uses a **fresh SQL Server database**. Existing accounts, individual entry codes, events, registrations, and migrations are not imported. This decision does not authorize deletion of an existing database. The documents describe proposed production behavior, not completed implementation or passing acceptance evidence.

## Shared architecture and protocol

One Angular `client` application composes `domain`, `api`, and presentational `components` sibling libraries in `frontend/projects/`. `api` owns interfaces, tokens, data contracts, HTTP adapters, SignalR connections, and conversion into readonly signals. Consumers inject tokens; concrete adapters are bound only at composition. Each interface, token, implementation, component class, template, and stylesheet has its own file. `components` composes published Cornerstone UI and has no application-service dependency. The mock workspace remains independent.

The existing .NET `Api`, `Application`, `Domain`, `Infrastructure`, and `Provisioning` projects remain architectural homes under `backend/src`. Domain references no project. Application owns commands, queries, validators, handlers, and persistence interfaces; Infrastructure implements them. Controllers only bind, dispatch through MediatR **12.5.0**, and return typed outcomes. Cross-cutting authentication, authorization, validation, and exception translation execute through middleware or dispatch behaviors. Microsoft.Extensions supplies DI, Configuration, Options, and logging.

`CompanionDbContext` is a proposed EF Core context with its own fresh migration history, separate from legacy `EventDbContext`. SQL Server stores a singleton `EventState`, participants, projects, teams, draws, sessions, operation receipts, authentication attempts, change records, and audit records. UTC timestamps come from `SYSUTCDATETIME()` inside transactions. SQL connections require encryption and server-certificate validation.

The API serves the single Angular build and four route fallbacks from one HTTPS origin. Public API paths start `/api/event`; administrator paths `/api/admin`; owner-private paths `/api/participant`. Public, administrator, and participant SignalR endpoints are `/api/event/live`, `/api/admin/live`, and `/api/participant/live`. Distinct cookie schemes and path scopes prevent unrelated identity substitution. Administrator and participant identities may coexist in a browser; logout clears only the selected identity. Browser tabs on the same origin share production cookies, unlike the mock's illustrative tab roles.

### Commands, versions, and retries

HTTP performs all mutations. SignalR pushes notifications and supplies no arbitrary group-join or data-mutation methods. `CommandEnvelope<T>` contains `operationId` (UUID), `expectedVersion` (decimal string), and `input` (typed payload). Every event-data mutation serializes on the singleton event row using a transaction-owned update lock held through commit. An event-wide increasing `bigint` version deliberately accepts conservative conflicts at this event's small scale. All SQL bigint versions travel as decimal strings, avoiding JavaScript number precision loss.

The transaction checks current authority, looks up the actor-scoped operation receipt, compares its canonical input digest, then checks version and business rules. An exact authorized retry resolves the original durable outcome before checking a now-stale version. Reuse with changed input returns `409 operation-mismatch`. A new stale operation returns `409 stale-version` with an authorized current projection. Successful event mutations increment the version and atomically save state, receipt, audit metadata, and an `EventChange` row. A failed commit exposes no partial success.

`CommandResult<T>` contains `operationId`, `committedVersion`, and the authorized result. Receipts retain stable IDs, outcome codes, and version, not copies of personal fields or cookie secrets. Replayed results resolve those IDs through current privacy rules; deleted subjects return a tombstone outcome. This preserves operation identity without reintroducing deleted personal data. Participant entry uses the separate protected receipt flow described in its feature design. Authentication and credential replacement use their own serialization and do not require an event version.

Failures use ProblemDetails with `code`, `correlationId`, optional field errors, and authorized current state for conflicts. Codes distinguish `400 validation`, `401 session-required`, `403 forbidden`, `409 stale-version/entry-closed/draw-active`, `429 throttled` with positive `Retry-After`, and `503 storage-unavailable`. Drafts survive ordinary rejection and require explicit reapply with a new operation ID/version. Session invalidation and Countdown profile closure clear private drafts. No mutation is silently queued or replayed after disconnect.

### Projections and realtime delivery

`PublicEventSnapshot` contains version, server time, configured target/copy, current screen, projects, public team labels/members, eligible count, and privacy-filtered draw history. It excludes email, introduction answers, internal sessions, and unassigned private roster records. `ParticipantSnapshot` exposes only the authenticated owner's public label and saved optional fields. `AdministratorSnapshot` adds the full roster and permitted editing state. Public member labels pair an optional name with a stable participant label; identifiers alone grant no access.

`EventChange` contains version, change kind, affected stable IDs, and commit time, never serialized personal payloads. Every API instance runs `EventChangePublisher`, reading committed changes at a proposed 250 ms interval with an independent cursor and pushing to its own connected clients using SignalR. Instances do not compete for or globally mark records delivered. This avoids a new broker at this scale and does not substitute browser polling for SignalR. Session watchers use the same interval for credential revision, revocation, and expiry. Durable records recover a crash after commit but before notification.

Notifications carry only version and affected projection kinds. Clients coalesce notifications and load authorized snapshots; every push remains free of private roster content. Snapshot capture is transactionally coherent. A client buffers observed versions while loading, rejects older snapshots, and reloads if its highest observed version is newer. Reconnect and visibility restoration reauthorize both private channels before loading private data. HTTP snapshot reads do not renew administrator activity. A reconnect never replays a finished animation.

Connected session invalidation sends a content-free `SessionInvalidated`, clears private client state, and closes the private connection. Every privileged HTTP request still checks the database revision and expiry; a notification is not an authorization boundary. Private dispatch repeats authorization immediately before sending, and transport-origin checks reject cross-site connections. When storage cannot establish authority, private controls/data are cleared or withheld and synchronization is marked unavailable.

### Data invariants

`EventState` contains `Id=1`, `Version`, `CurrentScreen`, `TeamsFormed`, and monotonic next participant/team label counters. A unique normalized-email index enforces one current identity per trimmed, case-insensitive full email. No dot/plus rewriting occurs. SQL Unicode columns allow up to twice the scalar limit in UTF-16 code units; Application validators count Unicode scalars before persistence. Required/optional blank semantics follow L2-040.

`Participant.TeamId` is nullable and references one team, preventing duplicate membership. `Team.ProjectId` is nullable; multiple teams may reference one project. Empty teams persist. Draws hold immutable IDs/times and candidate identities plus mutable public-label snapshots so renames and deletions can redact retained presentation. A unique non-null `RaffleDraw.WinnerParticipantId` index prevents repeat wins; deletion nulls it and redacts all matching snapshot labels without deleting the draw.

### Existing implementation and mock transition

| Existing artifact | Target treatment |
|---|---|
| `EventDbContext` and Identity-backed administrator accounts | Reference existing transaction patterns; introduce a fresh companion schema and revision-based shared credential |
| `AuthenticationBudget` | Reuse database-backed locking approach; replace legacy thresholds with five/source and twenty/deployment failed checks per five minutes |
| `OperationReceipt`, `ApiExceptionHandler`, readiness handlers | Adapt to new scope and privacy-preserving receipts; retain structured error/readiness responsibilities |
| API `Program` and separate admin static build | Serve one app; add three scoped SignalR endpoints and remove legacy routes from the companion deployment |
| Provisioning tool, `SecretInput`, `OperatorTargetProfileStore` | Retain useful protected-input/target patterns; replace legacy command scope and exit codes |
| Mock `IEventService`, `MockEventService`, and `apply-command` | Split production services by capability; replace browser authority with HTTP, SignalR, and durable transactions |
| Mock Countdown, Projects, Teams, and Raffle page components | Preserve composition and user flow; bind production service contracts through tokens |
| Mock `@mock/components` and proposal tokens | Implement upstream in Cornerstone, publish, then adopt npm release; no production local substitute |
| Relative mock clock, browser storage, reset/offline controls, `0042` | Rehearsal conveniences only; use configured UTC target, server sessions, and explicitly provisioned production credential |

The run sheet supplies “RTR — Reconciliation Through Relationships” and its relationship, shared-learning, and facilitator-reviewed matching context. Initial provisioning stores that one brief project description with absent repository/demo links. Seed logic runs only during fresh initialization, never on every API start. No attendees or guest projects are inferred. Configuration supplies September 9, 2026, 17:00–21:00 America/Toronto, Stone Church — Davenport Community Campus, 45 Davenport Rd, Toronto; the countdown target is `2026-09-09T21:20:00Z`.

## Delivery and review

This documentation work changes no application behavior and adds no tests. Future implementation uses ATDD: real API/SQL/SignalR integration, installed CLI and direct SQL verification, and Playwright with injected mock contracts and one page object per screen. Diagram rendering and document review validate these artifacts, not the proposed runtime.

Each feature includes C4, typed structure, and behavior diagrams, with rendered PNGs adjacent to PlantUML sources. Exact requirement quotations retain the specs' original wording. New design prose uses the software-design-document house style.
