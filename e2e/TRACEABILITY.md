# Acceptance criteria traceability

Every acceptance criterion in [docs/specs/L2.md](../docs/specs/L2.md), and what covers it in this suite.

Statuses:

- **Covered** - a passing browser test asserts it.
- **Partly covered** - some of it is asserted; the rest is skipped or belongs elsewhere.
- **Skipped** - the test exists and is written in full, but the behaviour is not implemented, so it runs as `test.fixme`.
- **Out of scope** - not observable in a browser with the backend mocked. It belongs to the API, CLI, database, load, or review evidence.

This is a reviewed document, not a test. Nothing here parses the specifications, per AGENTS.md.

## Summary

| Status | Criteria |
| --- | --- |
| Covered | 74 |
| Partly covered | 6 |
| Skipped, not implemented | 5 |
| Out of scope for a browser | 49 |
| Not covered | 0 |
| **Total** | **134** |


## L2-002: Manage participants on Countdown

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-002/AC1 | Covered | countdown-roster.spec.ts - adding an email-only participant enters them in the raffle once |  |
| L2-002/AC2 | Covered | countdown-roster.spec.ts - an edit is saved and survives a refresh with the same identity<br>countdown-roster.spec.ts - a duplicate email is rejected without changing anything |  |
| L2-002/AC3 | Partly covered | countdown-roster.spec.ts - deletion is confirmed before it removes the participant _(skipped)_<br>countdown-roster.spec.ts - cancelling deletion changes nothing _(skipped)_<br>countdown-roster.spec.ts - removing a participant asks before doing anything | Removal asks first and changes nothing until answered. Confirming and cancelling are blocked by the confirm-dialog defect. |
| L2-002/AC4 | Covered | countdown-roster.spec.ts - an empty roster says so<br>countdown-roster.spec.ts - a failed add explains itself and keeps the entered address<br>countdown-roster.spec.ts - a failed edit keeps the proposed values and claims no success |  |
| L2-002/AC5 | Covered | countdown-roster.spec.ts - the roster stays usable after the public screen has advanced |  |

## L2-003: Email entry and raffle confirmation

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-003/AC1 | Covered | countdown-entry.spec.ts - an unused valid email is entered into the raffle and offered optional details |  |
| L2-003/AC2 | Covered | countdown-entry.spec.ts - the creating browser keeps one entry across a repeated submission |  |
| L2-003/AC3 | Skipped | countdown-entry.spec.ts - a known email submitted without its session is told to ask an administrator _(skipped)_ |  |
| L2-003/AC4 | Covered | countdown-entry.spec.ts - a malformed email is refused without confirming an entry<br>countdown-entry.spec.ts - a failed save keeps the entered address and stays retryable<br>countdown-entry.spec.ts - throttled entry explains how long to wait |  |
| L2-003/AC5 | Skipped | countdown-entry.spec.ts - entry after Countdown closes is refused as closed _(skipped)_ |  |

## L2-004: Private entry continuity

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-004/AC1 | Covered | countdown-entry.spec.ts - a refreshed browser restores the entry and its saved details |  |
| L2-004/AC2 | Out of scope | - | Server-side authorization of a substituted participant id. No browser surface. |
| L2-004/AC3 | Covered | countdown-entry.spec.ts - leaving the browser clears the private entry but keeps the public screen<br>countdown-entry.spec.ts - an invalidated entry session clears private content |  |

## L2-005: Countdown, welcome, and brief event information

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-005/AC1 | Covered | countdown-clock.spec.ts - an unregistered viewer sees the welcome, the details, and a running countdown<br>countdown-clock.spec.ts - a device clock ten minutes out does not shift the remaining time |  |
| L2-005/AC2 | Covered | countdown-clock.spec.ts - reaching the target shows zero and waits for an administrator |  |
| L2-005/AC3 | Covered | countdown-clock.spec.ts - a target beyond a day is shown as days plus the remaining time |  |
| L2-005/AC4 | Covered | countdown-clock.spec.ts - an unavailable clock shows the scheduled time and says so<br>countdown-clock.spec.ts - the clock recovers once server time is available again |  |

