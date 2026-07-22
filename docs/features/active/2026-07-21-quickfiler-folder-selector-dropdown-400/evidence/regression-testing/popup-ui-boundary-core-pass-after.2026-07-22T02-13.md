# Popup UI-boundary core pass-after, restarted pass

Timestamp: 2026-07-22T02:13:24Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: This restarted focused gate supersedes the 02-09 artifact. VSTest discovered and passed all 15 tests in 1.9476 seconds. Failures: 0. Skips: 0. The added readiness-failure regression observed the identical exception exactly once and then disposed the messenger/control exactly once through dispatched cleanup.

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
- `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurfaceOnBoundary`
- `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`
- `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`
