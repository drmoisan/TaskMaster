# Regression Fail-Before (issue #781)

Timestamp: 2026-09-05T16-47

Task: [P1-T4] [expect-fail]

This artifact records the deliberate red state of the new test class against the **unfixed**
guard. A failing test run is the expected outcome for this task and for this task only.

## Invocation 1 — build the test project

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

EXIT_CODE: 0

The platform value is `AnyCPU` with no space. The solution-level alias `Any CPU` used by
[P0-T6], [P0-T7], [P2-T3] and [P2-T4] is not substituted here:
`QuickFiler.Test/QuickFiler.Test.csproj` declares
`<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>` at line 12 and defines
`OutputPath` only under `Debug|AnyCPU`, `Release|AnyCPU`, `Debug|x86` and `Release|x86`, so a
project-file build invoked with `Any CPU` matches no property group and fails before compiling.

Build result: `Build succeeded.` with 0 Warning(s) and 0 Error(s).

## Invocation 2 — run the new test class

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx "/ResultsDirectory:TestResults\fail-before-781" "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/Settings:scripts\vscode\TaskMaster.cli.runsettings"`

EXIT_CODE: 1

ExpectedExitCode: 1

Both invocations were issued from the repository root inside a `pwsh -NoProfile -Command`
process, using the `vstest.console.exe` resolved by [P0-T4].

## Output Summary

Total tests: 7. Passed: 2. Failed: 5. Skipped: 0. Total time 2.4168 seconds.
Run result: `Test Run Failed.`

Every test name with its outcome, as printed by the runner:

| Test | Outcome | Duration |
| --- | --- | --- |
| `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | **Failed** | 991 ms |
| `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | **Failed** | 77 ms |
| `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | **Failed** | 44 ms |
| `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | **Failed** | 45 ms |
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | Passed | 75 ms |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | Passed | 38 ms |
| `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | **Failed** | 39 ms |

All four [P1-T4] acceptance conditions hold:

1. The msbuild invocation exits 0, so this is a **runtime** red rather than a compile red: the
   tests were built and executed, and they failed on the guard's behaviour rather than failing to
   compile.
2. The five tests named by the task are each recorded **Failed**.
3. The two worker-thread tests are each recorded **Passed**. They are corroborating rather than
   discriminating: the pre-fix reference comparison also rejects a worker-thread call, because a
   worker thread's ambient context is null and is therefore not reference-equal to the context the
   viewer captured.
4. The executed test count is **7**.

The five failures are the defect this issue reports. In each of them the call is made on the
thread that constructed the viewer, and the pre-fix `ThrowIfOffUiBoundary` rejects it because the
ambient `SynchronizationContext` at the call site is not the same instance as the one captured in
the constructor.

The `.trx` produced under `TestResults\fail-before-781\` was not copied into this evidence
folder: a `.trx` carries `runUser` and `computerName` host tokens, and `TestResults\` is already
git-ignored.
