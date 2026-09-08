# Continuous Azure deployment acceptance

These operational criteria extend L2-038, L2-041 and L2-045; they do not claim
completion of the live-event performance or product requirements.

1. Given built participant and admin applications, when opening their root or
   deep links on the API origin, then the corresponding shell loads; missing
   APIs and assets return 404 without SPA HTML.
2. Given a trusted ingress proxy, when HTTPS is forwarded, then secure requests
   work and client addresses are preserved; untrusted forwarding is ignored.
3. Given an administrator session, when readiness is requested, then SQL access,
   migration currency and release identity are checked without leaking secrets;
   unauthenticated requests are denied and unavailable SQL reports not-ready.
4. Given encrypted persistent session keys, when the process restarts with the
   same configuration, then existing cookies remain usable.
5. Given a push to main, when all acceptance suites pass, then its immutable
   package is migrated, deployed and smoke-checked automatically. Failed checks
   or migrations prevent deployment, and temporary SQL access is removed.
6. Given concurrent main pushes, when releases execute, then they queue without
   interrupting active migrations and never overwrite a newer release with an
   older one. Given a failed release, an operator can restore a retained compatible
   artifact without rebuilding or reversing database migrations.
7. Given a pull request, when CI runs, then application browser tests execute in
   two shards concurrently with backend tests and release packaging. Every
   existing acceptance suite and dependency audit remains required; the `verify`
   check fails if any release or browser job fails, is canceled, or is skipped.
8. Given unchanged NuGet lock files, when a subsequent run restores dependencies,
   then it reuses the package cache while still enforcing locked restore.

The `release` job builds production applications, tests the API, runs recording
and release-helper tests, and smoke-tests the actual published package against
disposable SQL. The browser jobs build their own shared Angular libraries and
run all application cases across two shards; the first also checks the design
system. Reports are retained separately per job. Deployment waits for the
aggregate `verify` check, and main pushes retain serialized production execution.
