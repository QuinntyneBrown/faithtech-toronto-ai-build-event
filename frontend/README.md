# Angular applications

The `admin` and `client` applications share the `api`, `domain`, and presentational
`components` libraries. Requirements and designs live in the repository `docs`.

Run `npm ci`, then `npm run build` to build libraries before applications.
`npm start` serves administration; `npm run start:client` serves the participant app.
Run browser acceptance tests with `npm --prefix ../e2e test` after installing that
package. Tests use injected mock adapters; backend acceptance tests use SQL Server.

Design tokens originate in the independent `design-system` project. After changing
the authoritative tokens, run `npm run tokens` and commit both copies.