## L2-007: Exactly four screens and manual progression

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-007/AC1 | Covered | screen-flow.spec.ts - closing Countdown moves connected public browsers to Projects<br>screen-flow.spec.ts - public entry is closed once Projects opens | The two-second delivery budget is an API concern; this asserts the client follows the change. |
| L2-007/AC2 | Covered | screen-flow.spec.ts - Projects then Team selection then Raffle advance in order |  |
| L2-007/AC3 | Covered | screen-flow.spec.ts - a stale advance is refused so no screen is skipped |  |
| L2-007/AC4 | Covered | screen-flow.spec.ts - a deep link to a future screen is returned to the saved one<br>screen-flow.spec.ts - a refreshed browser lands on the saved screen without replaying |  |
| L2-007/AC5 | Partly covered | screen-flow.spec.ts - an administrator browses an earlier screen while viewers stay live<br>screen-flow.spec.ts - a locally browsed screen shows a current-screen indicator and a way back to live _(skipped)_ |  |
| L2-007/AC6 | Covered | screen-flow.spec.ts - the current screen does not change on its own |  |

## L2-009: Administrator member rearrangement

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-009/AC1 | Partly covered | teams-rearrange.spec.ts - moving a member changes one membership and reaches other views<br>teams-rearrange.spec.ts - a member can be dragged from one team onto another _(skipped)_ | Drag and drop does not exist. The equivalent control is covered; the drag half is skipped. |
| L2-009/AC2 | Covered | teams-rearrange.spec.ts - an unassigned participant joins an existing team exactly once<br>teams-rearrange.spec.ts - a new team gets its own label and no project |  |
| L2-009/AC3 | Covered | teams-rearrange.spec.ts - a fourth member can be added without any rebalancing |  |
| L2-009/AC4 | Covered | teams-rearrange.spec.ts - a rejected move keeps the confirmed membership and explains the retry |  |
| L2-009/AC5 | Covered | teams-rearrange.spec.ts - a move leaves each team's project where it was<br>teams-rearrange.spec.ts - an ordinary participant is offered no way to move anyone |  |

## L2-010: Form random teams once on first opening

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-010/AC1 | Covered | teams-formation.spec.ts - advancing to Team selection forms and renders the grouping once<br>teams-formation.spec.ts - ${count} entered participants render as ${sizes.join("/")}<br>teams-formation.spec.ts - no entered participants shows an explicit empty state | The screen renders each identity exactly once. The grouping rule itself is proved in the API tests. |
| L2-010/AC2 | Out of scope | - | Stated as an API integration fixture with a controlled shuffle. |
| L2-010/AC3 | Out of scope | - | Concurrent first-opening and lost-response handling inside the server. |
| L2-010/AC4 | Covered | teams-formation.spec.ts - a failed formation leaves Projects current with no partial teams |  |
| L2-010/AC5 | Covered | teams-formation.spec.ts - manual corrections survive another viewer opening the screen |  |

## L2-011: RTR project cards and team project assignment

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-011/AC1 | Covered | projects.spec.ts - the RTR card is readable without an invented repository or demo |  |
| L2-011/AC2 | Covered | projects.spec.ts - an empty catalogue says so and offers the administrator an add action<br>projects.spec.ts - a supplied link opens its target without control of this page |  |
| L2-011/AC3 | Covered | teams-rearrange.spec.ts - assigning a project reaches viewers without disturbing anything else | Client rendering only; delivery timing belongs to the API tests. |
| L2-011/AC4 | Covered | teams-rearrange.spec.ts - two teams share a project and clearing one leaves the other |  |

## L2-012: Manage projects on Projects

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-012/AC1 | Covered | projects.spec.ts - an added project reaches other viewers and survives a refresh<br>projects.spec.ts - an edited project updates its card | Client rendering only; delivery timing belongs to the API tests. |
| L2-012/AC2 | Covered | projects.spec.ts - a blank required field is refused and the draft is kept<br>projects.spec.ts - an invalid link is refused and the catalogue is unchanged<br>projects.spec.ts - a failed save keeps the proposed values |  |
| L2-012/AC3 | Partly covered | projects.spec.ts - removing a project asks before doing anything<br>projects.spec.ts - a confirmed removal clears the card and every assignment _(skipped)_ | Removal asks first. Confirming is blocked by the confirm-dialog defect. |
| L2-012/AC4 | Out of scope | - | Persistence across an API restart, and server-side denial of participant writes. |

## L2-013: Optional introduction information

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-013/AC1 | Covered | optional-profile.spec.ts - leaving every optional field blank keeps the raffle entry<br>optional-profile.spec.ts - no optional detail is needed to follow the event onwards |  |
| L2-013/AC2 | Covered | optional-profile.spec.ts - a saved subset comes back after a refresh<br>optional-profile.spec.ts - clearing the name removes it and leaves the public label |  |
| L2-013/AC3 | Covered | optional-profile.spec.ts - a failed optional save keeps the draft and confirms the entry stands |  |
| L2-013/AC4 | Covered | optional-profile.spec.ts - answers stay private to the participant and the administrator |  |
| L2-013/AC5 | Skipped | optional-profile.spec.ts - an unsaved draft is discarded with notice when Countdown closes _(skipped)_ |  |

