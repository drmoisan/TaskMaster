# Runtime UI-boundary fail-before proof

Timestamp: 2026-07-22T01:43:05.5799466Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: Expected failure-first result. VSTest discovered 10 tests: 8 passed and exactly 2 named new regressions failed for their intended runtime assertions. There were no build, discovery, crash, timeout, or unrelated failures. Total time: 2.4069 seconds.

## Every discovered named result

| Result | Test |
|---|---|
| PASS | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` |
| PASS | `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` |
| PASS | `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink` |
| PASS | `DispatcherActionFailure_IsReportedExactlyOnce` |
| PASS | `ProductionCaptureWithoutUiContext_FailsFast` |
| PASS | `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary` |
| PASS | `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary` |
| EXPECTED FAIL | `OpenAsync_AmbientNullWorkerCompletions_KeepEveryPopupOperationOnOwnerBoundary` |
| EXPECTED FAIL | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` |
| PASS | `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` |

## Intended failure details

- Popup continuation: `surface-size@10` was recorded off the owning boundary after worker factory/readiness completion. This proves the residual host/control continuation defect rather than a source-token inference.
- Mouse retry: the exact cross-thread exception reached the captured synchronization context; only one open request occurred; the selector and host were closed after the second click instead of retrying cleanly.

## Old defect versus residual defect

The completed P3 coordinator/provider-post defect remains fixed: all six existing `BreadcrumbUiThreadDispatchTests` and the new worker `selectorToggle` callback-entry proof passed. The remaining failures begin after coordinator callback entry, at the asynchronous popup factory/host/control boundary and at ItemViewer's unobserved failed-open/retry pipeline. These are the independently demonstrated P5 residual defects.
