# FaithTech Toronto AI Build Event companion

## Product direction — September 9, 2026

Build an extremely focused companion to the September 9, 2026 build event and its PowerPoint presentation. This direction completely supersedes the earlier reusable event-platform scope. Deliver one Angular web application, a .NET API using SignalR for realtime updates, and a small administrator-passcode CLI.

The application has exactly four screens, in this order:

1. **Countdown:** countdown, welcome message, brief event information, and email entry. Successful entry adds the participant to the raffle and confirms it. An optional section collects name, “What you make”, and “What's on your heart”. Administrators can view, add, update, and delete participants on this screen and close countdown to advance everyone to Projects.
2. **Projects:** cards describing RTR projects. Administrators can add, update, and remove projects here.
3. **Team selection:** opening this stage forms random teams of three once. Administrators can rearrange members by drag and drop and assign a project to each team.
4. **Raffle:** administrators press **DRAW NAME**. Participant names cycle in an animation, settle on the selected winner, and falling confetti-like particles celebrate the result using WebGPU when available.

Administrator login uses a shared four-digit passcode within the same application. Anyone with the necessary database access can change the passcode directly in the database or through the CLI delivered with the application. A browser administrator session does not confer database access. There is no separate administrator application or login screen: login and editing use controls, panels, or dialogs within the four screens.

All UI must come from **`@quinntyne/cornerstone` on npm**, including tokens, controls, cards, dialogs, team interactions, and raffle effects. When a required component or capability is missing, build and test it in Cornerstone, release a new npm version, and update this application's dependency to that version. Locally copied components or a separate app-owned design system do not satisfy this requirement.

## Event context

[run-sheet.html](run-sheet.html) supplies event context: September 9, 2026, 5:00–9:00 PM, Stone Church — Davenport Community Campus, 45 Davenport Rd, Toronto. Welcome is at 5:20, introductions at 5:28, project pitches at 5:50, team formation at 6:05, demos at 8:30, and the raffle at 8:50.

RTR means **Reconciliation Through Relationships**. The run sheet describes the July hackathon project with rightrelationship.ca, responding to the TRC's Calls to Action through registration/onboarding, shared learning, facilitator-reviewed matching with mutual consent, a cohort map, messaging, and scheduling. These describe the project being presented; they are not capabilities of this companion app. Actual repository and demo URLs must be supplied, not guessed. The guest project's details must be entered by an administrator before it is shown.

The run sheet and PowerPoint guide the human presenter. They do not add application screens, automatically advance screens, control PowerPoint, or define app styling. Teaching, prayer, build guidance, networking, demo playback, and closing remain in the event and presentation.

## Specification defaults

The following resolve unspecified boundaries and are adjustable product defaults, not additional user-confirmed instructions:

- Countdown targets the 5:20 PM welcome (Toronto time); reaching zero waits for the administrator. Doors open at 5:00 PM. All screen transitions are administrator-controlled.
- Initial groups contain three people, with a final group of one or two for any remainder. Administrators can rearrange into pairs or other sizes, including the run sheet's low-attendance contingency.
- Team formation uses entered participants with equal random weight; optional profile answers do not affect grouping.
- Email entry closes when Countdown closes. Administrators can still correct the roster by locally revisiting Countdown; late additions remain unassigned until moved into a team.
- One normalized email yields one raffle entry. Previous winners are excluded from subsequent draws. Name is optional; an assigned public label identifies unnamed participants without publishing their email.
- Optional profile answers are private to the participant's entry session and administrators. Email alone never authorizes reading or editing somebody's saved profile.

## Removed scope

No multi-event configuration/copying/publication, timed stage engine, individual participant entry codes, administrator accounts/passwords, participant-selected teams/projects, project proposals, messaging, networking recommendations, quizzes, prize catalogue, demo schedule/player, showcase, recap, venue map, Liturgy connection, standalone design-system site, generic SQL console, or broad operator administration suite is part of this product. No sound feature is required.

[specs/L1.md](specs/L1.md) and [specs/L2.md](specs/L2.md) define the complete capabilities and Given–When–Then acceptance criteria. Existing implementations, mocks, deployment guides, and deleted historical specifications do not establish compliance with this new direction.
