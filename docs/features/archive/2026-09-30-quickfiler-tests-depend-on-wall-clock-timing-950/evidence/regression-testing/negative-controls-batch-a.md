# Negative-control batch A run (P5-T11)

Timestamp: 2026-10-02T01-11
Task: P5-T11 [expect-fail]
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-CONTROLS-A (the eight FILTER-TARGETS expressions for test 1, test 3, test 4, the teardown test, the three zero-batch tests and R4; test 2 excluded; QCT. expanded), TASKID p5-t11 and NAMES-TARGETS, executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONTROLS-A" "/ResultsDirectory:coverage\test-results\950\p5-t11" "/Logger:trx;LogFileName=p5-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=8 executed=8 passed=1 failed=7
RESULT_COUNT: 8
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Failed duration=00:00:00.1859851
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Failed duration=00:00:00.0017485
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Failed duration=00:00:00.0077444
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Failed duration=00:00:00.2862076
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Failed duration=00:00:00.1859136
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Failed duration=00:00:00.0085546
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0825982
RESULT Worker_DoWork_CapturesRemainingLoadTask = Failed duration=00:00:00.1862274
MESSAGE InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing :: Did not expect any exception, but found System.NullReferenceException: Object reference not set to an instance of an object. at QuickFiler.Controllers.QfcDatamodel.InitEmailQueue(Int32 batchSize, BackgroundWorker worker) in REDACTED-PATH\QuickFiler\Controllers\QfcDatamodel.cs:line 285 at QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.<>c__DisplayClass7_0.<InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing>b__0() in REDACTED-PATH\QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs:line 147 at FluentAssertions.Specialized.DelegateAssertions`2.InvokeSubjectWithInterception() in /_/Src/FluentAssertions/Specialized/DelegateAssertions.cs:line 174.
MESSAGE RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally :: Expected ReadLivenessFlag(model) to be False because the finally must clear the flag on the throwing path too, or the gate would poll forever, but found True.
MESSAGE InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker :: Expected loaderInvokedTcs.Task.IsCompleted to be True because the injected RemainingEmailLoader must be invoked by the started worker, but found False.
MESSAGE InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop :: Test method QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop threw exception: System.NullReferenceException: Object reference not set to an instance of an object.
MESSAGE DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle :: Expected loaderEntered.Task.IsCompleted to be True because the synchronous starter must reach the injected RemainingEmailLoader, but found False.
MESSAGE RemainingLoadActive_AfterLoaderCompletes_BecomesFalse :: Expected ReadLivenessFlag(model) to be False because the finally around the awaited loader must clear the flag once it completes, but found True.
MESSAGE Worker_DoWork_CapturesRemainingLoadTask :: Expected loaderEntered.Task.IsCompleted to be True because the synchronous starter must reach the injected RemainingEmailLoader, but found False.

Per-control verdict against the P5-T11 acceptance:
- A1 test 1: Failed; message contains "the synchronous starter must reach the injected RemainingEmailLoader" (0.186 s)
- A2 test 3: Failed; message contains "the finally around the awaited loader must clear the flag once it completes" (0.009 s)
- A3 test 4: Failed; message contains "the finally must clear the flag on the throwing path too" (0.002 s)
- A4 teardown: Failed; message contains "the synchronous starter must reach the injected RemainingEmailLoader" (0.186 s)
- A5 Z1: Failed; message contains "the injected RemainingEmailLoader must be invoked by the started worker" (0.008 s)
- A6 Z0: Failed; message contains "NullReferenceException" (0.186 s)
- A7 Z2: Failed; message contains "NullReferenceException" (0.286 s)
- A8 R4: Passed (0.083 s); the pin neutralises the injected gate-free writer
No Timeout, Aborted or NotExecuted outcome; no Sequence file; no control passed that should fail. No CONTROL DEFECT.
