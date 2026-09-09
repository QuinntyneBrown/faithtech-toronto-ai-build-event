# Importing event seeds with the operator CLI

Install the current local tool with `./eng/scripts/Install-OperatorTool.ps1`.
Use the repository's pinned .NET runtime. MediatR remains pinned to 12.5.0.
The CLI connects directly to SQL; an API deployment is not required to import.

## Configure and inspect a target

Supply a connection through protected process configuration. Do not put it in
arguments, source files, or ordinary logs. Profiles store the environment-variable
name, never its value. This implementation uses process secret references instead
of adding a second persistent password store.

```powershell
faithtech-admin target configure --target production --server <server>.database.windows.net --database FaithTech --environment production --connection-env ConnectionStrings__EventDatabase
faithtech-admin target inspect --target production --json
```

Profiles default to `%LOCALAPPDATA%/FaithTechTorontoAiBuildEvent/operator/targets.json`.
Use `--config <absolute-path>` consistently to select another profile file.
Names are case-insensitive; duplicate names are rejected. Production requires
encryption and certificate validation. No command creates databases, changes SQL
firewalls, grants permissions, or falls back to another connection.

`--connect-timeout` defaults to 30 seconds and `--command-timeout` to 60 seconds.
Both accept positive integer seconds. Inspect reports these values and the actual
server, database, and authenticated principal without credentials.

## Preview and apply

```powershell
faithtech-admin migrate --target production --preview --json
faithtech-admin operations apply <preview-id> --target production --approve <preview-id> --json
faithtech-admin events import --create --file "C:/path/event seed.json" --target production --preview --operation-id <guid> --json
faithtech-admin operations show <operation-id> --target production --json
faithtech-admin operations apply <preview-id> --target production --approve <preview-id> --json
faithtech-admin operations reconcile <operation-id> --target production --json
```

Migration preview lists applied and pending migrations. Apply checks the same
history and tool assemblies under a migration lock. Each migration retains its
own transaction boundary; earlier commits remain reported after a later failure.
Deployment/bootstrap callers use `eng/scripts/Invoke-ReviewedMigration.ps1` to
configure, preview, and explicitly approve their release migration. Bare `migrate`
is no longer a mutation command. Test setup creates its disposable database first.

Import preview shows the event ID and full before/after event and schedule values.
It never writes event data. Creation requires both `event` and `schedule` and
always produces a draft. Update uses `--event <id> --version <reviewed-version>`
instead of `--create`; titles never select events. Omitted fields are retained,
explicit null clears nullable fields, and a supplied schedule replaces stages and
windows together. Stage identities cannot be borrowed from another event.
Publication, logo, registrations, credentials, and participation history survive
updates. Published events retain required data and elapsed lifecycle restrictions.

The seed is strict UTF-8 JSON, optionally with a BOM. Duplicate/unknown fields,
contradictory times, malformed values, and invalid schedules fail before mutation.
`publication`, `status`, `source`, `timetable`, and `operatorRunSheet` are inert
string metadata. They never fetch files or URLs or publish an event.

Approval binds the actual principal and target, tool assemblies, operation,
source file hash, and reviewed version. Changed input needs a new preview.
Interactive apply requires typing the displayed server/database; redirected input
requires `--approve` matching the completed preview ID. Cancellation never grants
approval. Import data, attribution, audit, and receipt commit in one transaction.
Retries of the same operation return its saved result; changed intent conflicts.

## Recovery and local artifacts

Artifacts live in a sibling `<config-name>.state` directory with owner-only access.
On Windows, previews use CurrentUser DPAPI. On Unix, AES-GCM uses a key in that
restricted directory. Protect and retain this directory for recovery. Preview
output contains event content and is intended for the operator, not public logs.
Journals contain identities, action/outcome checkpoints, and durations, not seed
content, credentials, or query rows. A Started-only entry means unconfirmed.

JSON output contains schemaVersion, operationId, previewId, target, principal,
outcome, durationMs, result, and a redacted error. Text mode renders the same data
with indentation. Exit codes: 0 success/preview/reconciled; 1 confirmed failure or
partial migration; 2 syntax/configuration/validation; 3 connection/authentication/
authorization; 4 conflict; 5 confirmed cancellation; 6 unconfirmed; 7 committed
but result delivery failed. Existing account commands keep their documented codes.

After interruption, allow the bounded five-second receipt/history check or use
`operations reconcile`. It never repeats a mutation. Receipt absence establishes
no wrapped commit only while receipt history is intact; database restoration or
manual receipt deletion requires separate inspection. Retrying a partially applied
migration requires a fresh preview of its remaining migrations.

The September 9 input is `.local/run/september-9-seed-preview.json`: 21 stages,
17:00–21:00 America/Toronto (UTC−04:00), selection 18:05–18:15, demos 20:30–20:50,
and Use Liturgy off. Its missing logo and coordinates remain absent. Importing this
draft does not establish publication or readiness of unfinished live-event features.
