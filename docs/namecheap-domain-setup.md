# Connect faithtech-toronto-ai-build-event.com to Azure

Verified against provider documentation on 7 September 2026. This guide accompanies the [Azure production deployment runbook](azure-production-deployment.md). It describes configuration you perform in your accounts; no DNS records or Azure resources have been changed by creating this document.

Use the existing Namecheap domain with **one Linux App Service B1 web app**, three hostname bindings, and free Azure managed certificates. No domain transfer, paid Namecheap hosting, paid certificate, PremiumDNS upgrade, or Azure DNS zone is needed for this setup.

## 1. Know which address serves what

| Public address | Expected behavior |
| --- | --- |
| `https://faithtech-toronto-ai-build-event.com/` | Participant site |
| `https://www.faithtech-toronto-ai-build-event.com/` | Redirect to the apex, retaining the path and query string |
| `https://admin.faithtech-toronto-ai-build-event.com/` | Redirect to `/admin/` on the admin hostname |
| `https://admin.faithtech-toronto-ai-build-event.com/admin/` | Admin sign-in/application |

The admin bundle currently uses an `/admin/` base URL. The application, not DNS, must implement the redirects and frontend routing described in the deployment guide. API requests use `/api` on the same hostname as their frontend; do not add a separate `api` hostname or cross-origin URL for this plan. The independent design-system site uses its generated Azure Static Web Apps HTTPS address, so it needs no domain record here.

Before directing participants here, finish the deployment prerequisites, publish a tested release, and verify it through its default Azure hostname. Admin security remains server-side authentication and authorization; knowing the admin URL must never grant access.

## 2. Check Namecheap's nameservers first

