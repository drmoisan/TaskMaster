# Specification tests on the unmodified fixture (issue #968, task P1-T6)

Timestamp: 2026-10-03T02-57
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName=QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome|FullyQualifiedName=QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher|FullyQualifiedName=QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores` (FILTER-PC-T234), TASKID p1-t6 and NAMES the last three NAMES-PC names; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-PC-T234" "/ResultsDirectory:coverage\test-results\968\p1-t6" "/Logger:trx;LogFileName=p1-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=3 executed=3 passed=3 failed=0
- RESULT_COUNT: 3
- RESULT EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores = Passed duration=00:00:00.0009269
- RESULT EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher = Passed duration=00:00:00.0167470
- RESULT EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome = Passed duration=00:00:00.0583118
- All three specification tests pass on the unmodified fixture: the "passes before and after the fix" half of the labels AC6 requires.
