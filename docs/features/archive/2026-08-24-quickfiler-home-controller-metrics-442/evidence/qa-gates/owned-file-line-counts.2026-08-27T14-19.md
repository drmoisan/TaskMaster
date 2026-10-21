# Phase 6 — Post-Format Line Counts of the Seven Owned Files

Timestamp: 2026-08-27T14-19
Task: [P6-T7]
Command: `wc -l` over the seven owned files, run after the [P6-T1] formatting pass
EXIT_CODE: 0

## Output Summary

**Every one of the seven counts is at most 499, and
`QuickFiler/Controllers/QfcHomeController.cs` is at or below its pre-change count of 487.** Both
acceptance conditions hold.

| # | Owned file | Pre-change ([P0-T10]) | Post-format | Change | Headroom to 500 |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | 487 | **449** | -38 | 51 |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 234 | **215** | -19 | 285 |
| 3 | `QuickFiler/Controllers/EfcHomeController.cs` | 441 | **445** | +4 | 55 |
| 4 | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 87 | **124** | +37 | 376 |
| 5 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 144 | **147** | +3 | 353 |
| 6 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 421 | **453** | +32 | 47 |
| 7 | `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | 244 | **490** | +246 | 10 |

Total across the seven files: 2323 lines, against 2058 pre-change.

### The two files that carried cap pressure

- `QuickFiler/Controllers/QfcHomeController.cs` closes at **449**, which is 38 lines below its
  pre-change 487 and 5 lines below the approximately-454 target the spec's AC-21 names. The
  reduction is the removal of the defective metrics machinery (the `BlockingCollection` field,
  `_metricsConsumers`, the two unused `static` fields, the `async void TimedConsumerAsync` handler
  and three `using` directives) by [P5-T10] and [P5-T12].
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` closes at **490**, the tightest of
  the seven with 10 lines of headroom. It absorbed the bulk of this feature's new EFC test coverage.
  It is within the cap, and it is flagged here so that any future addition to that file is
  understood to require an extraction rather than an append.

`QuickFiler/Controllers/EfcHomeController.cs` grew by 4 lines rather than shrinking; that growth is
the `Stopwatch.StartNew()` conversion plus the `private int _isExecuting` declaration, and it leaves
55 lines of headroom.

The formatter rewrote zero files in this pass (see
`evidence/qa-gates/csharpier-format.2026-08-27T14-18.md`), so these counts are simultaneously the
post-change and the post-format counts.
