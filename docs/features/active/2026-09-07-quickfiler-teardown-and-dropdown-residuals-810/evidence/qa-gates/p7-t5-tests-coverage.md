# [P7-T5] Post-Change Coverage Collection

Timestamp: 2026-09-08T10-23
Command: `dotnet-coverage collect --output coverage\810-post.cobertura.xml --output-format cobertura --settings coverage\810-effective-coverage.config -- $vstest <nine assemblies> '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p7-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'`, with `$vstest` re-bound per D3, the same nine explicitly named assemblies, the same unchanged settings file and the same filter as [P0-T12]
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary: The nine-assembly instrumented run executed 7163 tests with zero failures and exited 0. The test total is the [P0-T12] baseline of 7153 plus exactly the 10 tests this plan adds. The nine first-party packages aggregate to 84.63 percent line coverage and 79.39 percent branch coverage, the same figures to two decimal places as the baseline.

POST-TOTAL-TESTS: 7163
POST-FAILED-TESTS: 0
POST-LINES-COVERED: 113595
POST-LINES-VALID: 134219
POST-BRANCHES-COVERED: 26940
POST-BRANCHES-VALID: 33932
POST-LINE-PERCENT: 84.63
POST-BRANCH-PERCENT: 79.39

POST-FAILED-SET:
(empty — no test failed in this run)

NEWLY-FAILING: NONE

## Why `NEWLY-FAILING` is NONE

`POST-FAILED-SET` is empty. The empty set is a subset of the `BASELINE-FAILED-SET` recorded by [P0-T12], which is itself empty (`BASELINE-FAILED-TESTS: 0`). No test failed here, so no test failed here that did not fail in the baseline.

## The `ExpectedExitCode` derivation

The task fixes the declared expectation mechanically: `ExpectedExitCode:` is set to 0 when `BASELINE-FAILED-TESTS` from [P0-T12] is 0 and to 1 otherwise. That baseline value is 0, so the declared value is 0, and the observed exit code is also 0. The expectation is met.

The intermittently failing case that [P0-T11] observed, `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, passed in this run as it did in the [P0-T12] baseline and in [P1-T10].

## Test-count reconciliation

| | Tests |
| --- | --- |
| [P0-T12] baseline total | 7153 |
| Added by [P1-T1] (AC1) | 1 |
| Added by [P2-T1] (AC3) | 1 |
| Added by [P3-T1] (AC4) | 1 |
| Added by [P4-T1] (AC5) | 1 |
| Added by [P6-T1] (AC7) | 6 |
| Expected total | 7163 |
| Observed total | 7163 |

The reconciliation is exact, which also confirms that both new files entered the compilation and that no pre-existing case was removed or renamed. [P4-T1] also appended an assertion to an existing case rather than adding one, which is why AC5 contributes 1 rather than 2.

## Per-package aggregation

Every descendant `line` element of each first-party package was counted as valid, counted as covered when its `hits` attribute is greater than zero, and its `condition-coverage` `(covered/valid)` pair added into the branch totals. This is the same all-descendant selection [P0-T12] used.

| Package | Lines covered | Lines valid | Branches covered | Branches valid |
| --- | --- | --- | --- | --- |
| QuickFiler | 20611 | 25632 | 4910 | 6346 |
| UtilitiesCS | 79180 | 89094 | 18694 | 22450 |
| TaskVisualization | 2899 | 3230 | 666 | 800 |
| SVGControl | 1757 | 3712 | 600 | 1276 |
| ToDoModel | 2193 | 3819 | 496 | 1016 |
| Tags | 1428 | 1540 | 348 | 380 |
| TaskMaster | 4927 | 6564 | 1038 | 1460 |
| TaskTree | 592 | 620 | 188 | 204 |
| VBFunctions | 8 | 8 | 0 | 0 |
| **Total** | **113595** | **134219** | **26940** | **33932** |

The `QuickFiler` package gained 60 valid lines and 60 covered lines against the baseline, which is the new `BreadcrumbPopupOwnerRegistry` entering the denominator fully covered together with the net effect of the edits to the other production files. `UtilitiesCS` moved by 8 covered lines and 4 covered branches with an unchanged denominator, which is instrumentation nondeterminism in an assembly this plan does not touch rather than an effect of any change here.

## D11 and D12 compliance

The collector, the configuration, the assembly selection and the filter are all identical to [P0-T12], so both sides of the [P7-T8] comparison come from one instrument. `coverage/810-effective-coverage.config` was reused unchanged and not rewritten. `coverage/` and `TestResults/` are gitignored, so the raw Cobertura document and the TRX are local run outputs and are never committed; only the parsed values above are transcribed.

## D13 compliance

No TRX content and no TRX filename is reproduced. The three counters were parsed from the run's `ResultSummary/Counters` element and the non-passing outcome list from its `UnitTestResult` elements joined to the `UnitTest/TestMethod` definitions by test id; that list came back empty.
