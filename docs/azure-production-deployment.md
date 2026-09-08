# Lowest-cost usable Azure production deployment

## Automated deployment (8 September 2026)

The repository now contains GitHub Actions CI/CD. The manual release commands
below remain useful for recovery; routine releases use the workflows instead.

- Resource group: `rg-faithtech-prod`, Canada Central, subscription
  `4a1b5113-89f9-4d27-acfe-581493385536`.
- Linux B1 app: `faithtech-uyosyof73ce5o`; .NET 10, Always On, HTTPS Only.
- Initial URL: `https://faithtech-uyosyof73ce5o.azurewebsites.net`.
- SQL server: `faithtech-sql-uyosyof73ce5o`; database `FaithTech`, Basic, 5 DTUs,
  2 GB, locally redundant backups and seven-day short-term retention.
- The purchased Namecheap domain is not bound yet. Custom DNS and independent
  design-system deployment are separate steps.

Every push to `main` enters the release queue, including documentation-only
pushes. Pull requests run verification without production access. The pipeline
installs locked dependencies, audits npm packages, builds Angular/.NET/gallery,
runs API acceptance against disposable SQL Server, runs both mock-based Angular
browser suites and gallery tests, then starts and verifies the published package
against another disposable database. No test database is shared with production.
Builds enforce the existing strict TypeScript and .NET warning settings. The
repository has no standalone lint command; workflow/script syntax is reviewed
without inventing architecture tests or suppressing existing behavior tests.

Successful main runs use GitHub's `production` environment and Azure OIDC to
migrate with a separate database identity, remove temporary runner SQL access,
upload the exact tested ZIP, and check routes, assets, authenticated SQL readiness,
source revision and sign-out. CI never creates administrator accounts during
ordinary releases. A failed migration prevents uploading the new package. A failed
smoke check marks deployment failed; it does not reverse migrations automatically.

`GET /api/admin/readiness` requires the existing administrator session. Its body
contains `ready`, assembly `revision` (including source SHA), and an opaque
process `instance`. An available but outdated database returns `ready: false`;
SQL errors during session validation use the API's existing 503 response.
Unrecognized APIs and missing static files remain 404. Participant deep links
under `/events/` and administrator links under `/admin/` load their own bundles.

Release runs and manual rollback share a concurrency group. `queue: max` retains
up to 100 pending runs; GitHub cancels further arrivals if that documented limit
is reached. Runs are not interrupted by later pushes. A recorded successful run
number prevents a delayed older release overwriting a newer successful release.
No workflow queues can guarantee processing after GitHub outages or manual cancellation.

### Bootstrap and configuration

From a clean checkout on Windows with PowerShell 7, Azure CLI, GitHub CLI, SQLCMD,
the pinned SDK and Node installed, sign into the intended Azure subscription and
GitHub repository. Bootstrap scripts run sequentially:

```powershell
pwsh -File eng/azure/provision.ps1
pwsh -File eng/azure/identity.ps1
npm --prefix frontend ci
npm --prefix frontend run build
dotnet restore backend/FaithTechTorontoAiBuildEvent.slnx --locked-mode
pwsh -File eng/scripts/package-release.ps1 -Revision (git rev-parse HEAD)
pwsh -File eng/azure/database.ps1
pwsh -File eng/azure/configure-app.ps1
```

Packaging requires a fresh `artifacts/release` directory. Existing generated
passwords are recovered from Windows DPAPI-protected files under
`%LOCALAPPDATA%/FaithTech/production-secrets`; reruns preserve them and existing
accounts. This directory also holds the password-encrypted Data Protection PFX.
These machine/user-bound recovery files are not a portable backup: export the
credentials and certificate into the operator's password manager/recovery storage
using a private session before retiring this Windows profile. Do not publish them.

