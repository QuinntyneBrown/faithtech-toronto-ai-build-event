# Acceptance tests

Playwright tests for the client application, using the Page Object Model. The
backend is mocked completely: these run with no API process, no SQL Server, and
no SignalR server.

```bash
npm ci --prefix ../frontend      # the workspace this suite serves
npm ci
npx playwright install --with-deps chrome
npm test
```

The config starts `ng serve client` on port 4210 itself, so nothing else needs
to be running.

## Layout

| Path | What lives there |
| --- | --- |
| `specs/` | One file per area. Tests state intent; they hold no selectors. |
| `page-objects/` | One object per screen, plus the inline panels. These own the DOM. |
| `mocks/` | The in-memory backend: the event store, the REST interceptor, the hubs. |
| `fixtures/` | Wiring that installs the mocks before anything navigates. |
| `TRACEABILITY.md` | Every acceptance criterion in `docs/specs/L2.md` and what covers it. |

Each spec file carries the header `docs/specs/L1.md` requires: `// Acceptance
Test`, `// Traces to:`, and `// Description:`. Test titles name the criterion
they prove, as `L2-003/AC1: ...`.

## How the backend is mocked, and why this way

`docs/specs/L1.md` and `AGENTS.md` both describe mocking by injecting mock
service contracts under an Angular `acceptance` configuration. That
configuration was removed in `952a90c`, and restoring it would mean adding a
bootstrap file and a `fileReplacements` block inside `frontend/`.

**This suite mocks at the network boundary instead**, so nothing outside `e2e/`
changes. `page.route` answers every `/api/**` call from an in-memory store, and
`page.routeWebSocket` answers the SignalR hubs. The production adapters do run,
against a fake network, which is the one way this departs from the written
approach. It was a deliberate choice; if the constraint is ever lifted, the
provider-swap seam is a single file, `frontend/projects/client/src/main.ts`,
which binds all nine service tokens in one place.

### The hubs matter more than the REST calls

`EventService` sets `connected` only after the `event-updates` hub handshake
completes, and nearly every control in the application is bound
`[disabled]="!event.connected() || ..."`. Without a working hub mock, almost
nothing is testable. Two further hubs, `/api/participant/updates` and
`/api/admin/updates`, invalidate private sessions; they live under `/api`, so
the REST mock defers those paths back to the hub mock.

`HubMock` exposes what tests need: `pushEventUpdate`, `invalidateParticipantSession`,
`invalidateAdministratorSession`, and `goOffline` / `goOnline`. Going offline also
refuses the negotiation the client immediately retries, so a disconnected state
holds still instead of racing SignalR's automatic reconnect.

### Things that will catch you out

- `version` must be a decimal integer string. `BigInt(state.version)` throws otherwise,
  inside a subscriber, and takes the stream with it.
- `currentScreen` decides which route a browser lands on. A browser without an
  administrator session is pushed to the event's current screen, so
  `page.goto('/raffle')` returns to `/countdown` unless the fixture says otherwise.
- The administrator login is rendered only on Countdown. Sign in **before**
  moving the event on; once Projects opens there is no route back to it.
- Team labels and rendered member names must be unique. The templates track by
  them, and duplicates raise a duplicate-key error.
- Push notifications only after the hub is connected. Asserting that a
  connection-gated control is enabled is the readiness signal.

## What is skipped, and why

Criteria whose behaviour is not implemented are written in full and marked
`test.fixme`, so they report as skipped now and turn red the moment the feature
lands. Nothing is silently narrowed. `TRACEABILITY.md` lists each one.

The largest of them is a real defect. `ConfirmDialogComponent` declares its data
as a required signal input, but the application opens it through `DialogService`,
which passes the payload as CDK `DIALOG_DATA`. CDK does not bind component
inputs, so reading `data()` throws `NG0950`; the dialog renders an empty title,
an empty message, and two unlabelled buttons, and its `resolved` output is never
wired to the `closed` observable the application subscribes to. **Participant and
project deletion cannot be confirmed at all.**
