# Detailed-design review record

## Reviewed scope

Review date: September 9, 2026. The design set refines the current [L1](../specs/L1.md) and [L2](../specs/L2.md) specifications and the approved [Angular mock](../mocks/README.md). The [index](README.md) maps every active requirement to its primary feature.

| Artifact | Result |
|---|---|
| Feature documents | 18 self-contained designs across 7 subsystems |
| Active detailed requirements | All 34 represented, with exact source text and their L1 parent |
| C4 views | 54 sources and rendered PNGs: context, container, component per feature |
| Class views | 18 sources and rendered PNGs with typed members and relationships |
| Sequence views | 35 sources and rendered PNGs covering primary and alternate behavior |
| Total diagrams | 107 sources, 107 rendered PNGs |
| Document shape | Each feature has Overview, Description, Requirements, and Diagrams |

## Verification performed

PlantUML syntax checking completed successfully over the whole design tree. The installed software-design-document renderer completed with **107 rendered, 0 failed**, using `C:/tools/plantuml.jar`. Each image decodes and has a matching source. Feature image and relative-document links resolve. Requirement quotations match the current L2 definitions after Markdown table encoding, with unchanged identifiers and correct L1 links. C4 sources use offline standard-library includes and C4 macros.

Visual review covered the diagram set using feature contact sheets, with direct inspection of the entry sequence and focused reinspection of revised security, recovery and synchronization diagrams. The review corrected SQL procedure placement, operator-versus-browser boundaries, query/mutation handler labels, and unused sequence lifelines. The CLI procedure resides in SQL Server and is independently callable from a normal SQL client; it does not call back into the CLI.

The prose review preserves exact specification quotations, including their original use of “must”. New descriptive prose uses the skill's third-person register. Diagram class views show feature-relevant members, not complete implementation declarations. Existing types are identified as reuse candidates; proposed types and behavior do not imply completed code. HTTP and wire contracts are described by the feature text and shared protocol; diagrams supply structure and sequencing rather than an additional competing API definition.

The documentation review used temporary local rendering/contact-sheet tools outside the repository. No architecture tests, specification-parsing tests, application tests, or application implementation changes were added. No production API, SQL operation, CLI installation, SignalR load, browser accessibility journey, or restore exercise was executed as part of this documentation task.

## Decisions carried into the designs

- Fresh database initialization replaces data migration; existing databases are neither deleted nor imported by this task.
- Exactly four public screens remain, with administrator controls in the same app and manual adjacent progression.
- An event-wide version and serialized transactions favor a small, explicit concurrency model. Exact retries use actor-scoped receipts before stale-version checking.
- Email alone never recovers private ownership. A protected pre-submission receipt covers response loss; clear/expiry/deletion revokes recovery authority.
- SignalR sends content-free version/invalidation notifications; authorized coherent HTTP snapshots supply current state. Each instance observes the committed SQL change feed independently.
- SQL and CLI share one atomic passcode operation. Credential locks, revision checks, and connected-session invalidation define the revocation boundary.
- Four-digit verifier format, 250 ms server observation interval, and backup schedule are stated design choices. Runtime measurements remain necessary; the chosen interval alone does not prove the two-second target.
- The mock's published Cornerstone version is an inspected baseline. Required missing UI is delivered upstream and adopted through an actual subsequent npm release; no unreleased version number or package completeness is claimed.
- Random grouping and raffle selection use production cryptographic randomness with controllable integration fixtures. Presentation effects never select or repeat a result.

## Remaining implementation evidence

The documentation is complete; production acceptance remains future work. Required evidence includes real API/SQL/SignalR behavior, installed CLI and direct SQL replacement, published Cornerstone behavior, mocked-contract Playwright journeys, load measurements, and isolated backup restore. The operator design states how that evidence is collected without certifying the legacy implementation.

Package release version, measured Chrome/OS version, deployment resources, and observed timing results are captured when implementation verification occurs. They are evidence fields, not invented current facts. No missing product decision is represented by a placeholder.

The previous design tree was already removed from the working tree. This replacement documents the focused companion; unrelated pre-existing deletions remain outside its commits.