The `production` environment restricts deployment to `main` without a reviewer
gate. Variables: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`,
`AZURE_RESOURCE_GROUP`, `AZURE_APP_NAME`, `AZURE_SQL_SERVER`, `AZURE_WEBAPP_URL`.
Secrets: `MIGRATION_CONNECTION_STRING`, `PRODUCTION_DIGEST_KEY`, `SMOKE_USERNAME`,
`SMOKE_PASSWORD`. The federated identity has Website Contributor on this app only
and a custom SQL firewall role on this SQL server only. Website Contributor can
manage this app's configuration; protect `main` and review deployment code accordingly.
The runtime SQL user has reader/writer roles; the migration user has `db_owner`
only in `FaithTech`, not server-administrator privileges.

Data Protection keys persist under `/home/data/faithtech-keys`, encrypted using
the uploaded certificate with a fixed application name. The certificate is loaded
from App Service's private certificate mount; runtime settings never ship in the
ZIP. Back up encrypted key XML, the PFX/password and stable digest separately from
SQL. Retain old decrypting certificates when rotating; do not replace a certificate
and delete its old private key while session keys still require it.

### Verification and rollback

The first main-triggered release is recorded in
[workflow run 34263393987](https://github.com/QuinntyneBrown/faithtech-toronto-ai-build-event/actions/runs/34263393987)
for commit `0722ecaf14d7db6513be0f800a975cfe078e786e`; the run's deployment job
and conclusion are the authoritative outcome. The preceding
[PR verification](https://github.com/QuinntyneBrown/faithtech-toronto-ai-build-event/actions/runs/34262771000)
passed 139 API tests, 90 application browser tests, three gallery tests, seven
helper/release tests, and the published-package SQL/authentication smoke test.
Live Azure session continuity passed on 8 September 2026: requesting a restart
is asynchronous, so `smoke-release.ps1 -Restart` requires a different process
instance while using the same authenticated session before it reports success.

GitHub retains release ZIPs, manifests, provisioning bundles and test reports for
90 days. Keep the current and previous two releases in access-controlled recovery
storage if they must outlive this retention. To restore a compatible package,
run **Roll back production package** from `main`, provide a successful main
deployment run ID, and confirm schema compatibility. It validates provenance and
SHA-256, deploys the retained ZIP without rebuilding or migrating, and runs the
current smoke/deployment tooling. Never use binary rollback for incompatible schema
changes; follow the point-in-time database recovery procedure below instead.

Local bootstrap secrets and actual event data must never be included in public
GitHub artifacts. Workflow logs include synthetic checks and release identifiers;
deployment checks do not create events, participants, or send messages.

Decision and price check: 7 September 2026. Region: **Canada Central**. Currency: **USD**, pay-as-you-go, before tax. This is an operator runbook and a deployment-readiness plan; it does not establish that the unfinished event application is production-ready.

## Recommendation and monthly cost

Use **one Linux App Service B1 instance with Always On**, an **Azure SQL Database Basic (5 DTU, 2 GB)** database, and an independent **Static Web Apps Free** site for the design system. This is the lowest-cost practical starting configuration among the options compared below. The 200-participant requirement must pass on the actual deployment before this size is accepted for the event.

“No cold starts” means the application is not unloaded for inactivity and the database does not auto-pause. Enable Always On explicitly: it is off by default. Deployments, platform maintenance, faults, and restarts can still interrupt service. The owner accepts a single application instance and recovery outages. [Always On configuration](https://learn.microsoft.com/en-us/azure/app-service/configure-common).

| Item | Rate / assumption | Estimated monthly cost |
| --- | --- | ---: |
| Linux App Service B1 | $0.018/hour × 730 hours; 1 core, 1.75 GB RAM, 10 GB plan storage | $13.14 |
| Azure SQL Database Basic, primary database | $0.177/day × (730 ÷ 24); provisioned DTU tier | $5.38 |
| Design system, Static Web Apps Free | Within the plan's storage and transfer quotas | $0.00 |
| Namecheap BasicDNS and Azure managed SNI certificates | Existing domain; no paid DNS zone or purchased certificate | $0.00 additional |
| **Base total** | Rounded after calculation | **$18.52** |

The compute/database rates were retrieved from the public [Azure Retail Prices API](https://learn.microsoft.com/en-us/rest/api/cost-management/retail-prices/azure-retail-prices), filtering `armRegionName=canadacentral`, `type=Consumption`, and the primary Linux/Basic meters. Exclude Windows, Cloud Services, Spot, reservations, and secondary database meters. Recheck your subscription's portal estimate before purchase; CAD billing uses Azure's billing conversion, not an assumed exchange rate. See [Linux App Service pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/linux/) and [SQL Database pricing](https://azure.microsoft.com/en-us/pricing/details/azure-sql-database/single/).

This is a base estimate, not a spending cap. Allow for outbound transfer beyond included allowances, extra retained backups, temporary restore/load-test resources, monitoring ingestion or paid alert rules, and CI usage beyond your allowance. Domain renewal is separate and already owning the domain does not remove renewal fees. Start with a **US$30-equivalent monthly budget**, alerts at 50%, 80%, and 100%, and review the forecast before the event. Budget alerts notify; they do not stop spending. Do not automatically stop the production app when a budget is crossed.

The Free design-system site contains public static assets only. It has no production event data or API and no availability SLA; this tradeoff does not put event runtime availability on that free service. Do not depend on its URL for application CSS at runtime: build the mirrored local tokens into Angular. [Static Web Apps plans](https://learn.microsoft.com/en-us/azure/static-web-apps/plans).

### Why not a Linux VM?

These Canada Central alternatives use the same 730-hour month, a Standard static IPv4 address at $0.005/hour, and Standard SSD managed disks. Prices exclude disk transactions, extra backups, transfer, and operator time.

| Alternative | Monthly calculation | Base total | Assessment |
| --- | --- | ---: | --- |
| B2ats_v2, 1 GB RAM, managed SQL Basic | $7.67 compute + $2.64 E4 32-GiB disk + $3.65 IP + $5.38 SQL | $19.34 | Already above App Service; very little RAM for the OS, proxy and .NET. |
| B2als_v2, 4 GB RAM, managed SQL Basic | $30.51 compute + $2.64 E4 disk + $3.65 IP + $5.38 SQL | $42.19 | More memory, but self-managed server operations. |
| B2als_v2 with local SQL Server Express | $30.51 compute + $5.28 E6 64-GiB disk + $3.65 IP | $39.44 | Adds off-VM backup storage/transactions and SQL maintenance. |

The 32-GiB disk comparison assumes a compatible small Linux OS image; an image requiring a larger disk raises the estimate. B-series CPU credits can run out under sustained event traffic, reducing CPU capacity. Even the 4-GB option needs load testing. [Basv2 specifications](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/general-purpose/basv2-series).

Local SQL Server needs a supported x64 Linux distribution, SQL Server Express production licensing, durable disks, memory limits leaving room for the API, and scheduled off-machine backup/restore. SQL Server on Linux requires at least 2 GB just to start; do not place it on the 1-GB VM. Never use Developer edition for production. [SQL installation requirements](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/setup?view=sql-server-ver17), [SQL editions](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022?view=sql-server-ver16).

A VM also requires OS/.NET/SQL patching, a reverse proxy and certificate renewal, firewall rules, service supervision, disk monitoring, and recovery ownership. If requirements later justify one, use a regular always-running VM, restrict SSH to operator addresses, expose only HTTP/HTTPS publicly, keep SQL private, and budget backups separately. Avoid Spot eviction, free trial dependencies, idle-unloading hosting, and auto-pausing databases for the live event. No PostgreSQL/SQLite migration is assumed: this repository uses EF Core SQL Server.

## URL and artifact layout

| Address | Responsibility |
| --- | --- |
| `https://faithtech-toronto-ai-build-event.com/` | Participant Angular application |
| `https://www.faithtech-toronto-ai-build-event.com/` | Permanent redirect to apex, preserving path and query |
| `https://admin.faithtech-toronto-ai-build-event.com/admin/` | Admin Angular application; this hostname's `/` redirects here |
| `/api/*` on the participant/admin origins | The same .NET API, with server-side authorization |
| Separate generated `*.azurestaticapps.net` URL | Independently built design-system gallery |

