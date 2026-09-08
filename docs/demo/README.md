# FaithTech narrated application demos

Real local application recordings with Microsoft Zira Desktop English narration,
visible captions, and separate WebVTT transcripts. Each video is one continuous
successful take. The API and CLI videos display live requests and subprocess
output in a browser terminal; they do not simulate product responses.

<!-- generated-demo-catalog:start -->
| Recording | Purpose | Measured media | Status |
|---|---|---|---|
| [admin](admin.webm) · [poster](admin-poster.png) | Host event setup, schedule and roster | 171.09 s · 1280 × 720 · 16.59 MiB · VP8/Opus | Reviewed |
| [client](client.webm) · [poster](client-poster.png) | Participant entry and sessions | 89.09 s · 1280 × 720 · 6.99 MiB · VP8/Opus | Reviewed |
| [api](api.webm) · [poster](api-poster.png) | Shared event and access service | 75.85 s · 1280 × 720 · 6.19 MiB · VP8/Opus | Reviewed |
| [provisioning](provisioning.webm) · [poster](provisioning-poster.png) | Operator schema and account administration | 78.17 s · 1280 × 720 · 6.08 MiB · VP8/Opus | Reviewed |
| [design-system](design-system.webm) · [poster](design-system-poster.png) | Independent foundations and controls for designers and builders | 99.25 s · 1280 × 720 · 8.62 MiB · VP8/Opus | Reviewed |

- **admin:** [entrypoint](../../frontend/projects/admin/src/main.ts); HTTPS /admin/; provisioned administrator; shared API + SQL. Reproduce with `node --experimental-transform-types e2e/demo/run.mjs admin`.
- **client:** [entrypoint](../../frontend/projects/client/src/main.ts); HTTPS /events/{id}; email + entry code; shared API + SQL. Reproduce with `node --experimental-transform-types e2e/demo/run.mjs client`.
- **api:** [entrypoint](../../backend/src/FaithTechTorontoAiBuildEvent.Api/Program.cs); HTTPS /api; cookies + CSRF; SQL. Reproduce with `node --experimental-transform-types e2e/demo/run.mjs api`.
- **provisioning:** [entrypoint](../../backend/src/FaithTechTorontoAiBuildEvent.Provisioning/Program.cs); CLI subprocess; Windows SQL identity; SQL. Reproduce with `node --experimental-transform-types e2e/demo/run.mjs provisioning`.
- **design-system:** [entrypoint](../../design-system/index.html); Local HTTP /design-system/; public; no API dependency. Reproduce with `node --experimental-transform-types e2e/demo/run.mjs design-system`.

### admin

[Captions and transcript](admin.vtt) · [Chapter metadata](admin-chapters.json) · [Verification](admin-verification.json)

- 00:00 — Host workspace
- 00:13 — Authenticated access
- 00:27 — Create a draft
- 00:40 — Saved venue and content
- 00:50 — Waiting message
- 01:00 — Reference confirmation
- 01:13 — Selection window
- 01:24 — Presentation window
- 01:36 — Stage content
- 01:48 — Optional connection off
- 02:00 — Issue participant access
- 02:13 — Update a participant
- 02:25 — Deactivate access
- 02:37 — Restore eligibility

Narration levels: mean -21.4 dBFS; peak -2.8 dBFS.

### client

[Captions and transcript](client.vtt) · [Chapter metadata](client-chapters.json) · [Verification](client-verification.json)

- 00:00 — Protected event entrance
- 00:11 — Real authentication, seeded publication
- 00:26 — Rejected code
- 00:39 — Join the event
- 00:52 — Session survives refresh
- 01:02 — Current implementation boundary
- 01:17 — Sign out

Narration levels: mean -21.5 dBFS; peak -2.9 dBFS.

### api

[Captions and transcript](api.vtt) · [Chapter metadata](api-chapters.json) · [Verification](api-verification.json)

- 00:00 — Shared application service
- 00:12 — Create an event
- 00:27 — Read persisted state
- 00:38 — Reject invalid input
- 00:50 — Enforce access
- 01:02 — Related workflows

Narration levels: mean -21.2 dBFS; peak -2.7 dBFS.

### provisioning

[Captions and transcript](provisioning.vtt) · [Chapter metadata](provisioning-chapters.json) · [Verification](provisioning-verification.json)

- 00:00 — Operator tooling
- 00:10 — Apply migrations
- 00:27 — Provision an operator
- 00:41 — Verify authentication
- 00:52 — Revoke access
- 01:04 — Completed operator workflow

Narration levels: mean -21.4 dBFS; peak -2.5 dBFS.

