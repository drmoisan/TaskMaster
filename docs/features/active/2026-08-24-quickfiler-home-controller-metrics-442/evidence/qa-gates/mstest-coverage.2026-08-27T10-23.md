# Phase 6 re-run — coverage-enabled full test suite

Timestamp: 2026-08-27T10-23
Task: [P6-T5]
Command: `pwsh -NoProfile -File "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
EXIT_CODE: 0

RunDisposition: ALL_TESTS_PASSED

## Output Summary

```
Test Run Successful.
Total tests: 6701
     Passed: 6701
```

- Total: **6701**, Passed: **6701**, Failed: **0**, Skipped: 0
- Discovered 9 test assemblies, all within this worktree.
- Line coverage: **85.1255%** (`lines-covered=54379`, `lines-valid=63881`) against the >= 85% floor.
- Branch coverage: **79.1912%** (`branches-covered=12924`, `branches-valid=16320`) against the >= 75% floor.

Both coverage thresholds in `.claude/rules/general-unit-test.md` are met. The line-coverage margin
is thin at 0.13 percentage points above the floor, and `lines-covered` is known to drift slightly
between runs on an identical tree, so this figure should be treated as approximately at-floor rather
than comfortably above it.

## The previously failing test is resolved

The prior run of this gate (`mstest-coverage.2026-08-26T11-30.md`) recorded 6518 tests with 1
failure:

`QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`

That test now **passes**. The fix changed the reflection seed value from `true` to `1` at
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64`, because [P3-T5] / AC-14 had changed
`_isExecuting` from `volatile bool` to `private int`. `CompareExchange(ref _isExecuting, 1, 0) == 0`
treats 0 as free and 1 as taken, so seeding 1 reproduces the original intent of placing the guard in
its taken state.

The total rose from 6518 to 6701 because the branch was brought up to date with the integration
base, which added sibling test coverage.

## Two earlier attempts and why they did not count

Attempt 1 aborted with `MSTest with coverage failed with exit code -1` after 1064 passing tests and
wrote no coverage file. No test had failed; the aggregate run terminated without a summary.

Attempt 2 completed 6701 tests with 3 failures, all in
`UtilitiesCS.Test.HelperClasses.FileIO2_Tests`, and all with the same cause:

```
System.IO.IOException: The process cannot access the file
'<repo-root>\UtilitiesCS.Test\TestData\FileIO2\sample.csv'
because it is being used by another process.
```

`WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` deliberately opens that
fixture exclusively to exercise retry behavior, so a pre-existing handle on the file fails that test
and cascades to the two CSV readers in the same class. `sample.csv` is referenced by exactly one
source file (`FileIO2_Tests.cs`), ruling out cross-class contention inside the assembly; the residual
handle is attributable to attempt 1's aborted run, which had assemblies executing in parallel.

Verified by re-running that assembly alone:

```
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
         /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
         /TestCaseFilter:TestCategory!=LiveOutlook
EXIT_CODE: 0
Test Run Successful. Total tests: 4763  Passed: 4763
```

The 3 failures were therefore environmental, not defects, and attempt 3 recorded above confirms a
fully green suite. Live test processes belonging to a different worktree were observed during this
window and were deliberately left running; their command lines referenced only their own tree.

## Per-file line-rate, five owned production files

Aggregated across every `class` element sharing the same `filename` attribute, taking the union of
line numbers and treating a line as covered when any class element records a non-zero hit count.

| Owned production file | Covered | Valid | Line-rate |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcHomeController.cs` | 224 | 228 | **98.25%** |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 62 | 69 | **89.86%** |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 64 | 64 | **100.00%** |
| `QuickFiler/Controllers/QfcHomeController.cs` | 170 | 223 | **76.23%** |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 88 | 110 | **80.00%** |

Each file resolved to exactly one `class` element, so no cross-partial aggregation was required.

`QfcHomeController.cs` at 76.23% and `QfcHomeController.Metrics.cs` at 80.00% sit below the
repository-wide 85% line floor. Both nevertheless rose sharply against their baselines
(68.40% and 63.31% respectively), so this feature improved them rather than degrading them.

## The apparent 14-point coverage regression was a measurement artifact

`coverage-delta.2026-08-26T11-30.md` recorded a repository-wide line rate falling from 84.84% to
70.28%, a signed difference of -14.56 pp, and labelled the baseline `post-processed` and the
post-change figure `raw`. That comparison is not apples-to-apples, and the cause is now established.

`Invoke-MSTestWithCoverage.ps1` calls `Invoke-DotnetCoverageCollection` first, which throws on a
non-zero coverage exit code. Post-processing runs only after that call returns. Post-processing is
what removes `<package>` elements for third-party assemblies that `dotnet-coverage` instruments at
runtime. Because the prior run ended with a failing test, the collection step threw and the
Cobertura file was never post-processed, leaving third-party packages in the denominator.

Observable consequences, both verified:

- Prior artifact: 17,863,442 bytes, third-party packages present, `line-rate="0.7028..."`.
- This run: 10,703,618 bytes, exactly 9 first-party production packages
  (`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`,
  `UtilitiesCS`, `VBFunctions`), no test assemblies, `line-rate="0.851255"`.

Comparing like with like, post-processed against post-processed:

| Metric | Baseline (post-processed) | This run (post-processed) | Signed difference |
| --- | --- | --- | --- |
| Repository-wide line rate | 84.84% | 85.1255% | **+0.29 pp** |

Coverage moved UP, not down. No coverage regression is attributable to this feature.

## Independent threshold confirmation, and its actual floor

This run also passed `Assert-CoberturaLineCoverageThreshold`, which the wrapper invokes after
post-processing and which throws when the rate is below its floor. That floor is **80%**, per
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487`, so clearing it independently
corroborates >= 80% only. The >= 85% line and >= 75% branch obligations in
`.claude/rules/general-unit-test.md` are met by the measured 85.1255% and 79.1912%, with the line
figure only 0.13 pp clear of its floor.