One App Service hosts both Angular bundles and the API; do not create an extra paid plan for each frontend. Retain the admin build's `/admin/` base URL. DNS alone does not choose a frontend or redirect a path. Application host/path routing must implement the table. Same-origin API calls preserve the current relative `/api/admin` requests and secure host-only cookies without adding CORS. A separate admin hostname is not an authorization boundary.

## Application hosting status and public-domain prerequisites

The hosting implementation below is committed in PR #5. Custom domains remain a separate rollout. See [implementation evidence](IMPLEMENTATION.md) and the acceptance authority in [L2](specs/L2.md) for product readiness.

| Area | Observed state and required release behavior |
| --- | --- |
| Build and static files | CI builds Angular before publishing .NET, packages admin under `wwwroot/admin` and client under `wwwroot`, and starts the published package against disposable SQL before deployment. |
| Host/path routing | Root, `/events/` deep links, and `/admin/` deep links are implemented and smoke-tested. Unknown APIs and missing assets return 404. Custom admin-host root and `www` redirects must be added when those names are bound. |
| Proxy HTTPS and client IP | Configured trusted proxies/networks are processed before HSTS, HTTPS checks, and authentication. Acceptance tests cover trusted and untrusted forwarding; live Azure HTTPS succeeds. Reverify client-address behavior if ingress topology changes. |
| Hosts and sessions | Only the current Azure hostname is allowed. Secure, HttpOnly, SameSite and antiforgery behavior remain enabled. Certificate-encrypted keys persist outside the ZIP with a stable application name; live session continuity across a process restart passed. Add custom names only during their rollout and back up the key recovery material. |
| Readiness and diagnostics | Administrator-only `/api/admin/readiness` checks SQL, migration currency, revision and process identity. Console logging is enabled. Event monitoring and alerts required by L2-045 remain a separate operational readiness check; no public `/health` route is configured. |
| Event completeness | Deployment verification does not establish complete event functionality or the 200-participant capacity target. Verify the intended event journeys and load criteria before inviting participants. |

