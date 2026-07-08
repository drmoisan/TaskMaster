# QA Gate — Phase 3 Banned-API Sweep (P3-T7)

Timestamp: 2026-06-28T19-45
Command: grep -nE "DateTime\.Now|Task\.Delay" in the four target files

Result — zero ACTIVE (non-commented) banned-API matches. All remaining matches are commented-out (`//`) and out of scope:

| File | Active matches | Commented-only remaining |
|------|----------------|--------------------------|
| QfcDatamodel.FrameBuilding.cs | 0 | lines 54, 61, 76, 79 (`//logger.Debug ... DateTime.Now`) |
| QfcDatamodel.QueueProcessing.cs | 0 | none |
| QfcHomeController.cs | 0 | lines 44, 264, 278, 283, 289 (`//logger.Debug ... DateTime.Now`) |
| QfcHomeController.Metrics.cs | 0 | lines 28, 29 (`//var ... DateTime.Now`) |

All eight active sites replaced with `TimeProvider.GetLocalNow().LocalDateTime` (timestamps) or `TimeProvider.Delay(TimeSpan.FromMilliseconds(n)[, token])` (delays). Note: `TimeProvider.Delay` is not a banned symbol (only `Task.Delay` is banned).

Out-of-scope files (e.g., EfcHomeController, QfcFormController, QfcItemController, QfcQueue) were not touched.