1. Sign into Namecheap, open **Domain List → Manage** beside `faithtech-toronto-ai-build-event.com`, and inspect **Nameservers** on the Domain tab.
2. If it says **Namecheap BasicDNS**, keep it. Open **Advanced DNS → Host Records**. BasicDNS is the default assumed by the instructions below.
3. If it uses third-party nameservers, edit records at that provider instead. If using Namecheap hosting nameservers, its cPanel manages the zone. An unavailable Host Records editor usually indicates the authoritative zone is elsewhere. [Namecheap host-record management](https://www.namecheap.com/support/knowledgebase/article.aspx/434/2237/how-do-i-set-up-host-records-for-a-domain/).

Do not switch a working domain's nameservers just to reveal an editor. Save the current records first, particularly MX, SPF, DKIM, DMARC and other TXT verifications. If you deliberately move DNS hosting, reproduce the entire needed zone and coordinate any DNSSEC/DS change before changing delegation. For a newly purchased, unused domain with only parking records, selecting BasicDNS is sufficient after checking there is no existing email/site configuration to preserve.

## 3. Copy the three real values from Azure

In Azure portal open the **web app**, not its App Service plan, SQL server, or Static Web App. In **Settings → Custom domains → Add custom domain**, select **All other domain services**. Enter the apex domain and inspect Azure's required DNS records. Also copy the default hostname from Overview. [Azure custom-domain setup](https://learn.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-custom-domain).

| Placeholder used below | Copy this exact value |
| --- | --- |
| `<APP-INBOUND-IPV4>` | IPv4 address Azure shows for the apex A record in Add custom domain. This is **not** an outbound IP or SQL address. |
| `<APP-DEFAULT-HOSTNAME>` | Actual default hostname from the web app's Overview, including any generated suffix. Copy the hostname only, without `https://` or a trailing path. |
| `<APP-VERIFICATION-ID>` | Custom domain verification ID shown by Azure for this web app. Verify the value shown when adding each hostname. |

These markers are instructions to substitute Azure's values, not literal DNS values. Do not guess an app hostname from its resource name. Keep Azure's dialog open while configuring Namecheap.

## 4. Enter these six DNS records

In Namecheap **Advanced DNS → Host Records**, use **Add New Record** (or edit the matching existing record). Select **Automatic** TTL for this initial setup. Namecheap currently documents a default 30-minute TTL for these records; previously cached answers can persist for their old TTL. [Namecheap TTL and Host fields](https://www.namecheap.com/support/knowledgebase/article.aspx/434/2237/how-do-i-set-up-host-records-for-a-domain/).

| Type | Host, exactly as entered at Namecheap | Value / Target | TTL |
| --- | --- | --- | --- |
| A Record | `@` | `<APP-INBOUND-IPV4>` | Automatic |
| TXT Record | `asuid` | `<APP-VERIFICATION-ID>` | Automatic |
| CNAME Record | `www` | `<APP-DEFAULT-HOSTNAME>` | Automatic |
| TXT Record | `asuid.www` | `<APP-VERIFICATION-ID>` | Automatic |
| CNAME Record | `admin` | `<APP-DEFAULT-HOSTNAME>` | Automatic |
| TXT Record | `asuid.admin` | `<APP-VERIFICATION-ID>` | Automatic |

Save every row. `@` represents the apex; Host fields such as `asuid.admin` are relative to your domain. Do not enter the full domain a second time in that field. Keep the verification TXT records after validation to protect the hostname mapping. [Azure record requirements](https://learn.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-custom-domain).

Use an **A record at `@`**, not an apex CNAME. Preserve unrelated MX/TXT records there. Remove only conflicting apex parking A/ALIAS/URL Redirect records and obsolete IPv6 AAAA records pointing elsewhere. This plan does not publish IPv6; an old AAAA record can direct some visitors to the wrong server. [Namecheap A-record instructions](https://www.namecheap.com/support/knowledgebase/article.aspx/319/2237/how-can-i-set-up-an-a-address-record-for-my-domain/).

At `www` and `admin`, replace conflicting old A/AAAA/CNAME or URL Redirect records; a CNAME must not coexist with other record types at the same owner name. The TXT owners `asuid.www` and `asuid.admin` are different names, so they do not conflict. Both CNAME targets must point **directly to Azure's default web app hostname**, not to `@` or to another custom alias. Do not enable a Namecheap URL Frame or CDN/SSL proxy for these records. [Namecheap CNAME instructions](https://www.namecheap.com/support/knowledgebase/article.aspx/9646/2237/how-to-create-a-cname-record-for-your-domain/).

## 5. Validate all hostnames and enable HTTPS in Azure

1. Return to **Custom domains → Add custom domain** for the apex. Select **Validate**, confirm the DNS checks, then **Add**. Repeat for the full `www.faithtech-toronto-ai-build-event.com` and `admin.faithtech-toronto-ai-build-event.com` names. All three bind to the same web app. A resolving DNS record alone is not an Azure hostname binding.
2. Choose **App Service Managed Certificate** and **SNI SSL** for each hostname. If certificates were deferred, open **Certificates → Managed certificates → Add certificate**, select each hostname, validate and add. Managed certificates are free, renew automatically while their prerequisites remain satisfied, and require a separate certificate for each of these names. [Managed certificates](https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate).
3. In Custom domains verify each certificate is bound and marked **Secured**. If it says **No binding**, add the TLS binding using that hostname's certificate and SNI SSL. Creating a certificate alone does not finish its binding. Set **HTTPS Only** on and minimum TLS 1.2 or higher. [TLS binding instructions](https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-bindings).
4. Check restrictive CAA records if issuance fails. Microsoft's current documentation identifies DigiCert and, where needed, `0 issue digicert.com`. Preserve other needed issuers; use Namecheap's CAA fields to allow the documented issuer instead of deleting unrelated certificate policy. Keep the apex A and direct subdomain CNAME mappings for renewal, and recheck current Azure requirements if the issuer changes. [Certificate prerequisites](https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate).

Both the original `www` URL and its redirect destination need valid certificates: TLS is checked before an HTTPS redirect can be read. The application must perform the `www` redirect and the admin-root redirect; neither CNAME records nor an Azure certificate creates those redirects. Do not use a paid IP-based SSL binding for this configuration.
