# Pass-after run of the nine target tests (P4-T4)

Timestamp: 2026-10-02T01-08
P4-RESTART: 0
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-TARGETS (nine FullyQualifiedName= expressions, QCT. expanded, joined with |), TASKID p4-t4 and NAMES-TARGETS, executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-TARGETS" "/ResultsDirectory:coverage\test-results\950\p4-t4" "/Logger:trx;LogFileName=p4-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=9 executed=9 passed=9 failed=0
RESULT_COUNT: 9
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1525930
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0573582
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0013640
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0028547
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.2647588
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0730800
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0031888
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1367933
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0028913

All nine target tests Passed under the repository runsettings (Workers=0, Scope=ClassLevel).
