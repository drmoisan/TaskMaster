# Fail-before: pin-count regression test on the unmodified fixture (issue #968, task P1-T5, expect-fail)

Timestamp: 2026-10-03T02-57
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName=QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` (FILTER-PC-T1), TASKID p1-t5 and NAMES `"EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease"`; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-PC-T1" "/ResultsDirectory:coverage\test-results\968\p1-t5" "/Logger:trx;LogFileName=p1-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 1 (the deliberate fail-before outcome)
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=1 executed=1 passed=0 failed=1
- RESULT_COUNT: 1
- RESULT EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease = Failed duration=00:00:00.1765029
- MESSAGE EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease :: Expected afterFirstRelease to refer to System.Windows.Threading.Dispatcher { HasShutdownFinished = False, HasShutdownStarted = False, Hooks = System.Windows.Threading.DispatcherHooks{ }, Thread = System.Threading.Thread { ApartmentState = ApartmentState.STA {value: 0}, CurrentCulture = en-US, CurrentUICulture = en-US, ExecutionContext = System.Threading.ExecutionContext{ }, IsAlive = True, IsBackground = True, IsThreadPoolThread = False, ManagedThreadId = 36, Name = "UiThreadDispatcherFixture.ParkedDispatcher", Priority = ThreadPriority.Normal {value: 2}, ThreadState = ThreadState.Background|WaitSleepJoin {value: 36} } } because a holder that did not take the last pin must not lose the dispatcher, but found <null>.
- The MESSAGE contains `to refer to`, `ParkedDispatcher`, `a holder that did not take the last pin must not lose the dispatcher` and `but found <null>`: the first-release assertion failed because the first pin's release nulled the field (fact 13). Not REGRESSION DID NOT FAIL and not FAIL-BEFORE WRONG REASON.

Fixture state: the fixture is at BASE content. Second payload, `pwsh -NoProfile -Command '<CMD-HASH payload>'` with FILES `"QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs"` (exit 0, WORKTREE-LEAF agent-a291a7fbabf9d0229), printed `HASH QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6`, equal to the P0-T12 BASE-HASH for FIX.

The test ran alone by fully qualified name, so the shared static started from a null baseline (fact 7: SetupAssemblyInitializer does not write it). This run is the fail-before half of AC5.
