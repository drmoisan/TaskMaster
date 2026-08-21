# qfc-home-controller-metrics-duration-misread (Issue #443)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-home-controller-metrics-duration-misread/ (Issue #443)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #443
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/443
- Last Updated: 2026-08-08
## Summary

QuickFiler session-duration metrics are recorded incorrectly: `WriteMetricsAsync` reads the freshly restarted stopwatch instead of the swapped-out one, and both metrics writers use `TimeSpan.Seconds` (the 0-59 component) instead of `TotalSeconds`.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon; any session that records metrics
- Data source or fixture: Live Outlook mailbox

## Steps to Reproduce

1. Launch QuickFiler and file messages for a measurable interval longer than 60 seconds.
2. Complete the session so the metrics write path runs.
3. Inspect the recorded duration value.

## Expected Behavior

The recorded duration equals the elapsed time of the completed filing interval.

## Actual Behavior

Two independent errors corrupt the value:

- On the end-of-database path the recorded duration is approximately 0 seconds regardless of the real interval.
- Where a duration is recorded at all, an interval of 90 seconds is written as 30 because only the seconds component is taken.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; the values are written to the session metrics CSV.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no user-visible functional regression in filing, but recorded performance metrics are wrong, so any tuning decision based on them is unsound. Note this defect is currently masked by the separate defect in which metrics are never flushed to disk at all.

## Suspected Cause / Notes

Found during read-only research for epic child F7 (`quickfiler-qfc-home-controller-coverage`, issue #433) under parent epic #136. Report-only; deliberately not fixed inside a coverage child. Evidence is recorded in
`docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/research/QfcHomeController.Metrics.cs.research.2026-08-07T20-50.md` (findings D1, D2, D8, D9).

- **Wrong stopwatch.** `QfcHomeController.Metrics.cs:121` reads `StopWatch.Elapsed` (`_stopWatch`); the commented-out line 120 shows it previously read `_stopWatchMoved`. Production calls `SwapStopWatch()` *before* the metrics write on the end-of-database path (`QfcFormController.EventHandlers.cs:191-192` -> `BackGroundMoveAsync` -> `WriteMetrics`), so `_stopWatch` at that moment is the freshly restarted stopwatch and the true interval sits unread in `_stopWatchMoved`. The sibling method `QuickFileMetrics_WRITE` (`Metrics.cs:42`) reads `_stopWatchMoved` — the two writers disagree. On the `MoveAndIterate` path (`EventHandlers.cs:157-161`) the swap in `LoadUiFromQueue` races `BackGroundMoveAsync`, making the value non-deterministic as well.
- **Seconds truncation.** `Metrics.cs:42` and `Metrics.cs:121` use `TimeSpan.Seconds` rather than `TotalSeconds`. `Metrics.cs:44` compounds it by deriving `startTime` from the full `Elapsed` while `duration` uses the truncated value, so the calendar appointment span and the CSV duration disagree.
- **Related formatting defects, same code path (fix together or split as judged).** `Metrics.cs` lines 31, 53, 56, 108, 110, 132, 135 format with `CultureInfo.CurrentCulture`, so on a non-invariant culture the `##0.00` numbers gain a comma decimal separator and corrupt the comma-delimited CSV. The `"hh:mm"` format renders 14:30 as `02:30` with no AM/PM designator.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the duration-text construction in `QfcHomeController.Metrics.cs`, extracted as a pure function so the value can be asserted without a live stopwatch.
- [ ] Integration scenario to retest: run a filing session longer than 60 seconds on both the end-of-database and `MoveAndIterate` paths and confirm the recorded duration matches.
- [ ] Manual verification notes: confirm CSV output is culture-invariant and that recorded times are unambiguous.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
