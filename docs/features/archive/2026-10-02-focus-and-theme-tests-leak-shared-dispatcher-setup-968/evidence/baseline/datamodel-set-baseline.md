# Baseline: run of the four datamodel test classes (issue #968, task P0-T15)

Timestamp: 2026-10-03T02-51
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTests.` (FILTER-DATAMODEL), TASKID p0-t15 and an empty NAMES list; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-DATAMODEL" "/ResultsDirectory:coverage\test-results\968\p0-t15" "/Logger:trx;LogFileName=p0-t15.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- RESULT_COUNT: 21
- BASELINE-DATAMODEL-COUNTERS: COUNTERS total=21 executed=21 passed=21 failed=0
- BASELINE-DATAMODEL-FAILED: NONE

RESULT lines (trx-derived):

- TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing = Passed duration=00:00:00.1230152
- RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0006986
- QfcRemainingQueueAdmission_DeclaresNoScoringDelegate = Passed duration=00:00:00.0078372
- DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed duration=00:00:00.0014174
- ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay = Passed duration=00:00:00.0467492
- TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook = Passed duration=00:00:00.0004596
- Cleanup_CalledTwice_DoesNotThrow = Passed duration=00:00:00.0013325
- InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1354669
- InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0033879
- DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1490163
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate = Passed duration=00:00:00.0005784
- QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs = Passed duration=00:00:00.0015069
- WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay = Passed duration=00:00:00.0012343
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.1258511
- Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0009340
- TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.0005803
- RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0015370
- ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder = Passed duration=00:00:00.0178297
- RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0010153
- InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.1868361
- QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout = Passed duration=00:00:00.0062247

No MESSAGE lines were printed. No Timeout or Aborted outcome and no Sequence file (no BASELINE HANG).
