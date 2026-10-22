# Phase 6 — Toolchain step 4 (measured): post-change Cobertura coverage

Timestamp: 2026-09-09T14-45

Task: [P6-T6]

## Command

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
dotnet-coverage collect --output coverage\823-post.cobertura.xml --output-format cobertura `
  --settings coverage\823-effective-coverage.config -- $vstest `
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll `
  Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
  ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
  VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p6-t6' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

This is the [P0-T12] command with `--output coverage\823-post.cobertura.xml`, the same unchanged
`coverage/823-effective-coverage.config`, the same nine explicitly named assemblies, the same
four-clause filter and a new results directory. It is the MEASURED run per D11.

EXIT_CODE: 0
ExpectedExitCode: 0

The declaration is keyed to this run rather than to the baseline: this run reported no failed test,
so the expectation is 0 and the observed code is 0.

## Test counters

POST-TOTAL-TESTS: 7187
POST-FAILED-TESTS: 0
POST-FAILED-SET: NONE
NEWLY-FAILING: NONE

Counters read from the TRX `ResultSummary/Counters` element: `total=7187`, `executed=7187`,
`passed=7187`, `failed=0`, `error=0`, `timeout=0`, `aborted=0`, `notExecuted=0`,
`inconclusive=0`, with zero `UnitTestResult` nodes whose outcome is not `Passed`.

`NEWLY-FAILING: NONE` holds because `POST-FAILED-SET` is empty and the empty set is a subset of
`BASELINE-FAILED-SET` from [P0-T12], which was itself `NONE`. The stop-and-report branch was not
taken.

Test-count reconciliation: the baseline total was 7185 and this run's total is 7187, a difference
of exactly the two tests this plan adds.

## Aggregation method

Identical to [P0-T12]: the same nine first-party packages, aggregated by summing every descendant
`line` element, counting each as valid and as covered when its `hits` attribute is greater than
zero, and adding the two integers of each `condition-coverage` `(covered/valid)` pair into the
branch totals. The document again carries the five non-first-party packages `log4net`,
`Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive` and
`System.Linq.Async`, none of which is included.

## Per-package totals

| Package | Lines covered | Lines valid |
| --- | --- | --- |
| QuickFiler | 20645 | 25664 |
| UtilitiesCS | 79338 | 89220 |
| TaskVisualization | 2899 | 3230 |
| SVGControl | 1757 | 3712 |
| ToDoModel | 2193 | 3819 |
| Tags | 1428 | 1540 |
| TaskMaster | 4927 | 6564 |
| TaskTree | 592 | 620 |
| VBFunctions | 8 | 8 |

## Aggregate figures

POST-LINES-COVERED: 113787
POST-LINES-VALID: 134377
POST-BRANCHES-COVERED: 26994
POST-BRANCHES-VALID: 33972
POST-LINE-PERCENT: 84.68
POST-BRANCH-PERCENT: 79.46

Output Summary: Measured post-change collection at exit 0. 7187 tests total, 7187 passed, 0 failed.
First-party aggregate line coverage 84.68 percent (113787 of 134377) and branch coverage 79.46
percent (26994 of 33972), both marginally above the baseline. No placeholder value appears anywhere
in this artifact.
