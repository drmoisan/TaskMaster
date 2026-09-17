# P0-T10 — Baseline Repository-Wide Test and Coverage Run

Timestamp: 2026-09-17T02-18

Command (two payloads):

1. `CMD-COVERAGE` with `STAGE` `baseline`:
   `dotnet-coverage collect --output coverage\baseline-900.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-900.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:TestResults\900\baseline" "/Logger:trx;LogFileName=baseline-900.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`
   (vstest.console.exe resolved through vswhere)
2. `CMD-POSTPROCESS` with `STAGE` `baseline`.

EXIT_CODE: 0

CHANNEL: COMMAND

No `ExpectedExitCode:` line is written, because the observed `COLLECT_EXIT_CODE:` was 0.

## Output Summary

ASSEMBLY_COUNT: 9

    ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
    ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
    ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
    ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
    ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
    ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
    ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
    ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
    ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll

Every line begins with `\` and contains the segment `\bin\Debug\`. The paths are printed relative to
the worktree root, so the worktree's own location cannot appear in them; the clause fails when the
discovery filter admits an `obj\` or `ref\` copy, or when the root-trimming substring is wrong.
Neither occurred. All nine first-party test assemblies were discovered, which is the expected
population for this solution.

### TRX counters

Selected TRX: `TestResults\900\baseline\baseline-900.trx`. One TRX was found in the results
directory (`TRX_COUNT: 1`), with `LastWriteTime` `2026-09-17T02-17-28`, so no re-run disambiguation
was needed.

    COUNTERS total=7288 executed=7288 passed=7288 failed=0

The console additionally printed `Test Run Successful.`, `Total tests: 7288` and `Passed: 7288`, and
printed no `Failed:` and no `Skipped:` line, which is the shape of an all-green
`vstest.console.exe` run. There is no `skipped` counter in `ResultSummary/Counters` and none is
reported.

### BASELINE-FAILING-SET:

    (empty: no test was recorded Failed on this run)
    DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue KNOWN-FLAKY #780

The known-flaky line is recorded under the heading whether or not that test failed, so that P5-T5's
`NEWLY-FAILING:` computation has a stable reference. On this run it passed.

Both in-scope tests were observed in this repository-wide, fully parallel run:

    AFFINITY InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    AFFINITY ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed

Consistent with P0-T9, the original tests' intermittent failure did not manifest on this sampling.
That is a record of the pre-existing state, not evidence that the assumption they encode is sound.

### BASELINE-COVERAGE:

    line-rate=0.852658
    branch-rate=0.796851
    lines-covered=55948
    lines-valid=65616
    branches-covered=13564
    branches-valid=17022

All six root attribute values are recorded. These are the figures P5-T6 compares the final run
against under the two-branch comparability rule.

FLOOR: MET

`Assert-CoberturaLineCoverageThreshold` did not throw, so the repository line rate is above its
80 percent floor. `BASELINE COVERAGE FLOOR NOT MET` was not reached.

### Instrumentation-scope observations (recorded, not gated)

    ItemViewerBreadcrumbClassCount=0
    ThreadAffinityTestsClassCount=0

The pair `0` and `0` is the expected value. `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` belongs to
the `ItemViewer` partial class, which carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Viewers/ItemViewer.cs:20`, and `QuickFiler.Test.dll` is excluded from the instrumented
denominator by the `.*\.Test\.dll$` module exclusion that
`ConvertTo-DerivedCoverageSettingsXml` appends to the derived settings. This pair selects P5-T6's
`CHANGED-CODE COVERAGE: NOT MEASURABLE` branch. `INSTRUMENTATION SCOPE CHANGED` was not reached and
is not reported.

## Artifact hygiene

The raw Cobertura document (about 18 MB) and the TRX stay under `coverage/` and `TestResults/`
respectively, both git-ignored, and neither is copied into the feature folder. Only the six root
attribute values and the derived counts above are recorded here, which is what the repository's
committed-test-evidence rule requires: a projection of the tool's output, never the tool's raw
document.

## Execution note

The run was launched as a detached process with its output redirected to a git-ignored log under
`coverage/`, and awaited by process handle, rather than run inline, because a repository-wide
instrumented run exceeds a single foreground command window. The standard-error log was empty
(0 bytes). The shared build lock for item 900 was held for the whole run, which also serialises this
workstation's instrumented runs against sibling worktrees; two simultaneous `dotnet-coverage collect`
sessions on one machine are a known deadlock source. The lock was released immediately after the
post-processing step completed.