### design-system

[Captions and transcript](design-system.vtt) · [Chapter metadata](design-system-chapters.json) · [Verification](design-system-verification.json)

- 00:00 — A shared visual language
- 00:11 — Colour
- 00:23 — Typography
- 00:37 — Actions
- 00:50 — Keyboard dialog
- 01:00 — Inputs
- 01:13 — Feedback
- 01:25 — Responsive and independent

Narration levels: mean -21.2 dBFS; peak -2.4 dBFS.
<!-- generated-demo-catalog:end -->

## What these recordings establish

The application source is revision `a8324c7b57777a09ac3e578b557137eaa8c0be71`.
Recording-tool changes are committed separately on `chore/narrated-demos`.
See [implementation evidence](../IMPLEMENTATION.md) and [acceptance criteria](../specs/L2.md).

- Admin: given an authenticated host, saved event details, reference stages,
  activity windows, and roster changes survive rereads or refreshes.
- Client: given a published fixture and individual entry code, rejected access
  gives feedback, successful access survives refresh, and sign-out protects the
  event route again.
- API: given an authenticated administrator, valid creation persists, a rejected
  title adds nothing, and sign-out causes protected reads to return 401.
- Provisioning: given isolated SQL storage, migration succeeds, a newly created
  operator authenticates, and disabling it revokes its existing session.
- Gallery: given the standalone site, controls and dialog work, Escape restores
  focus, disabled actions remain disabled, and a narrower viewport has no overflow.

The participant client currently has a basic, largely unstyled shell. Countdown,
live stages, teams, messaging, quizzes, raffle, and showcase activity screens are
unfinished. Publication has no application endpoint yet: the participant fixture
is explicitly marked published in its disposable database, matching the setup in
[ParticipantAccessTests](../../backend/tests/FaithTechTorontoAiBuildEvent.AcceptanceTests/ParticipantAccessTests.cs).
This is disclosed in the narration. No planned activities are presented as working.
Liturgy remains disabled. The HTML design prototypes were excluded by request.
Shared libraries, test runners, and third-party SQL infrastructure are not separate
demo applications; no independent worker was found.

## Prerequisites and setup

Run all following commands from the repository root in PowerShell. Required:
Windows, Node 22.21 or compatible Node 22 with experimental TypeScript transforms,
npm, Playwright's matching Chromium headless shell, the .NET SDK pinned by `global.json`, SQL Express,
`sqlcmd` on PATH, an exportable localhost HTTPS certificate in CurrentUser/My,
and Windows System.Speech with Microsoft Zira Desktop installed. The SQL account
uses Windows authentication and needs permission to create and drop demo databases.

```powershell
$env:FAITHTECH_DEMO_DOTNET = "$env:LOCALAPPDATA\FaithTech\dotnet\dotnet.exe"
$env:FAITHTECH_DEMO_SQL_SERVER = '.\SQLEXPRESS'
pwsh -NoProfile -File e2e/demo/setup.ps1
```

The setup script installs all three locked npm dependency sets and the matching
Chromium headless shell, builds Angular and
the standalone gallery, restores locked .NET dependencies, builds the solution,
and runs recording-helper behavior checks. It does not install an SDK, database,
voice, or certificate. Follow the [backend setup](../../backend/README.md)
when those platform prerequisites are missing.

