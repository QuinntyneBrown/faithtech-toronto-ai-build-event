# FaithTech event design system

A separately owned, light-only HTML/CSS implementation of the Cornerstone foundations and component treatments used by the event mock suite. It has its own package manifest, static build, gallery, and browser checks. It has no runtime dependency on the event prototype or either companion repository.

## Reference and ownership

Reference: `C:\projects\Cornerstone`, revision `554636edf74af788e83aa33a4380a05b031db16a`.

`tokens.css` is authoritative here. Cornerstone's light token names and values are preserved under `--cs-*`. Its optional dark theme is omitted. Additional tokens express event layouts and literal sizing from the reference components without changing existing values. `theme.css` reproduces the base theme; `patterns.css` supplies the corresponding native control treatments and token-based compositions.

The favicon and logo mark are local copies of Cornerstone artwork. Base theme, token, and component-derived styles retain the [Cornerstone MIT license](LICENSE.cornerstone). The venue illustration is an original sample artifact using the same palette. No assets, source paths, symlinks, packages, or build inputs are shared with Cornerstone or Liturgy at runtime.

The host navigation is composed on light surfaces, as the product prompt requires. Cornerstone's `CsShell` inverted sidebar is not used. Screen layouts, content, reviewer tools, and raffle graphics are event-specific compositions, not new overrides of shared component tokens.

## Component migration map

| Local treatment | Future Cornerstone counterpart |
|---|---|
| `.cs-button`, secondary/ghost/danger/small variants | `CsButtonDirective` / `csButton` |
| `.cs-card`, raised/interactive variants | `CardComponent` / `cs-card` |
| `.cs-field`, `.cs-input`, `.cs-select`, `.cs-textarea` | Field and input/select/textarea primitives |
| `.choice` checkbox/radio controls | Checkbox and radio components |
| `.choice-card` selected/unselected surfaces | `ChoiceCardComponent` |
| `.avatar`, `.person` | `AvatarComponent`, `PersonComponent` |
| `.cs-pill`, `.cs-alert`, `.cs-skeleton` | Pill, alert, skeleton components |
| `.cs-table-container`, `.cs-table` | Table container and table primitives |
| Native modal dialog with header/body/actions | Dialog shell, confirm dialog, unsaved-changes dialog |
| Local logo mark and wordmark composition | Logo mark and brand lockup |
| Directory, conversations, quiz, schedule, project compositions | People directory, message thread/composer, quiz, schedule list, project card |

Native controls implement the mock interactions. The last row describes future composition targets, not a claim of Angular API compatibility. No Angular library has been published or adopted in this change. At migration, install the agreed npm package, bind real application contracts in the composition layer, and replace the native treatments with the mapped components while preserving approved visuals.

## Verification and commands

```powershell
npm ci
npm start
npm test
npm run build
```

The gallery is at `http://127.0.0.1:4317/design-system/`; the independently deployable output is `dist/site/`. `npm test` runs the gallery checks without importing the mock application. `npm run test:mocks` also reviews the event artifacts.

The reference fixture under `tests/reference/` captures Cornerstone light tokens, base CSS, avatar, and choice-card styles. Browser comparisons cover rendered colour, typography, padding, margins, borders, radii, shadows, dimensions, and disabled states of representative primitives. Reference and local screenshots are included with the mock review artifacts. Page compositions also receive responsive and accessibility review.

API implementation references: [WebGPU canvas configuration](https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/configure) and [Playwright browser channels](https://playwright.dev/docs/api/class-browsertype#browser-type-launch-option-channel).
