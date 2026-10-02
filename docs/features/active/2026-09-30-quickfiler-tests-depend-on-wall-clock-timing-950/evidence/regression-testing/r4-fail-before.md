# R4 deterministic fail-before repro (P1-T6, Defect B)

Timestamp: 2026-10-02T00-59
Task: P1-T6 [expect-fail]
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-R4 (FullyQualifiedName=QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores), TASKID p1-t6 and NAMES "Transaction_SecondCallerCannotInstallUntilTheFirstRestores", executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-R4" "/ResultsDirectory:coverage\test-results\950\p1-t6" "/Logger:trx;LogFileName=p1-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Edit applied: Delivered Source R-INJECT on the unmodified R4 file (one discarded `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` inserted between the `original` read and `transactionA.Install(liveA);`, P1-T2). The test ran alone by fully qualified name, so `UiThread._dispatcher` started null (fact 7: the assembly initializer does not write it) and `original` was null. The injected gate-free writer then seeded the parked dispatcher inside window 1. This run is both the Defect B fail-before evidence and the R4 negative control of the spec Test Strategy table (pre-fix shape with the injected writer).

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=1 executed=1 passed=0 failed=1
RESULT_COUNT: 1
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Failed duration=00:00:00.1933723
MESSAGE Transaction_SecondCallerCannotInstallUntilTheFirstRestores :: Expected observedByB to refer to <null> because the first transaction restores before it releases the gate, so the waiter cannot observe the pre-restore value, but found System.Windows.Threading.Dispatcher { HasShutdownFinished = False, HasShutdownStarted = False, Hooks = System.Windows.Threading.DispatcherHooks{ }, Thread = System.Threading.Thread { ApartmentState = ApartmentState.STA {value: 0}, CurrentCulture = en-US, CurrentUICulture = en-US, ExecutionContext = System.Threading.ExecutionContext{ }, IsAlive = True, IsBackground = True, IsThreadPoolThread = False, ManagedThreadId = 37, Name = "UiThreadDispatcherFixture.ParkedDispatcher", Priority = ThreadPriority.Normal {value: 2}, ThreadState = ThreadState.Background|WaitSleepJoin {value: 36} } }.

The message contains `to refer to` and `ParkedDispatcher`: the CI failure signature of spec Repro and Evidence (the second caller observed the parked dispatcher where the null baseline was expected).
