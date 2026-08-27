# Phase 5 — QFC Metrics Flush Scoped Suite, Green State (#442)

Timestamp: 2026-08-26T11-23
Task: [P5-T13]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p5-t13"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

TRX file: `TestResults\p5-t13\<account>_<HOST>_2026-08-26_11_23_58_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Successful.
Total tests: 11
     Passed: 11
 Total time: 1.5295 Seconds
```

- Passed: **11**
- Failed: **0**
- Skipped: **0**

| Test | Source task | Red ([P5-T7]) | Green (this run) |
| --- | --- | --- | --- |
| `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` | [P5-T2] | Failed | **Passed** (23 ms) |
| `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` | [P5-T3] | Failed | **Passed** (4 ms) |
| `WriteMetricsAsync_PassesUncancelledTokenToWriter` | [P5-T4] | Failed | **Passed** (6 ms) |
| `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` | [P5-T5] | Failed | **Passed** (2 ms) |
| `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` | [P5-T6] | Passed | **Passed** (1 ms) |

The remaining six tests in the class also pass:
`QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`,
`GetMoveDiagnostics_NullAppointment_DoesNotThrow`,
`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`, and
`QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`.

The class held 12 tests at [P5-T7] and holds 11 here because [P5-T11] deleted
`NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`, whose production call site no
longer exists.

## Changes that produced this transition

| Task | Change |
| --- | --- |
| [P5-T8] | `WriteMetricsAsync` now filters null and whitespace-only diagnostic entries and then awaits `MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None)`. The token is deliberately `CancellationToken.None`, never the controller's `Token`. |
| [P5-T9] | Both `NonBlockingProducer` overloads and the unreachable consumer-scheduling block were deleted from `QfcHomeController.Metrics.cs`. |
| [P5-T10] | `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName`, and `TimedConsumerAsync` were deleted from `QfcHomeController.cs`. |
| [P5-T11] | `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` was deleted. |
| [P5-T12] | `using System.Collections.Concurrent;`, `using System.Timers;`, and `using System.Linq;` were removed from `QfcHomeController.cs`; the analyzer gate reports zero errors. |

## Determinism correction made during this task

The first execution of this run passed 11 of 11 but three tests each took **10 seconds**:
`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`, and
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`.

Cause: those three tests do not assign `MetricsFileWriter`, so the seam kept its production default
`FileIO2.WriteTextFileAsync`. Before [P5-T8] the method ended at `NonBlockingProducer`, which only
enqueued and never touched the filesystem; after [P5-T8] it reaches the writer. The default writer
composes a path under the fixture's `C:\FakeDocs` root, which does not exist, and
`FileIO2.WriteTextFileAsync` catches the resulting `IOException` and retries up to 100 times with
`await Task.Delay(100)` between attempts, which is exactly 10 seconds of wall-clock wait per test.

That is a filesystem dependency and a wall-clock wait inside a unit test, prohibited by
`.claude/rules/general-unit-test.md` and by AC-17. It was introduced by this feature's own change,
so correcting it is in scope.

Correction: `BuildLooseMetricsController()` now assigns a no-op `MetricsFileWriter` returning
`Task.CompletedTask`. Every test that asserts on the flush still overrides it with its own capturing
delegate, so no assertion was weakened. After the correction the whole class runs in 1.5295 seconds
and no test creates, probes, or waits on a file. `C:\FakeDocs` was confirmed absent from the
filesystem both before and after, so no stray file was left behind.
