# Datamodel set: the four datamodel classes in one invocation (issue #968, task P6-T8)

Timestamp: 2026-10-03T03-22
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-DATAMODEL (`FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcDatamodelTests.`), TASKID p6-t8 and an empty NAMES list; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-DATAMODEL" "/ResultsDirectory:coverage\test-results\968\p6-t8" "/Logger:trx;LogFileName=p6-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- RESULT_COUNT: 21
- COUNTERS total=21 executed=21 passed=21 failed=0
- DATAMODEL-NOT-PASSED: NONE
- Both NAMES-LIVENESS tests are Passed (AC32; the pass-after half of AC25, AC30 and AC31).

RESULT lines (all Passed):

- RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0006167
- TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.0006287
- InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1399477
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring = Passed duration=00:00:00.1335748
- QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout = Passed duration=00:00:00.0063675
- ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder = Passed duration=00:00:00.0171499
- InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.1885456
- ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay = Passed duration=00:00:00.0534466
- TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing = Passed duration=00:00:00.1319062
- QfcRemainingQueueAdmission_DeclaresNoScoringDelegate = Passed duration=00:00:00.0069972
- DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1568722
- TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook = Passed duration=00:00:00.0004617
- RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0016046
- InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0033922
- DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed duration=00:00:00.0014250
- TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate = Passed duration=00:00:00.0007624
- QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs = Passed duration=00:00:00.0014760
- WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay = Passed duration=00:00:00.0014605
- Cleanup_CalledTwice_DoesNotThrow = Passed duration=00:00:00.0009039
- RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0008538
- Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0007835
