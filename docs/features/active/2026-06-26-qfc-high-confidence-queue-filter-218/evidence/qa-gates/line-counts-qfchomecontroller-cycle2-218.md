# Line Counts After QfcHomeController Extraction (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command: `$files=@('QuickFiler/Controllers/QfcHomeController.cs','QuickFiler/Controllers/QfcHomeController.Metrics.cs','QuickFiler/Controllers/QfcHomeController.Iteration.cs'); foreach($f in $files){ $n=(Get-Content -LiteralPath $f).Count; ... } | Format-Table` (line values via `(Get-Content).Count`).

EXIT_CODE: 0

Output Summary:

| File | Lines | Limit | Result |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcHomeController.cs | 454 | 500 | PASS |
| QuickFiler/Controllers/QfcHomeController.Metrics.cs | 226 | 500 | PASS |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 82 | 500 | PASS |

- After the metrics extraction (P2-T1), `QfcHomeController.cs` measured 525 lines — still over 500 — so the P2-T3 fallback was applied: the queue-iteration block (`IterateQueueAsync`, `Iterate`, `Iterate2`, `SwapStopWatch`) was extracted verbatim into the new `QfcHomeController.Iteration.cs` partial and wired into `QuickFiler/QuickFiler.csproj`.
- After the iteration extraction, `QfcHomeController.cs` is 454 lines — under the 500-line limit.
- Metrics partial (`QfcHomeController.Metrics.cs`, 226 lines) contains `QuickFileMetrics_WRITE`, `WriteMetricsAsync`, `WriteMoveToCalendar`, and both `NonBlockingProducer` overloads, moved verbatim. The `_metrics`/`_metricsConsumers`/`_lockObject`/`_fileName` fields and `TimedConsumerAsync` remain in `QfcHomeController.cs` (shared across the partial by partial-class semantics; behavior unchanged).
- Iteration partial (`QfcHomeController.Iteration.cs`, 82 lines) contains the four queue-iteration methods moved verbatim, preserving `ConfigureAwait(false)` and cancellation propagation.
- The home-controller public surface (`IQfcHomeController`) is unchanged; method accessibilities were preserved across the move.
