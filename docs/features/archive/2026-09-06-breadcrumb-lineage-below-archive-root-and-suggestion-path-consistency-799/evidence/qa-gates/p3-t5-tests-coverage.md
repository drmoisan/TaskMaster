# [P3-T5] Nine-assembly coverage run (post-change)

Timestamp: 2026-09-07T07-58

Command: dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p3-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
(then the pinned D13 aggregation block over artifacts\csharp\coverage.xml)

EXIT_CODE: 0

ExpectedExitCode: 0

## Aggregation output line (verbatim, printed by the pinned block)

```
LINES_COVERED=113143 LINES_VALID=133765 BRANCHES_COVERED=26746 BRANCHES_VALID=33736 PACKAGES_MATCHED=9
```

FINAL-LINES-COVERED: 113143
FINAL-LINES-VALID: 133765
FINAL-BRANCHES-COVERED: 26746
FINAL-BRANCHES-VALID: 33736
FINAL-PACKAGES-MATCHED: 9

## Derived percentages

FINAL-LINE-PERCENT: 84.58
FINAL-BRANCH-PERCENT: 79.28

## Test counters

FINAL-TOTAL-TESTS: 7085
FINAL-FAILED-TESTS: 0

Both values are read from the TRX `ResultSummary/Counters` element: total 7085, executed 7085, passed 7085,
failed 0, aborted 0, error 0. `FINAL-FAILED-TESTS` is taken from the `failed` attribute and NOT from the console,
because vstest prints no `Failed:` line at all on a fully passing run. The console printed `Test Run Successful.`,
`Total tests: 7085` and `Passed: 7085`.

NEWLY-FAILING: NONE

`BASELINE-FAILED-TESTS` from [P0-T12] is 0 and `FINAL-FAILED-TESTS` is 0, so `FINAL-FAILED-TESTS` is less than or
equal to the baseline and the newly-failing set is empty by construction: no test failed in this run, therefore no
test failed here that was passing in the baseline. This is also where a regression in the two
GetRelevantOlPathPortion assertions in ToDoModel.Test would have surfaced, because [P2-T9] changed the
include-children true branch of GetOlSubpath and ToDoModel.Test is executed by no Phase 2 command; both are inside
the 7085 passing tests.

## Test-count delta against the [P0-T12] baseline

BASELINE-TOTAL-TESTS: 7048
FINAL-TOTAL-TESTS: 7085
DELTA: +37

The delta is exactly the tests this plan adds. `[TestMethod]` attribute counts measured in the five new test files
after the [P3-T1] format pass:

```
UtilitiesCS.Test\OutlookObjects\Folder\ArchiveStemProjectionTests.cs = 11
UtilitiesCS.Test\OutlookObjects\Folder\ArchiveChainProjectionTests.cs = 7
UtilitiesCS.Test\OutlookObjects\Folder\OutlookFolderHierarchyProviderTrimTests.cs = 8
UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorRecentsProjectionTests.cs = 4
QuickFiler.Test\Controllers\BreadcrumbBridgeRouterScoreJoinTests.cs = 7
```

That is 30 new tests in UtilitiesCS.Test and 7 in QuickFiler.Test, summing to 37 and accounting for the whole
delta with nothing left over. The two per-assembly figures agree independently with the [P0-T11] baseline totals
and the [P2-T17] post-change totals: `BASELINE-UT-TOTAL: 4786` against `POST-UT-TOTAL: 4816` is +30, and
`BASELINE-QFT-TOTAL: 1363` against `POST-QFT-TOTAL: 1370` is +7. No test was removed; the two retargeted tests
were renamed or rewritten in place and remain one test each.

## Packages enumerated

The Cobertura document contains 14 `package` elements, the same 14 the baseline document contained. The nine
first-party packages the aggregation matches are QuickFiler, SVGControl, Tags, TaskMaster, TaskTree,
TaskVisualization, ToDoModel, UtilitiesCS and VBFunctions, so `FINAL-PACKAGES-MATCHED` is 9 and equals
`BASELINE-PACKAGES-MATCHED`, which is this task's acceptance condition. A zero there would have meant a
package-name mismatch rather than a genuine zero. The five packages present but not matched are log4net,
Mono.Reflection, Microsoft.IO.RecyclableMemoryStream, System.Linq.Async and System.Interactive, all third-party.

## Identical-method guarantee (D13)

The four counters on both sides are produced by one collector (`dotnet-coverage`, D12 form, never
`/EnableCodeCoverage`), one derived settings document (`coverage\799-effective-coverage.config`), one nine-assembly
selection named explicitly on the command line, and one test-case filter. The aggregation block is the same block
[P0-T12] used, over the same nine first-party package names. [P3-T8] makes the comparison.

## TRX document read

`TestResults\799-p3-t5\<user>_<host>_2026-09-07_07_53_07_net481.trx` — the only TRX in that results directory
(count verified as 1), so no most-recently-modified selection was needed. Filename reduced per R3; no TRX content
is pasted.

## Cobertura document

artifacts\csharp\coverage.xml exists on disk after the run. The artifacts directory is git-ignored at `.gitignore`
line 57, so this document is a local tool output rather than committed evidence, and the acceptance here is
on-disk existence and the recorded counters rather than `git ls-files`. Under R1 it is a tool output document and
not an evidence artifact: .claude/hooks/enforce-evidence-locations.ps1 names artifacts/csharp/ as an explicitly
permitted path at its line 26 and does not list it among the forbidden prefixes at lines 64-77.

The file did not exist before this task ran (verified before [P3-T1]), so it was not present as a formatting input
during the [P3-T1] pass and did not contribute to the 1601-file counts [P3-T1] and [P3-T2] recorded. The [P3-T6]
loop did not restart, so the deletion contingency this task describes was not exercised.

## Exclusions (R13, D14)

The run excludes `TestCategory=LiveOutlook` and the four shell-icon classes HelperClasses.ShellUtilities_Tests,
HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests and
EmailIntelligence.OSBrowser_Tests, which is the identical exclusion set the [P0-T12] baseline carried, so the
reduced denominator is visible on both sides of the [P3-T8] comparison. The nine test assemblies are named
explicitly on the command line, so no worktree copy under a `.claude` segment can be enumerated or loaded.

Output Summary: The full nine-assembly suite ran green under `dotnet-coverage`: 7085 tests, 7085 passed, 0 failed,
exit code 0. `NEWLY-FAILING: NONE`. The pinned D13 aggregation matched all nine first-party packages, equal to the
baseline, and produced 113143 covered of 133765 valid lines (84.58 percent) and 26746 covered of 33736 valid
branches (79.28 percent). artifacts\csharp\coverage.xml exists and all five `FINAL-` counter lines are numeric.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
