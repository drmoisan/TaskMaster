# P3-T6 — `WinFormsPumpHostTests` Self-Tests

Timestamp: 2026-08-22T10-37

Command:
```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx `
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p3-t6 `
  /TestCaseFilter:"FullyQualifiedName~WinFormsPumpHostTests"
```

EXIT_CODE: 0

Output Summary:

TRX: `evidence/regression-testing/p3-t6/2026-08-22_10_37_02_net481.trx`

TRX `<Counters>` verbatim:

```
total="13" executed="13" passed="13" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
```

| # | Test | Outcome | Duration |
| --- | --- | --- | --- |
| 1 | `Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread` | Passed | 112 ms |
| 2 | `InvokeAsyncAction_WhenPosted_RunsOnThePumpThread` | Passed | 7 ms |
| 3 | `InvokeAsyncFactory_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue` | Passed | 5 ms |
| 4 | `RunAsyncVoid_WhenPosted_StartsAndResumesOnThePumpThread` | Passed | 7 ms |
| 5 | `RunAsyncResult_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue` | Passed | 8 ms |
| 6 | `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` | Passed | 5 ms |
| 7 | `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` | Passed | 47 ms |
| 8 | `InvokeAsync_WhenWorkThrows_FaultsTheAwaitedTaskWithTheOriginalException` | Passed | 64 ms |
| 9 | `RunAsyncVoid_WhenWorkFaults_SurfacesTheOriginalUnwrappedException` | Passed | 6 ms |
| 10 | `RunAsyncResult_WhenWorkFaults_SurfacesTheOriginalUnwrappedException` | Passed | 8 ms |
| 11 | `PostingMembers_AfterStop_FaultWithObjectDisposedException` | Passed | 7 ms |
| 12 | `Dispose_CalledTwice_IsANoOp` | Passed | 7 ms |
| 13 | `StopAsync_WhenThePumpLoopRecordedAnException_RethrowsIt` | Passed | 8 ms |

Acceptance: exactly 13 executed, 13 passed, 0 failed, 0 skipped (`notExecuted="0"`).

`WinFormsPumpHostTests.cs` was not edited by this plan (443 lines, at the plan's do-not-touch list),
and `TimeoutMs = 30000` retains its current value; P3-T8 records that check.
