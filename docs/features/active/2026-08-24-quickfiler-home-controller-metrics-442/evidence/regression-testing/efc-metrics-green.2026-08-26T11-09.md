# Phase 2 — EFC Metrics Scoped Suite, Green State (#451)

Timestamp: 2026-08-26T11-09
Task: [P2-T9]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p2-t9"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

TRX file: `TestResults\p2-t9\<account>_<HOST>_2026-08-26_11_09_32_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Successful.
Total tests: 13
     Passed: 13
```

- Passed: **13**
- Failed: **0**
- Skipped: **0**

Every test named in [P1-T1] through [P1-T9] passes:

| Test | Source task | Result |
| --- | --- | --- |
| `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` | [P1-T1] | Passed (37 ms) |
| `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` | [P1-T2] | Passed (1 ms) |
| `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields` | [P1-T3] | Passed (< 1 ms) |
| `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding` | [P1-T4] | Passed (33 ms) |
| `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` | [P1-T5] | Passed (< 1 ms) |
| `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator` | [P1-T6] | Passed (< 1 ms) |
| `StopWatch_AfterControllerConstruction_IsRunning` | [P1-T7] | Passed (112 ms) |
| `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` | [P1-T8] | Passed (3 ms) |
| `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload` | [P1-T9] | Passed (6 ms) |

The four pre-existing tests in the class also pass unchanged:
`BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines`,
`QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter`,
`QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter`, and
`QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter`.

## Fixes that produced this transition

| Task | Change |
| --- | --- |
| [P2-T1] | Added `using System.Globalization;` to `EfcHomeController.Metrics.cs`. |
| [P2-T2] | Replaced `_stopWatch = new Stopwatch();` with `_stopWatch = Stopwatch.StartNew();` at `EfcHomeController.cs:76` and `:225`. |
| [P2-T3] | Changed the argument at `EfcHomeController.Metrics.cs` from the 0-59 `Seconds` component to `TotalSeconds`. |
| [P2-T4] | Widened both `int elapsedSeconds` parameters to `double elapsedSeconds`. |
| [P2-T5] | Inserted the missing comma field separator between the interpolated `ToRecipientsName` and `SenderName`. |
| [P2-T6] | Wrapped `ToRecipientsName`, `SenderName`, and `selectedFolder` in `QfcCollectionController.xComma(...)`. |
| [P2-T7] | Passed `CultureInfo.InvariantCulture` to the two numeric format calls. |
| [P2-T8] | Implemented `QuickFileMetrics_WRITE(string filename)` as guarded delegation to the three-argument overload. |
