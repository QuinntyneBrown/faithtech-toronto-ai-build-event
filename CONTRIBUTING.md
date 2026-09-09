# Contributing

Start with the [product scope](docs/prompt.md), [acceptance criteria](docs/specs/L2.md),
and [implementation status](docs/IMPLEMENTATION.md). Search existing
[issues](https://github.com/QuinntyneBrown/faithtech-toronto-ai-build-event/issues)
before proposing work. Code, design, accessibility, testing, and documentation contributions are welcome.

## Local development

Use the .NET SDK in [global.json](global.json), Node.js 22 with npm 10.9.4
(matching CI), PowerShell, and SQL Server. The default development database is
`FaithTech` on `.\SQLEXPRESS` with Windows authentication. Other environments
must supply their own `ConnectionStrings__EventDatabase`.

Run from the repository root:

```powershell
dotnet restore backend/FaithTechTorontoAiBuildEvent.slnx --locked-mode
dotnet tool restore
npm --prefix frontend ci
npm --prefix frontend run build
dotnet build backend/FaithTechTorontoAiBuildEvent.slnx --no-restore
dotnet dev-certs https --trust
```

Configure and initialize a local database, then start the API in the same terminal:

```powershell
$env:ConnectionStrings__EventDatabase = 'Server=.\SQLEXPRESS;Database=FaithTech;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=false'
$env:Security__DigestKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Provisioning -- migrate
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Provisioning -- create-admin operator
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Api -- --environment Development --urls https://localhost:7443
```

The provisioning command prompts for the administrator password without echoing it.
Open administration at `https://localhost:7443/admin/`; the participant shell is
served at the origin root. Event access requires a configured roster and entry code.
Preserve the digest key privately across restarts; generate it once per local setup.
Never commit credentials or paste production secrets into these examples.

See the [backend guide](backend/README.md) for configuration and the
[frontend guide](frontend/README.md) for Angular development servers.
Install or update the packaged operator CLI with `./eng/scripts/Install-OperatorTool.ps1`.

## Validation

Run the checks for the area changed. Install dependencies with `npm ci` in each
JavaScript project before using its commands.

| Area | Checks from the repository root |
| --- | --- |
| Backend | Build the frontend first, then `dotnet build backend/FaithTechTorontoAiBuildEvent.slnx` and `dotnet test backend/FaithTechTorontoAiBuildEvent.slnx` |
| Angular | `npm --prefix frontend run build`, `npm --prefix e2e ci`, then `npm --prefix e2e test` |
| Design system | `npm --prefix design-system ci`, `npm --prefix design-system test`, then `npm --prefix design-system run build` |
| Event mocks | `npm --prefix design-system run test:mocks` |
| Release scripts | `node --test eng/scripts/*.test.mjs` |
| Documentation | Review the Markdown, verify relative links, and compare commands with the referenced scripts and manifests |

API acceptance tests create and drop isolated databases. Use a test SQL Server
account with those permissions; set `FAITHTECH_TEST_SQL` to override `.\SQLEXPRESS`.
Browser tests require Google Chrome and start their own mock-backed Angular servers
on ports 4200 and 4201, which must be free. See [Playwright configuration](e2e/playwright.config.ts).

## Implementing and reviewing changes

Follow [AGENTS.md](AGENTS.md): use Given-When-Then acceptance criteria, begin behavior
changes with a failing acceptance test, and commit small verified increments.
API tests exercise behavior through integration tests; browser tests use page objects.
Do not add tests for code structure or naming. Keep MediatR pinned to 12.5.0.

Preserve Clean Architecture and vertical slices. Frontend consumers inject service
tokens; presentational components remain independent of application services.
Change authoritative design tokens in `design-system/` first, then run
`npm --prefix frontend run tokens` and commit both copies.

In a pull request, explain the problem, resulting behavior, acceptance coverage,
and checks performed. Include screenshots for visual changes using fictional data.
Describe configuration or migration effects and any remaining limitations.
Retain third-party attribution and only contribute material you have permission to share.
