# faith-tech-toronto-ai-build-event

## Project Overview

Build a reusable web platform for live FaithTech build events, beginning with the
September 9, 2026 Toronto AI Build Event. A participant-facing Angular client and
an admin-only Angular application share a .NET API. Administrators configure
events, participant access, schedules, content, venue branding, and activities.
Participants enter with an email address and individual entry code, see a venue
countdown, and move automatically through scheduled stages for team and project
selection, building, networking and private messaging, quizzes, raffles, and demos.
Project showcases and recaps retain repository and demo links after the event.

The solution uses Cornerstone's published components and design system for every
frontend UI, with an independent, accessible, responsive, light-theme gallery.
Optional per-event Liturgy project links support
continued work after events; this connection is disabled by default and for the
September 9 event. Core event features work independently of Liturgy, which owns
ongoing project management. Product scope and acceptance criteria live in
[docs/prompt.md](docs/prompt.md) and [docs/specs/](docs/specs/).

## Speed Is Not the Goal

Quality and completeness beat finishing fast. Finish the whole task - edge cases,
error paths, no stubs or `TODO`s. If it is bigger than it looked, complete it and
say what it cost rather than quietly narrowing scope.

## Technology

- Use .NET for the API.
- Use MediatR, pinned to **12.5.0**. Do not upgrade. 12.5.0 is the last release
  under plain Apache-2.0; from 13.0.0 MediatR is commercially licensed, free only
  under a registered Community tier that lapses above $5M USD annual revenue.
- Use Microsoft.Extensions libraries and patterns: dependency injection, Options,
  and Configuration.
- Use Angular for the web client.

## Architecture and Design

- Implement requirements radically simply: the least code that satisfies the
  acceptance criteria, and nothing more. Simple in design, never reduced in scope.
- Apply SOLID principles throughout the codebase.
- Organize features and behaviors into vertical slices.
- Keep back-end code in `backend/`, with source in `src` and tests in `tests`.
- A command-line tool is another project under `backend/src`, not a second root.

## Backend

- Use `FaithTechTorontoAiBuildEvent` as the .NET solution's root namespace.
- Use Clean Architecture. Dependencies point inward. `Domain` references nothing.
- Keep controllers thin: bind, dispatch through MediatR, return. No logic in a
  controller.
- Commands, queries, handlers, and validators live in `Application`.
- One file per type. Every class, interface, record, and enum gets its own file,
  named for the type it holds.
- Folders and namespaces agree. Controllers live in a `Controllers` folder and are
  namespaced `FaithTechTorontoAiBuildEvent.Api.Controllers`.

## Command-Line Tools

- Use `System.CommandLine` for all CLI tools.
- Apply SOLID principles and use `Microsoft.Extensions` libraries for dependency
  injection, logging, Options, and Configuration.
- Follow the command-per-file pattern: each command lives in its own file, named
  for the command type.
- Package every CLI as a .NET tool, installable through `dotnet tool`.
- Provide a script under `<ROOT>/eng/scripts/` for each tool that builds and
  packages the latest local source, then installs or updates that build on the
  developer's machine. The script must support both first-time installation and
  updating an existing installation.

## Frontend

- `frontend/` is an Angular workspace: the `api`, `components`, and `domain`
  libraries and the application project are siblings under `frontend/projects/`.
- Prefer signals over RxJS, and hold state in signals. Reach for RxJS only for
  genuine streams and events.
- No single-file components. Template, styles, and class each live in their own file.

### Cornerstone is mandatory for every UI

All frontend UI applications, including admin, client, galleries, prototypes, and
any future UI, must compose all interfaces from `@quinnntyne/cornerstone`
components and `@quinntynne/cornerstone-design-system` foundations. This applies
to every screen, state, dialog, and shared component, without exceptions for small
changes, one-off designs, or temporary implementations.

- Use Cornerstone for all foundational UI: cards, buttons, inputs, controls,
  navigation, dialogs, tables, feedback, and every other reusable visual primitive.
- Use its design system for all spacing, layout foundations, colours, typography,
  sizing, radii, borders, shadows, motion, and other design tokens.
- This repository owns event-specific composition and behavior. Do not implement
  local foundational components, copy Cornerstone source or styles, introduce
  substitute UI libraries, or recreate primitives with custom HTML/CSS.
  Use native elements through Cornerstone's supported component/directive APIs.
