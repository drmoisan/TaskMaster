# P3-T3 — Mutation M2, Precondition Run Inline (expect-fail)

Timestamp: 2026-09-17T02-25

Command (three, in order):

1. `CMD-CENSUS` over `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
2. `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\p3-t3.msbuild.log;Verbosity=normal"` (resolved through vswhere)
3. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic" "/ResultsDirectory:TestResults\900\p3-t3" "/Logger:trx;LogFileName=p3-t3.trx"` (resolved through vswhere)

EXIT_CODE: 1

ExpectedExitCode: 1

CHANNEL: COMMAND

## The mutation

The line `action();` was inserted as the first statement of `RunOnDedicatedWorkerThread`, immediately
before `Exception captured = null;`. The delegate therefore runs synchronously on the calling thread
before the dedicated thread is constructed or started, which is exactly the failure mode the
precondition exists to catch: a future refactor that removes the `Join()` or invokes the delegate
directly.

On the calling thread `scope.Viewer.UiDispatcher.CheckAccess()` is true, because the calling thread
is the thread that constructed the viewer. `isOwnerThread` is therefore `true`, the `BeFalse`
assertion throws `AssertFailedException` out of the helper before the `try`/`catch` that would have
captured it, and the test fails at the precondition rather than at any boundary assertion.

No production file was edited. The mutation is test-only and is reverted by P3-T4.

## Output Summary

TOKEN action(); = 2

One in the helper's `try` block, one inserted. Every other token is unchanged from the "After P2-T1
and P2-T2" column, `ClearViewerDispatcher(scope.Viewer);` is back to 1 confirming no M1 residue, and
`LINES` is 491, one more than the committed 490. The file's `SHA256` under the mutation is
`EF6B6917FB8B63A9DFD3D8C241A99FFECEEA5DA5BAA8CDF40FBD5C820402429F`.

MSBUILD_EXIT_CODE: 0

`CSC_OUT_LINES: 2`, `ZERO_ERRORS_LINES: 1`, `DLL_ADVANCED: True`.

    COUNTERS total=2 executed=2 passed=0 failed=2
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Failed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Failed

The console printed `Test Run Failed.` and `VSTEST_EXIT_CODE: 1`.

### Failure messages, verbatim

    MESSAGE ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic :: Expected isOwnerThread to be False because the dedicated worker thread must not be the thread that constructed the viewer, or the boundary assertion would pass vacuously, but found True.
    MESSAGE InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic :: Expected isOwnerThread to be False because the dedicated worker thread must not be the thread that constructed the viewer, or the boundary assertion would pass vacuously, but found True.

## Acceptance, and comparison against the prediction

All four conditions hold.

- The census shows `action();` exactly 2.
- `MSBUILD_EXIT_CODE: 0`.
- `total` 2, `failed` 2.
- Each `MESSAGE` line contains `vacuously`.

The observed failing assertion matches the D-4 prediction exactly. The prediction was that both
tests fail at the precondition with a message containing `vacuously`, because `isOwnerThread` is
`true` when the delegate runs inline. The messages name `isOwnerThread`, report `to be False ... but
found True`, and carry the `BeFalse` reason text verbatim. Neither test passed.
`MUTATION PREDICTION MISMATCH` was not reached.

## What this establishes

AC2's clause "so the boundary assertion cannot pass vacuously" is a behavioural claim, and this run
is the observation of it failing. The precondition is genuinely exercised on every run of these
tests, and it is what fails first when the delegate does not run on a thread distinct from the
constructing thread. Without this evidence the precondition could have been an assertion that never
evaluates, or that evaluates and cannot fail.

Read together with P3-T1, the two mutations separate the two properties the rewritten tests claim.
M1 disables the guard and shows the boundary assertions fail; M2 removes the thread distinction and
shows the precondition fails. Neither mutation is defeated by the other's assertions, so each gate is
independently load-bearing.

This is also the direct refutation of the defect's root assumption. Under the original `Task.Run`
shape the same `isOwnerThread` condition was true whenever the runtime inlined the work item, but
nothing asserted it, so the test reported a pass while exercising no thread boundary at all.

## Build lock

The build and the test run ran inside a held shared build lock for item 900, released immediately
after the run completed.
