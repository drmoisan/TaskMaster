# P5-T181 — Uninstrumented focused pass-after for `BreadcrumbUiThreadDispatchTests`

Timestamp: 2026-07-22T15-07Z

Command: `$asm=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; & $vstestPath $asm '/InIsolation' '/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests'; $LASTEXITCODE`

EXIT_CODE: 0

No coverage instrumentation was used. The filter is the unmodified single-class filter; it was not narrowed and no case
was skipped.

## Discovered case list (9 discovered, 9 passed, 0 failed, 0 skipped)

| # | Case | Result | Duration |
|---:|---|---|---|
| 1 | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` | **Passed** | 232 ms |
| 2 | `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` | Passed | 29 ms |
| 3 | `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink` | Passed | 2 ms |
| 4 | `DispatcherActionFailure_IsReportedExactlyOnce` | Passed | < 1 ms |
| 5 | `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess` | Passed | 7 ms |
| 6 | `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost` | Passed | 2 ms |
| 7 | `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask` | Passed | 5 ms |
| 8 | `ProductionCaptureWithoutUiContext_FailsFast` | Passed | 46 ms |
| 9 | `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary` | Passed | 2 ms |

VSTest summary: `Test Run Successful. Total tests: 9  Passed: 9  Total time: 1.4135 Seconds`.

Output Summary: Uninstrumented focused run of `BreadcrumbUiThreadDispatchTests` discovered exactly 9 cases and passed
9/9 with zero failed and zero skipped, exit `0`. The previously failing case
`SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` is explicitly listed as **Passed**. The
filter was not narrowed and no case was skipped to obtain the pass. No restart of P5-T178 was required. EXIT_CODE: 0.
