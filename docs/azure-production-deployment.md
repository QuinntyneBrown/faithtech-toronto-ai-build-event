# Lowest-cost usable Azure production deployment

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
| B2als_v2, 4 GB RAM, managed SQL Basic | $30.51 compute + $2.64 E4 disk + $3.65 IP + $5.38 SQL | $42.18 | More memory, but self-managed server operations. |
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

## Application prerequisites: complete before public deployment

The workspace inspected on the date above includes ongoing static-hosting edits. Recheck the chosen release commit, not just a local working tree. See [implementation evidence](IMPLEMENTATION.md) and the acceptance authority in [L2](specs/L2.md).

| Area | Observed state and required release behavior |
| --- | --- |
| Build and static files | Ongoing edits package `frontend/dist/admin/browser` into `wwwroot/admin` and `frontend/dist/client/browser` into `wwwroot`. Build Angular before publishing .NET. Confirm these edits are committed in the release. |
| Host/path routing | Admin `/admin/` fallback exists in the inspected edits. Add/verify apex index and client deep-link fallback, admin-host root redirect, and `www` redirect. Never return SPA HTML for unknown `/api` routes or missing assets. Keep a successful root response on the default Azure hostname for Always On. |
| Proxy HTTPS and client IP | The inspected API rejects non-HTTPS requests and has no explicit forwarded-header middleware. Configure trusted App Service proxy addresses/networks and forwarded scheme/address processing **before** HSTS, HTTPS checks, and authentication. Verify real client IPs for rate limits and reject spoofed forwarded chains; do not blindly clear all proxy trust restrictions. |
| Hosts and sessions | Limit accepted hosts to the actual Azure default hostname plus the three custom names. Keep Secure, HttpOnly, SameSite and antiforgery behavior. Use durable Data Protection keys outside the deployed ZIP with a stable application name; verify keys persist on this Linux hosting setup and protect/back them up. |
| Readiness and diagnostics | No readiness route was present in the inspected entry point. Implement a protected operator readiness check that exercises SQL and reports failure without secrets, plus request/error metrics and correlation logs required by L2-045. Do not configure a fictional existing `/health` route. |
| Event completeness | The client route table was empty; substantial participant/event journeys remain in the implementation queue. Hosting admin sign-in does not satisfy the live-event requirements. Complete and verify the intended event journeys before inviting participants. |

Follow [Microsoft's App Service .NET hosting guidance](https://learn.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore) and [forwarded-header configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0). For key persistence and encryption choices use [Data Protection configuration](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0). These are prerequisites, not changes made by this documentation task.
