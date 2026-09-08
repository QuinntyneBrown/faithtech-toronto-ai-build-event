# Backend

Install the SDK pinned in the root `global.json`. On this workstation the isolated
SDK is `%LOCALAPPDATA%\FaithTech\dotnet`; prepend that directory to PATH when using
the commands below. Restore the local EF tool with `dotnet tool restore`.

```powershell
dotnet build backend/FaithTechTorontoAiBuildEvent.slnx
dotnet test backend/FaithTechTorontoAiBuildEvent.slnx
```

Acceptance tests create uniquely named `FaithTechAcceptance_*` databases on
`.\SQLEXPRESS` using Windows authentication and remove those databases afterward.
Set `FAITHTECH_TEST_SQL` to a different test server connection when required. The
test account needs permission to create and drop its isolated test databases.

Set `ConnectionStrings__EventDatabase` through the deployment's secret/configuration
provider. Development API settings use Windows authentication against local SQL
Express. The CLI reads environment configuration and has no default credentials.
The API also requires `Security__DigestKey`: at least 32 cryptographically random
bytes encoded as base64, supplied by the secret provider and shared by all API
instances. Preserve it across restarts so authentication budgets keep their keys.
Do not commit it. Tests generate isolated ephemeral keys automatically.

```powershell
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Provisioning -- migrate
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Provisioning -- create-admin operator
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Provisioning -- disable-admin operator
dotnet run --project backend/src/FaithTechTorontoAiBuildEvent.Api -- --environment Development --urls https://localhost:7443
```

Provisioning prompts for a password without echo. Automated operators may provide
it through redirected standard input from a secret provider; never put it in a
command argument. Disabling an account revokes all of its sessions transactionally.
Apply migrations once before starting API instances. Local HTTPS requires an
ASP.NET Core development certificate.

Only the verified increments listed in `docs/IMPLEMENTATION.md` currently exist.
The application is under construction and has not passed production acceptance.
