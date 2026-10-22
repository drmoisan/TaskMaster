# [P1-T10] QuickFiler.Test Suite After the AC1 Change

Timestamp: 2026-09-08T09-50
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p1-t10' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
ExpectedExitCode: 1
Output Summary: 1381 tests ran, all 1381 passed and none failed. The total is the [P0-T11] baseline total of 1380 plus the one test [P1-T1] added. The observed exit code is 0, which is one better than the declared expectation: the single case that failed in the [P0-T11] baseline passed in this run.

POST-QFT-TOTAL: 1381
POST-QFT-PASSED: 1381
POST-QFT-FAILED: 0

POST-QFT-FAILED-SET:
(empty — no test failed in this run)

NEWLY-FAILING: NONE

## Why `NEWLY-FAILING` is NONE

`POST-QFT-FAILED-SET` is empty. The empty set is a subset of the `BASELINE-QFT-FAILED-SET` recorded by [P0-T11], which contains the single name `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`. No test failed here, so no test failed here that did not fail in the baseline.

`POST-QFT-FAILED` is 0 and `BASELINE-QFT-FAILED` is 1, so the required relation `POST-QFT-FAILED` less than or equal to `BASELINE-QFT-FAILED` holds.

## `ExpectedExitCode` and the observed exit code

The task fixes the declared expectation mechanically: `ExpectedExitCode:` is set to 0 when `BASELINE-QFT-FAILED` from [P0-T11] is 0 and to 1 otherwise. `BASELINE-QFT-FAILED` is 1, so the declared value recorded above is 1. The observed exit code is 0 because no test failed.

The divergence is recorded rather than reconciled, and it is not a regression in either direction. It arises because the baseline failing case is intermittent rather than deterministic: it failed in the [P0-T11] baseline run and passed in the [P0-T12] nine-assembly baseline run captured seven minutes later on the same unmodified tree, and it passed again here. The expectation is keyed to the [P0-T11] observation, so a run in which that case passes cannot match the declared value however the production change behaves.

Both readings of this task's substantive gate are satisfied: no test failed, and no test failed that was not already failing in the baseline.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the non-passing outcome list from its `UnitTestResult` elements joined to the `UnitTest/TestMethod` definitions by test id; that list came back empty.
