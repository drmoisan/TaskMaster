# [P4-T4] `UtilitiesCS.Test` methods research R7 enumerated as at risk

Timestamp: 2026-09-08T02-23

Command: identical in shape to [P1-T11] except for the log file name and the results directory, with the class filter extended by the two new contract classes.

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p4t4.trx' '/ResultsDirectory:TestResults\809-p4t4' '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FolderRemapViewer_Tests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersViewer_Tests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests|FullyQualifiedName~UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitApartmentContract_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests'
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 82
     Passed: 82
```

TRX selected: `p4t4.trx`, `LastWriteTimeUtc` `2026-09-08T04:56:47.5018833Z`.

TRX `ResultSummary/Counters`: `total` 82, `executed` 82, `passed` 82, `failed` 0.

The `failed` attribute is 0.

SKIPPED_DERIVED: 0

The 82 discovered tests are the 65 the [P1-T11] filter selected plus the 17 this delivery adds.

## The six individually named rows

| Fully-qualified test | Outcome | Duration |
|---|---|---|
| `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` | Passed | 00:00:00.0448694 |
| `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` | Passed | 00:00:00.0028518 |
| `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` | Passed | 00:00:00.0025348 |
| `UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.EnterUiContextAsync_WhenUiSyncContextPostsSynchronously_CompletesUsingDefaultAction` | Passed | 00:00:00.0015010 |
| `UtilitiesCS.Test.EmailIntelligence.FolderRemapViewer_Tests.SetController_WithSyntheticController_ConfiguresTreeDelegates` | Passed | 00:00:00.0062163 |
| `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersViewer_Tests.SetController_WithSyntheticController_ConfiguresBothTreeDelegates` | Passed | 00:00:00.3815654 |

## Second pass, run after the [P4-T5] line-budget trim

PASS_2_TIMESTAMP: 2026-09-08T02-31

The trim recorded in `p4-t2-format-and-builds.md` changed `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, one of the files this filter covers, so this task was re-run unchanged.

PASS_2_EXIT_CODE: 0

PASS_2_OUTPUT_SUMMARY: `Test Run Successful.`, `Total tests: 82`, `     Passed: 82`. TRX selected `p4t4b.trx`, `LastWriteTimeUtc` `2026-09-08T05:01:12.9542131Z`; counters `total` 82, `executed` 82, `passed` 82, `failed` 0; derived skipped count 0. Every one of the six individually named rows was `Passed` again.

---

The method name `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` carries a deliberately inaccurate suffix and was not renamed, because its fully-qualified name is quoted inside a committed `TestCaseFilter` evidence artifact.

The two viewer tests are the in-repo pair that actually drive `Init()` through to a successful `Initialize()`. Both remain `[STATestClass]` and both now additionally carry `[DoNotParallelize]`; both still assert `NotThrow`, so the AC1 precondition does not reject the STA path they exercise.