## L2-020: Raffle pool and public results

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-020/AC1 | Covered | raffle-draw.spec.ts - only eligible participants are counted<br>raffle-draw.spec.ts - a single remaining entrant is counted in the singular | The rendered eligible count; the eligibility rule is server-side. |
| L2-020/AC2 | Partly covered | raffle-draw.spec.ts - results use public labels and never an email address<br>raffle-draw.spec.ts - participants sharing a name are still told apart _(skipped)_ |  |
| L2-020/AC3 | Partly covered | raffle-draw.spec.ts - with nobody eligible the draw is disabled<br>raffle-draw.spec.ts - a viewer is never offered the draw control<br>raffle-draw.spec.ts - an empty pool says there are no eligible participants _(skipped)_ |  |
| L2-020/AC4 | Out of scope | - | Continued exclusion after a rename is computed server-side. That a public viewer never sees the roster is asserted in L2-020/AC2 and L2-035/AC3. |

## L2-021: Fair, durable, single-result draw

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-021/AC1 | Covered | raffle-draw.spec.ts - the drawn winner is named once and then excluded | Exclusion after winning, as rendered. The controlled-randomness half is an API fixture. |
| L2-021/AC2 | Out of scope | - | Concurrent draws against one raffle version, resolved in the server. |
| L2-021/AC3 | Out of scope | - | Idempotency by operation identity across a lost response. |
| L2-021/AC4 | Covered | raffle-draw.spec.ts - no second draw can start while one is running |  |
| L2-021/AC5 | Out of scope | - | Serialization of a deletion racing a draw. The resulting "Removed participant" label is produced server-side. |
| L2-021/AC6 | Covered | raffle-draw.spec.ts - a draw that cannot be committed announces nobody and stays retryable<br>raffle-draw.spec.ts - a lost draw response invites a safe retry |  |

## L2-022: Animated name cycling and WebGPU celebration

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-022/AC1 | Covered | raffle-presentation.spec.ts - names cycle and then settle on the committed winner<br>raffle-presentation.spec.ts - the celebration stops after the saved effects window |  |
| L2-022/AC2 | Covered | raffle-presentation.spec.ts - a browser joining mid-draw picks up the remaining timeline<br>raffle-presentation.spec.ts - a browser arriving after the reveal shows the winner without replaying |  |
| L2-022/AC3 | Covered | raffle-presentation.spec.ts - the winner is revealed even without a working GPU celebration |  |
| L2-022/AC4 | Covered | raffle-presentation.spec.ts - cycling and particles are omitted and the winner still appears |  |
| L2-022/AC5 | Covered | raffle-presentation.spec.ts - the winner is exposed as announced text, not only animation |  |

## L2-029: Consume the published Cornerstone UI

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-029/AC1 | Out of scope | - | The clean-checkout build is a CI concern. Exercising the four screens against the adopted release is what this whole suite does. |
| L2-029/AC2 | Out of scope | - | Review evidence: package version and component provenance. |

## L2-030: Package-owned tokens and styling

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-030/AC1 | Out of scope | - | Review evidence: visual conformance to the installed release. |
| L2-030/AC2 | Out of scope | - | Review evidence: token ownership stays upstream. |

## L2-032: Consistent light theme

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-032/AC1 | Covered | theme.spec.ts - every screen uses the light theme with a ${colorScheme} preference<br>theme.spec.ts - the administrator overlays keep the light theme |  |
| L2-032/AC2 | Covered | theme.spec.ts - a preference that changes while viewing does not switch the theme<br>theme.spec.ts - a stored dark preference does not survive into the theme<br>theme.spec.ts - no theme selector is offered anywhere |  |

## L2-033: Complete missing UI in Cornerstone first

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-033/AC1 | Out of scope | - | Requires the upstream Cornerstone example alongside the app. |
| L2-033/AC2 | Out of scope | - | Clean-checkout install from the updated lockfile. A CI concern. |
| L2-033/AC3 | Out of scope | - | Review evidence: upstream version and provenance. |