Follow [Microsoft's App Service .NET hosting guidance](https://learn.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore) and [forwarded-header configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0) when changing ingress. For key persistence and encryption choices use [Data Protection configuration](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0).

## 1. Create the Azure resources

Use the Azure portal for provisioning and secret entry. Run later build/deployment commands in **PowerShell 7 on the operator's machine**, from the repository root, with Azure CLI installed and authenticated. Use one resource group, `rg-faithtech-prod`, and record the subscription, globally unique app/server names, actual default hostname, resource IDs and owner in a private operations record. Examples below use replacement markers; they are not existing resources.

1. Create the resource group in **Canada Central**. In Cost Management, create the budget described above for this group. Assign a primary and backup operator with MFA and only the permissions needed for deployment/recovery. [Budget setup](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets).
2. Create **Web App**, Publish **Code**, OS **Linux**, runtime **.NET 10**, in Canada Central. Create a **Basic B1** App Service plan, one instance. Verify Linux pricing on Review + create. Use a globally unique app name; copy its actual default hostname from Overview, including any generated suffix. Do not infer it from the app name.
3. Confirm .NET 10 is listed by `az webapp list-runtimes --os linux`. If unavailable in this subscription, resolve runtime availability before deployment; do not downgrade this `net10.0` application or silently switch to a paid container registry.
4. In the web app's Configuration / General settings, enable **Always On**, require minimum TLS **1.2 or higher**, and set the startup command to `dotnet /home/site/wwwroot/FaithTechTorontoAiBuildEvent.Api.dll`. Set **HTTPS Only** on. Retain the built-in Linux stack's port configuration; `WEBSITES_PORT` is for custom containers, which this plan does not use.
5. Create **SQL Database**, an empty `FaithTech` database on a new logical SQL server in Canada Central. Under Configure database choose **DTU → Basic**, 5 DTUs, 2 GB. Choose **locally redundant backup storage** for minimum cost and Canadian locality; accept that this provides no cross-region restore. Do not accept a portal default vCore/serverless or SQL Managed Instance deployment. [DTU tiers](https://learn.microsoft.com/en-us/azure/azure-sql/database/service-tiers-dtu?view=azuresql).
6. Use SQL authentication for this minimal setup. Store the SQL server administrator password in the operators' password manager; it is a provisioning/recovery credential, never the app's runtime credential. In SQL Networking use selected public networks, leave **Allow Azure services and resources to access this server** disabled, and temporarily allow only the operator's current public IP.
7. Add firewall entries for every address in the web app's **possible outbound IP addresses**. Record them; recheck after plan/tier changes. These are outgoing SQL client addresses, not the inbound address used for the domain. Do not add `0.0.0.0` as a shortcut. Use encrypted SQL connections. [SQL firewall rules](https://learn.microsoft.com/en-us/azure/azure-sql/database/firewall-configure?view=azuresql), [App Service IP addresses](https://learn.microsoft.com/en-us/azure/app-service/overview-inbound-outbound-ips).

The low-cost topology uses SQL's authenticated, firewall-restricted public endpoint. A private endpoint, NAT Gateway, Front Door, WAF, load balancer, paid Azure DNS zone, and container registry are not part of this bill. If organizational policy requires them, recalculate the baseline before provisioning.

## 2. Configure secrets and database identities

In App Service **Settings → Environment variables → App settings**, enter the following. App settings are separate from files published with the app. Limit who can read them. Do not paste actual secrets into Git, command-line arguments, screenshots, build logs or this guide.

| Setting | Production value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__EventDatabase` | `Server=tcp:<sql-server>.database.windows.net,1433;Database=FaithTech;User ID=faithtech_runtime;Password=<runtime-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False` |
| `Security__DigestKey` | At least 32 cryptographically random bytes, base64-encoded; store the same value securely for the operator CLI and disaster recovery. |
| `AllowedHosts` | Semicolon-separated apex, `www`, `admin`, and the actual default Azure hostname. |
| `SCM_DO_BUILD_DURING_DEPLOYMENT` | `false`; upload prebuilt artifacts. |
| `WEBSITE_RUN_FROM_PACKAGE` | `1`; application files are read-only, separate from persistent keys/data. |
| `Logging__LogLevel__Default` | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | `Warning` |

Generate the digest using a cryptographic password-manager generator or .NET's `RandomNumberGenerator.GetBytes(32)` followed by base64 encoding; generate it once and save it. Changing it without a migration/recovery plan changes the application's HMAC verifiers. Keep the key stable across deploys. Do not copy the development Windows integrated-security SQL Express connection string to Azure.

After migration in step 3, connect to **FaithTech**, using the SQL administrator in an encrypted query session, and create a contained runtime database user with a separate generated password:

```sql
CREATE USER [faithtech_runtime] WITH PASSWORD = '<generated-runtime-password>';
ALTER ROLE db_datareader ADD MEMBER [faithtech_runtime];
ALTER ROLE db_datawriter ADD MEMBER [faithtech_runtime];
```

Replace the password marker only in the private query session. The current EF CRUD implementation needs data access; the runtime does not need `db_owner` or schema-change permissions. Future stored procedures may require narrowly scoped EXECUTE grants. Run migrations and `create-admin` through the separate provisioning identity; do not grant the web application those privileges. Protect the saved runtime connection string and provisioning connection string separately. [Contained database users](https://learn.microsoft.com/en-us/sql/relational-databases/security/contained-database-users-making-your-database-portable?view=sql-server-ver17).

Persist Data Protection keys in a directory outside `/home/site/wwwroot`, such as `/home/data/faithtech-keys`, with a stable application discriminator, restricted access, and an encrypted recovery copy. Merely setting a directory in this document does not configure it: the application prerequisite must implement and verify the behavior. If explicit file persistence is used, configure key encryption rather than assuming that persistence itself encrypts the XML files. Database backups do not include these keys, the HMAC secret, or Azure configuration.

## 3. Build, migrate and deploy

Use a clean, tested release checkout and the SDK pinned by `global.json` (currently .NET 10.0.400), a Node version supported by the installed Angular version, and npm 10.9.4 from the frontend package metadata. Do not build on the small production instance. From the repository root, run each command in order and stop on any nonzero exit code:

```powershell
npm --prefix frontend ci
npm --prefix frontend run build
dotnet restore backend/src/FaithTechTorontoAiBuildEvent.Api --locked-mode
dotnet restore backend/src/FaithTechTorontoAiBuildEvent.Provisioning --locked-mode
$releaseId = Get-Date -Format 'yyyyMMdd-HHmmss'
$releaseRoot = Join-Path ([IO.Path]::GetTempPath()) "faithtech-release-$releaseId"
New-Item -ItemType Directory -Path $releaseRoot
dotnet publish backend/src/FaithTechTorontoAiBuildEvent.Api -c Release --no-restore -p:UseAppHost=false -o "$releaseRoot/api"
dotnet publish backend/src/FaithTechTorontoAiBuildEvent.Provisioning -c Release --no-restore -p:UseAppHost=false -o "$releaseRoot/provisioning"
```

Use fresh build outputs from the clean checkout; the Angular build script builds `api`, `components`, `domain`, `admin`, and `client` in dependency order. Inspect the publish directory: `FaithTechTorontoAiBuildEvent.Api.dll`, its runtime/dependency files, `wwwroot/index.html`, and `wwwroot/admin/index.html` must exist. Inspect the admin index's `/admin/` base path and referenced assets. This relies on the packaging prerequisite above being present in the release. Keep the provisioning executable **outside** the web ZIP.

Run required API acceptance tests against a disposable SQL database and the repository's Playwright acceptance suites before publishing a release; never point destructive acceptance fixtures at production. Retain the results with the Git commit and release artifact. UI mocks establish UI behavior only; they do not prove Azure, SQL or live event behavior.

For initial migration, use a private terminal without transcript logging. These PowerShell 7 prompts avoid putting secrets in shell history. Set `ASPNETCORE_ENVIRONMENT` for the web app, and `DOTNET_ENVIRONMENT` for this generic-host CLI. The provisioning connection uses the SQL administrator and the target **FaithTech** database, with encryption and certificate validation enabled.

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
$env:ConnectionStrings__EventDatabase = Read-Host 'Provisioning connection string' -MaskInput
$env:Security__DigestKey = Read-Host 'Saved production digest key' -MaskInput
try {
    dotnet "$releaseRoot/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll" migrate
    if ($LASTEXITCODE -ne 0) { throw 'Migration failed; do not deploy.' }
    dotnet "$releaseRoot/provisioning/FaithTechTorontoAiBuildEvent.Provisioning.dll" create-admin event-operator
    if ($LASTEXITCODE -ne 0) { throw 'Administrator provisioning failed.' }
} finally {
    Remove-Item Env:ConnectionStrings__EventDatabase -ErrorAction SilentlyContinue
    Remove-Item Env:Security__DigestKey -ErrorAction SilentlyContinue
    Remove-Item Env:DOTNET_ENVIRONMENT -ErrorAction SilentlyContinue
}
```

`create-admin` prompts for its password without echoing it. On subsequent releases run `migrate` only; do not rerun initial account creation. The same CLI supports `disable-admin <username>` for revocation. Now create the runtime database user from step 2 and save its connection string in App Service. Remove the temporary operator SQL firewall rule after provisioning; add it temporarily again only when needed.

Package the **contents** of the API publish directory, not the directory itself. Log into the intended Azure subscription and deploy using Microsoft Entra credentials:

```powershell
Compress-Archive -Path "$releaseRoot/api/*" -DestinationPath "$releaseRoot/web.zip"
Get-FileHash "$releaseRoot/web.zip" -Algorithm SHA256
az login
az account set --subscription '<subscription-id>'
az webapp deploy --resource-group rg-faithtech-prod --name '<actual-app-name>' --src-path "$releaseRoot/web.zip" --type zip
```

Copy the ZIP, SHA256, source commit, provisioning bundle and configuration-name inventory into access-controlled release storage. Store secret values in the password manager, not the release archive. Check startup logs, root response, the protected SQL readiness check, and an admin sign-in on the default HTTPS hostname before changing public DNS. Diagnose a failed startup before retrying; do not change Production to Development to expose an exception page. [ZIP deployment](https://learn.microsoft.com/en-us/azure/app-service/deploy-zip), [read-only package deployment](https://learn.microsoft.com/en-us/azure/app-service/deploy-run-package).

For the design system, create a separate Static Web App on **Free** with deployment source **Other**. Select Canada Central for its available regional setting; static content is globally distributed, so this is not a guarantee that public gallery assets stay exclusively in Canada. Build independently and deploy with the Azure Static Web Apps CLI installed on the operator machine:

```powershell
npm --prefix design-system ci
npm --prefix design-system run build
$env:SWA_CLI_DEPLOYMENT_TOKEN = Read-Host 'Gallery deployment token from Azure' -MaskInput
try {
    swa deploy ./design-system/dist/site --env production
    if ($LASTEXITCODE -ne 0) { throw 'Gallery deployment failed.' }
} finally {
    Remove-Item Env:SWA_CLI_DEPLOYMENT_TOKEN -ErrorAction SilentlyContinue
}
```

Obtain the token from the gallery resource's **Manage deployment token**, keep it private, and record the SWA CLI version used. Publish `dist/site`, not the mock bundle. Verify the generated HTTPS URL, gallery assets and absence of an application runtime dependency. [SWA CLI installation and deployment](https://learn.microsoft.com/en-us/azure/static-web-apps/static-web-apps-cli-deploy).

Configure the public domain using [the Namecheap guide](namecheap-domain-setup.md), then perform the checks below through the custom domains. Keep Liturgy integration disabled for September 9; no Liturgy service is required by this topology.

## 4. Acceptance before the September 9 event

Record the release SHA, region, SKU, database tier, date, browser/network conditions, raw timings and operator for each rehearsal. Use a synthetic event on the deployed stack. These checks describe required behavior; they have **not** been run by writing this guide.

| Given | When | Required result |
| --- | --- | --- |
| Valid production configuration | Start the release on a clean deployment | SQL readiness and admin sign-in succeed; completed participant journey succeeds. Missing settings fail clearly without logging values. |
| Custom domains and certificates | Visit apex, `www`, and admin over HTTP and HTTPS | HTTPS works without warnings; redirects follow the URL table; deep links refresh correctly; missing assets/API routes do not return HTML. |
| Valid/invalid sessions | Sign in, mutate with and without CSRF, sign out, then retry | Secure cookies work; unauthorized and CSRF-invalid requests are rejected; sign-out/revocation take effect. |
| Trusted Azure ingress | Send forged forwarded headers and real requests from separate clients | Scheme handling works and spoofing cannot bypass HTTPS or combine/spoof source-address rate limits. |
| Always On and provisioned SQL | Leave the app without participant traffic for at least 30 minutes, then request it | Process remains warm; SQL has not paused; no startup penalty from inactivity. |
| Saved event mutations and active sessions | Restart App Service, then reconnect | Durable data and keys survive; session behavior remains correct; application rebuilds event state from SQL/time. |
| A synthetic SQL outage | Check readiness, attempt a write, then restore access | Not-ready/failure is reported without false success; normal behavior resumes without lost acknowledged writes. |
| A synthetic event backup | Restore into an isolated database and open it through a private test deployment | Registrations, selections, messages, quiz results and raffle history are coherent; record recovery duration and recovery point. |

For **L2-043**, use the real API and durable SQL: 200 connected participants plus two administrators; two minutes warm-up; ten minutes measured with one read per participant every five seconds and one valid mutation every 30 seconds; one admin configuration change per minute; include a scheduled stage transition and raffle. Repeat for another ten minutes without restarting or cleaning up. Also test 200 first quiz answers within one second. API p95 must be at most **1,000 ms**, unexpected failures at most **1%**, every observed connected client must show updates within **two seconds**, and acknowledged mutations must not be lost. Report dropped clients and intended rejection responses separately. Use valid reusable writes, not rejected/duplicate answers as load. Full criteria: [L2-043 through L2-045](specs/L2.md#l2-043-measurable-performance-baseline).

Do not assume 5 DTUs meets this load. During rehearsal inspect SQL DTU/CPU/data IO/log IO/worker utilization and App Service CPU, memory, request latency and errors. If SQL is saturated, raise Basic to Standard S0 and rerun; if still saturated, raise to S1/S2 as needed. If application CPU or memory is constrained, raise B1 to B2, then B3 if needed. Change the measured bottleneck first, record the new monthly price and rerun all performance intervals. If failures occur without saturation, fix the application instead of hiding them with capacity. Retest SQL firewall addresses after resizing.

The production size is the **lowest configuration that passes**, not necessarily the $18.52 candidate. Provision/load-test ahead of the event, freeze a verified release before doors open, keep the proven size for the whole event, and rehearse any later downsize before using it. Do not deploy or resize during a quiz, raffle, or other critical live stage.

## 5. Operations, backup and recovery

Assign an operator to watch Azure metrics and protected readiness during the event; inspect every 15 minutes and immediately on participant-reported failures. Enable bounded application console logs (Information, seven days/100 MB where configurable); measure actual retention/storage. Preserve correlation IDs and stable audit identifiers while excluding credentials, participant emails, message bodies and profile/report text as L2-045 requires. Monitor SQL space before the Basic 2-GB limit, CPU/DTU pressure and request failures. Add notification rules for sustained errors, not-ready status and capacity pressure after checking their price in Azure Monitor; paid alert/ingestion costs are outside the base estimate. Avoid automatic verbose tracing or uncapped Application Insights ingestion.

Set Azure SQL short-term retention to **seven days** (Basic supports at most seven). Azure schedules weekly full backups, differential backups every 12 or 24 hours, and transaction-log backups approximately every ten minutes. The exact restore point depends on completed backups; this is not a zero-loss promise. Confirm point-in-time restore is available after the initial backups, before accepting event traffic. LRS backup storage cannot recover from a regional disaster. [Automated backups and retention](https://learn.microsoft.com/en-us/azure/azure-sql/database/automated-backups-overview?view=azuresql).

Rehearse before the event with synthetic data and repeat after material persistence changes. Target a recovery rehearsal within **60 minutes**, recording the observed time and recoverable-data gap rather than claiming a guaranteed RTO/RPO. Store the current and two previous release ZIPs, hashes, configuration inventory, HMAC secret, protected Data Protection keys/decryption material, and operator access instructions in access-controlled recovery storage/password management. Refresh that recovery set on each release or key change. Never put participant data in a public artifact or issue.

### Restore procedure

1. Record incident time and last known good operation. Stop production writes using a maintenance response (or stop the app if no maintenance mode exists); retain the original database for investigation.
2. In Azure SQL database Overview, select **Restore**, choose a UTC point before the incident, and restore to a **new named database** on the server. Restore cannot overwrite the original. The restored database incurs charges; wait for completion. [Point-in-time restore](https://learn.microsoft.com/en-us/azure/azure-sql/database/recovery-using-backups?view=azuresql).
3. Use a separate restricted test app to validate the restored database with a compatible release and isolated key/application identity. Verify saved synthetic event records and retry/reconciliation behavior. In a real incident restrict restored private data to authorized operators; do not send notifications or run scheduled actions during validation.
4. At cutover, set production's runtime connection string to the restored database and verify its contained user/permissions. Restore the matching HMAC/key recovery material if it was lost; deliberately invalidate sessions if key continuity cannot be recovered. Start production, verify readiness and sign-in, and reread representative saved data before reopening writes.
5. Report the recovery point and any operations after it that need reconciliation. Do not re-run raffles, quiz awards or other committed actions blindly. Retain incident evidence, then remove temporary test resources once validation and the retention decision are complete.

Seven-day PITR is a recovery window, not long-term archival. Keeping the production database running preserves the showcase; do not delete the logical SQL server after the event. If a historical event snapshot must remain restorable beyond seven days, configure and price long-term retention separately before that window expires. Keep secrets/key backups separate from SQL backup storage.

### Release rollback and routine maintenance

Before every upgrade, confirm a usable restore point and retain the old package. Review migration compatibility: prefer additive migrations so the previous application can still read the database. Basic has no deployment slots; plan a short maintenance window and deploy the prebuilt ZIP. Run migrations through the operator CLI, never automatically on every web startup.

If a release fails and the schema is backward-compatible, redeploy the preceding ZIP with `az webapp deploy` and verify readiness/authentication. If schema/data changes are incompatible, use the restore procedure and explicitly reconcile later writes; rolling back binaries alone is insufficient. Do not assume App Service's optional backup feature replaces SQL backups or is included in this tier.

Azure maintains the managed hosting OS/runtime platform; the team still updates .NET/Angular/NuGet/npm dependencies, including supported patches, and tests the resulting release. Keep MediatR pinned to **12.5.0**. Review bills, database growth, certificates and backup restore availability monthly. Keep Always On enabled between events for recap access, and remove only specifically identified temporary test resources. Stopping an App Service app does not stop billing for its allocated plan. [App Service plan billing](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans).
