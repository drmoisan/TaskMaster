# Baseline — Write Set Line Counts (P0-T8)

Timestamp: 2026-09-05T19-35

Command: `(Get-Content -LiteralPath '<path>').Count`, run once per file listed below.

EXIT_CODE: 0

Output Summary:

| File | Counting command | Observed | Expected |
|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | `(Get-Content -LiteralPath 'UtilitiesCS/Threading/UiThread.cs').Count` | 172 | 172 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | `(Get-Content -LiteralPath 'UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs').Count` | 77 | 77 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/UiThread_Tests.cs').Count` | 179 | 179 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count` | 514 | 514 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs').Count` | 206 | 206 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs').Count` | 348 | 348 |
| `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs').Count` | 241 | 241 |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs').Count` | 201 | 201 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `(Get-Content -LiteralPath 'QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs').Count` | 320 | 320 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs').Count` | 393 | 393 |

Every observed count equals its expected value. There is no deviation to report before Phase 1
begins.

The three remaining production files in the Write Set — `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, and
`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` — are deliberately outside this baseline. The
edits P1-T6 and P1-T7 make to them are one-for-one line replacements that cannot change a line
count, so no size gate in Phases 2, 4, or 7 reads a baseline for them.
