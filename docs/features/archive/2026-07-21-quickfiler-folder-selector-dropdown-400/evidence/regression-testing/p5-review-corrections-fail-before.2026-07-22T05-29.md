# P5 review corrections fail-before proof

Timestamp: 2026-07-22T05:29:13.4353865Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: Expected failure-first result. The bounded canonical rerun completed in 2.4344 seconds and discovered 25 tests: 22 passed and exactly 3 named regressions failed at their intended cleanup-ownership or Dispose-race assertions. There were no zero-test, discovery, build, crash, timeout, queue-determinism, creator-thread, or unrelated failures. The earlier terminated diagnostic is nonpassing historical evidence and was not reused.

## Every discovered named result

| Result | Test |
|---|---|
| PASS | `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary` |
| PASS | `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary` |
| PASS | `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession` |
| PASS | `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle` |
| PASS | `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` |
| PASS | `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp` |
| PASS | `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp` |
| PASS | `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface` |
| PASS | `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment` |
| PASS | `Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach` |
| PASS | `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce` |
| EXPECTED FAIL | `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource` |
| EXPECTED FAIL | `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly` |
| PASS | `DirectAdapters_CreateGuardAndReportThroughOwnedBoundary` |
| PASS | `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (0)` |
| PASS | `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (1)` |
| PASS | `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (2)` |
| PASS | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` |
| PASS | `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` |
| PASS | `Placement_StaleCurrentCheck_StopsSubsequentMutations (1)` |
| PASS | `Placement_StaleCurrentCheck_StopsSubsequentMutations (2)` |
| PASS | `Placement_StaleCurrentCheck_StopsSubsequentMutations (3)` |
| PASS | `Placement_StaleCurrentCheck_StopsSubsequentMutations (4)` |
| PASS | `HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce` |
| EXPECTED FAIL | `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity` |

## Intended assertions

- Cleanup retry: a first messenger-disposal failure produced one attempt instead of the required retry (`expected 2`, `actual 1`).
- Wrapper/direct ownership: stale-host cleanup directly disposed a control already owned by its wrapper (`expected 1`, `actual 2`).
- Dispose race: queued Reset/open lifecycle work executed three late callbacks after the two allowed pre-Dispose callbacks (`expected 2`, `actual 5`).

All synchronization-context exception snapshots, creator-thread execution checks, worker-provider dispatch guards, keyboard/mouse open equivalence, retry behavior, placement-generation checks, and retained popup factory/cleanup guards passed.
