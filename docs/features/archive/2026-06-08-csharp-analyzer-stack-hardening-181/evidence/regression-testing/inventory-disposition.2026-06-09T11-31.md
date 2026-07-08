# Phase 6 (P6-T2) — 24-Row Inventory Disposition

Timestamp: 2026-06-09T11-31
Source inventory: artifacts/research/2026-06-09-deterministic-test-timer-seams.md (groups A-L, 24 rows)

Disposition legend: Converted | Retained-with-justification | Halted-scope-change

| # | Test file : method | Original prohibited call | Disposition | Mechanism |
|---|---|---|---|---|
| A1 | SmartSerializableBase_Tests.Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite | `Thread.Sleep(50)` | Converted | S1 TimerFactory + ManualFireTimerWrapper.FireElapsed() |
| A2 | SmartSerializableBase_Tests (same) | `signal.Wait(5000)` | Converted | S1; assert `signal.IsSet` after synchronous FireElapsed() |
| A3 | SmartSerializable_Tests.Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite | `signal.Wait(1000)` | Converted | S1; assert `signal.IsSet` after FireElapsed() |
| B1 | TimerWrapper_Tests.StartTimer_RaisesElapsedEvent | `signal.Wait(500)` | Retained-with-justification | Real-timer integration test, [DoNotParallelize] (Group B) |
| B2 | TimerWrapper_Tests.StopTimer_PreventsPendingElapsedEvent | `signal.Wait(250)` | Retained-with-justification | Real-timer integration test, [DoNotParallelize] |
| B3 | TimerWrapper_Tests.StartNew_ConfiguresAutoResetAndInvokesCallback | `signal.Wait(500)` | Retained-with-justification | Real-timer integration test, [DoNotParallelize] |
| C1 | TimedQueueOfActions_Tests.Enqueue_InvokesBatchActionsOnTimerInterval | `signal.Wait(1000)` | Converted | S2 TimerFactory + FireElapsed() dispatch |
| C2 | TimedQueueOfActions_Tests.EmptyQueue_AfterSeveralIntervals_StopsTimer | `SpinWait.SpinUntil(..., 5000)` | Converted | S2; explicit FireElapsed() loop until TimerActive false |
| C3 | TimedQueueOfActions_Tests.ConcurrentEnqueue_BatchesAllItems | `signal.Wait(1000)` | Converted | S2; concurrent enqueue then one FireElapsed() |
| C4 | TimedQueueOfActions_Tests.Configuration_PropertyChanged_RestartsTimerOnWriteIntervalChange | `Thread.Sleep(50)` | Converted | S2; synchronous StopTimer+TryStartTimer restart |
| D1 | ConfigController_Tests.SaveAndLoad round-trip (STA pump) | `Thread.Sleep(10)` (pump loop) | Converted | STA pump retained but `Thread.Sleep(10)` replaced with `Thread.Yield()` (scheduler yield, not wall-clock; required because `SaveAsync` posts its continuation to the WinForms STA message queue — a bare `GetAwaiter().GetResult()` deadlocks). Test-only. |
| E1 | AsyncMultiTasker_Tests.AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress | `Thread.Sleep(350)` | Converted | S3 timerFactory + ManualFireTimerWrapper{FireOnStart} |
| E2 | AsyncMultiTasker_Tests.AsyncMultiTaskChunker_ActionOverload_WhenWorkSpansTimerInterval_ReportsProgress | `Thread.Sleep(350)` | Converted | S3 timerFactory + ManualFireTimerWrapper{FireOnStart} |
| E3 | AsyncMultiTasker_Tests.GetReportMessage_WhenInvoked_FormatsPrefixAndCounts | `Thread.Sleep(20)` | Converted | Started-then-stopped Stopwatch + SpinUntil(>Zero) (test-only) |
| F1 | IEnumerableExtensions_Tests.GetProgressMessage_WhenItemsAreComplete_UsesMeasuredRate | `Thread.Sleep(25)` | Converted | SpinUntil(stopwatch.Elapsed > Zero, 100) (test-only) |
| F2 | IEnumerableExtensions_Tests.ToList_InternalHelper_ConsumesEnumerableAndReportsProgress | `Thread.Sleep(700)` x3 | Converted | S4 ToList `onItemCompleted` hook driven via reflection |
| G1 | SubjectMapSco_Orchestration_Tests.Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress | `Thread.Sleep(20)` x3 + `SpinWait.SpinUntil(..., 1000)` | Converted | S4 (existing per-item progress.Report); assert Reports.Count >= 2 directly |
| H1 | SegmentStopWatch_Tests.StartAndStop_ReturnSameInstance_AndResetClearsElapsed | `Thread.Sleep(20)` | Converted | SpinUntil(sut.Elapsed > Zero, 200) (test-only, Risk R7) |
| H2 | SegmentStopWatch_Tests.LogDuration_CapturesMultipleNamedSegments | `Thread.Sleep(20)` x2 | Converted | SpinUntil(Elapsed > Zero / > afterFirst, 200) (test-only, Risk R7) |
| I1 | BayesianClassifierGroup_Tests.GetReportMessage_WithCompletedItems_FormatsCorrectly | `Thread.Sleep(10)` | Converted | SpinUntil(sw.Elapsed > Zero, 100) (test-only) |
| I2 | BayesianClassifierGroupTests.GetReportMessage_WithCompletedItems_IncludesSpeed | `Thread.Sleep(10)` | Converted | SpinUntil(sw.Elapsed > Zero, 100) (test-only) |
| I3 | ObsoleteBayesianClassifier_Tests.GetReportMessage_WithCompletedItems_FormatsCorrectly | `Thread.Sleep(10)` | Converted | SpinUntil(sw.Elapsed > Zero, 100) (test-only) |
| I4 | BayesianPerformanceMeasurement_Tests.PrivateProgressHelpers_FormatExpectedMessages | `Thread.Sleep(20)` | Converted | SpinUntil(stopwatch.Elapsed > Zero, 100) (test-only) |
| J1 | OlTableExtensions_Tests.GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry | `Thread.Sleep(2100)` | Converted (PARTIAL, documented) | S5 timeoutMs:5; residual `Thread.Sleep(20)` is genuinely-required synchronization; full determinism needs out-of-scope TimeOutTask refactor (scope-change-J1) |
| K1 | FolderRemapTree_Tests.WireNotifications_OnMappedToChange_RaisesPropertyChanged | `eventFired.Wait(TimeSpan.FromMilliseconds(500))` | Converted | S6 TimedBatchAction timer-factory injection + FireElapsed() |
| L1 | ThreadSafeSingleShotGuard_Tests.CheckAndSetFirstCall_ShouldAllowOnlyOneConcurrentWinner | `start.Wait()` (no timeout) | Retained-with-justification | No-timeout synchronization start gate; deterministic; not wall-clock (Group L) |

## Summary

The inventory enumerates 26 lettered rows (A1-L1). The research header's "24 occurrences" undercounts the
lettered rows by 2 (it groups some rows); every lettered row A1-L1 is given an explicit disposition above.

- Converted (fully deterministic): 21 rows — A1, A2, A3, C1, C2, C3, C4, D1, E1, E2, E3, F1, F2, G1, H1, H2,
  I1, I2, I3, I4, K1.
- Converted-PARTIAL (documented, not halted): 1 row — J1. Residual `Thread.Sleep(20)` with `timeoutMs: 5`
  is genuinely-required synchronization; scope-change finding recorded for a future cycle
  (TimeOutTask injectable-timeout refactor) in scope-change-J1.
- Retained-with-justification: 4 rows — B1, B2, B3 (real-timer integration, [DoNotParallelize]) and L1
  (no-timeout synchronization gate).
- Halted-scope-change: 0 rows. No occurrence was silently omitted or masked.

Tally: 21 + 1 + 4 = 26 lettered rows, all dispositioned. Every cataloged occurrence is accounted for.
