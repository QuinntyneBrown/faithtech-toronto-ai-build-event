# Requirements correction plan

Implement the approved five-pass review using the mocks as evidence of questions, not as authoritative product behavior. Update existing IDs in place and add IDs only for distinct missing requirements. No mock or application implementation changes.

## Decisions

- Cornerstone light styling and local `--cs-*` tokens supersede the Liturgy visual reference.
- Closed quizzes and selection windows cannot reopen through schedule edits.
- Repository/demo links persist per event/team/project pairing.
- Preserve established scope and defaults; label new editorial defaults in the review record.

## Ordered work

1. Coverage: missing journeys, configuration reuse, directory privacy, draft publication.
2. Precision: selection windows, quiz lifecycle, demo ordering, presentation timestamps.
3. Corrections: consistent Cornerstone reference and requirement provenance.
4. Lifecycle: closure, link ownership, reassignment, revocation, recovery.
5. Acceptance: observable criteria, cross-cutting consistency, complete traceability review.

Each increment must pass editorial review and `git diff --check` before commit. Documentation review is not an application test. Detailed findings and final evidence belong in `docs/specs/REVIEW.md`.