## L2-034: Supported browser and capabilities

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-034/AC1 | Covered | accessibility.spec.ts - a complete journey raises no blocking browser error |  |
| L2-034/AC2 | Covered | diagnostics.spec.ts - a restored tab resynchronizes to the saved screen<br>diagnostics.spec.ts - a restored tab shows a completed draw without replaying it |  |

## L2-035: Responsive four-screen layouts

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-035/AC1 | Covered | responsive.spec.ts - the ${screen} screen never scrolls sideways at any supported width<br>responsive.spec.ts - the populated administrator roster never scrolls the page sideways<br>responsive.spec.ts - empty and error states hold up at every width |  |
| L2-035/AC2 | Covered | responsive.spec.ts - the primary actions stay reachable on the smallest viewport |  |
| L2-035/AC3 | Covered | responsive.spec.ts - a revealed winner is readable on the presentation display |  |

## L2-036: Keyboard, semantics, and feedback

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-036/AC1 | Covered | accessibility.spec.ts - a participant can enter using only the keyboard<br>accessibility.spec.ts - optional details can be saved using only the keyboard<br>accessibility.spec.ts - an administrator can sign in and add a participant from the keyboard |  |
| L2-036/AC2 | Covered | accessibility.spec.ts - a rejected submission announces itself and keeps what was typed | Retained values and announcements are covered. Focus returning from a dismissed dialog is blocked by the confirm-dialog defect. |
| L2-036/AC3 | Covered | accessibility.spec.ts - the ticking raffle label is hidden from assistive technology<br>accessibility.spec.ts - the revealed winner is announced in one live region |  |
| L2-036/AC4 | Covered | accessibility.spec.ts - the ${screen} screen meets the contrast thresholds<br>accessibility.spec.ts - the administrator surfaces on Countdown meet the same thresholds |  |

## L2-037: Reduced motion and resilient presentation

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-037/AC1 | Covered | raffle-presentation.spec.ts - a revealed winner stays readable with motion suppressed |  |
| L2-037/AC2 | Covered | raffle-presentation.spec.ts - stopping the effects keeps the saved result on screen<br>raffle-presentation.spec.ts - one viewer stopping effects does not change another view |  |
| L2-037/AC3 | Covered | raffle-presentation.spec.ts - a draw in progress is announced without relying on the cycling name |  |

## L2-038: Shared four-digit administrator login

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-038/AC1 | Covered | admin-login.spec.ts - the provisioned passcode enables administrator controls<br>admin-login.spec.ts - "${rejected}" does not authenticate<br>admin-login.spec.ts - more than four digits cannot be entered at all |  |
| L2-038/AC2 | Covered | admin-login.spec.ts - a browser without a session sees and reaches no privileged surface | Server denial of privileged requests and SignalR subscriptions. |
| L2-038/AC3 | Covered | admin-login.spec.ts - signing in grants privileges to that browser only |  |
| L2-038/AC4 | Covered | admin-login.spec.ts - signing out clears the privileged view<br>admin-login.spec.ts - an invalidated administrator session clears the privileged view |  |
| L2-038/AC5 | Covered | admin-login.spec.ts - with no passcode provisioned, no guess grants access |  |

## L2-039: Public and private data boundaries

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-039/AC1 | Out of scope | - | Inspection of endpoint and hub payloads. The rendered half is asserted in L2-013/AC4 and L2-038/AC3. |
| L2-039/AC2 | Out of scope | - | Server-side authorization of participant ids and hub groups. |
| L2-039/AC3 | Covered | realtime.spec.ts - an invalidated entry session clears the private view at once<br>realtime.spec.ts - a reconnecting private view must reauthorize before showing content | Client clearing only; the two-second budget belongs to the API tests. |
| L2-039/AC4 | Skipped | countdown-entry.spec.ts - the entry form explains enrolment, label use, and privacy before submission _(skipped)_ |  |

## L2-040: Validate text, emails, and links

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-040/AC1 | Covered | validation.spec.ts - a title at the 200-character limit saves<br>validation.spec.ts - a title one character over the limit is refused with the draft kept<br>validation.spec.ts - a description one character over the limit is refused<br>validation.spec.ts - whitespace-only required text is refused<br>validation.spec.ts - an address longer than the limit cannot be entered |  |
| L2-040/AC2 | Covered | validation.spec.ts - plus tags and subdomains are separate accepted addresses<br>validation.spec.ts - casing and surrounding whitespace resolve to one address<br>validation.spec.ts - "${malformed}" is refused |  |
| L2-040/AC3 | Covered | validation.spec.ts - markup and script text is rendered literally and never executed |  |
| L2-040/AC4 | Covered | validation.spec.ts - the link "${rejected}" is refused<br>validation.spec.ts - clearing an optional link removes it |  |

