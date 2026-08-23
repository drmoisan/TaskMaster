# P5 collapsed-readiness disposal-ownership nine-class pass

Timestamp: `2026-07-22T08:43:54.6012121+00:00`

Command: `$vswhere='C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'; $installation=& $vswhere -latest -products * -property installationPath; $vstest=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $assembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests'; & $vstest $assembly /InIsolation "/TestCaseFilter:$filter" '/Logger:console;Verbosity=detailed'`

EXIT_CODE: `0`

Output Summary: `PASS. VSTest 18.8.0 selected exactly nine classes and discovered exactly 70 cases: 70 passed, zero failed, and zero skipped in 2.6627 seconds. The formerly failing viewer-attachment test passed with the retained non-saturating tracker and exact-once disposal assertions.`

## Class counts

| Class | Passed | Failed | Skipped |
|---|---:|---:|---:|
| `BreadcrumbUiThreadDispatchTests` | 9 | 0 | 0 |
| `BreadcrumbSelectorToggleUiBoundaryTests` | 4 | 0 | 0 |
| `BreadcrumbPopupControlDispatchTests` | 13 | 0 | 0 |
| `BreadcrumbSelectorOpenRetryTests` | 8 | 0 | 0 |
| `BreadcrumbDropDownReadinessTests` | 12 | 0 | 0 |
| `BreadcrumbCollapsedSurfaceReadinessTests` | 10 | 0 | 0 |
| `BreadcrumbDropDownCoverageThresholdTests` | 7 | 0 | 0 |
| `BreadcrumbDuplicateIdentityIntegrationTests` | 4 | 0 | 0 |
| `BreadcrumbBridgeCoordinatorProbabilityTests` | 3 | 0 | 0 |
| **Total** | **70** | **0** | **0** |

## Every test result

### BreadcrumbBridgeCoordinatorProbabilityTests — 3 passed

- PASS `SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes`
- PASS `SetSuggestions_SuccessfulUpgradeRetainsScoreAndLatestSelection`
- PASS `SetSuggestions_UnresolvedEmptyAndFailureRetainFallbackProbability`

### BreadcrumbUiThreadDispatchTests — 9 passed

- PASS `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`
- PASS `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext`
- PASS `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink`
- PASS `DispatcherActionFailure_IsReportedExactlyOnce`
- PASS `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`
- PASS `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost`
- PASS `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask`
- PASS `ProductionCaptureWithoutUiContext_FailsFast`
- PASS `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary`

### BreadcrumbSelectorToggleUiBoundaryTests — 4 passed

- PASS `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`
- PASS `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary`
- PASS `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession`
- PASS `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle`

### BreadcrumbPopupControlDispatchTests — 13 passed

- PASS `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`
- PASS `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp`
- PASS `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`
- PASS `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`
- PASS `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment`
- PASS `Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach`
- PASS `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce`
- PASS `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource`
- PASS `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly`
- PASS `DirectAdapters_CreateGuardAndReportThroughOwnedBoundary`
- PASS `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (0)`
- PASS `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (1)`
- PASS `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (2)`

### BreadcrumbSelectorOpenRetryTests — 8 passed

- PASS `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly`
- PASS `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle`
- PASS `Placement_StaleCurrentCheck_StopsSubsequentMutations (1)`
- PASS `Placement_StaleCurrentCheck_StopsSubsequentMutations (2)`
- PASS `Placement_StaleCurrentCheck_StopsSubsequentMutations (3)`
- PASS `Placement_StaleCurrentCheck_StopsSubsequentMutations (4)`
- PASS `HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce`
- PASS `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity`

### BreadcrumbDuplicateIdentityIntegrationTests — 4 passed

- PASS `ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence`
- PASS `OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence`
- PASS `ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence`
- PASS `CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability`

### BreadcrumbDropDownReadinessTests — 12 passed

- PASS `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`
- PASS `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`
- PASS `CaptureCurrent_ControlledContext_CreatesOperationsWithoutInvokingWebView`
- PASS `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture (0,"initializer")`
- PASS `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture (1,"html")`
- PASS `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture (2,"operations")`
- PASS `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture (3,"initializer")`
- PASS `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture (4,"html")`
- PASS `RunAsync_NullAction_ThrowsArgumentNullException`
- PASS `DisposeSurfaceAsync_NullSurface_ReturnsCompletedTask`
- PASS `ObserveReadinessAsync_CancellationRethrowsWithoutReporting`
- PASS `ObserveInitializationAsync_CancellationReportsIdenticalExceptionOnce`

### BreadcrumbCollapsedSurfaceReadinessTests — 10 passed

- PASS `AttachAsync_PendingAndUnrelatedNavigation_DefersReadyPublicationUntilExactSuccess`
- PASS `AttachAsync_ExactNavigationFailure_LeavesNoReadyMessenger`
- PASS `Reset_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
- PASS `Dispose_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
- PASS `LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger`
- PASS `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`
- PASS `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`
- PASS `NavigationReadiness_UnrelatedCompletionCannotReleaseExactNavigation`
- PASS `NavigationReadiness_SynchronousSuccessDetachesBeforeNavigationReturns`
- PASS `NavigationReadiness_FailureAndSynchronousExceptionDetachEveryPath`

### BreadcrumbDropDownCoverageThresholdTests — 7 passed

- PASS `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`
- PASS `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface`
- PASS `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus`
- PASS `OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry`
- PASS `OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle`
- PASS `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface`
- PASS `OpenAsync_LegacyFactoryReturnsNull_ReportsNoSurfaceAndRollsBack`

## Exact-once ownership observations

The passing `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment` retains these non-saturating observations:

- Readiness failure: `failedSurface.DisposeCount == 1` and subscriber count `0`.
- Pending reset: `resetSurface.DisposeCount == 1` and subscriber count `0`.
- Successful pooled reuse: `reusedSurface` attaches with subscriber count `1`.
- Ready reset after reuse: `reusedSurface.DisposeCount == 1` and subscriber count returns to `0`.
- Pending viewer disposal: `disposedSurface.DisposeCount == 1` and subscriber count `0`.

The creator-thread assertions also passed: captured exceptions were empty, the executed-thread observation was non-empty, every drained callback ran on the captured creator thread, and the prior synchronization context was restored on that thread. The wider 70-case result preserves the P5-T66 subscription, replay, readiness, retry, rollback, duplicate-identity, and probability behavior.

## Immutable tuple after testing

- `BreadcrumbMessengerHub.cs`: `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2`.
- `BreadcrumbCollapsedSurfaceController.cs`: `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE`.
- `BreadcrumbCollapsedSurfaceReadinessTests.cs`: `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3`.
