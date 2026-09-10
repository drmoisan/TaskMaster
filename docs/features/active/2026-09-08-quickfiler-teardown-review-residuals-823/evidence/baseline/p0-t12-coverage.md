# Phase 0 — Measured baseline coverage (dotnet-coverage Cobertura)

Timestamp: 2026-09-09T13-54

Task: [P0-T12]

## Effective settings file

`coverage/823-effective-coverage.config` was written first, as a copy of the repository-root
`coverage.config` with one appended `<ModulePath>` exclusion matching `.*\.Test\.dll$`, so the test
assemblies do not enter the denominator. `coverage/` is gitignored, so the file is a local working
file and is not committed (D12).

## Command

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
dotnet-coverage collect --output coverage\823-baseline.cobertura.xml --output-format cobertura `
  --settings coverage\823-effective-coverage.config -- $vstest `
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll `
  Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
  ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
  VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p0-t12' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' `
  '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The nine assemblies are named explicitly rather than discovered, which is what keeps any sibling
worktree build out of the run (D10). This is the MEASURED run per D11 and is the sole source of the
numeric coverage values in this plan.

EXIT_CODE: 0

## Test counters

BASELINE-TOTAL-TESTS: 7185
BASELINE-FAILED-TESTS: 0
BASELINE-FAILED-SET: NONE

Counters read from the TRX `ResultSummary/Counters` element: `total=7185`, `executed=7185`,
`passed=7185`, `failed=0`, `error=0`, `timeout=0`, `aborted=0`, `notExecuted=0`,
`inconclusive=0`, with zero `UnitTestResult` nodes whose outcome is not `Passed`. Per D13 no TRX
content is reproduced.

## Aggregation method

The nine first-party packages `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`,
`QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl` and `VBFunctions` were aggregated by summing
every descendant `line` element: each counts as valid, and counts as covered when its `hits`
attribute is greater than zero. Branch totals add the two integers of each `condition-coverage`
`(covered/valid)` pair. The Cobertura document additionally carries the packages `log4net`,
`Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive` and
`System.Linq.Async`, none of which is first-party and none of which is included in these totals.
[P6-T6] uses the identical selection so the two sides compare like for like.

## Per-package totals

| Package | Lines covered | Lines valid |
| --- | --- | --- |
| QuickFiler | 20637 | 25658 |
| UtilitiesCS | 79330 | 89216 |
| TaskVisualization | 2899 | 3230 |
| SVGControl | 1757 | 3712 |
| ToDoModel | 2193 | 3819 |
| Tags | 1428 | 1540 |
| TaskMaster | 4927 | 6564 |
| TaskTree | 592 | 620 |
| VBFunctions | 8 | 8 |

## Aggregate figures

BASELINE-LINES-COVERED: 113771
BASELINE-LINES-VALID: 134367
BASELINE-BRANCHES-COVERED: 26992
BASELINE-BRANCHES-VALID: 33972
BASELINE-LINE-PERCENT: 84.67
BASELINE-BRANCH-PERCENT: 79.45

Output Summary: Measured baseline collected at exit 0. 7185 tests total, 7185 passed, 0 failed.
First-party aggregate line coverage 84.67 percent (113771 of 134367) and branch coverage 79.45
percent (26992 of 33972). No placeholder value appears anywhere in this artifact.
