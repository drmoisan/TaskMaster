# Fixture test class pass-after (issue #968, task P6-T5)

Timestamp: 2026-10-03T03-21
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-FT-CLASS (`FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.`), TASKID p6-t5 and NAMES-FT; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-FT-CLASS" "/ResultsDirectory:coverage\test-results\968\p6-t5" "/Logger:trx;LogFileName=p6-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=8 executed=8 passed=8 failed=0
- RESULT_COUNT: 8
- RESULT EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed duration=00:00:00.0694314
- RESULT EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed duration=00:00:00.0010332
- RESULT Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed duration=00:00:00.0015689
- RESULT EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed duration=00:00:00.0031842
- RESULT TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed duration=00:00:00.0019584
- RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0029685
- RESULT BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed duration=00:00:00.0457345
- RESULT Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed duration=00:00:00.0042058
- R1 to R6 and the #743 and #882 tests pass with their assertions unchanged (AC10); R4 passes without its pin and with the try/finally (AC14).