- The authoritative source is
  [QuinntyneBrown/Cornerstone](https://github.com/QuinntyneBrown/Cornerstone).
  If any needed card, button, variant, token, layout primitive, or other UI
  foundation is missing, first implement and test it in that repository, publish
  the updated package to npm, then update this repository's dependency and
  lockfile to consume the published version. Only then compose the dependent UI.
- Local copies, workspace links, unpublished builds, and temporary fallbacks do
  not satisfy this requirement. If the upstream release is unavailable, the
  dependent UI work remains blocked; do not bypass the upstream-first workflow.
- Existing local foundations are migration debt, not precedent or an exemption.
  When changing a UI, migrate its affected foundations to the published packages.

### Where a component belongs

Placement follows what a component knows, and it is not negotiable.

- `components`: presentational compositions of published Cornerstone components,
  never local implementations of buttons, cards, pills, or other foundations.
  Takes an input, emits an output, injects no application service, and imports no
  other workspace project. Published Cornerstone packages and Angular primitives
  like `Router` are allowed; an `api` contract is not. That leaf position is what
  lets the library publish to npm.
- `domain`: components that inject an `api` contract through its token and render
  what it returns.
- The application project: routed page components, composing the other two and
  owning routing, guards, and dialogs.
- Dependencies run one way, application to `domain` to `api`. A presentational
  component that turns out to need a service moves to `domain`.

### Interface-driven service consumption - mandatory on the frontend

Every service an application consumes is reached through an interface and an
`InjectionToken`. No component, store, or feature imports a concrete implementation.

- `IQuoteService` declares the contract and `QUOTE_SERVICE` is its `InjectionToken`;
  the interface, the token, and each implementation live in separate files (with contract files named `<entity>-service.contract.ts`, e.g., `quote-service.contract.ts`).
- Contracts are named `I<Entity>Service`, singular, with no `Api` suffix. Data
  shapes (`QuoteResult`) take no prefix, and the production implementation takes
  the unprefixed name (`QuoteService`), never an `Impl` suffix.
- Consumers call `inject(QUOTE_SERVICE)` only. Composition binds the token to the
  HTTP adapter in production and to a mock under Playwright, so a test never
  reaches the real implementation.
- HTTP calls and observable-to-signal conversion stay inside the `api`
  implementations; `domain` types carry no HTTP dependency.

## Design System

The local design-system gallery is a deliverable in its own right, not a folder
inside the front end. It sits at `design-system/`, beside `backend/` and `frontend/`, with its own
`package.json`, its own tests, and its own build, deploys as its own static site,
and carries no runtime dependency on the application.

Cornerstone owns all foundational components and design tokens. The local gallery
and every frontend application consume the published
`@quinnntyne/cornerstone` and `@quinntynne/cornerstone-design-system` packages;
this repository must not maintain authoritative or manually mirrored copies.
Every component stylesheet uses the published CSS custom properties for design
values. A hard-coded colour, spacing, dimension, or font stack is a defect.
Add missing foundations in Cornerstone and publish them to npm before consuming
them here, following the mandatory workflow above. Keep the event UI light-theme,
accessible, and responsive using those published foundations.

## Implementation

Implement with the **incremental implementation** skill from Addy Osmani's
`agent-skills`. It is mandatory for any change touching more than one file, and it
is not optional because the work looks small once you have read the requirement.

    /agent-skills:incremental-implementation

Install it once per machine:

    /plugin marketplace add addyosmani/agent-skills
    /plugin install agent-skills@addy-agent-skills

What the skill requires of you:

- Build in thin vertical slices. Implement one slice, test it, verify it, then
  expand. Never implement a whole feature in one pass.
- Leave the system working and testable at the end of every increment. An
  increment that does not build is not an increment.
- Stop and test before you have written ~100 lines. If you are tempted to write
  more than that before running anything, the slice is too big - cut it.
- Commit each verified increment, so the history reads as a sequence of working
  states rather than one drop.

This pairs with ATDD below: the acceptance test defines the slice, and the slice
is done when that test passes. It does not license shipping less than the whole
requirement - see *Speed Is Not the Goal*. Slices are how the work lands, not how
much of it lands.

## Testing Approach

Use acceptance test-driven development (ATDD): begin with a failing acceptance
test, link it to explicit criteria written using the Given-When-Then format,
implement until it passes, and keep criteria, tests, and implementation aligned.

Back end: integration tests against the API. Front end: Playwright, using the
Page Object Model - one page object per screen, owning the selectors and the
interactions. Tests state intent; page objects know the DOM. Never put a
selector in a test.

### Never write architecture tests

Never add a test that asserts the shape of the codebase rather than its behavior:
no structure, layout, or naming tests; no banned-API scans; no traceability tests
that parse the specifications. Those constraints belong to the compiler, the
formatter, and review. A test suite exists to prove behavior.

## Folder Structure

```text
faithtech-toronto-ai-build-event/
|-- backend/
|   |-- src/
|   `-- tests/
|-- frontend/
|   `-- projects/
|       |-- admin/ <-- admin app
|       |-- client/ <-- client app
|       |-- api/
|       |-- components/
|       `-- domain/
|-- design-system/
|-- eng/
|   `-- scripts/
|-- e2e/
|   |-- page-objects/
|   `-- specs/
`-- docs/
    `-- specs/
```
