# Phase 4 — QFC Stopwatch and Culture Regression Tests, Red State (#443)

Timestamp: 2026-08-26T11-14
Task: [P4-T4] `[expect-fail]`
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p4-t4"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX file: `TestResults\p4-t4\<account>_<HOST>_2026-08-26_11_14_45_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Failed.
Total tests: 7
     Passed: 5
     Failed: 2
```

The two tests the task requires to fail, fail. The five others pass, including the
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` test that [P4-T3] updated to populate
`_stopWatchMoved`; that update is forward-compatible and does not disturb the pre-fix behaviour.

### Failing tests with verbatim failure messages

**1. `WriteMetricsAsync_ReadsMovedStopwatchForDuration` ([P4-T1], root cause RC-2)**

> Test method QuickFiler.Controllers.Tests.QfcHomeControllerMetricsTests.WriteMetricsAsync_ReadsMovedStopwatchForDuration threw exception:
> Moq.MockException:
> Expected invocation on the mock once, but was 0 times: x => x.GetMoveDiagnostics(It.IsAny\<string\>(), It.IsAny\<string\>(), It.Is\<double\>(d => d > 0), It.IsAny\<string\>(), It.IsAny\<DateTime\>(), null)

The fixture sets `_stopWatchMoved` to a stopped stopwatch reporting 30 seconds and `_stopWatch` to a
freshly constructed stopwatch. The pre-fix source at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:121` reads `StopWatch.Elapsed.Seconds`, that is
the session stopwatch, so the `duration` argument reaching `GetMoveDiagnostics` is `0` and the
`d > 0` predicate never matches.

**2. `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` ([P4-T2], root cause RC-9)**

> Test method QuickFiler.Controllers.Tests.QfcHomeControllerMetricsTests.WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator threw exception:
> Moq.MockException:
> Expected invocation on the mock once, but was 0 times: x => x.GetMoveDiagnostics(It.IsAny\<string\>(), It.Is\<string\>(text => !(text.Contains(","))), It.IsAny\<double\>(), It.IsAny\<string\>(), It.IsAny\<DateTime\>(), null)

Under `de-DE` the pre-fix source formats the `durationMinutesText` field with the current culture,
producing a decimal comma, so the "contains no comma" predicate never matches.

### Passing tests in this run

- `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` (pre-existing, issue #97)
- `GetMoveDiagnostics_NullAppointment_DoesNotThrow` (pre-existing, issue #97)
- `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` (updated by [P4-T3])
- `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (pre-existing, issue #222)
- `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` (pre-existing, issue #222;
  deleted later by [P5-T11])

## Determinism note

Neither new test reads a wall clock. `StoppedStopwatchWithElapsed(30)` assigns the stopwatch's
internal elapsed-tick field directly to `Stopwatch.Frequency * 30`, so the reported interval is
exactly 30 seconds on any host. A start/stop pair was deliberately not used, because it does not
guarantee a non-zero elapsed value and would make the assertion time-dependent. The culture test's
assertion is independent of the elapsed value entirely, and its `finally` block restores
`CultureInfo.CurrentCulture`.
