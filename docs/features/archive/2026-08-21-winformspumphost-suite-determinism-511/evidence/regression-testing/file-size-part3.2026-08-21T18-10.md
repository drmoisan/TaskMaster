# P3-T2 — `Part3.cs` Line Count After Both Regression Tests

Timestamp: 2026-08-22T10-35

Command:
```
(Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs).Count
```

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Pre-change count (P0-T7 baseline, before Phase 1) | **290** |
| After P1-T1 (`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`) | 339 (+49) |
| Post-change count, after P3-T1 (`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`) | **398** (+59) |
| 500-line cap | 398 < 500 — **holds**, 102 lines of headroom remain |

Acceptance: the recorded post-change count of 398 is less than 500.

Line-number stability re-verified in the same measurement (spec AC 1 and AC 2 cite these):

| Method | Declaration line |
| --- | --- |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | **131** |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | **175** |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` (P1-T1) | 301 |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` (P3-T1) | 356 |

Both new tests were appended after the last existing method, so neither pre-existing declaration
line moved.

`dotnet tool run csharpier check .` reports `Checked 1517 files` with exit code 0, so the recorded
count is the formatter-stable count and no later format step will change it.

Note: the +59 delta for P3-T1 includes the corrected `<remarks>` block recording the measurement
described in `webview-child-handle-measurement.2026-08-21T18-10.md`. The first draft of the method
was 53 lines; the corrected form is 59.
