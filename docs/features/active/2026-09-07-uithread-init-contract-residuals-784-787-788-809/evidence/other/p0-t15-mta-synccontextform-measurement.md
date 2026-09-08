# [P0-T15] Decision-D5 measurement — does `new SyncContextForm(); Show();` throw on an MTA thread on this host?

Timestamp: 2026-09-08T00-47

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Tests:Worker_RunWorkerCompleted_HandlesCompletionCorrectly' '/InIsolation' '/Logger:trx;LogFileName=p0t15.trx' '/ResultsDirectory:TestResults\809-p0t15'
```

`/Tests:` and `/TestCaseFilter:` are mutually exclusive in vstest and the four stalling shell-icon classes live in `UtilitiesCS.Test`, so no filter was passed here.

EXIT_CODE: 0
ExpectedExitCode: 0

`ExpectedExitCode:` is set to the observed value because this task **measures** rather than gates. Either observed outcome is a valid result of the measurement, so declaring an expectation other than the observed one would convert a measurement into a gate that decision D5 does not authorise.

MTA_INITIALIZE_OUTCOME: COMPLETED

## Output Summary

```
Test Run Successful.
Total tests: 1
     Passed: 1
```

TRX selected: `p0t15.trx`, `LastWriteTimeUtc` `2026-09-08T04:23:02.2600330Z`, selected as the most recently modified `.trx` under `TestResults\809-p0t15`.

TRX `ResultSummary/Counters`: `total` 1, `executed` 1, `passed` 1, `failed` 0.

| Fully-qualified test | Outcome | Duration |
|---|---|---|
| `QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` | Passed | 00:00:00.3959381 |

The TRX carries no `Message` and no `StackTrace` element for that result, because the result is `Passed`. The value of `MTA_INITIALIZE_OUTCOME` is `COMPLETED`, so no exception type or message is recorded; that record is required only on the `THREW` branch.

## The inference

The inference is exact. In a single-test run no earlier test can have consumed the latch at `UtilitiesCS/Threading/UiThread.cs:36`, so `UiThread.Init(false)` at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` necessarily entered `Initialize()`. The Act at `:346-353` invokes `QfcHomeController.Worker_RunWorkerCompleted`, whose body reaches `UiThread.Dispatcher.Invoke(...)`; `UiThread.Dispatcher` throws when `_dispatcher` is null. The two assertions at `:356-357` therefore hold only if `Initialize()` ran to completion on the MTA MSTest worker, which requires `new SyncContextForm()` at `UiThread.cs:51` and `Show()` at `:54` not to have thrown.

The test is declared with a plain `[TestMethod]` at `:325` on a class carrying no `[STATestClass]`, so the executing apartment is the MSTest default, which research R4 established as MTA from three independent in-tree sources.

Both assertions passed, so `new SyncContextForm(); Show();` **completed without throwing on an MTA thread on this execution host**.

## Consequence for the #782 narrative

The recorded #782 mechanism requires `new SyncContextForm(); Show();` to throw on a non-STA thread. This measurement shows that it does not throw on this host, so **that mechanism narrative is not reproducible as stated on this execution host**. [P6-T4] carries the reconciliation and records the disposition; the AC2 "reproduce the #782 regression scenario as a test" clause is discharged there by the forced-throw scenario driven through the factory seam rather than by the narrative.

This is a measurement of one host at one point in time. It refutes the narrative's necessary precondition on this host; it does not establish what was observed on the host where #782 was recorded.
