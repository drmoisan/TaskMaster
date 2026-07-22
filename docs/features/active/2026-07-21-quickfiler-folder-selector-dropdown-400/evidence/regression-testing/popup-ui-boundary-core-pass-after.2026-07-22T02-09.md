# Popup UI-boundary core pass-after

Timestamp: 2026-07-22T02:09:19Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: VSTest discovered and passed all 14 focused tests in 1.5446 seconds. Failures: 0. Skips: 0. The suite proves worker initialization re-enters five explicit factory/cleanup stages, ambient context alone does not authorize inline control access, nested synchronous dispatch may inline only inside its current callback, scheduling/action/initialization/navigation failures are observed exactly once, the completed coordinator-post guards remain passing, and readiness defers attachment/show/focus with deterministic rollback on failure.

Named results: all PASS.

- `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`
- `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext`
- `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink`
- `DispatcherActionFailure_IsReportedExactlyOnce`
- `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`
- `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost`
- `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask`
- `ProductionCaptureWithoutUiContext_FailsFast`
- `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary`
- `SurfaceFactory_WorkerInitializationCompletion_DispatchesEveryUiStage`
- `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUpOnBoundary`
- `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUpOnBoundary`
- `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`
- `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`
