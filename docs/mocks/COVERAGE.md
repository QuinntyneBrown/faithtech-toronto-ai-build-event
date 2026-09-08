# Prompt coverage and design review

The [screen index](http://127.0.0.1:4317/docs/mocks/) is the interactive inventory. Every item below has a rendered screen or in-context overlay; the index also links its alternate states. This checklist is reviewed documentation, not parsed by a traceability test.

| Prompt capability | Review surfaces | Status |
|---|---|---|
| Participant names, email/code access | Host participant list and add/edit/remove dialogs; participant access, rejection, expiry | Complete |
| Countdown, venue logo and location | Countdown, local map, venue fields and local logo preview | Complete |
| Configurable event and stage timing | Event settings, stage editor, reorder/remove, overlap errors, simulated automatic transitions | Complete |
| Configurable content and links | Stage instructions/resources, project descriptions and URLs, venue and event details | Complete |
| Random or self-selected teams | Team selection/detail, full state, random assignment/result, host assignment preview and roster editor | Complete |
| Predefined or participant-proposed projects | Project selection/detail, restricted state, proposal dialog, host choice settings and project editor | Complete |
| Profiles and networking | Profile editor, participant profiles, directory/search, recommendations and no matches | Complete |
| Participant chat | Conversation list, person picker, thread, composer, empty/sending/failure/retry | Complete |
| Quizzes | Introduction, waiting, question/selection, feedback/results/closed, question/answer editor and host responses | Complete |
| Prizes and raffles | Prize editor/removal, draw confirmation/animation/winner/history, eligible pool, exhausted draws, optional sound | Complete |
| Demo schedule and showcase | Host lineup/edit/reorder/remove, participant schedule/current demo, showcase and recap | Complete |
| Reusable future events | New-event form, separate sample event sessions, reopen previous event | Complete |
| Admin-only interface | Host sign-in/rejection and access-denied artifact; direct review links deliberately bypass mock sign-in | Complete as design |
| Liturgy disabled by default and for September | No participant Liturgy links on selection, detail, or recap; saved URLs remain in host editor | Complete |
| Optional Liturgy links for future events | Future scenario; project selection/detail/recap links; membership explanation | Complete |
| Independent event and project progress | Event stages switch screens; no phase completion, boards, work items, invitations, or Liturgy API calls | Complete |
| Identical light design foundations | Local Cornerstone tokens, theme, logo, controls and parity specimens; dark OS remains light | Complete |
| Recovery and overlays | Save errors/retry, unsaved edits, offline/reconnection, loading, navigation/account and not-found | Complete |

## Review evidence

Final review: **25 browser checks passed** in Google Chrome **152.0.7977.76**. Inventory: **33 screens**, **73 screen/state combinations**, **37 in-context dialogs**. The screenshot set contains **62 screen/dialog captures plus 2 component-parity captures**. The observed celebration renderer was **WebGPU**; the explicit canvas fallback and reduced-motion checks also passed.

The reference and local primitive specimen PNGs have the same SHA-256: `48D6B39CA6BD9C834BD1D26DA2075B15F53568B75686A7D5E7DE3EDA68C02543`. This establishes exact parity for the captured representative primitives, not every possible component or application layout in Cornerstone.

Both the independent gallery build and the complete mock bundle were opened from their built directories in Chrome without missing assets or runtime errors.

- Rendering sweep: every indexed screen/state at 320, 768, 1024, and 1440 pixels; no horizontal page overflow or missing assets.
- Overlay sweep: every indexed dialog on a narrow viewport, with Escape dismissal.
- Accessibility: WCAG A/AA browser audit for all main screens and all dialogs; standalone gallery audit.
- Workflow review: invalid/valid entry, team/project choice, Liturgy URL preservation, draft cancellation/discard, quiz scoring, distinct raffle winners, schedule overlap rejection, automatic event opening, save retry, independent event creation.
- Effects: WebGPU-or-canvas renderer, explicitly forced canvas fallback, and reduced-motion celebration.
- Component parity: captured Cornerstone reference versus local rendered primitives, plus side-by-side images.

Run `npm run test:mocks` from `design-system/` to reproduce browser verification. Review screenshots are linked from [screenshots/README.md](screenshots/README.md). The suite is design verification, not ATDD and not a production acceptance suite.

## Deliberate artifact boundaries

Event times, venue, participants, projects, demo destinations, and prizes are identified sample content. Authentication, registration, live mapping, network chat, true random draws, and external workspace access are simulated or represented visually. Review controls are outside participant product content. Shared sign-in, project creation, invitations, progress synchronization, and ongoing project management remain outside first-version scope.
