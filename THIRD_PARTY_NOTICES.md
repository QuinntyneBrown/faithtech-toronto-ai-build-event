# Third-party notices

This repository does not currently include a root license for its original code.
Third-party materials retain their own licenses; preserve their notices.
This guide is not a complete inventory of transitive dependencies.

## Cornerstone materials

The design system includes Cornerstone-derived tokens, theme and component styles,
and copied favicon and logo artwork from revision
`554636edf74af788e83aa33a4380a05b031db16a`. The retained
[Cornerstone MIT license](design-system/LICENSE.cornerstone) and
[design-system guide](design-system/README.md) document attribution and ownership.
The frontend mirrors design tokens from this independent design system.

## Packages and runtimes

Exact versions are recorded in backend `packages.lock.json` files and the
[frontend](frontend/package-lock.json), [browser acceptance](e2e/package-lock.json),
and [design-system](design-system/package-lock.json) lockfiles. Refer to each
installed package's license and notices before redistribution.
MediatR must remain pinned to **12.5.0**, as required by [AGENTS.md](AGENTS.md).
SQL Server, browser software, SDKs, and hosted services have their own terms.

When adding external material, record its source, version, license, and required
attribution; retain license files beside vendored assets and update relevant lockfiles.
