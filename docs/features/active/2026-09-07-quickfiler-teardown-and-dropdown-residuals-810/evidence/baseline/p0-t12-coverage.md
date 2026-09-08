# [P0-T12] Baseline Cobertura Coverage

Timestamp: 2026-09-08T09-25
Command: `dotnet-coverage collect --output coverage\810-baseline.cobertura.xml --output-format cobertura --settings coverage\810-effective-coverage.config -- $vstest <nine assemblies> '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p0-t12' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'`, with `$vstest` re-bound per D3
EXIT_CODE: 0
Output Summary: The nine-assembly instrumented run executed 7153 tests with zero failures and exited 0. The Cobertura document was written and the nine first-party packages aggregate to 84.63 percent line coverage and 79.39 percent branch coverage. `$vstest` was printed before use and was non-empty.

BASELINE-TOTAL-TESTS: 7153
BASELINE-FAILED-TESTS: 0
BASELINE-LINES-COVERED: 113543
BASELINE-LINES-VALID: 134159
BASELINE-BRANCHES-COVERED: 26926
BASELINE-BRANCHES-VALID: 33916
BASELINE-LINE-PERCENT: 84.63
BASELINE-BRANCH-PERCENT: 79.39

BASELINE-FAILED-SET: none

## Nine explicitly named assemblies

`QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`, each supplied as `<project>\bin\Debug\<project>.dll`. Naming the nine explicitly is what keeps any sibling worktree build out of the run.

## Per-package aggregation

Every descendant `line` element of each first-party package was counted as valid, counted as covered when its `hits` attribute is greater than zero, and its `condition-coverage` `(covered/valid)` pair added into the branch totals.

| Package | Lines covered | Lines valid | Branches covered | Branches valid |
| --- | --- | --- | --- | --- |
| QuickFiler | 20551 | 25572 | 4892 | 6330 |
| UtilitiesCS | 79188 | 89094 | 18698 | 22450 |
| TaskVisualization | 2899 | 3230 | 666 | 800 |
| SVGControl | 1757 | 3712 | 600 | 1276 |
| ToDoModel | 2193 | 3819 | 496 | 1016 |
| Tags | 1428 | 1540 | 348 | 380 |
| TaskMaster | 4927 | 6564 | 1038 | 1460 |
| TaskTree | 592 | 620 | 188 | 204 |
| VBFunctions | 8 | 8 | 0 | 0 |
| **Total** | **113543** | **134159** | **26926** | **33916** |

The Cobertura document also carries five packages that are not first-party and are excluded from the aggregation: `log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async`, `System.Interactive`.

## Settings file

`coverage/810-effective-coverage.config` was written by this task as a copy of the repository-root `coverage.config` with one appended `<ModulePath>` exclusion matching `.*\.Test\.dll$`, so the test assemblies do not enter the denominator. That file is reused unchanged by [P7-T5], so both sides of the comparison come from one collector, one configuration, one assembly selection and one filter (D11). It is a gitignored working file under `coverage/` (D12) and is never committed.

## Divergence from the [P0-T11] scoped run

[P0-T11] observed one failing `QuickFiler.Test` case, `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`. That case passed in this nine-assembly run. The divergence is recorded as an observation rather than resolved: the two runs use different assembly sets and different instrumentation, so the case is not deterministic across them. The consequence for later tasks is that the two baselines are keyed independently — [P1-T10] compares against `BASELINE-QFT-FAILED: 1` from [P0-T11], and [P7-T5] compares against `BASELINE-FAILED-TESTS: 0` from this artifact.

## D12 and D13 compliance

`coverage/` is ignored by `.gitignore:144` (`coverage/*`) and `TestResults/` by `.gitignore:39`. The raw Cobertura document and the TRX are local run outputs and are never committed; only the parsed values above are transcribed. No TRX content and no TRX filename is reproduced.
