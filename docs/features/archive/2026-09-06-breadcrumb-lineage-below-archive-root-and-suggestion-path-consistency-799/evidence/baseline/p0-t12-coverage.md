# [P0-T12] Nine-assembly coverage baseline

Timestamp: 2026-09-07T07-12

Command: dotnet-coverage collect --output coverage\799-baseline.cobertura.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t12' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
(then the pinned D13 aggregation block over coverage\799-baseline.cobertura.xml)

EXIT_CODE: 0

## Aggregation output line (verbatim, printed by the pinned block)

```
LINES_COVERED=112855 LINES_VALID=133485 BRANCHES_COVERED=26642 BRANCHES_VALID=33624 PACKAGES_MATCHED=9
```

BASELINE-LINES-COVERED: 112855
BASELINE-LINES-VALID: 133485
BASELINE-BRANCHES-COVERED: 26642
BASELINE-BRANCHES-VALID: 33624
BASELINE-PACKAGES-MATCHED: 9

## Derived percentages

BASELINE-LINE-PERCENT: 84.55
BASELINE-BRANCH-PERCENT: 79.24

BASELINE_FLOOR: MET — measured against the D13 comparability index (84.55 percent lines) and not against the
repository line-coverage rate. The D13 aggregation counts every `line` element under a matched package, which
selects class-level and method-level elements alike and therefore over-counts the denominator relative to a
de-duplicated per-line count. The value is sound for the identical-method comparison [P3-T8] makes and is not a
policy measurement. No task in this plan gates on it, and a pre-existing repository floor result would not halt
the plan either way.

## Test counters

BASELINE-TOTAL-TESTS: 7048
BASELINE-FAILED-TESTS: 0

Read from the TRX `ResultSummary/Counters` element: total 7048, passed 7048, failed 0, aborted 0. The run printed
`Test Run Successful.` and `Total tests: 7048 / Passed: 7048`.

## Packages enumerated

The Cobertura document contains 14 `package` elements. The nine first-party packages the aggregation matches are
QuickFiler, SVGControl, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel, UtilitiesCS and VBFunctions, so
`PACKAGES_MATCHED` is 9 and no package-name mismatch occurred. The five packages present but not matched are
log4net, Mono.Reflection, Microsoft.IO.RecyclableMemoryStream, System.Linq.Async and System.Interactive, all
third-party.

## TRX document read

`TestResults\799-p0-t12\<user>_<host>_2026-09-07_06_48_12_net481.trx` — the only TRX in that results directory
(count verified as 1), so no most-recently-modified selection was needed. Filename reduced per R3; no TRX content
is pasted.

## Exclusions (R13, D14)

The run excludes `TestCategory=LiveOutlook` and the four shell-icon classes HelperClasses.ShellUtilities_Tests,
HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests and
EmailIntelligence.OSBrowser_Tests. The nine test assemblies are named explicitly on the command line, so no
worktree copy under a `.claude` segment can be enumerated or loaded. Instrumentation used the derived settings
document `coverage\799-effective-coverage.config`, which is `coverage.config` plus one appended
`<ModulePath>` entry excluding `*.Test.dll` modules from the measured denominator.

Output Summary: The full nine-assembly suite ran green under `dotnet-coverage` (D12 form, not
`/EnableCodeCoverage`): 7048 tests, 7048 passed, 0 failed, exit code 0, 55.5 seconds. The pinned D13 aggregation
matched all nine first-party packages and produced 112855 covered of 133485 valid lines (84.55 percent) and 26642
covered of 33624 valid branches (79.24 percent). These five counters are the baseline side of the [P3-T8]
comparison, which must apply this identical aggregation to the post-change document and must state `lines-valid`
comparability as an explicit precondition.
