# Phase 0 — Pre-Change Line Counts of the Seven Owned Files

Timestamp: 2026-08-26T10-42
Task: [P0-T10]
Command: `wc -l QuickFiler/Controllers/QfcHomeController.cs QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
EXIT_CODE: 0

## Output Summary

| # | Owned file | Pre-change lines | Headroom to the 500-line cap |
| --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | **487** | 13 |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | **234** | 266 |
| 3 | `QuickFiler/Controllers/EfcHomeController.cs` | **441** | 59 |
| 4 | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | **87** | 413 |
| 5 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | **144** | 356 |
| 6 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | **421** | 79 |
| 7 | `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | **244** | 256 |

Total across the seven files: 2058 lines.

Two files carry meaningful pressure against the 500-line cap and are tracked explicitly by later
tasks:

- `QuickFiler/Controllers/QfcHomeController.cs` at 487 lines matches the figure the plan's settled
  design decision 10 records. [P6-T7] requires the post-format count to be at or below this 487.
  The Phase 5 deletions ([P5-T10] and [P5-T12]) are what create the headroom.
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` at 421 lines matches the figure
  the plan's [P5-T15] records, leaving 79 lines of headroom before [P5-T11] recovers more by
  deleting `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`.

The remaining five files have ample headroom.
