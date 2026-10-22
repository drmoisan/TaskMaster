# [P2-T14] Pass-after for every test in the [P1-T20] inventory

Timestamp: 2026-09-06T14-57

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p2-t14' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests|FullyQualifiedName~QfcFormControllerCancelTeardownTests|FullyQualifiedName~QfcHomeControllerCleanupTests|FullyQualifiedName~QfcDatamodelTeardownTests|FullyQualifiedName~QfcHomeControllerIterationTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 0

Output Summary: `Test Run Successful. Total tests: 76, Passed: 76, Total time: 1.8104 Seconds.`

P2-T14-TOTAL-RUN: 76
P2-T14-TOTAL-PASSED: 76
P2-T14-TOTAL-FAILED: 0

The run totals are recorded separately and are deliberately not asserted against the inventory
count: the filter selects whole classes, so it also runs the tests that were already green at the
end of Phase 1 (the #608 pin, the negative controls, the D6 capture pin, and every unaffected
pre-existing test in the six classes).

## `PASS-AFTER` lines, one per [P1-T20] inventory entry

Each line below was derived by looking the method name up in the `TestResults\791-p2-t14` TRX and
reading its `outcome` attribute. All names are in namespace `QuickFiler.Controllers.Tests`.

PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_Launch_LogsCutoffQuantityAndBounds
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging
PASS-AFTER: QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop
PASS-AFTER: QfcQueuePurePathsTests.DequeueNextItemGroupWithOutcomeAsync_ZeroAcceptanceCeilingGate_ReportsScanCapReachedStop
PASS-AFTER: QfcFormControllerCancelTeardownTests.ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive
PASS-AFTER: QfcFormControllerCancelTeardownTests.ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors
PASS-AFTER: QfcFormControllerCancelTeardownTests.ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup
PASS-AFTER: QfcFormControllerCancelTeardownTests.ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup
PASS-AFTER: QfcFormControllerCancelTeardownTests.ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup
PASS-AFTER: QfcFormControllerCancelTeardownTests.ButtonCancel_Click_ActionThrows_DoesNotRethrow
PASS-AFTER: QfcHomeControllerCleanupTests.Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup
PASS-AFTER: QfcHomeControllerCleanupTests.Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted
PASS-AFTER: QfcDatamodelTeardownTests.TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing
PASS-AFTER: QfcDatamodelTeardownTests.QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout
PASS-AFTER: QfcDatamodelTeardownTests.QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs
PASS-AFTER: QfcDatamodelTeardownTests.Cleanup_CalledTwice_DoesNotThrow
PASS-AFTER: QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask

PASS-AFTER-COUNT: 25
P1-T20-INVENTORY-COUNT: 25
COUNTS-EQUAL: YES

## Additional named acceptances satisfied by this run

The tests the Phase 2 tasks name as "still passes" controls are in the same six classes and are
among the 76 that passed:

- [P2-T2]: `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext` — its filtered
  `ContainSingle` predicate still selects exactly the per-candidate score line, so the added launch
  line did not break it.
- [P2-T3]: `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding` and
  `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` — the new stop reason is not
  routed into the queue-closing branch, and genuine exhaustion still closes it.
- [P1-T6] / #608: `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` — green before and after.
- [P1-T9]: `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` — green before and
  after under its rebased bound.

## Re-run after the [P2-T15] topology repair

[P2-T15] surfaced one newly-failing pre-existing test and its repair changed
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, so this command was re-run verbatim
against the repaired build to keep this artifact's result attributable to the delivered code rather
than to an intermediate one. The re-run printed `Test Run Successful. Total tests: 76, Passed: 76,
Total time: 1.7424 Seconds` with exit code 0 — identical counts — and overwrote
`TestResults\791-p2-t14`. Every `PASS-AFTER` line above therefore describes the final build.
