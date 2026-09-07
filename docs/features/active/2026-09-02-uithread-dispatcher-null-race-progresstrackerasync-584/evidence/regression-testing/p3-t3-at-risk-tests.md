# P3-T3 — At-risk UtilitiesCS.Test classes after the fix

Timestamp: 2026-09-03T08-35

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t3 /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests|FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests|FullyQualifiedName~WpfDispatcherYieldTests|FullyQualifiedName~OutlookFolderTreeServiceConcurrencyTests"
```

EXIT_CODE: 0

## Output Summary

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 41
     Passed: 41
 Total time: 2.2356 Seconds
```

- **Total tests: 41** (console summary block)
- **Passed: 41** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p3-t3/`)

TRX `<Counters .../>`: `total="41" executed="41" passed="41" failed="0"`.

This task records no `Skipped` figure, so the `total` minus `executed` derivation does not apply and
the `notExecuted` attribute was not read.

TRX SELECTED: most recently modified .trx in TestResults/p3-t3/
Last-modified timestamp of the selected file: `2026-09-03 08:35:42.461615800 -0400`.
That directory held two `.trx` files (an earlier one dated 2026-09-02 from a prior preparation-cycle
run, and the one this task produced). The selected file's own name is not recorded and the run's
`Results File:` console line is not quoted.

### Every executed test and its outcome (console-observed)

```text
  Passed YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield [62 ms]
  Passed GetSnapshotAsync_ConcurrentInitialRequests_CoalesceOntoOneBuild [62 ms]
  Passed YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback [47 ms]
  Passed YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher [2 ms]
  Passed YieldAsync_WithoutDispatcher_RemainsStrict [1 ms]
  Passed GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher [96 ms]
  Passed Increment_ShouldUpdateProgressAndForwardScaledValueAndJobName [9 ms]
  Passed Report_ShouldClampValuesAboveOneHundred [< 1 ms]
  Passed Report_ShouldThrowForNegativeValues [1 ms]
  Passed SpawnChild_ShouldUseRemainingAllocationFromCurrentProgress [< 1 ms]
  Passed Increment_ShouldAccumulateProgressValues [< 1 ms]
  Passed Increment_ShouldClampAt100 [< 1 ms]
  Passed Report_WithTupleOverload_ShouldSetValueAndJobName [< 1 ms]
  Passed Report_DoubleOverload_ShouldThrowForNegative [< 1 ms]
  Passed Report_DoubleOverload_ShouldClampAbove100 [< 1 ms]
  Passed SpawnChild_WithAllocation_ShouldCreateChildWithSpecifiedAllocation [< 1 ms]
  Passed SpawnChild_WithDoubleAllocation_ShouldRoundAndCreateChild [< 1 ms]
  Passed Report_WithDoubleAndJobName_ShouldClampAt100 [< 1 ms]
  Passed Report_WithDoubleAndJobName_ShouldThrowForNegative [< 1 ms]
  Passed Constructor_WithParent_ShouldInheritJobName [< 1 ms]
  Passed Report_WithJobName_RootReportsToStubPane [< 1 ms]
  Passed SpawnChild_FromProgressedParent_MapsChildProgressIntoParentRange [< 1 ms]
  Passed Report_At100Percent_SetsProgressToMaxAndForwardsToParent [< 1 ms]
  Passed Report_WithValueAndJobName_UpdatesProgressAndForwardsMessage [< 1 ms]
  Passed Report_ViaChild_ShiftsParentProgressByAllocatedRange [< 1 ms]
  Passed Report_At100Percent_WhenRootTracker_ClosesProgressViewer [158 ms]
  Passed Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi [8 ms]
  Passed ReportAsync_WithNegativeValue_ThrowsArgumentOutOfRangeException [1 ms]
  Passed ReportAsync_WithValueOver100_ClampsTo100 [< 1 ms]
  Passed ReportAsync_At100Percent_WhenRootTracker_ClosesProgressViewer [3 ms]
  Passed Constructor_WithTokenSource_ShouldSetDefaultProperties [< 1 ms]
  Passed Allocation_ShouldBeSettable [< 1 ms]
  Passed StartingAt_ShouldBeSettable [< 1 ms]
  Passed JobName_ShouldBeSettable [< 1 ms]
  Passed Constructor_WithScreenOverload_HasSameDefaultsAsBasicConstructor [< 1 ms]
  Passed Tracker_SetAllocationAndJobName_BothPropertiesReflectUpdatedValues [< 1 ms]
  Passed ChildTracker_ConfiguredWithSubRange_AllocationAndStartingAtArePreserved [< 1 ms]
  Passed InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker [5 ms]
  Passed AddEntry_UseUiThreadFalse_ActionRunsExactlyOnce [12 ms]
  Passed AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException [1 ms]
  Passed OnApplicationIdle_FirstItemThrows_SubsequentItemStillExecutes [< 1 ms]
```

41 lines, all `Passed`. No `Failed` and no `Skipped` per-test line appears.

## Acceptance

1. `Total tests` is **41**, greater than zero — the filter matched. Satisfied.
2. All five named tests are present in the executed set and all five passed:
   - `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` — Passed
   - `YieldAsync_WithoutDispatcher_RemainsStrict` — Passed
   - `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` — Passed
   - `GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher` — Passed
   - `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` — Passed (the
     `[STATestMethod]` in `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` that writes
     `UiThread._dispatcher`)

   Satisfied.
3. The failing set is **empty**, so it is trivially a subset of the empty `BASELINE_FAILURE_SET`
   recorded in P0-T10. No `PRE-EXISTING FAILURE:` record is required. Satisfied.

The two tests that exercise the "dispatcher unavailable" path —
`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` and
`YieldAsync_WithoutDispatcher_RemainsStrict` — both still pass against the now-throwing accessor,
confirming their existing handling absorbs an `InvalidOperationException` from
`UiThread.Dispatcher` as it previously absorbed a `NullReferenceException`. No assertion in any of
these files was modified by this plan.
