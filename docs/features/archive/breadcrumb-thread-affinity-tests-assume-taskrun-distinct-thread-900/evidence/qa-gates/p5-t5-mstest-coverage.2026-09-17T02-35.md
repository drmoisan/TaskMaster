# P5-T5 — Final Repository-Wide Test and Coverage Run (loop iteration 2)

Timestamp: 2026-09-17T02-35

Command: `CMD-COVERAGE` with `STAGE` `final`:
`dotnet-coverage collect --output coverage\final-900.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-900.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:TestResults\900\final" "/Logger:trx;LogFileName=final-900.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`
(vstest.console.exe resolved through vswhere)

EXIT_CODE: 0

CHANNEL: COMMAND

No `ExpectedExitCode:` line is written, because this run reported no failed test.

## Output Summary

    Test Run Successful.
    Total tests: 7288
         Passed: 7288
    COLLECT_EXIT_CODE: 0

The console printed no `Failed:` and no `Skipped:` line, which is the shape of an all-green
`vstest.console.exe` run.

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

### TRX counters

Selected TRX: `TestResults\900\final\final-900.trx`, `TRX_COUNT: 1`, `LastWriteTime`
`2026-09-17T02-35-12`. The results directory was removed before this iteration launched, so the
single TRX present is this run's.

    COUNTERS total=7288 executed=7288 passed=7288 failed=0

FINAL-FAILING-SET: (empty: no test was recorded Failed)

NEWLY-FAILING: NONE

`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, the test P0-T10 recorded under
its failing-set heading as `KNOWN-FLAKY #780`, was observed `Passed` on this run.

### The seven in-scope results

The seven `UnitTest` definitions whose `TestMethod` `className` ends
`ItemViewerBreadcrumbThreadAffinityTests` were resolved by id to their results, so the match is on
the declaring class rather than on a name substring:

    AFFINITY_TEST_DEFS: 7
    AFFINITY_RESULTS: 7
    RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed

This is the strongest evidence for AC6 in the plan: the two rewritten tests pass inside a full
7288-test, nine-assembly, fully parallel run under the unchanged CLI runsettings, not merely in a
scoped two-test run.

## Acceptance

All four conditions hold.

- `ASSEMBLY_COUNT:` is 9, equal to the P0-T10 value and at least 1, so the baseline and final
  coverage figures cover the same assembly set.
- Every `ASSEMBLY:` line begins with `\` and contains the segment `\bin\Debug\`.
- `NEWLY-FAILING: NONE`.
- The seven `ItemViewerBreadcrumbThreadAffinityTests` results in the TRX all read `Passed`.

## Loop context

This is iteration 2 of the P5-T1 through P5-T7 loop. Iteration 1 of this step failed on an
environmental file-contention failure in
`UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile`,
which opens the repository's own `TaskMaster.sln` and found it held by a resident MSBuild
node-reuse worker left behind by the plan's own `/m` rebuilds. The full record, the diagnosis and
the actions taken are in
`evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md`. Before this iteration's
coverage run, the MSBuild node-reuse workers created by this iteration's own P5-T3 and P5-T4
rebuilds were terminated so the machine was quiescent; no test, command, setting, tolerance or gate
was modified.

## Artifact hygiene

The raw Cobertura document and the TRX stay under `coverage/` and `TestResults/`, both git-ignored,
and neither is copied into the feature folder.

## Build lock

This task ran inside a held shared build lock for item 900, released immediately after the run
completed.
