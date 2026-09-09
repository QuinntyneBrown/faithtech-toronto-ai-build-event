# faith-tech-toronto-ai-build-event

## Project Overview

Build a dedicated companion to the September 9, 2026 Toronto AI Build Event and
its PowerPoint. One Angular application shares a .NET API with SignalR realtime
updates. Exactly four screens exist: Countdown, Projects, Team selection, Raffle.
Participants enter with email only and join the raffle; personal details are
optional. A shared four-digit passcode enables administrator controls within
those same screens. Administrators advance the event, manage participants and
projects, rearrange random teams, assign projects, and draw raffle winners.
The passcode is changeable directly in the database and through a packaged CLI.

All UI comes from `@quinntyne/cornerstone` on npm. Missing UI capabilities must
be implemented in Cornerstone, released, and consumed through the new version.
There is no separate admin app, local design system, or Liturgy integration.
This September 9 direction supersedes the earlier reusable platform. Event
context is in `docs/run-sheet.html`. Product scope and acceptance criteria live in
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

### Where a component belongs

Placement follows what a component knows, and it is not negotiable.

- `components`: presentation composition of Cornerstone UI only. Takes an input, emits
  an output, injects no application service, imports no other project. An Angular
  primitive like `Router` is fine; an `api` contract is not. That leaf position is
  what lets the library publish to npm.
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

`@quinntyne/cornerstone` owns all UI, design tokens, styles, and reusable visual
behaviors. Consume its npm release directly; do not copy or mirror its tokens
or implement replacement controls in this repository. Missing components,
tokens, drag-and-drop behavior, accessibility fixes, or raffle effects must be
built and tested in Cornerstone, released to npm, and adopted by version update.
Application code composes that UI and supplies event state and actions. Keep
the accessible, responsive light theme. The former `design-system/` project is
legacy implementation, not a required deliverable under the new scope.

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
|       |-- client/ <-- single app, including inline admin controls
|       |-- api/
|       |-- components/
|       `-- domain/
|-- eng/
|   `-- scripts/
|-- e2e/
|   |-- page-objects/
|   `-- specs/
`-- docs/
    `-- specs/
```
