# Pass-after: pin-count tests on the fixed fixture (issue #968, task P2-T8)

Timestamp: 2026-10-03T03-00
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.` (FILTER-PC-CLASS), TASKID p2-t8 and NAMES-PC; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted; followed by git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-PC-CLASS" "/ResultsDirectory:coverage\test-results\968\p2-t8" "/Logger:trx;LogFileName=p2-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=4 executed=4 passed=4 failed=0
- RESULT_COUNT: 4
- RESULT EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher = Passed duration=00:00:00.0139703
- RESULT EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease = Passed duration=00:00:00.0554082
- RESULT EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores = Passed duration=00:00:00.0009809
- RESULT EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome = Passed duration=00:00:00.0009174

PHASE2-PORCELAIN (git status --porcelain -- QuickFiler QuickFiler.Test, exit 0):
```
 M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs
```

PHASE2-PORCELAIN equals PHASE1-PORCELAIN (FEATURE/evidence/regression-testing/pin-count-file-census.md) plus exactly one extra line, ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`. AC5 statement: the only difference between the P1-T5 fail-before run and this pass-after run is the fixture file.