## L2-041: Protect credentials and browser sessions

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-041/AC1 | Out of scope | - | Deployment configuration: HTTPS, cookie attributes, and logs. |
| L2-041/AC2 | Out of scope | - | Server-side origin and antiforgery checks. |
| L2-041/AC3 | Out of scope | - | Database inspection of the stored verifier. |
| L2-041/AC4 | Covered | admin-login.spec.ts - history navigation after signing out exposes no cached roster |  |

## L2-042: Bounded abuse and retry feedback

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-042/AC1 | Out of scope | - | Server-side throttling of administrator checks. |
| L2-042/AC2 | Out of scope | - | Deployment-wide throttling across instances and restarts. |
| L2-042/AC3 | Out of scope | - | Server-side throttling of public submissions. |
| L2-042/AC4 | Covered | admin-login.spec.ts - a refused passcode is explained without disclosing the passcode<br>admin-login.spec.ts - throttled sign-in explains the retry delay<br>admin-login.spec.ts - an ended session asks for the passcode again |  |

## L2-043: Measurable live-event performance

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-043/AC1 | Out of scope | - | Load and performance measurement. |
| L2-043/AC2 | Out of scope | - | Load and performance measurement. |
| L2-043/AC3 | Out of scope | - | Load and performance measurement. |
| L2-043/AC4 | Out of scope | - | Load reporting. |

## L2-044: SignalR synchronization, persistence, and conflicts

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-044/AC1 | Out of scope | - | Real SignalR delivery. L2.md states plainly that mock tests do not prove it. |
| L2-044/AC2 | Covered | realtime.spec.ts - a repeated notification does not disturb the rendered state<br>realtime.spec.ts - an older notification never rolls the screen back |  |
| L2-044/AC3 | Covered | realtime.spec.ts - reconnecting replaces state that changed while disconnected<br>realtime.spec.ts - reconnecting after a completed draw does not replay it |  |
| L2-044/AC4 | Covered | realtime.spec.ts - a conflicting save is refused and the draft is kept for reapplication |  |
| L2-044/AC5 | Covered | realtime.spec.ts - an entry whose response is lost is not duplicated by a retry<br>realtime.spec.ts - a lost roster addition is not duplicated by a retry |  |
| L2-044/AC6 | Out of scope | - | Storage failure mid-operation and state after a restart. |

## L2-045: Minimal operational diagnostics and recovery

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-045/AC1 | Skipped | diagnostics.spec.ts - a failed save gives the person a traceable error reference _(skipped)_ |  |
| L2-045/AC2 | Out of scope | - | Readiness endpoint behaviour. |
| L2-045/AC3 | Out of scope | - | Backup and restore against an isolated database. |
| L2-045/AC4 | Out of scope | - | Deployment runbook verification. |

## L2-049: Install and update the passcode CLI

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-049/AC1 | Out of scope | - | Command-line tool. |
| L2-049/AC2 | Out of scope | - | Command-line tool. |
| L2-049/AC3 | Out of scope | - | Command-line tool. |

## L2-050: Explicit database target

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-050/AC1 | Out of scope | - | Command-line tool. |
| L2-050/AC2 | Out of scope | - | Command-line tool. |
| L2-050/AC3 | Out of scope | - | Command-line tool. |

## L2-051: Database operator access and protected input

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-051/AC1 | Out of scope | - | Database principal permissions. |
| L2-051/AC2 | Out of scope | - | Command-line tool input handling. |
| L2-051/AC3 | Out of scope | - | Command-line tool connection configuration. |

## L2-063: Replace the passcode directly through the database

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-063/AC1 | Out of scope | - | Direct database replacement of the passcode. |
| L2-063/AC2 | Out of scope | - | Tying session revocation to a passcode replacement is a database concern. That an administrator view clears the moment its session is invalidated is asserted in L2-038/AC4. |
| L2-063/AC3 | Out of scope | - | Direct database replacement of the passcode. |
| L2-063/AC4 | Out of scope | - | Concurrent database replacements. |

## L2-064: Replace the passcode through the installed CLI

| Criterion | Status | Covered by | Notes |
| --- | --- | --- | --- |
| L2-064/AC1 | Out of scope | - | Command-line tool. |
| L2-064/AC2 | Out of scope | - | Command-line tool. |
| L2-064/AC3 | Out of scope | - | Command-line tool. |
| L2-064/AC4 | Out of scope | - | Command-line tool. |
