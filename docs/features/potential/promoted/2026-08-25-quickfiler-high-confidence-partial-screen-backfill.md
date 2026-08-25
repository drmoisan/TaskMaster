# quickfiler-high-confidence-partial-screen-backfill (Issue #608)

- Date captured: 2026-08-25
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-high-confidence-partial-screen-backfill/ (Issue #608)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #608
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/608
- Last Updated: 2026-08-25
## Summary

QuickFiler high-confidence mode stops a scan when the first-batch deadline expires and returns a non-empty partial batch immediately, even though fewer than the current screen's `ItemsPerIteration` messages qualified and more source messages remain. The regression affects the first screen and subsequent screens.

## Environment

- OS/version: Windows 11; live Outlook desktop session
- Python version: n/a (C# / .NET Framework 4.8.1 VSTO add-in)
- Command/flags used: QuickFiler launched with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Outlook folder with sparse messages above the configured confidence threshold; the observed form displayed seven or eight items per screen

## Steps to Reproduce

1. Enable QuickFiler high-confidence mode with a threshold that few available messages meet.
2. Launch QuickFiler against a folder where the current form size yields an `ItemsPerIteration` value of seven or eight.
3. Let the first high-confidence scan evaluate many candidates. In the observed run it scanned nearly 40 messages and accepted one before the deadline expired.
4. Observe that QuickFiler returns and displays that one accepted message instead of continuing until the requested screen count is satisfied or the source is exhausted.
5. File the displayed messages and advance to later screens; the same partial-return behavior can recur.

## Expected Behavior

QuickFiler must use the current form's `ItemsPerIteration` value as the requested high-confidence batch size. It must continue dequeuing, scoring, and discarding below-threshold candidates until it has collected that many qualifying messages or it has genuinely exhausted the available source. This contract must apply to the initial screen and every subsequent screen.

## Actual Behavior

`QfcStreamingDequeueConfidenceGate.DequeueAsync` returns its current `accepted` list when `DefaultFirstBatchDeadline` expires, even when `accepted.Count` is greater than zero but less than `quantity` and `_sourceActive()` indicates that more messages remain. The initial `QfcHomeController.RunAsync` path and later `QfcHomeController.IterateQueueAsync` path both use deadline-bearing dequeue calls, so either path can surface an undersized screen without source exhaustion.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: observed approximately 40 candidates scanned, one accepted, then an immediate return to a one-item screen

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the mode violates its page-fill contract and can require repeated undersized screens while qualifying messages remain. The behavior affects normal use of high-confidence mode and contradicts the completed issue #233 streaming-backfill acceptance criteria.

## Suspected Cause / Notes

- Closed issue #233 explicitly required a request for N high-confidence items to scan until N qualifying items were collected or the source was exhausted, including a request-seven / scan-many regression case.
- Closed issue #424 introduced `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` to bound pre-UI latency. The live gate returns the accepted prefix when that deadline expires.
- Open issue #446 is already prepared in the in-progress `quickfiler-bug-family` epic, but its current specification addresses only an empty deadline result being mistaken for source exhaustion. It explicitly states that the #446 fix does not change how long a scan runs, so it does not cover this non-empty partial-batch regression.
- Primary files to inspect are `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, and `QuickFiler/Controllers/QfcHomeController.Iteration.cs` plus their existing tests.

## Proposed Fix / Validation Ideas

- [ ] Add a failing deterministic unit test that requests seven qualifying items, interleaves approximately 40 below-threshold candidates, crosses the current deadline, and still returns seven while the source remains active.
- [ ] Add source-exhaustion boundary tests proving that fewer than the requested count returns only when the source is genuinely exhausted, including zero and non-zero partial results.
- [ ] Verify both the initial-screen and subsequent-screen call paths pass the calculated `ItemsPerIteration` quantity through unchanged and cannot treat a deadline as permission to display an undersized non-exhausted batch.
- [ ] Preserve ordinary-mode parity and inclusive confidence-threshold semantics.
- [ ] Reconcile the issue #424 latency objective and the prepared issue #446 outcome contract explicitly so the implementation does not make either plan's assumptions stale without documentation.
- [ ] Run the full C# toolchain in the required format, analyzer, nullable/compiler, and MSTest-with-coverage order.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
