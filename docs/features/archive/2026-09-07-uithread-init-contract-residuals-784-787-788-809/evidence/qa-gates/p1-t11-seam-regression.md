# [P1-T11] Phase 1 seam regression — the `UtilitiesCS.Test` classes that touch `UiThread` state

Timestamp: 2026-09-08T01-11

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p1t11.trx' '/ResultsDirectory:TestResults\809-p1t11' '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FolderRemapViewer_Tests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersViewer_Tests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests|FullyQualifiedName~UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests'
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 65
```

The run also printed `     Passed: 65` and printed no `Failed:` line and no `Skipped:` line.

TRX selected: `p1t11.trx`, `LastWriteTimeUtc` `2026-09-08T04:31:03.0717030Z`.

TRX `ResultSummary/Counters`: `total` 65, `executed` 65, `passed` 65, `failed` 0.

The `failed` attribute is 0.

SKIPPED_DERIVED: 0

All 65 discovered methods across the seven filtered classes reported outcome `Passed`. The five pre-existing `SynchronizationContextAwaiter_Tests` methods (`Constructor_NullContext_ThrowsArgumentNullException`, `IsCompleted_WhenContextIsNotCurrent_ReturnsFalse`, `IsCompleted_WhenContextMatchesCurrent_ReturnsTrue`, `GetResult_DoesNotThrow`, `OnCompleted_PostsCallbackToContext`) and both `UiThread_Dispatcher_Tests` methods passed, as did the two viewer tests that drive `Init()` to success and the `FolderPredictorTests` reflection test.

Phase 1 introduced the factory seam, the message constant, the reset hook and the interface without changing any observable behaviour of `Init()`, `Initialize()` or `IsCompleted`.
