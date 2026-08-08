# qfc-home-controller-metrics-never-flushed (Issue #442)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-home-controller-metrics-never-flushed/ (Issue #442)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #442
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/442
- Last Updated: 2026-08-08
## Summary

`QfcHomeController.WriteMetricsAsync` enqueues QuickFiler session metrics into a `BlockingCollection<string>` that no consumer ever drains, so the metrics are never written to disk. The consumer guard can never pass and the consumer timer is never started.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon; any session that completes and writes metrics
- Data source or fixture: Live Outlook mailbox

## Steps to Reproduce

1. Launch QuickFiler and file at least one batch of messages so a metrics line is produced.
2. Complete the session so `WriteMetricsAsync` runs.
3. Inspect the configured session metrics file (`Globals.FS.Filenames.EmailSession`).

## Expected Behavior

The session metrics line is appended to the configured metrics file.

## Actual Behavior

No metrics line is written. The line is added to the in-memory `_metrics` collection and remains there until the process exits.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; the failure is silent.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no user-visible functional regression in filing, but the QuickFiler performance-metrics feature produces no output at all, so the data intended to drive tuning does not exist.

## Suspected Cause / Notes

Found during read-only research for epic child F7 (`quickfiler-qfc-home-controller-coverage`, issue #433) under parent epic #136. Report-only; deliberately not fixed inside a coverage child. Evidence is recorded in
`docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/research/QfcHomeController.Metrics.cs.research.2026-08-07T20-50.md` (findings D3, D4, D5) and
`.../research/QfcHomeController.cs.research.2026-08-07T20-50.md`.

- `_metricsConsumers` (`QuickFiler/Controllers/QfcHomeController.cs:356`) is initialized to `0` and is only ever **decremented** (`QfcHomeController.Metrics.cs:228`, `QfcHomeController.cs:366`). No code path in the repository increments it. The guard `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` (`QfcHomeController.Metrics.cs:226`) can therefore never be true, so `TimedConsumerAsync` is never subscribed.
- Even if the guard passed, `QfcHomeController.Metrics.cs:229-230` constructs `new System.Timers.Timer(2000)` into a **local**, subscribes `TimedConsumerAsync` to `Elapsed`, and never calls `Start()` or sets `Enabled`. The local is immediately eligible for collection and is never disposed.
- `_metrics` never receives `CompleteAdding()`, so `QfcHomeController.cs:367` (`foreach` over `GetConsumingEnumerable`) would block indefinitely if the consumer ever did run.
- `_fileName` (`QfcHomeController.cs:358`) is assigned at `Metrics.cs:153` and never read; `TimedConsumerAsync` uses `Globals.FS.Filenames.EmailSession` instead. It is also `static` on an instance-scoped concern.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the consumer-scheduling path in `QfcHomeController.Metrics.cs` and `TimedConsumerAsync` in `QfcHomeController.cs`, behind an injectable writer seam so no test touches disk.
- [ ] Integration scenario to retest: complete a QuickFiler session and confirm the metrics file receives the expected line.
- [ ] Manual verification notes: confirm the timer is started and disposed, and that `CompleteAdding()` is called on shutdown so the consumer terminates.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
