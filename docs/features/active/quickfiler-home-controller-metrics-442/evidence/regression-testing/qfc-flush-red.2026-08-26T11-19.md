# Phase 5 — QFC Metrics Flush Regression Tests, Red State (#442)

Timestamp: 2026-08-26T11-19
Task: [P5-T7] `[expect-fail]`
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p5-t7"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX file: `TestResults\p5-t7\<account>_<HOST>_2026-08-26_11_19_38_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Failed.
Total tests: 12
     Passed: 8
     Failed: 4
```

The four tests from [P5-T2] through [P5-T5] fail. The test from [P5-T6] passes, as the plan states
it must. At this point the `MetricsFileWriter` seam exists ([P5-T1]) but `WriteMetricsAsync` still
routes through `NonBlockingProducer`, so the seam is never invoked.

### Failing tests with verbatim failure messages

**1. `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` ([P5-T2], root cause RC-1)**

> Expected captures to contain a single item because the flush must invoke the writer exactly once, but the collection is empty.

The capture list is empty, which is exactly the observable form of RC-1: the diagnostic lines are
handed to a `BlockingCollection` whose consumer can never start, because the
`Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` guard compares against a value the
field never holds, and the `System.Timers.Timer` behind that guard is never started even if reached.

**2. `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` ([P5-T3], root cause RC-1)**

> Expected writerCompleted to be True because the writer must complete before WriteMetricsAsync returns, but found False.

The flush-timing invariant does not hold on the pre-fix source: nothing is written by the time the
returned Task completes.

**3. `WriteMetricsAsync_PassesUncancelledTokenToWriter` ([P5-T4], root cause RC-1)**

> Test method QuickFiler.Controllers.Tests.QfcHomeControllerMetricsTests.WriteMetricsAsync_PassesUncancelledTokenToWriter threw exception:
> System.OperationCanceledException: The operation was canceled.

The pre-fix source passes the controller's session `Token` into `NonBlockingProducer`, whose
`ct.ThrowIfCancellationRequested()` aborts the whole flush when the session has been cancelled. This
is the defect AC-4 targets: a session cancellation raised while the write is in flight destroys the
metrics rather than being ignored by it.

**4. `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` ([P5-T5], root cause RC-1 plus CFN-2)**

> Expected captures to contain a single item, but the collection is empty.

The capture list is empty for the same reason as test 1, so the filtering assertion is not even
reached on the pre-fix source.

### Passing tests in this run

- `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` ([P5-T6], the deliberate guard
  test that must pass on the pre-fix source)
- `WriteMetricsAsync_ReadsMovedStopwatchForDuration` ([P4-T1], green since Phase 4)
- `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` ([P4-T2], green since Phase 4)
- `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` ([P4-T3])
- `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` (pre-existing)
- `GetMoveDiagnostics_NullAppointment_DoesNotThrow` (pre-existing)
- `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (pre-existing)
- `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` (pre-existing; deleted by [P5-T11])

## Determinism note

No test in this set reads a wall clock, sleeps, or touches the filesystem. The suspension in
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning` uses `Task.Yield`, never `Task.Delay` and
never `Thread.Sleep`, so the happens-before assertion is structural rather than timing-based.
