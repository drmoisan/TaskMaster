# [P0-T11] QuickFiler.Test Baseline Run

Timestamp: 2026-09-08T09-22
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p0-t11' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 1
Output Summary: 1380 tests ran, 1379 passed and 1 failed. The single failure is pre-existing on this branch before any task of this plan edits a source file, so it forms the baseline failing set that [P1-T10] and [P7-T5] compare against. `$vstest` was printed before use and was non-empty.

BASELINE-QFT-TOTAL: 1380
BASELINE-QFT-PASSED: 1379
BASELINE-QFT-FAILED: 1

## Baseline failing set

BASELINE-QFT-FAILED-SET:
QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores

This is one fully qualified test name. Any later failure whose name is not in this set is newly failing.

## Consequences for later tasks

Because `BASELINE-QFT-FAILED` is 1 rather than 0:

- [P1-T10] must declare `ExpectedExitCode: 1`, per its own conditional wording.
- [P1-T10] must show `POST-QFT-TOTAL: 1381`, being this baseline total plus the one test [P1-T1] adds.
- [P7-T5] must key its `ExpectedExitCode:` to the [P0-T12] baseline failure count rather than to this one, because [P7-T5] is a nine-assembly run.

## D13 compliance

The run wrote a TRX whose default filename embeds `runUser` and `computerName`. No TRX content and no TRX filename is reproduced in this artifact. The four counter values above were read from the TRX `ResultSummary/Counters` element and the failing test name from the `UnitTest/TestMethod` definition it referenced; both are parsed values, not transcribed document content.

## D12 compliance

`TestResults/` is ignored by `.gitignore:39` (`[Tt]est[Rr]esult*/`), so the TRX is a local run output and is never committed.
