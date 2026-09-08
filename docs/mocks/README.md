# FaithTech Toronto HTML design artifacts

The clickable prototype follows [the product prompt](../prompt.md). The user's Cornerstone instruction supersedes the prompt's interim Liturgy visual reference. These are design artifacts using local sample data, not a production application.

## Open the prototype

From this repository in PowerShell:

```powershell
Set-Location design-system
npm ci
npm start
```

- [Screen and dialog index](http://127.0.0.1:4317/docs/mocks/)
- [Standalone design system](http://127.0.0.1:4317/design-system/)
- [Participant entrance](http://127.0.0.1:4317/docs/mocks/?screen=access)
- [Host entrance](http://127.0.0.1:4317/docs/mocks/?screen=admin-login)
- [Review screenshots](screenshots/README.md)

Serve the files over HTTP; browsers restrict JavaScript module loading from `file://` URLs. The preview server binds to localhost only.

Participant sample: `alex@example.com` / `BUILD26`. Host sample: `host@example.com` / `host-demo`. These are visible fixture values, not real credentials. Direct screen links intentionally bypass sign-in so every artifact is reviewable.

## Review interactions

The footer's **Design review controls** expose screen/state selection, the optional companion-app scenario, session reset, and the simulated clock. The initial clock is paused at 17:42. Play from 17:59 to see the countdown open the welcome stage automatically; the accelerated setting advances one minute per second. All schedule times are sample values.

`?screen=project&item=care` opens a project. Add `&dialog=project-links` to open its editor. The index lists all supported combinations. `scenario=future` enables optional Liturgy links; `scenario=september` hides them. Changing scenarios selects illustrative event branding. Use the host settings to edit an event without changing scenarios.

Edits, messages, selections, scores, and raffle history live in session storage. New events start with their own empty lists; the event list lets you reopen previous event sessions. Reset restores the complete September sample. No network requests perform registration, sign-in, messaging, uploads, or companion-app integration.

Raffle draws cycle names and choose repeatable sample winners, excluding previous winners. Sound starts only after **Enable sound**. The celebration uses WebGPU when available, falls back to a canvas, and respects reduced motion. `&renderer=canvas` explicitly demonstrates the fallback. The direct `winner` state supplies an illustrative winner when no draw has occurred.

Project and venue content is illustrative. `example.com` links demonstrate placement and external-link behavior; they are not real project destinations. The map is a local illustration, not a live mapping service. Ongoing project management remains outside these artifacts.

## Build and verify

Run these commands from `design-system/`:

```powershell
npm run build         # Independent static gallery: dist/site/
npm run build:mocks   # Complete static prototype: dist/bundle/
npm test              # Standalone gallery, accessibility, and Cornerstone parity
npm run test:mocks    # Full browser review suite in installed Google Chrome
node scripts/capture.mjs  # Refresh screenshots while the preview server is running
```

The complete bundle is self-contained. Serve `dist/bundle/` and open `docs/mocks/`; deploy `dist/site/` separately for the design-system site. Neither requires access to Cornerstone or Liturgy. No publishing or production deployment is performed.

Browser checks verify rendering, real interactions, focusable dialogs, responsive overflow, light-theme behavior, accessibility, and failure recovery. They are post-implementation design verification, not ATDD. They do not assert folder structure, naming, or specification traceability.

See [coverage and review notes](COVERAGE.md) and [Cornerstone provenance and migration mapping](../../design-system/README.md).
