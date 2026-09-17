# P0-T9 — Baseline Scoped Run of the Original, Unfixed Test Class

Timestamp: 2026-09-17T02-15

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/ResultsDirectory:TestResults\900\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx"`
(vstest.console.exe resolved through vswhere)

EXIT_CODE: 0

CHANNEL: COMMAND

No `ExpectedExitCode:` line is written, because this run reported `failed` equal to 0. The plan
writes `ExpectedExitCode: 1` only when `failed` is greater than 0.

## Output Summary

COUNTERS total=7 executed=7 passed=7 failed=0

`total` is exactly 7, so the filter matched the intended class and nothing else, and the assembly is
the one rebuilt by P0-T8 rather than a stale copy. A run whose `total` were not exactly 7 would be a
failure of this task, because `vstest.console.exe` reports a zero-match filter without a non-zero
exit.

There is no `skipped` counter in the TRX `ResultSummary/Counters` element and none is reported.

RESULT lines, all seven:

    RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed

These seven names are exactly the seven `[TestMethod]` declarations the plan's fact 1 records at
lines 39, 89, 129, 168, 204, 237 and 280 of the target file. The run order above is the order the
TRX records, not the declaration order; the parallel run under `Workers=0`, `Scope=ClassLevel` does
not preserve declaration order and is not expected to.

MESSAGE lines: none. No test failed on this run.

ORIGINAL-FLAKE-OBSERVED: NO

Neither `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` nor
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` was recorded `Failed` on this
run.

## Interpretation

`NO` is a valid record of the pre-existing state and not a refutation of the defect. The defect is a
race between the runtime's local-queue pop inside the wait-inlining path of
`Task.Run(...).GetAwaiter().GetResult()` and a remote worker's steal of the same work item. The
stolen branch runs the delegate on a different pool thread and the test passes; the inlined branch
runs it on the constructing thread and the test fails. A single green observation samples the stolen
branch once and says nothing about the inlined branch's reachability, which is exactly why this item
does not gate on a deterministic failing run of the original tests.

This is the observation the P1-T2 fail-before exception dossier cites by its
`ORIGINAL-FLAKE-OBSERVED:` value. The deterministic non-vacuity evidence for the replacement tests
is produced later, by the two mutation runs in Phase 3.

## Results-directory hygiene

The TRX for this run is `TestResults\900\p0-t9\p0-t9.trx`. `TestResults/` is git-ignored at
`.gitignore:39`, and no TRX is copied into the feature folder. The results directory and the log
file name were passed explicitly so that no file name carries an account name or a machine name.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the test run and
released immediately after it completed.
