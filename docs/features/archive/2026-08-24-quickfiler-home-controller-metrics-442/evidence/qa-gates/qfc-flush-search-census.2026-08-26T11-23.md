# Phase 5 — QFC #442 Post-Fix Search Census

Timestamp: 2026-08-26T11-23
Task: [P5-T14]
Command: `git grep -nE "NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName" -- QuickFiler/Controllers/`
EXIT_CODE: 1 (no match)

This artifact carries the search half of acceptance criterion AC-3. Its pre-fix counterpart is
`evidence/baseline/defect-site-census.2026-08-26T10-42.md`, search 7.

## Output Summary

| Search | Scope | Pre-fix hits | Post-fix hits |
| --- | --- | --- | --- |
| `NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName` | `QuickFiler/Controllers/` | **13** | **0** |

The post-fix count is zero and the recorded pre-fix count is greater than zero, so the task's
acceptance condition holds and the gate is falsifiable rather than vacuous.

The command produced no output and `git grep` exited 1, its no-match status.

## Pre-fix hits and their disposition

| Pre-fix site | Identifier | Disposition |
| --- | --- | --- |
| `QfcHomeController.Metrics.cs:153` | `_fileName` | deleted with the assignment, by [P5-T8] |
| `QfcHomeController.Metrics.cs:154` | `NonBlockingProducer` | replaced with `await MetricsFileWriter(...)`, by [P5-T8] |
| `QfcHomeController.Metrics.cs:190` | `NonBlockingProducer` | overload deleted, by [P5-T9] |
| `QfcHomeController.Metrics.cs:197` | `NonBlockingProducer` | deleted with the overload body, by [P5-T9] |
| `QfcHomeController.Metrics.cs:201` | `NonBlockingProducer` | overload deleted, by [P5-T9] |
| `QfcHomeController.Metrics.cs:226` | `_metricsConsumers` | unreachable guard deleted, by [P5-T9] |
| `QfcHomeController.Metrics.cs:228` | `_metricsConsumers` | deleted with the guard body, by [P5-T9] |
| `QfcHomeController.Metrics.cs:230` | `TimedConsumerAsync` | timer subscription deleted, by [P5-T9] |
| `QfcHomeController.cs:356` | `_metricsConsumers` | field deleted, by [P5-T10] |
| `QfcHomeController.cs:357` | `_lockObject` | field deleted, by [P5-T10] |
| `QfcHomeController.cs:358` | `_fileName` | field deleted, by [P5-T10] |
| `QfcHomeController.cs:362` | `TimedConsumerAsync` | method deleted, by [P5-T10] |
| `QfcHomeController.cs:366` | `_metricsConsumers` | deleted with the method body, by [P5-T10] |

The `_metrics` `BlockingCollection` field at `QfcHomeController.cs:353-355` was deleted by [P5-T10]
in the same edit. It is not part of the alternation the criterion names, so it was verified
separately: `git grep -n '_metrics' -- QuickFiler/` returns no match (exit 1).

## What the zero count proves

AC-3 uses this search as the structural half of the flush-timing invariant. A zero result
establishes that no part of the flush was left behind on a timer, on a background consumer, or in
residual controller state:

- no producer remains that hands lines to a queue instead of writing them,
- no `System.Timers.Timer` remains to defer the write,
- no consumer-count field remains to gate a consumer that could never start,
- no static `_fileName` remains to carry state between a producer and a deferred consumer,
- no `_lockObject` remains from the abandoned locking scheme.

The behavioural half of the invariant is carried by
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`, recorded green in
`evidence/regression-testing/qfc-flush-green.2026-08-26T11-23.md`.
