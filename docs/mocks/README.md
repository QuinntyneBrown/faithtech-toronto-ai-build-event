# FaithTech Toronto event companion — Angular mock

A complete, interactive **design and planning artifact** for the four-screen event companion. This workspace is independent of the production frontend and backend. It uses Angular 21 and the published `@quinntyne/cornerstone@0.2.0` package.

## Run

Use Node.js 22.12+ and npm. From this directory:

```powershell
npm install
npm start
```

Open **http://localhost:4300** in Chrome. The mock administrator passcode is **0042**.

```powershell
npm run build
```

The build command compiles the application to `dist/app/browser`. A static server needs an index fallback for Angular routes. The development server provides this automatically. This design artifact has no automated test suite; verification consists of compilation and manual browser checks.

## Workspace

```text
docs/mocks/
  angular.json
  package.json
  projects/
    app/           # Four routed screens, event state, mock adapter, host tools
```

The app consumes published Cornerstone components directly: `cs-countdown`, `cs-team-board`, `cs-raffle-stage`, and `cs-review-dialog`. Its mock service is consumed through `IEventService` and `EVENT_SERVICE`; state lives in Angular signals.

## Rehearse the evening

1. **Countdown:** enter a new synthetic email (for example, `visitor@example.com`). Raffle entry is immediate. Add or skip your name, what you make, and what's on your heart. Duplicate emails do not create extra entries. Optional details stay out of public team and raffle UI.
2. **Host view:** choose Admin login and enter `0042`. On Countdown, search the roster, add a late arrival, inspect/edit optional details, or delete someone with confirmation. Seeded names and emails are fictional.
3. **Synchronized participant view:** expand Mock controls at the bottom and choose Open participant tab. This opens the same origin without copying the host session. Keep it beside the host tab to rehearse screen advances and shared edits. Use this link rather than the browser's Duplicate Tab command, which can copy session storage.
4. **Projects:** close Countdown using the host action. Public email entry closes. Read the RTR card, add/edit/remove projects, and optionally supply real HTTPS repository/demo links. Unknown URLs and guest-project details are deliberately not fabricated.
5. **Team selection:** advance from Projects. The current roster is shuffled once into teams of three, with a final one- or two-person remainder. Reopening the screen does not reshuffle. Drag a member using the dotted handle, or use their Move to menu. Move to an existing team, Unassigned, or New team. Assign or clear a project beneath each team; several teams can share one project.
6. **Raffle:** advance to Raffle and press DRAW NAME. Every connected tab follows the same five-second cycling timeline and displays the same saved winner. The following five seconds celebrate with falling particles. Previous winners cannot win again. The draw button becomes available again when the celebration finishes.

Hosts can revisit any already-opened screen using the progress navigation without moving public viewers backward. Return to live rejoins the current screen. Deleting an assigned project clears its assignments without removing team members. New participants added after team formation remain Unassigned until moved. Deleting a winner preserves the draw as “Removed participant”.

## Mock controls and persistence

- **Reset populated mock** restores 13 fictional entrants, the RTR card, and Countdown. **Reset empty mock** starts with no entrants and the RTR card. Both require confirmation and reset every connected tab's shared data and private participant entry.
- **Restart 20-minute clock** changes the shared countdown target without advancing the event. The initial relative clock makes the design useful beyond event day; the event copy still describes September 9, 2026. Zero never advances the screen automatically.
- **Disconnect this tab** pauses that tab's updates and mutations. **Reconnect** restores the latest state. This simulates connection loss; it does not control the machine's network.
- **Fail next save** rejects the next mutation in that tab, retaining the form and leaving saved state unchanged. Toggle it again to cancel.
- The host-only raffle preview control forces the lightweight particle renderer in that tab. Reduced-motion preferences remove cycling/particles. Stop effects ends nonessential movement without changing the winner.

BroadcastChannel notifies same-origin tabs; localStorage holds the shared synthetic event; Web Locks serialize mutations and reject concurrent stale changes. SessionStorage holds each tab's illustrative role and participant identity. Refresh restores saved data. An already-revealed draw is displayed without replaying its animation when a viewer joins or returns to Raffle. Tabs share state only within the same browser profile and origin, not between machines, private-browser contexts, or ports.

Use localhost or HTTPS in Chrome with storage enabled. If stored fixture data becomes incompatible, reset it through Mock controls. The UI explains unavailable storage and synchronization. Do not enter real personal information: browser persistence and the public demo passcode are presentation conveniences, not production authorization or privacy controls.

## Published Cornerstone composition

Buttons, inputs, fields, textareas, selects, cards, badges, alerts, countdown, team board, raffle stage, and review dialog use the published Cornerstone package. The app supplies event state and commands: it owns the relative clock, saved raffle selection and synchronized reveal time, and mock persistence; Cornerstone owns presentation, control state, focus containment, drag-and-drop, and effects.

## Deliberate boundaries

There is no API, real SignalR server, database, CLI, production session expiration, authentication rate limiter, or upstream npm publication. Browser state simulates realtime behavior for design review; it does not establish backend correctness or satisfy the operational/security requirements in `docs/specs`. Mock controls are rehearsal utilities inside the four-screen app, not additional product screens. The run sheet and PowerPoint inform copy and flow, with no slide-player integration.
