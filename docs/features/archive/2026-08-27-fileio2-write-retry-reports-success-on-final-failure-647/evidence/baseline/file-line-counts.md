# P0-T8 — Pre-Change Line Counts of the Five Footprint Files

Timestamp: 2026-08-31T18-45
Command: Get-Content -LiteralPath <path> and read the returned array's Count property, once per path
EXIT_CODE: 0

## Counts

- `UtilitiesCS/To Depricate/FileIO2.cs` = 232
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` = 215
- `TaskMaster/AppGlobals/AppOlObjects.cs` = 467
- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` = 116
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` = 453

DRIFT: none. All five observed counts equal the values recorded in P0-T8 of the plan while it was authored (232, 215, 467, 116, 453 in the same order).

Output Summary: Five integer counts recorded, one per named path. No drift against the plan's authoring-time observation. `TaskMaster/AppGlobals/AppOlObjects.cs` at 467 leaves 33 lines of headroom under the 500-line limit, which is the constraint P4-T5 works within.
