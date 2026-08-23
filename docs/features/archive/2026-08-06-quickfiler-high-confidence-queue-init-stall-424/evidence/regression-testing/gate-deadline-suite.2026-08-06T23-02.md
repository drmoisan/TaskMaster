# [P1-T9] Gate Deadline Suite — Full Run

- **Issue:** #424
- **Task:** [P1-T9]
- **Scope:** both files of the partial class — `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` + `.Part2.cs`

Timestamp: 2026-08-06T23-02

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 18
     Passed: 18
```

Both files are the same partial class, so the single `FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests` filter covers the base file and `Part2.cs`.

## Pre-existing tests — all 8 pass unmodified in behavior

| Test | Pins | Result |
|---|---|---|
| `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext` | dequeue-time selection + per-candidate `Probability debug` line | Passed |
| `DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet` | accept/reject/backfill, master-queue order | Passed |
| `DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults` | partial results on exhaustion | Passed |
| `DequeueAsync_ThresholdComparisonIsInclusive` | inclusive threshold at the cutoff | Passed |
| `DequeueAsync_PropagatesCancellationBeforeTakingSourceItem` | cancellation before first take | Passed |
| `DequeueAsync_BelowThresholdItemsAreDiscarded` | discard below threshold | Passed |
| `DequeueAsync_WhenSourceInitiallyEmpty_WaitsWithTimeProviderBeforeRetry` | `FakeTimeProvider` empty-poll wait | Passed |
| `DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives` | `sourceActive` continue-polling | Passed |

Not one of these test methods was edited. The only change to the base file was adding the `partial` keyword to the class declaration and extending the reflection-based `CreateGate` helper with the new optional `firstBatchDeadline` parameter (the helper probes for the deadline-aware constructor and falls back to the older shapes).

## New tests added in Phase 1 — all 10 pass

| Test | Task | Covers |
|---|---|---|
| `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` | P1-T1 | AC 1, AC 11 — bounded scan (fail-before 51 takes / pass-after <= 13) |
| `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound` | P1-T5(a) | AC 2 — empty list at the bound, 17 unscanned remain queued |
| `DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem` | P1-T5(b) | AC 4 — in-flight score completes; its acceptance is included |
| `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates` | P1-T5(c) | AC 1 — no take after return; remainder still takeable |
| `DequeueAsync_QuantitySatisfiedBeforeExpiry_ReturnsUnchangedBatchAndOrder` | P1-T6(a) | AC 3 — fast path unchanged in content and order, no added take, no delay |
| `DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior` | P1-T6(b) | AC 3 — `Timeout.InfiniteTimeSpan` restores the pre-change unbounded scan (51 takes) |
| `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause` | P1-T6(c) | guard clause — `TimeSpan.Zero` and `-5 s` rejected with `ArgumentOutOfRangeException` |
| `DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging` | P1-T7 | exactly one expiry line with `Accepted=0 Scanned=3`; per-candidate logging unchanged |
| `DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled` | P1-T8(a) | AC 8 — cancellation during the empty-queue poll, both deadline configurations |
| `DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled` | P1-T8(b) | AC 8 — post-score cancellation check, both deadline configurations |

The two cancellation tests each assert under **both** deadline configurations (enabled default and `Timeout.InfiniteTimeSpan`), giving four passing cancellation results in total.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest run above | 0 (18/18) |

One CS0104 compile error was encountered and fixed during Phase 1: a bare `Action` in `Part2.cs` is ambiguous between `Microsoft.Office.Interop.Outlook.Action` and `System.Action` in this namespace; the declaration was qualified to `System.Action` and the loop restarted from formatting.

## File size

`QfcStreamingDequeueConfidenceGateTests.Part2.cs` measured **455 lines** after formatting — within the 500-line limit with 45 lines of headroom. Phase 2 tests are added to this file next; `[P5-T2]` carries the pre-decided relocation of the Phase 2 progress-callback tests into `Part3.cs` should the combined file exceed 500.
