# P3-T1 — Mutation M1, Guard Disabled (expect-fail)

Timestamp: 2026-09-17T02-24

Command (three, in order):

1. `CMD-CENSUS` over `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
2. `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\p3-t1.msbuild.log;Verbosity=normal"` (resolved through vswhere)
3. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic" "/ResultsDirectory:TestResults\900\p3-t1" "/Logger:trx;LogFileName=p3-t1.trx"` (resolved through vswhere)

EXIT_CODE: 1

ExpectedExitCode: 1

CHANNEL: COMMAND

## The mutation

The line `ClearViewerDispatcher(scope.Viewer);` was inserted into each of the two rewritten
delegates, immediately after the `isOwnerThread` `BeFalse` assertion statement and immediately
before the guarded call. The placement is load-bearing: the precondition still reads
`scope.Viewer.UiDispatcher.CheckAccess()` on a non-null owner and therefore still executes, while
the guarded call that follows finds a null owner and takes the guard's null-owner escape at
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:435-438`, so the boundary throw is never reached.

Placing the call earlier, immediately after constructing each `ViewerScope`, would null the owning
dispatcher before the precondition reads it, and the delegate would throw `NullReferenceException`
at the precondition. The boundary assertions would then never be exercised and the observed failure
would be `BeOfType` reporting `NullReferenceException` rather than the message assertion. That is
why the plan fixes the insertion point inside the delegate.

No production file was edited. The mutation is test-only and is reverted by P3-T2.

## Output Summary

TOKEN ClearViewerDispatcher(scope.Viewer); = 3

One pre-existing occurrence in the out-of-scope null-owner test plus the two inserted lines. Every
other token is unchanged from the "After P2-T1 and P2-T2" column, and `LINES` is 492, two more than
the committed 490. The file's `SHA256` under the mutation is
`B79D6C9B5FC0FECB64858DCFDE257F1B835BE23BDC4C5DFBD9579F3F3AF557AF`, which differs from `FIX-HASH:`
as expected while the mutation is in place.

MSBUILD_EXIT_CODE: 0

`CSC_OUT_LINES: 2`, `ZERO_ERRORS_LINES: 1`, `DLL_ADVANCED: True`. The mutated source was actually
compiled into the assembly the run then loaded.

    COUNTERS total=2 executed=2 passed=0 failed=2
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Failed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Failed

The console printed `Test Run Failed.` and `VSTEST_EXIT_CODE: 1`.

### Failure messages, verbatim

    MESSAGE InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic :: Expected captured.Message "Breadcrumb UI components must be constructed on an owning UI synchronization context." to contain "InitializeBreadcrumbPipeline".
    MESSAGE ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic :: Expected captured.Message "Breadcrumb UI components must be constructed on an owning UI synchronization context." to contain "ConfigureBreadcrumbDropDown".

## Acceptance, and comparison against the prediction

All four conditions hold.

- The census shows `ClearViewerDispatcher(scope.Viewer);` exactly 3.
- `MSBUILD_EXIT_CODE: 0`.
- `total` 2, `failed` 2.
- Each of the two `MESSAGE` lines contains both `owning UI synchronization context` and
  `to contain`.

The observed failing assertion matches the D-3 prediction exactly. The prediction was that both
tests fail on `captured.Message.Should().Contain(...)`, with the captured exception being the
`InvalidOperationException` thrown by `BreadcrumbUiDispatcher.CaptureCurrent()` when
`SynchronizationContext.Current` is null on the dedicated thread. The message text quoted in both
failures is that exception's text, and FluentAssertions' `to contain` phrase identifies the failing
assertion as the `Contain` call rather than any earlier one.

Two things follow from the message text, and they are the point of this mutation:

1. `captured.Should().NotBeNull(...)` passed, so an exception was captured and marshalled back
   across the dedicated thread boundary by the helper.
2. `captured.Should().BeOfType<InvalidOperationException>()` passed, because the assertion that
   failed is the one after it. The captured exception is exactly `InvalidOperationException`.

Neither `MESSAGE` line names `NullReferenceException` or `AssertFailedException`, and neither test
passed. `MUTATION PREDICTION MISMATCH` was not reached.

## What this establishes

The boundary assertions in the rewritten tests are not vacuous. When the guard is prevented from
throwing its boundary diagnostic, both tests fail, and they fail on the assertion that checks the
diagnostic names the guarded operation. A test that passed here would have been passing for a reason
unrelated to the contract it claims to assert.

## Build lock

The build and the test run ran inside a held shared build lock for item 900, released immediately
after the run completed.
