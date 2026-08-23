# P5-T47 cycle-4 controlling acceptance-criteria mapping

Timestamp: 2026-08-06T18-20

## Current P5 evidence

- P5-T41 predecessor reconciliation: `remediation-cycle4-predecessor-reconciliation.2026-08-06T16-14.md`.
- P5-T42 seam: `remediation-cycle4-testability-seam.2026-08-06T18-20.md`.
- P5-T43 AppOl coverage: `remediation-cycle4-appolobjects-coverage.2026-08-06T18-20.md`; fixture 20/20.
- P5-T44 controller coverage: `remediation-cycle4-filter-controller-coverage.2026-08-06T18-20.md`; fixture 25/25.
- P5-T45 Outlook/WPF coverage: `remediation-cycle4-outlook-wpf-coverage.2026-08-06T18-20.md`; fixture 12/12.
- P5-T46 exact wrapper: `remediation-cycle4-focused-coverage-green.2026-08-06T18-20.md`; eight assemblies and 6,166/6,166 tests passed.

## P5-T25 through P5-T27 reconciliation

- P5-T25: the predecessor mapping records candidate-view ownership, original synchronous failure identity, and terminal rechecks at archive-root and compatibility-view boundaries as PASS.
- P5-T26: the predecessor mapping records commit/subscription linearization and captured-dispatcher awaited initialization/refresh behavior as PASS.
- P5-T27: the predecessor mapping records exact exception/parameter identities, getter/subscription ordering, delayed-snapshot close races, and both task-signal interleavings as PASS.

All three map to `remediation-cycle4-predecessor-reconciliation.2026-08-06T16-14.md`, whose serialized two-assembly deterministic command passed 90/90. The current P5-T44 controller fixture reran the affected disposal and subscription boundaries (25/25), so the reconciliation is not based solely on historical output.

## Acceptance criteria

| ID | Current independent evidence | Status |
| --- | --- | --- |
| AC1 | Worker-started cold-build regression evidence retained by P5-T41 and the passing exact wrapper. | PASS |
| AC2 | AppOl composition tests, Outlook refresh/cleanup tests, and dedicated-STA dispatcher tests. | PASS |
| AC3 | Current changed-production inspection and strict-yield regressions retained by P5-T41; no worker fallback is introduced by P5 changes. | PASS |
| AC4 | `SetupAndLoadFailures_ResetOwnershipForAOneServiceRetry`, `DispatchAndCandidateDisposalFailures_PreserveTerminalBehavior`, and Outlook terminal-cleanup tests. | PASS |
| AC5 | Controller fixture 25/25, including `QueuedDispatcher_DisposeBeforeEntry_DoesNotMutateView` and factory/disposal cases. | PASS |
| AC6 | Current deterministic AppOl, controller, Outlook, and WPF fixtures use only mocks/fakes or dedicated STA hosts; no live Outlook, network, temporary files, sleeps, timers, polling, or retry loops. | PASS |
| AC7 | Final Phase 6 CSharpier/analyzer/nullable/coverage pass is not yet executed. | DEFERRED TO P6 |
| AC8 | Final feature-document reconciliation is Phase 7-owned. | DEFERRED TO P7 |

The first six verified source checkboxes in `spec.md` are checked. AC7 and AC8 remain unchecked.

## Code-review requirements

| ID | Current independent evidence | Status |
| --- | --- | --- |
| CR-001 | P5-T43 worker-first composition and retry fixtures. | PASS |
| CR-002 | P5-T44 queued-disposal tests and P5-T45 terminal cleanup tests. | PASS |
| CR-003 | P5-T45 notification cleanup and observer-failure containment tests. | PASS |
| CR-004 | P5-T44 factory, close-before-initialization, and no-post-dispose-mutation tests. | PASS |
| CR-005 | Retained P5-T41 ribbon-failure regression evidence. | PASS |
| CR-006 | `InjectedDispatcher_ActionInvokeAsync_ReportsSuccessFaultAndCancellation`; P5-T45 12/12. | PASS |
| CR-007 | Final comparable coverage and quality delta is Phase 6-owned. | DEFERRED TO P6 |

## Authorized scope and capacity

- Authorized partial/project pairs only: AppOl lifecycle partial with `TaskMaster.Test.csproj`; controller coverage partial with `UtilitiesCS.Test.csproj`; Outlook traversal partial with `UtilitiesCS.Test.csproj`.
- Each authorized partial retains exactly one adjacent compile entry. The controller coverage partial is 499 lines; the pre-authorized lifecycle-races partial is 296 lines; controller lifecycle production is 498 lines.
- The P5-T42 seam is instance-local and preserves public constructor behavior. The source inspection recorded in the refreshed task artifacts identifies no reflection, real viewer, global hook, or worker-fallback addition.
- P5-T46 changed production is 890/892 (99.7758%). Controller 101/102, lifecycle 334/335, AppOl 291/292, Outlook 359/359, and WPF 12/12; all emitted target methods are at least 95%.

## Decision

P5-T47 is complete as the controlling P5 mapping: AC1-AC6 and CR-001 through CR-006 are independently PASS. AC7/CR-007 and AC8 are explicitly deferred to their downstream phase gates and remain unchecked in the authoritative source. Phase 6 has not been started by this task.
