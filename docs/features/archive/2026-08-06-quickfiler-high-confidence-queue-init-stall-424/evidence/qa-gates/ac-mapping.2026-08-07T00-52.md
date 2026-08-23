# [P6-T6] Acceptance-Criteria Traceability

- **Issue:** #424
- **Task:** [P6-T6]
- **AC source:** `spec.md` `## Acceptance Criteria` (work mode `full-bug`, 13 items)

Timestamp: 2026-08-07T00-52

Command: cross-reference of `spec.md` acceptance criteria against plan tasks, test method names verified present in the codebase, and evidence artifacts verified present on disk
EXIT_CODE: 0
Output Summary: **All 13 acceptance criteria mapped.** Every referenced test method exists and passes; every referenced evidence artifact exists on disk.

---

| # | Acceptance criterion (abbreviated) | Plan tasks | Verifying tests | Evidence |
|---|---|---|---|---|
| 1 | Gate enforces the `TimeProvider` first-batch deadline; returns accepted-so-far; no `tryTakeNext` after expiry; unscanned remain takeable | P1-T1..P1-T5, P1-T9 | `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline`, `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates` | `deadline-fail-before.2026-08-06T22-41.md`, `deadline-pass-after.2026-08-06T22-48.md`, `gate-deadline-suite.2026-08-06T23-02.md` |
| 2 | Zero accepted before deadline -> empty list at the bound; form path proceeds; iteration still initiated | P1-T5(a), P4-T8 | `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound`, `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` | `gate-deadline-suite.2026-08-06T23-02.md`, `wiring-suite.2026-08-06T23-50.md` |
| 3 | Fast path identical in content and order; disabled sentinel reproduces current behavior | P1-T6(a)(b) | `DequeueAsync_QuantitySatisfiedBeforeExpiry_ReturnsUnchangedBatchAndOrder`, `DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior`, `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause` | `gate-deadline-suite.2026-08-06T23-02.md` |
| 4 | In-flight score at expiry completes; its acceptance is included | P1-T5(b) | `DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem` | `gate-deadline-suite.2026-08-06T23-02.md` |
| 5 | Progress callback once per scanned candidate, monotonic, none after return, throw propagates | P2-T2..P2-T5 | `DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically`, `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`, `DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable` | `fail-before-exception.progress-callback.2026-08-06T23-06.md`, `gate-progress-suite.2026-08-06T23-14.md` |
| 6 | `RunAsync` maps progress into [0, 30], monotonic, between the two label reports | P4-T2, P4-T3, P4-T5, P4-T8 | `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` + 12 `QfcScanProgressBandMapperTests` methods | `fail-before-exception.runasync-wiring.2026-08-06T23-37.md`, `wiring-suite.2026-08-06T23-50.md`, `coverage-delta.2026-08-07T00-48.md` (mapper 100%) |
| 7 | Datamodel-owned `volatile bool`, true across the first-await, cleared in a `finally`, consumed by `sourceActive` | P3-T1..P3-T8 | `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`, `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse`, `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` | `liveness-fail-before.2026-08-06T23-20.md`, `liveness-pass-after.2026-08-06T23-26.md`, `liveness-suite.2026-08-06T23-34.md` |
| 8 | Cancellation preserved during scanning and during the empty-queue wait; existing cancellation tests pass unchanged | P1-T3, P1-T8, P1-T9, P5-T1 | `DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled`, `DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled`, `DequeueAsync_PropagatesCancellationBeforeTakingSourceItem` (pre-existing, unmodified) | `gate-deadline-suite.2026-08-06T23-02.md`, `pinned-suites.2026-08-07T00-12.md` |
| 9 | Selection contract unchanged; inclusive-threshold/discard pins pass; admission tests unmodified | P1-T9, P3-T5, P3-T8, P5-T1 | `DequeueAsync_ThresholdComparisonIsInclusive`, `DequeueAsync_BelowThresholdItemsAreDiscarded`, `DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet` (order), 4 admission tests (byte-identical) | `gate-deadline-suite.2026-08-06T23-02.md`, `liveness-suite.2026-08-06T23-34.md` (regions 49-100 and 139-217 proven BYTE-IDENTICAL) |
| 10 | Deadline is an internal constant + internal seam; no settings/Designer/ribbon changes | P1-T3, P5-T3 | `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause`; `internal static readonly TimeSpan DefaultFirstBatchDeadline` at `QfcStreamingDequeueConfidenceGate.cs:22` | `scope-guard.2026-08-07T00-25.md` (0 changed files matching `QfSettings`, `IAppQuickFilerSettings`, `Settings.Designer.cs`, `TaskMaster/Ribbon/`) |
| 11 | A deadline regression test evidenced failing before and passing after | P1-T1, P1-T2, P1-T4 | `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` | `deadline-fail-before.2026-08-06T22-41.md` (EXIT 1, 51 takes), `deadline-pass-after.2026-08-06T22-48.md` (EXIT 0) |
| 12 | Genuinely unchanged pins byte-unmodified and passing; `QfcHomeControllerIssue218Tests.cs` passing with diff limited to the four overload-shape hunks | P4-T7, P5-T1 | 8 + 3 + 10 + 41 pinned tests; 2 `QfcHomeControllerIssue218Tests` tests | `pinned-suites.2026-08-07T00-12.md` (64/64; 4 hunks confirmed) |
| 13 | Full toolchain passes in order; change-scoped coverage gates met; repo-wide figures recorded | P6-T1..P6-T5 | full suite 6272/6272 | `final-qc-format.2026-08-07T00-28.md`, `final-qc-analyzers.2026-08-07T00-30.md`, `final-qc-nullable.2026-08-07T00-31.md`, `final-qc-tests.2026-08-07T00-45.md`, `coverage-delta.2026-08-07T00-48.md` |

---

## Verification of references

**All 20 referenced evidence artifacts exist on disk** under the canonical `evidence/{baseline,regression-testing,qa-gates}/` paths (9 baseline, 10 regression-testing, 10 qa-gates, plus two `.cobertura.xml` reports). No `artifacts/`-rooted evidence path was used anywhere.

**All referenced test methods exist and pass.** `QuickFiler.Test` full-assembly run: `Total tests: 846  Passed: 846`, EXIT_CODE 0.

Note: `pinned-suites.2026-08-06T23-58.md` is the retained fail-closed record of the first `[P5-T1]` attempt, which surfaced the `QfcHomeControllerIssue218Tests.cs` misclassification. It is superseded by `pinned-suites.2026-08-07T00-12.md` and is kept for audit continuity.
