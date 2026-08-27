# Phase 6 — Post-Format File Size Audit

Timestamp: 2026-08-26T11-30
Task: [P6-T7]
Command: `wc -l QuickFiler/Controllers/QfcHomeController.cs QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
EXIT_CODE: 0

Counts are taken after the [P6-T1] final formatting pass.

## Output Summary

| # | Owned file | Pre-change ([P0-T10]) | Post-format | Signed difference | At most 499 |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | 487 | **449** | -38 | yes |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 234 | **215** | -19 | yes |
| 3 | `QuickFiler/Controllers/EfcHomeController.cs` | 441 | **445** | +4 | yes |
| 4 | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 87 | **124** | +37 | yes |
| 5 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 144 | **147** | +3 | yes |
| 6 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 421 | **453** | +32 | yes |
| 7 | `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | 244 | **490** | +246 | yes |

Both acceptance conditions hold:

- **Every one of the seven counts is at most 499.** The largest is 490.
- **`QuickFiler/Controllers/QfcHomeController.cs` is at or below its pre-change count of 487.** It
  is 449, which is 38 lines below the pre-change count and 51 lines below the cap.

## Notes

`QfcHomeController.cs` fell from 487 to 449, close to the plan's settled decision 10 estimate of
"roughly 33 lines deleted, taking it to about 454". The realised reduction is 38 lines: [P5-T10]
deleted the four metrics-queue members and `TimedConsumerAsync`, and [P5-T12] deleted three
now-unused `using` directives. This resolves the cap pressure by design rather than by working
around it.

`EfcHomeControllerMetricsTests.cs` shows the largest growth, +246, because it received eleven of
this feature's new tests. It was compacted during Phase 1 to stay under the cap and ends at 490,
9 lines below it.

`QfcHomeControllerMetricsTests.cs` grew by only 32 net despite receiving seven new tests, because
[P5-T15] removed a dead strict-`MockRepository` fixture and consolidated the duplicated arrange code
of the two issue #97 tests into `BuildLooseMetricsController()`.
