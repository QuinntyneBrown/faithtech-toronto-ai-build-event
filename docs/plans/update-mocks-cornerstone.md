# Update mocks to Cornerstone 0.2.0

## Summary

Upgrade the independent Angular workspace in `docs/mocks` from Cornerstone `0.1.3` to `0.2.0`, the latest npm release verified during planning. Replace the four local component implementations with published Cornerstone components while preserving the existing mock flows.

## Implementation

- Pin `@quinntyne/cornerstone` to `0.2.0` and update the lockfile. Existing Angular 21 dependencies satisfy its peer requirements.
- Import `CountdownComponent`, `RaffleStageComponent`, `TeamBoardComponent`, and `ReviewDialogComponent` directly from Cornerstone. Replace their `mock-*` selectors with the corresponding `cs-*` selectors.
- Preserve countdown target/current-time bindings and host-controlled advancement.
- Preserve saved raffle results, synchronized reveal timing, fallback preview, reduced-motion behavior, and stop-effects controls. Winner selection stays in the mock service.
- Preserve team member moves and project assignments using Cornerstone's exported event types. Wire its new `newTeamRequested` output to the existing move command with `teamId: "new"`.
- Replace every review dialog in the application shell, participant manager, and project page. Preserve projected forms, validation, confirmation actions, and dismissal state.
- Remove the replaced local components, duplicate presentation types, and local raffle renderer/shader. Remove the native-control workaround now superseded by Cornerstone's control-state handling.
- Remove obsolete exports and dependencies made unused by these replacements. Retain unrelated workspace composition and update the README to describe published component usage and actual verification.

## Acceptance and verification

Use a dedicated mock Playwright configuration under `e2e`, serving port 4300, with one page object per screen and selectors confined to page objects.

Cover these Given–When–Then scenarios:

- Given an active countdown, when time reaches zero, then it displays zero without advancing the event.
- Given an administrator, when moving members by drag or menu, creating a team, or assigning/clearing a project, then the saved board updates correctly.
- Given a participant or disconnected tab, when editing is attempted, then editing controls cannot mutate state.
- Given an open dialog, when dismissed by its close control or Escape, then it closes and restores focus; when a save fails, then the form retains its data.
- Given a raffle draw, when its reveal time arrives, then connected tabs reveal the same winner; fallback, reduced motion, stop effects, and returning after reveal preserve the result without unwanted replay.

Build the mock workspace and check all four screens at desktop and mobile widths.

## Delivery constraints

- Install and read the mandatory incremental-implementation skill before implementation; it was not found locally.
- Work in small, acceptance-tested increments, verifying before approximately 100 new lines and committing each working increment.
- Preserve existing unrelated working-tree changes.
- Keep production frontend/backend behavior and persisted mock data formats unchanged.
