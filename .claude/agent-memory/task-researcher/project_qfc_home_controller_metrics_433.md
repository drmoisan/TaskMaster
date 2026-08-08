---
name: qfc-home-controller-metrics-433
description: Issue #433 (epic #136 child F7) research on QfcHomeController.Metrics.cs — unreachable BlockingCollection branch, dead metrics-consumer path, ratified delegate-writer precedent, #424 non-overlap
metadata:
  type: project
---

Research completed 2026-08-07 for epic child F7 (`quickfiler-qfc-home-controller-coverage`, issue #433) of the QuickFiler per-file coverage epic (#136), targeting `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (234 lines).

**Why:** Issue #136 mandates one production file at a time; F7's plan needs per-member gap analysis without running coverage (F1 owns the harness).

**How to apply:** Reuse these findings before re-deriving them for any QuickFiler metrics or producer-queue work.

Non-obvious findings worth keeping:

1. **`BlockingCollection<T>.TryAdd(T,int,CancellationToken)` cannot produce an OCE with an uncancelled token.** An internal cancellation from `CompleteAdding()` is converted to `InvalidOperationException` before it escapes. So the `else` arm of `catch (OperationCanceledException)` in `NonBlockingProducer` (the 20 ms `TimeProvider.Delay` retry) is unreachable through the concrete type, and `TryAdd` is non-virtual so subclassing cannot intercept. An injectable `Func<string,int,CancellationToken,bool>` adder seam is the only route to that branch.

2. **The QuickFiler metrics consumer never runs.** `_metricsConsumers` (`QfcHomeController.cs:356`) is initialized to 0 and only ever decremented — never incremented anywhere in the repo — so `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` is permanently false and `TimedConsumerAsync` is never subscribed. Even if it were, the `System.Timers.Timer` built at the call site is a local that is never started. Net effect: `WriteMetricsAsync` output accumulates in memory and is never written to disk.

3. **`WriteMetricsAsync` reads the wrong stopwatch.** It reads `StopWatch` (`_stopWatch`) while its own commented-out predecessor line read `_stopWatchMoved`; production calls `SwapStopWatch()` before the write, so the recorded duration is ~0 s. The sibling `QuickFileMetrics_WRITE` reads `_stopWatchMoved`. Both also use `TimeSpan.Seconds` (0-59 component) instead of `TotalSeconds`.

4. **Ratified precedent for the metrics writer seam:** `EfcHomeControllerDependencies.MetricsLineWriter` is an `Action<string,string[],string>` defaulting to `FileIO2.WriteTextFile`, paired with a pure static `EfcHomeController.BuildQuickFileMetricLines`. Same shape is the right answer for Qfc — but declare it locally; the Efc dependencies type belongs to sibling child F8.

5. **`Stopwatch.Elapsed` is non-virtual on a concrete class**, so no seam rung can control it. Pure-function extraction outranks all three seam rungs here. Replacing `_stopWatch` is blocked by live pins (`RunAsync_ExecutesCorrectly` asserts `StopWatch.IsRunning`; `StopWatch_PropertyWorksCorrectly` asserts instance identity).

6. **Issue #424 did not touch `QfcHomeController.Metrics.cs`** — its edits are confined to `RunAsync` in `QfcHomeController.cs`. #424 has already landed in the worktree (200 ms poll + `QfcScanProgressBandMapper` + `DefaultFirstBatchDeadline` present). Its AC 12 freezes `QfcHomeControllerIterationTests.cs` and constrains `QfcHomeControllerIssue218Tests.cs` — both are F7-owned files, so accidental edits are possible.

7. **`QfcHomeController.QuickFileMetrics_WRITE(string)` has no production caller** — it exists only to satisfy `IFilerHomeController`, whose EFC implementation throws `NotImplementedException`.

8. **`QfcHomeController.cs` is at 487/500 lines.** `QfcHomeController.cs:353-386` is a 34-line block of metrics-only members mis-located in the main partial and movable to the Metrics partial (which has ~266 lines of headroom) if headroom is ever needed.

Related: [[qfc-high-confidence-dual-pipeline]], [[qfc424-high-confidence-startup-stall]]
