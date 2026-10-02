# Pre-change run of the nine target tests (P0-T13)

Timestamp: 2026-10-02T00-52
Command: CMD-VSTEST with ASSEMBLY-QF (QuickFiler.Test\bin\Debug\QuickFiler.Test.dll), FILTER-TARGETS (the nine FullyQualifiedName= expressions, QCT. expanded to QuickFiler.Controllers.Tests., joined with |), TASKID p0-t13 and NAMES-TARGETS, executed as one pwsh -NoProfile -Command payload: PREFIX (Set-Location -LiteralPath "WORKTREE"; SetCurrentDirectory; WORKTREE-LEAF echo), TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-TARGETS" "/ResultsDirectory:coverage\test-results\950\p0-t13" "/Logger:trx;LogFileName=p0-t13.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=9 executed=9 passed=9 failed=0
RESULT_COUNT: 9
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0003198
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1369667
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0007117
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0605081
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.2066649
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0722791
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0029094
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1560974
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0133551

BASELINE-TARGETS-NOT-PASSED: NONE

Outcomes are observations of the pre-fix, load-dependent tests on an idle run and are not gated. The trx stays under the ignored coverage directory.
