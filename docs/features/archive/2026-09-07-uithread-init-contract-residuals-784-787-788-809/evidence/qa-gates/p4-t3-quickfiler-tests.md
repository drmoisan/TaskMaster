# [P4-T3] `QuickFiler.Test` methods this delivery can affect

Timestamp: 2026-09-08T02-21

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/InIsolation' '/Logger:trx;LogFileName=p4t3.trx' '/ResultsDirectory:TestResults\809-p4t3' '/TestCaseFilter:FullyQualifiedName~QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests|FullyQualifiedName~QuickFiler.Test.TestSupport.WinFormsPumpHostTests|FullyQualifiedName~QuickFiler.Controllers.Tests.EfcFormControllerTests|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests|FullyQualifiedName~QuickFiler.Helper_Classes.Tests.EmailMoveMonitorTests'
```

The five filter operands are the fully-qualified type names as declared, re-derived from their declarations rather than inferred from file paths, because a filter operand that matches no type selects zero tests silently. All five matched: the run discovered 70 tests spread across all five classes.

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 70
```

The run also printed `     Passed: 70` and printed no `Failed:` line and no `Skipped:` line.

TRX selected: `p4t3.trx`, `LastWriteTimeUtc` `2026-09-08T04:56:02.5648507Z`.

TRX `ResultSummary/Counters`: `total` 70, `executed` 70, `passed` 70, `failed` 0.

The `failed` attribute is 0.

SKIPPED_DERIVED: 0

## The four individually named rows

| Fully-qualified test | Outcome | Duration |
|---|---|---|
| `QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` | Passed | 00:00:00.0955621 |
| `QuickFiler.Test.TestSupport.WinFormsPumpHostTests.AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` | Passed | 00:00:00.0030778 |
| `QuickFiler.Test.TestSupport.WinFormsPumpHostTests.BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` | Passed | 00:00:00.0058242 |
| `QuickFiler.Controllers.Tests.EfcFormControllerTests.ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` | Passed | 00:00:00.0033845 |

## Second pass, run after the [P4-T5] line-budget trim

PASS_2_TIMESTAMP: 2026-09-08T02-31

The trim recorded in `p4-t2-format-and-builds.md` changed a `UtilitiesCS.Test` file and therefore triggered a solution rebuild, which rebuilds `QuickFiler.Test` as well. This task was re-run unchanged against that rebuild.

PASS_2_EXIT_CODE: 0

PASS_2_OUTPUT_SUMMARY: `Test Run Successful.`, `Total tests: 70`, `     Passed: 70`. TRX selected `p4t3b.trx`, `LastWriteTimeUtc` `2026-09-08T05:00:59.1988644Z`; counters `total` 70, `executed` 70, `passed` 70, `failed` 0; derived skipped count 0. All four individually named rows were `Passed` again, `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` in 00:00:00.0913172.

---

The first is the reconciled MTA caller: it no longer calls `UiThread.Init(false)` and instead installs a pumping dispatcher through the existing `UiThreadDispatcherFixture` transaction, so it is order-independent. The two `WinFormsPumpHostTests` rows are the pair that a bare owning-thread-identity predicate would have broken; both still post to the pump thread under the adopted predicate, because the awaited context is a `WindowsFormsSynchronizationContext` that is neither `_uiSyncContext` nor a `DispatcherSynchronizationContext`. The `EfcFormControllerTests` row injects a bare `new SynchronizationContext()` and still posts, because the ambient context on the MSTest thread is null and the predicate's `ambient is null` early return keeps it false.
