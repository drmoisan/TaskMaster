# Phase 4 — QFC Stopwatch and Culture Scoped Suite, Green State (#443)

Timestamp: 2026-08-26T11-16
Task: [P4-T10]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p4-t10"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

TRX file: `TestResults\p4-t10\<account>_<HOST>_2026-08-26_11_16_18_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Successful.
Total tests: 7
     Passed: 7
```

- Passed: **7**
- Failed: **0**
- Skipped: **0**

| Test | Source task | Red ([P4-T4]) | Green (this run) |
| --- | --- | --- | --- |
| `WriteMetricsAsync_ReadsMovedStopwatchForDuration` | [P4-T1] | Failed | **Passed** (16 ms) |
| `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` | [P4-T2] | Failed | **Passed** (4 ms) |
| `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | updated by [P4-T3] | Passed | **Passed** (3 ms) |
| `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` | pre-existing | Passed | Passed (317 ms) |
| `GetMoveDiagnostics_NullAppointment_DoesNotThrow` | pre-existing | Passed | Passed (6 ms) |
| `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` | pre-existing | Passed | Passed (1 ms) |
| `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` | pre-existing | Passed | Passed (4 ms) |

## Fixes that produced this transition

| Task | Change |
| --- | --- |
| [P4-T5] | Added `using System.Globalization;` to `QfcHomeController.Metrics.cs`. |
| [P4-T6] | `QuickFileMetrics_WRITE` now reads `_stopWatchMoved.Elapsed.TotalSeconds` instead of the 0-59 `Seconds` component. |
| [P4-T7] | `WriteMetricsAsync` now reads `_stopWatchMoved.Elapsed.TotalSeconds` instead of the session stopwatch's 0-59 `Seconds` component, and the commented-out line above it was deleted so the AC-7 search gate can reach zero. |
| [P4-T8] | `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);` replaces the reconstruction from a truncated integer cast. |
| [P4-T9] | `CultureInfo.InvariantCulture` passed to the four numeric format calls at lines 54, 57, 134, and 137. The date and time format calls are unchanged; the `"hh:mm"` 12-hour defect is CFN-4 and out of scope. |

`QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` continuing to pass confirms that adding the
invariant-culture argument to the two format calls in that method did not disturb the injected-clock
seam, whose assertions are on the date and time fields rather than the numeric ones.
