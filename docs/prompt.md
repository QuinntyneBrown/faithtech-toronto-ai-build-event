# FaithTech Toronto AI Build Event

Build a customizable web platform that guides participants through a live build event. The first use case is the September 9 FaithTech Toronto AI Build Event, but administrators should be able to configure the platform for future build events.

The platform has two experiences: a participant-facing client and an administration interface restricted to users with the admin role.

## Relationship to Liturgy

Treat this platform and Liturgy (`C:\projects\Liturgy`) as companion applications. The event platform brings people together for a build event; Liturgy gives their projects an ongoing workspace between events. Both applications must remain independently usable.

### Responsibilities

- The event platform owns event registration and access, countdowns, venue information, scheduled screens, team formation, project selection, networking, quizzes, raffles, and the demo schedule and showcase.
- Liturgy owns ongoing workspace membership, project journeys, phase requirements, work items, assignments, boards, and 5R reflections and progress.
- Keep ongoing project-management capabilities in Liturgy rather than duplicating them in the event platform.
- Keep event timing separate from project progress. A scheduled transition to the Develop segment changes the event screen; it must never automatically complete Discover or Discern requirements or advance a project's gated phase in Liturgy.

### Event-Level Opt-Out

- Provide an administrator-controlled "Use Liturgy" setting for each event, disabled by default. Administrators can choose whether an event uses the companion-app experience.
- For the September 9 FaithTech Toronto AI Build Event, keep "Use Liturgy" disabled. Run the entire evening through the event platform and participants' chosen build tools.
- When disabled, hide Liturgy-specific links, actions, onboarding, and recap content from participants. Do not require Liturgy accounts, workspace setup, or connectivity at any stage of the event.
- Keep team and project selection, building guidance, repository and demo links, and all other core event features available when Liturgy is disabled.
- Preserve any configured Liturgy project URLs when an administrator disables the setting, so they can be used if the event later opts in.

### First-Version Connection (When Enabled)

- Let administrators attach an optional Liturgy project URL to each event project.
- When "Use Liturgy" is enabled, show participants an "Open project in Liturgy" action for linked projects on the project-selection and project-detail screens.
- When enabled, include the Liturgy link alongside the project's repository and demo links in the event recap, so participants can continue building afterward.
- Allow projects at future events to link to the same Liturgy project, preserving continuity across build nights.
- Keep participation and all core event features available without a Liturgy account or connection. Access to a linked Liturgy workspace follows Liturgy's own sign-in and membership rules; joining an event team does not automatically grant that access.
- Use links for this first version. Do not require shared sign-in, automatic project creation, membership synchronization, or Liturgy API availability to run an event.

### Future Integration

After the September 9 event, explore more advanced workflows that use both companion applications during a single evening: form teams and choose projects in the event platform, open the associated Liturgy workspace to follow the 4D/5R process and manage work, then return to the event platform for demos and closing activities.

API-based project selection, workspace invitations, and project-progress summaries are possible later enhancements. These workflows are outside the first-version scope, must remain optional per event, and must preserve the responsibility boundaries above, including the separation between event timing and project progress.

## Design System

- **Confirmed September 7, 2026:** use [Cornerstone](https://github.com/QuinntyneBrown/Cornerstone), FaithTech's component library in `C:\projects\Cornerstone`, as this platform's light-theme visual reference. This supersedes the earlier interim Liturgy reference. Future npm adoption remains a separate decision.
- Match Cornerstone's light design token values, typography, colour palette, spacing, sizing, borders, radii, shadows, iconography, equivalent components, interaction states, and motion conventions. The standalone design system owns local `--cs-*` tokens; event compositions add missing tokens there without overriding reference primitives. Liturgy's current appearance does not override this reference.
- For the current version, each application must own a separate, self-contained design-system implementation in its own repository. Do not share design-system packages, source files, libraries, assets, or build/runtime dependencies between the applications at this stage.
- Inspect the recorded Cornerstone revision as a reference and maintain matching tokens, styles, components, and licensed local asset copies within this repository. This platform's standalone design-system site, mockups, and production interfaces must build and run without access to the Cornerstone or Liturgy repositories or deployments.
- Use a light theme throughout the participant client, administration interface, and standalone design-system site. All screens and states must remain light, regardless of the user's browser or operating-system theme preference. Do not include a dark mode or theme toggle.
- Apply these requirements to all mockups and production interfaces. Event-specific content and venue branding must fit within this platform's matching FaithTech design system without overriding its tokens or component styling.
- Design-system parity is mandatory even when "Use Liturgy" is disabled, including for September 9. The setting controls workflow integration only.
- Verify equivalent components and states side by side against the recorded Cornerstone light reference before accepting the design or implementation. Record event-layout adaptations, including light host navigation, separately from primitive parity; do not introduce a dark shell or theme toggle.

### Planned Adoption of Cornerstone

Both applications are intended to adopt the existing Cornerstone library (`@cornerstone/ui`) through npm for common components, design tokens, styles, and assets. This is the agreed direction; the separate implementations above are an interim arrangement. Keep them organized for migration to Cornerstone and consult its existing component APIs when designing equivalent interfaces.

Publishing and adopting Cornerstone are outside the current scope. Before migration, verify package availability and compatibility with both applications. Preserve identical appearance and behavior as they adopt the library. Each application must continue to build and run independently of the other application's repository or deployment. Cornerstone adoption is independent of the event's "Use Liturgy" setting.

## Participant Access and Countdown

- Administrators enter participant names for each event.
- Participants provide their email address and an entry code to access the event experience.
- Before the event starts, the client displays a countdown screen with a configurable venue logo and a map showing the event location.
- The live event experience opens automatically at the configured start time.

## Scheduled Event Flow

- Administrators configure the event's start and end times, along with the start and end times of each stage.
- The participant interface changes automatically as the event progresses through its scheduled stages.
- For the September 9 event, screens should appear at preconfigured times in step with the presentation, much like a timed slide deck with interactive activities.
- Administrators can customize all event content and links.

## Teams and Projects

- Provide an interactive team-selection screen that supports random assignment or participant-selected teams.
- Provide a project-selection screen where administrators can restrict choices to a predefined list or allow participants to propose their own projects.

## Messaging and Networking

- Let participants create profiles and share details about themselves.
- Allow participants to chat with one another.
- Recommend people to meet based on participant profile information.

## Raffles

- Administrators configure raffle prizes.
- Animate the draw by cycling through participant names, using WebGPU visuals and playful sound effects.
- Celebrate the selected winner with particle effects.
- Once a participant wins, exclude them from subsequent draws within that event.

## Browser Experience

Target the latest version of Google Chrome. Use modern browser capabilities, including WebGPU, to create a polished, engaging experience and enhance interactive moments such as the raffle.

## Reusable Event Features

Keep the following core capabilities available across events, with event-specific configuration:

- Participant management and access.
- Scheduled stages and automatic screen transitions.
- Team and project selection.
- Raffles and prizes.
- Quizzes.
- Participant messaging.
- Profiles, networking, and recommended connections.
- Venue maps and branding.
- A countdown screen before the event begins.
