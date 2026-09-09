# Event operator runbook

This runbook is for the September 9, 2026 FaithTech Toronto AI Build Event companion. It uses the single Angular client and .NET API; there is no separate administrator application.

## Before doors open

1. Provision a new, empty SQL Server database with encrypted transport and a certificate the operator machine trusts. Do not point this deployment at the retired multi-event database.
2. Set the API `ConnectionStrings__Companion` setting and start the API. Confirm `GET /api/health/ready` returns `200`. A `503` is not safe to ignore.
3. Install the local operator tool:

   ```powershell
   .\eng\scripts\Install-OperatorTool.ps1
   ```

4. Define a named target only on the operator workstation. For example:

   ```powershell
   $env:FAITHTECH_COMPANION_PRODUCTION_CONNECTION = 'Server=sql.example;Database=FaithTechCompanion;Encrypt=True;TrustServerCertificate=False;Integrated Security=True'
   faithtech-admin verify-connection --target production
   faithtech-admin set-admin-passcode --target production --interactive
   ```

   The connection must use `Encrypt=True` and `TrustServerCertificate=False`. Never put a passcode in an argument, script, ticket, clipboard history, or saved SQL statement. Record only the returned revision and time.

5. Open the public client in one browser and an administrator session in another. Confirm live screen changes and a roster refresh reach both browsers. Confirm the countdown page shows the live-event copy and the RTR project card.
6. Take and record a database-native full backup before doors open. Store it separately from the live database.

## During the event

- Keep a full database backup every 15 minutes and immediately after close. Record each completion or failure.
- Use the inline administrator controls on the active public screen to advance Countdown, Projects, Teams, and Raffle. A visible live-update warning means no mutation should be attempted until it reconnects and refreshes.
- Add/edit/delete participants only from Countdown. Deletion removes personal fields and invalidates that participant's browser session; it never redraws a completed raffle.
- If an administrator passcode needs rotation, run the explicit CLI command above or call `dbo.ReplaceAdminPasscode` through a parameterized, protected SQL RPC. Both paths invalidate existing administrator sessions, including when the same four digits are deliberately supplied.
- If a CLI invocation loses its result after sending the replacement, treat the outcome as unknown. Do not automatically retry; verify with the database operator and perform a deliberate new rotation if needed.

## Restore rehearsal and incident response

1. Stop participant and administrator traffic to the restore target. Preserve the live database and backup; never use a verification restore to overwrite production.
2. Restore into a distinct explicitly named database. Run database consistency checks and confirm the expected migration history.
3. Before admitting traffic, revoke administrator sessions, participant sessions, and entry receipts, then rotate the administrator passcode through the supported SQL or CLI operation.
4. Point a private verification API instance at the restored database. Check readiness, current screen, projects, teams, raffle draw IDs/times, and redacted deleted-winner labels. Verify old browsers and entry receipts cannot recover private data.
5. Record the backup timestamp, restore timestamp, counts, and known data-loss interval. A cutover to the restored target is a separate operator decision.

## After close

Take the final full backup, retain event backups for seven days on protected storage, and remove them through the deployment retention process. Never publish connection strings, passcodes, verifier data, cookies, emails, or optional participant answers in event notes or diagnostics.
