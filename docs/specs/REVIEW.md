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

## Pass 3 — Incorrect and contradictory requirements

| Finding and evidence | Requirements | Resolution |
| --- | --- | --- |
| Prompt/L1/L2 mandate Liturgy while mock/design-system documentation records Cornerstone. | Prompt design section, L1-011, L2-029-L2-033 | **User decision:** Cornerstone light is authoritative; retain independent local implementation and defer npm adoption. |
| L2 requires `--ft-*` while the approved design system owns `--cs-*`; inherited phase colours can override it. | L2-030-L2-031 | Use local `--cs-*` values and remove the superseded Liturgy phase palette mandate. |
| Exact parity could imply an inverted host shell or downloading system fonts. | L2-029, L2-031-L2-032 | Preserve the explicitly documented light navigation adaptation and local/system font delivery. |
| The named Cornerstone revision does not identify untracked card/choice-card sources in that checkout. | L2-031 | Require source snapshots/hashes for uncommitted reference material; representative fixture parity is not whole-library acceptance. |
| Blanket provenance language could imply every new detailed default was user-confirmed. | L1 basis/defaults, L2 provenance | Separate explicit decisions, existing defaults, new editorial defaults, and implementation evidence. |

Review: compared prompt, requirements, local tokens, design-system README, and reference checkout metadata. Cornerstone HEAD is `554636edf74af788e83aa33a4380a05b031db16a`; inspected tracked styles have no reported changes, while card/choice-card directories are untracked. Existing local reference specimens remain the reproducible artifact baseline; no external repository was modified. All remaining Liturgy requirements govern optional companion links and independence, not the visual source.

The local `design-system/tests/reference/cornerstone-light.css` specimen inspected for provenance has SHA-256 `3678174BA58E048318B4AD7CCB574FDF78E5CFA5B6708FE18A850667032E236A`. This identifies the existing specimen, not proof of exhaustive component parity.

## Pass 4 — Lifecycle, ownership, and edge cases

| Finding and evidence | Requirements | Resolution |
| --- | --- | --- |
| Editing times could reopen completed selection/quizzes or evade closure after downtime. | L2-006, L2-017, L2-044 | **User decision:** closed activities stay closed. Determine closure from the previous committed schedule; retain closure across restart. Completed-event edits also cannot reopen participant mutations. |
| Team/project switching had no rule for existing build links; shared project fields in mocks hide the ownership issue. | L2-009, L2-024-L2-025 | **User decision:** retain each pairing's links; show the selected pairing only, with one showcase entry per team. Membership changes never transfer team data. |
| Team/project corrections could leave demo references inconsistent. | L2-009, L2-012, L2-023 | Update a slot's project atomically with selection; require explicit slot removal before clearing its selection; reject team removal while dependencies remain. |
| Reactivation can collide with an email rebound while inactive; removing a roster entry could erase history. | L2-002-L2-004 | Retain history, reject conflicting reactivation, permit explicit binding recovery, and never reactivate revoked sessions. |
| Changing assignment mode, confirming random assignment, and adding unassigned participants were insufficiently distinguished. | L2-009-L2-010 | Mode changes preserve memberships; full reassignment and unassigned-only assignment are separate, version-checked administrator actions. |
| Turning proposals off could invalidate accepted choices; proposer ownership was unclear. | L2-011 | Accepted proposals join the managed catalog and persist; disable creation only. Proposing does not automatically select or require a team. |
| Missing moderation mock conceals report lifecycle and limited administrator visibility. | L2-016 | Define pending/resolved states, report identity, optional reason/notes, received-history reporting, and reported-message-only review. |
| Draft retention conflicted with sign-out cleanup; retry semantics did not distinguish changed input from replay. | L2-004, L2-044 | Invalidation clears private drafts, not durable history. Preserve operation identity on retry, return committed outcomes, reject changed reuse, and require explicit reapplication of stale edits. |

Review: followed each retained entity through create, edit, selection change, removal/deactivation, expiry, closing, interruption, retry, and return. Existing raffle persistence/fairness and same-event privacy requirements remain authoritative even where mock behavior differs.
