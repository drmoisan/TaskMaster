# Datamodel test set after the consolidation (issue #968, task P4-T11)

Timestamp: 2026-10-03T03-08
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-DATAMODEL (`FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTests.`), TASKID p4-t11 and an empty NAMES list; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-DATAMODEL" "/ResultsDirectory:coverage\test-results\968\p4-t11" "/Logger:trx;LogFileName=p4-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=21 executed=21 passed=21 failed=0
- RESULT_COUNT: 21

RESULT lines:

- Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0008534
- InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.1915728
- DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed duration=00:00:00.0014286
- RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0008081
- TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.0006964
- WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay = Passed duration=00:00:00.0012153
- ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay = Passed duration=00:00:00.0344447
- Cleanup_CalledTwice_DoesNotThrow = Passed duration=00:00:00.0010439
- InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0030668
- QfcRemainingQueueAdmission_DeclaresNoScoringDelegate = Passed duration=00:00:00.0077586
- ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder = Passed duration=00:00:00.0196485
- QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs = Passed duration=00:00:00.0013505
- TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook = Passed duration=00:00:00.0004574
- QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout = Passed duration=00:00:00.0063493
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.1253239
- RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0018275
- InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1374025
- RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0008401
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate = Passed duration=00:00:00.0005637
- DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1477616
- TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing = Passed duration=00:00:00.1223729

No MESSAGE lines; no new failure (no CONSOLIDATION BROKE A DATAMODEL TEST).
