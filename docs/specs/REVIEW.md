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

## Pass 5 — Acceptance criteria and final consistency

| Finding | Requirements | Resolution |
| --- | --- | --- |
| Two-second reconnect deadlines used inconsistent starting points; offline clients cannot instantly know remote revocation. | L2 measurement convention, L2-004, L2-007, L2-034, L2-043-L2-044 | Define connected update versus post-synchronization measurement, explicit stale states, and server enforcement versus client invalidation. |
| Publication allowed an address without defining how the required map marker was configured. | L2-001, L2-005, L2-040 | Require administrator-supplied coordinates for publication, define coordinate bounds and text fallback, and cover multi-day countdowns. No address is inferred from mock or deck. |
| Incomplete drafts, absent waiting text, and no stages lacked a consistent successful path. | L2-001, L2-006 | Preserve draft saving; define neutral fallback content and countdown/waiting/recap boundaries for a stage-free event. |
| Profile save could imply sharing or participant-controlled roster rename; recommendation ties lacked complete evidence. | L2-013-L2-014, L2-048 | Keep roster name read-only, sharing explicit, private fields retained only for the owner, and intersection/ranking deterministic. |
| Message filtering and timestamps lacked concrete multi-conversation acceptance. | L2-015 | Specify both participant identities, dated event-zone timestamps, stable ordering, and an A/B versus C/B privacy scenario. |
| Raffle confirmation/count could be mistaken for reserved eligibility. | L2-020-L2-022 | Select a prize explicitly, recheck eligibility at commit, retain attributable history, and separate sound and motion criteria. |
| Error summaries, removed dialog triggers, and user-initiated dirty navigation were underspecified. | L2-036-L2-037 | Add field-linked error focus, dismissal fallback, keep/discard behavior, and explicit active-audio mute acceptance. |
| Background requests could prevent administrator inactivity expiry indefinitely. | L2-038 | Only accepted requests from deliberate administrator interaction renew inactivity; add exact idle/absolute expiry acceptance. |
| Text lengths, tag counts, script-like text, and URL rejection allowed divergent validators. | L2-040 | Define Unicode scalar counting, normalized distinct tags, plain-text rendering, field limits, and explicit invalid URL/coordinate cases. |
| Local-storage-only wording left other script-readable credential persistence unspecified; retry throttling could count one accepted send twice. | L2-041-L2-042 | Cover session storage/IndexedDB and positive retry delays; committed message retries do not consume new allowance. |
| New report/profile fields lacked explicit diagnostic exclusion. | L2-045 | Extend redaction and split audit attribution from private-value exclusion criteria. |
| Copies could retain foreign references, discard prize configuration, or activate copied quizzes. | L2-017, L2-047 | Remap identities, copy prize definitions without awards, keep quizzes inactive, and reject invalid publication references. |
| Product evidence and static artifact evidence were conflated in mock coverage. | L2 acceptance delivery | Preserve separate browser, API, design, and operational evidence obligations; static checks do not certify production requirements. |

## Complete coverage review

The following mapping was reviewed manually against all 33 mock screens, 37 dialog entries, and their listed alternate states. Missing mock surfaces do not remove product requirements; the mock inventory is unchanged by this task.

Mock-only conveniences such as the review clock, fixture reset, direct sign-in bypass, generic role field, team-full state, and quiz restart are not new production requirements. A dedicated stage-reorder dialog is also not mandated: valid administrator edits of timed entries already provide schedule ordering. These distinctions prevent a coverage review from turning every mock control into extra product scope.

| Requirement group | Surfaces or source examined | Final review outcome |
| --- | --- | --- |
| L2-001-L2-002, L2-047 | Admin event list/settings/overview; participant add/edit/remove; prompt reuse | Draft/publication, reuse, credentials, revocation, and history covered. |
| L2-003-L2-004, L2-046 | Access/rejected/expired; account/sign-out/navigation; direct screen links | Existing email binding/concurrency criteria retained; event context, private cleanup, and return paths clarified. |
| L2-005-L2-008 | Countdown, schedule, stage edit/reorder/remove, welcome/build; presentation | Exact boundaries, coordinate fallback, timed entries, screen identity, and closure reviewed. |
| L2-009-L2-012 | Teams/team, join/leave/assignment, projects/project/proposal and admin editors | Unlimited capacity retained; proposal lifecycle, correction, dependency removal, and guidance covered. |
| L2-013-L2-016, L2-048 | Profile, people/person/connections, message picker/thread/failure/retry | Sharing, directory search, recommendations, pairwise privacy, blocking, reporting, and moderation covered. |
| L2-017-L2-019 | Quiz intro/waiting/question/feedback/results/closed; question editor/admin results | Multiple quizzes, activation, immutable submissions, closing, scoring, and ties covered. |
| L2-020-L2-022 | Raffle/draw/prize editors/winner/history/exhausted; effect fallbacks | Existing uniformity, atomic award, replay, fallback, and reduced-motion requirements retained; confirmation/history precision added. |
| L2-023-L2-025 | Demo editor/reorder/remove, demos/demo, showcase/recap, link editor | Team pairing ownership, slot validity, ordering, missing selections, and post-event access covered. |
| L2-026-L2-028 | September/future review scenarios, host opt-out, project/recap links | Existing opt-out, URL retention, membership separation, and no-API-integration requirements retained. |
| L2-029-L2-033 | Standalone gallery/tokens/parity specimens and design provenance | Superseded visual reference corrected; independence, light-only adaptation, and future adoption boundary retained. |
| L2-034-L2-037 | Loading/empty/error/overlay states, responsive and accessibility checks | Existing ten widths and zoom obligations retained; focus, sound, navigation, and synchronization made explicit. |
| L2-038-L2-042 | Admin login/denial, forms/upload/link controls, sessions and failure states | Role/event/ownership protections retained; expiry, limits, text safety, and credential persistence clarified. |
| L2-043-L2-045 | Save error/offline/reconnect artifacts; production requirements | Real-load, restart, backup/restore, readiness, and audit evidence remain required separately from mocks. |

## Verification and delivery

- Reviewed 14 L1 capabilities and 48 L2 requirements, with existing IDs preserved and new L2-046 through L2-048 each mapped to one existing parent. The L1 matrix covers every group above.
- Reviewed numbered criteria for observable Given/When/Then outcomes, boundary/error behavior, and cross-cutting applicability; split conflicting or independent outcomes where they obscured acceptance.
- Checked the final prompt/specification wording for stale visual mandates, unresolved placeholders, broken local references, and inconsistent closure/ownership/timing rules. Historical mentions of the superseded reference are explicitly labeled.
- Used editorial read-through and `git diff --check` for documentation increments. No specification-parsing or architecture tests were added. No application tests/builds were rerun for these prose-only edits; the earlier 25 passing static mock checks are not production acceptance evidence.
- Changes are confined to the requirements, the reconciled prompt design section, this review record, and task tracking. No mock/application behavior, screenshot, dependency, deployment, or external repository changes are part of this delivery.
