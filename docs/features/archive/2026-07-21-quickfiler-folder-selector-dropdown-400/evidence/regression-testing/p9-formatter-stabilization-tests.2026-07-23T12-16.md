# Phase 9 Formatter-Stabilization Focused Tests

- Timestamp: `2026-07-23T12:16:42Z`
- Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests' '/Logger:console;Verbosity=normal'`
- EXIT_CODE: `0`
- Output Summary: `Test Run Successful; total=13 passed=13 failed=0 skipped=0 elapsed=1.3455s; workspace_vstest_after=0 workspace_testhost_after=0`

## Authoritative Passing Result

```text
VSTest version 18.8.0 (x64)
Test Run Successful.
Total tests: 13
     Passed: 13
 Total time: 1.3455 Seconds
```

All eleven methods and all three data rows from the P8-T20 ledger were discovered and passed:

- `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`
- `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp`
- `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`
- `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`
- `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment`
- `Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach`
- `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce`
- `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource`
- `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly`
- `DirectAdapters_CreateGuardAndReportThroughOwnedBoundary`
- `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp` rows `0`, `1`, and `2`

A preceding clean-run detailed-console invocation also passed all 13 cases in 1.4286 seconds. The final normal-console invocation above confirms the required whole-class filter passes without relying on detailed logging.

## Superseded Runner Diagnostics

The first command used a `vswhere -requires Microsoft.VisualStudio.Component.TestTools.BuildTools` query that did not resolve VSTest in this Visual Studio 18 installation. No test process started and no source changed. Read-only installation inspection resolved both installed VSTest paths.

Two subsequent whole-class invocations exceeded their bounded host timeout while workspace-owned `vstest.console.exe` and child `testhost.exe` processes remained alive. They produced no assertion failure or completed test result and are nonpassing diagnostic attempts, not evidence for this task. Only the process pairs whose command lines contained this exact workspace and test assembly were stopped; the independent Visual Studio design-mode runner was not touched.

After the workspace-owned residual process pair was removed:

- A detailed whole-class run passed 13/13 in 1.4286 seconds.
- Both deterministic class halves passed 5/5 and 8/8 with normal console verbosity.
- The full normal class filter then passed 13/13 in 1.3455 seconds.
- Process inspection after the authoritative run reported `WORKSPACE_VSTEST=0` and `WORKSPACE_TESTHOST=0`.

No test, assertion, filter, runsettings, test code, production code, project file, or timeout behavior in the repository was changed during diagnosis. The authoritative command retains the exact P8-T25 class filter and `/InIsolation`.
