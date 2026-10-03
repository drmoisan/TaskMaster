# Pin-count class pass-after (issue #968, task P6-T4)

Timestamp: 2026-10-03T03-20
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-PC-CLASS (`FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.`), TASKID p6-t4 and NAMES-PC; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-PC-CLASS" "/ResultsDirectory:coverage\test-results\968\p6-t4" "/Logger:trx;LogFileName=p6-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=4 executed=4 passed=4 failed=0
- RESULT_COUNT: 4
- RESULT EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome = Passed duration=00:00:00.0008074
- RESULT EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease = Passed duration=00:00:00.0558775
- RESULT EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher = Passed duration=00:00:00.0139719
- RESULT EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores = Passed duration=00:00:00.0009991