This run used FFmpeg 9.0.1 essentials from the Windows build provider linked by
[FFmpeg](https://ffmpeg.org/download.html). Download and verify that same archive:

```powershell
New-Item -ItemType Directory -Force artifacts/demo-tools | Out-Null
Invoke-WebRequest 'https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip' -OutFile artifacts/demo-tools/ffmpeg.zip
if ((Get-FileHash artifacts/demo-tools/ffmpeg.zip).Hash -ne 'FEC81AE03971D9DD4BE3EBE02E263BD2EC1D789483F931BDBA5F5715E65DA2E9') {
    throw 'The upstream release changed. Obtain the matching 9.0.1 archive or review and document a newer encoder.'
}
Expand-Archive -LiteralPath artifacts/demo-tools/ffmpeg.zip -DestinationPath artifacts/demo-tools -Force
$env:FFMPEG = (Resolve-Path artifacts/demo-tools/ffmpeg-9.0.1-essentials_build/bin/ffmpeg.exe).Path
$env:FFPROBE = (Resolve-Path artifacts/demo-tools/ffmpeg-9.0.1-essentials_build/bin/ffprobe.exe).Path
```

## Record, review, and deliver

```powershell
# Whole sequence: provisioning, admin, client, API, gallery.
node --experimental-transform-types e2e/demo/run.mjs

# Individual reruns prepare their own database and other prerequisites.
node --experimental-transform-types e2e/demo/run.mjs admin
node --experimental-transform-types e2e/demo/run.mjs client
node --experimental-transform-types e2e/demo/run.mjs api
node --experimental-transform-types e2e/demo/run.mjs provisioning
node --experimental-transform-types e2e/demo/run.mjs design-system
```

Each invocation prints its unique `artifacts/demo-runs/take-<timestamp>` staging
directory. Run capture and playback sequentially on this Windows ARM workstation;
installed Chrome instances closed unexpectedly during earlier attempts. Capture
and review now use Playwright's pinned headless shell; the client, provisioning,
and gallery footage was captured successfully with installed Chrome.
There are no automatic retries. An interrupted or failed take is not promoted.
The delivered gallery take used the equivalent dedicated command
`node e2e/demo/gallery-run.mjs`, which prints a `gallery-<timestamp>` staging path.

```powershell
# Substitute the staging directory printed by the recorder.
$demoRun = 'artifacts/demo-runs/take-<timestamp>'
$demoApp = 'admin'
$demoVideo = "$demoRun/$demoApp/$demoApp.webm"
node e2e/demo/review.mjs $demoVideo
# Open the video and inspect every chapter, transitions, ending, sound and captions.
# review/ contains normal-speed playback results, decoded chapter frames and audio.
node e2e/demo/approve.mjs $demoVideo 'Describe the visual and playback review performed.'
node e2e/demo/promote.mjs "$demoRun/$demoApp"
node e2e/demo/catalog.mjs
```

Review checks the encoded file at normal speed, its dimensions and duration, all
chapter positions, complete playback, audio decoding, audible signal levels and
absence of clipping. Approval and promotion are tied to the video's SHA-256.
Posters come from the encoded footage. Promotion preflights and backs up the whole
video/poster/caption/metadata set and restores earlier files if copying fails.
Catalog generation preserves text outside its marked block. A failed rerun leaves
previous reviewed deliverables intact; its `results.json` identifies the failure.

## Reproducibility and isolation

[Recording sources](../../e2e/demo/) reuse existing [page objects](../../e2e/page-objects/)
without changing ordinary mock-based acceptance tests. Production Angular adapters
call the real API. A demo-only HTTPS static host serves built browser assets and
provides the participant deep-link fallback missing from the current API host.
The proxy forwards actual API traffic; certificate trust validation is relaxed
only for these localhost recording connections. Cookies, CSRF, and authorization
remain enabled. The gallery uses its independent static files.

Each API-backed story owns a random `FaithTechDemo_<32 hex digits>` database,
random credentials, digest key, local ports, API process, and temporary certificate
export. `ConnectionStrings__EventDatabase`, `Security__DigestKey`, and the Kestrel
certificate settings are supplied only in child-process environments. Existing
development databases and listening servers are never reused. Fixtures are
synthetic; no mail, payment, AI, or external messaging adapter is used.

Startup checks readiness before capture. The encoder warms before the story;
export trims only that preparation prefix. Small capture markers identify actual
encoded chapter times and are concealed in the final encode. Narration is mixed
as Opus into continuous VP8 WebM footage; no failed scenes are spliced out.
Actions have 15-second deadlines, navigation 30 seconds, API readiness 60 seconds,
CLI commands 60 seconds, and each take a 12-minute limit.

Cleanup runs after success or failure: it stops owned processes, closes local
listeners, drops only the verified run-specific database, and removes the temporary
certificate export. `cleanup.json` records failures and any remaining database
identifier. Raw takes, speech clips, logs, failures, and backups stay in ignored
`artifacts/`. No environment settings are changed in the parent shell except the
explicit configuration commands above. No deployment or external sending occurs.

```powershell
node --test e2e/demo/media.test.mjs e2e/demo/host.test.mjs e2e/demo/delivery.test.mjs
```

These behavioral checks cover caption clocks, encoded marker timing, command
failure and timeout handling, HTTP ranges and deep links, and rollback of an
interrupted promotion. They do not assert repository structure or naming.

This run passed all six helper checks, both existing `EventDraftTests` API
acceptance cases, the Angular workspace build, the .NET solution build (zero
warnings or errors), and the standalone gallery build. Every delivered workflow
also passed its real-service recording assertions. The API validation case can
be repeated with:

```powershell
& $env:FAITHTECH_DEMO_DOTNET test backend/FaithTechTorontoAiBuildEvent.slnx --no-build --filter 'FullyQualifiedName~EventDraftTests'
```
