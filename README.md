# FaithTech Toronto AI Build Event

A reusable platform for live FaithTech build events, beginning with the
September 9, 2026 Toronto AI Build Event. It brings event administration,
participant access, and the build-night experience into one workspace.

The project uses .NET, Angular, and SQL Server. Its scope includes:

- administrator tools for events, participant rosters, schedules, content, and venue branding;
- participant entry by email and individual code, followed by scheduled stages for
  teams, projects, building, networking, messaging, quizzes, raffles, and demos; and
- project showcases and recaps that preserve repository and demo links after the event.

**Under active development:** administrator access, event configuration, rosters,
and participant authentication are implemented. Live participant stages and most
activities remain unfinished. See [implementation evidence](docs/IMPLEMENTATION.md)
for the current completion boundary; the application has not passed production acceptance.

The independent, accessible, responsive design system follows Cornerstone's light
theme. Optional Liturgy links support continued project work after an event;
the connection is disabled by default and for the September 9 event.

## Getting started

### Prerequisites

- .NET SDK matching [global.json](global.json).
- Node.js 22 and npm 10.9.4, matching the repository's CI setup.
- SQL Server; local development defaults to `.\SQLEXPRESS` with Windows authentication.
- PowerShell for the commands below, and Google Chrome for browser acceptance tests.

### Build and run

From the repository root:

```powershell
dotnet restore backend/FaithTechTorontoAiBuildEvent.slnx --locked-mode
npm --prefix frontend ci
npm --prefix frontend run build
dotnet build backend/FaithTechTorontoAiBuildEvent.slnx --no-restore
```

Follow [local development setup](CONTRIBUTING.md#local-development) to configure
the database and digest key, trust the development certificate, apply migrations,
and create an administrator. Then start the API:

```powershell
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Api -- --environment Development --urls https://localhost:7443
```

Open administration at `https://localhost:7443/admin/`. The API also serves the
participant application at the origin root. See [CONTRIBUTING.md](CONTRIBUTING.md)
for validation commands and [backend setup](backend/README.md) for configuration details.

## Repository structure

| Directory | Purpose |
| --- | --- |
| [backend/src](backend/src) | .NET domain, application, infrastructure, API, and operator CLI |
| [backend/tests](backend/tests) | API integration and acceptance tests |
| [frontend/projects](frontend/projects) | Angular admin and client applications; api, domain, and components libraries |
| [e2e](e2e) | Playwright acceptance tests and page objects |
| [design-system](design-system) | Independent design tokens, component gallery, tests, and static build |
| [eng/scripts](eng/scripts) | Operator installation, release packaging, deployment, and smoke checks |
| [docs](docs) | Product scope, requirements, designs, mocks, and operational guides |

## Documentation

- [Product scope](docs/prompt.md), [high-level requirements](docs/specs/L1.md), and [acceptance criteria](docs/specs/L2.md)
- [Detailed designs](docs/detailed-designs/README.md) and [interactive screen mocks](docs/mocks/README.md)
- [Frontend guide](frontend/README.md) and [design-system guide](design-system/README.md)
- [Implementation status](docs/IMPLEMENTATION.md) and [demo guide](docs/demo/README.md)
- [Azure deployment](docs/azure-production-deployment.md) and [domain setup](docs/namecheap-domain-setup.md)
- [Support](SUPPORT.md)

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md), follow the [Code of Conduct](CODE_OF_CONDUCT.md),
and search [existing issues](https://github.com/QuinntyneBrown/faithtech-toronto-ai-build-event/issues)
before proposing changes. [Contributors](CONTRIBUTORS.md) are recognized for code,
documentation, design, testing, and review work.

Report suspected vulnerabilities privately through [SECURITY.md](SECURITY.md).

## License and attribution

A root license for the project's original code has not yet been specified.
Cornerstone-derived materials retain their included MIT license. See
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for attribution and dependency notices.
