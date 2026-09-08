# Requirements review — September 7, 2026

## Authority and method

This review corrects requirements, not the mock implementation. Sources are the user-approved plan and decisions, [product prompt](../prompt.md), [L1](L1.md), [L2](L2.md), presentation source, and the complete [mock inventory](../mocks/inventory.js), including its dialogs and alternate states. Mock implementation and coverage claims are evidence to inspect, not product authority.

The user explicitly confirmed Cornerstone as the visual reference, no reopening of closed quizzes/selection through schedule edits, and links retained per team/project pairing. Existing approved defaults remain. Additional resolutions below are specification defaults selected to make existing scope implementable, unless marked **user decision**. They are not claims of prior user confirmation.

Each pass reviews the full requirement set through a different lens, then records concrete findings and corrections. Verification is editorial; no test parses the specifications or asserts architecture. Product acceptance remains outstanding until its prescribed implementation evidence exists.

## Pass 1 — Coverage and missing requirements

| Finding and evidence | Requirements | Resolution |
| --- | --- | --- |
| Access and event-list mocks assume an event context, but its selection and navigation rules were implicit. | L2-046 (L1-002) | Specify event links, protected deep links, current-event return, and navigation without unlocking timed actions. |
| New-event mock supports reuse conceptually; only reference-event copying was specified. | L2-047 (L1-001), L2-008 | Define blank creation and configuration-only copying, draft status, date shifts, isolated identities, and opt-out default. |
| Directory/search/person mocks exist without directory search or private-profile fallback criteria. | L2-048 (L1-006), L2-013 | Expose active roster names for discovery; search optional fields only when shared, without inventing a biography. |
| Required mock fields obscure the specification's ability to save incomplete drafts. | L2-001 | Separate absent publication fields from malformed supplied values and expose field-specific publication errors. |

Review: these are missing or underspecified requirements. Existing team-capacity, quiz-feedback, and fixture-authentication differences are mock deviations and do not justify weakening established requirements.

## Pass 2 — Precision and underspecified behavior

| Finding and evidence | Requirements | Resolution |
| --- | --- | --- |
| A phase, timed screen, and selection stage were used interchangeably; multiple Discern screens could accidentally end selection. | L2-006-L2-008 | Separate screen identities from action windows; specify inclusive/exclusive boundaries, disabled activities, and dated overnight intervals. |
| The DST criterion implied any offset could resolve a nonexistent local time. | L2-006 | Require a valid local time; only ambiguous times can be resolved by a matching offset. |
| Main-screen following and preservation of chat/profile browsing appeared contradictory. | L2-007 | Replace only the main event screen automatically; announce the new activity while preserving other views and drafts. |
| Seven sample stages omit much of the presentation and conflate preparation with demos. | L2-008 | Transcribe 21 explicit timed entries, combine duplicate timestamps, retain 20:30 demos, and leave invented quiz/venue data out of the preset. |
| Quiz mocks imply one anonymous quiz; activation, question navigation, stage reference, and multi-quiz scores were undefined. | L2-017-L2-019 | Define draft/activation/window behavior, event-local quiz identity, question navigation, submitted-only feedback, separate quiz leaderboards, and unanswered results. |
| Demo editor/reorder lacks a specified relationship between order, start, and duration. | L2-023 | Reorder with a preview and consecutive recalculation, preserve durations, constrain to the presentation window, and explicitly handle gaps. |

Review: checked the complete set for actor, precondition, timing, and observable outcome. The table records requirement changes; well-specified existing behaviors remain intact. Schedule timestamps were checked against `new_slide` and `agent_run` entries and arrival/closing content in the presentation source, without executing the deck generator.
