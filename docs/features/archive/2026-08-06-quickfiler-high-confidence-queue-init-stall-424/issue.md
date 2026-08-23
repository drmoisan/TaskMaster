# quickfiler-high-confidence-queue-init-stall (Issue #424)

- Date captured: 2026-08-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/ (Issue #424)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #424
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/424
- Last Updated: 2026-08-06
- Work Mode: full-bug

## Summary

When QuickFiler runs with High Confidence mode enabled, the ProgressViewer stops at "Initializing Email Queue" for an extended period before the first QuickFiler screen appears. The same startup in normal mode presents its first screen promptly.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework VSTO add-in)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox; the configured QuickFiler source folder with a real message volume

## Steps to Reproduce

1. Enable High Confidence mode in QuickFiler settings (`HighConfidenceModeEnabled = true`, `HighConfidenceThreshold` at its configured value).
2. Launch QuickFiler against a mailbox folder containing a realistic number of messages.
3. Observe the ProgressViewer.

## Expected Behavior

The first QuickFiler screen appears within a short, bounded time. Progress reporting advances or otherwise reflects ongoing work, and the pre-UI wait does not grow without limit as the proportion of high-confidence items falls.

## Actual Behavior

The ProgressViewer displays "Initializing Email Queue" and stays there for an extended time. Progress remains at 0 for the entire wait; the label only changes once the full first batch of high-confidence items has been assembled.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: the log4net "Probability debug [QfcStreamingDequeueConfidenceGate.DequeueAsync]" lines emit one entry per scored candidate during the stall.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the feature remains functional but the startup latency in High Confidence mode makes the mode impractical for routine use.

## Suspected Cause / Notes

Preliminary read-only intake (to be validated by research):

- `QuickFiler/Controllers/QfcHomeController.cs:277` reports `(0, "Initializing Email Queue")`; the next report `(30, "Initializing Qfc Items")` is at line 297. Everything between those two lines is displayed under the stalled label with no intermediate progress.
- In High Confidence mode `RunAsync` sets `initializationBatchSize = 0`, so `InitEmailQueueAsync` returns immediately and the first UI batch instead comes from `DequeueNextItemGroupAsync(itemsPerIteration, 1000)`.
- `QfcDatamodel.QueueProcessing.DequeueWithHighConfidenceGateAsync` delegates to `QfcStreamingDequeueConfidenceGate.DequeueAsync`, which loops until it has accepted `quantity` items and awaits a full per-item score for every candidate, including every rejected one.
- `QfcDatamodel.ScoreRemainingQueueMailItemAsync` constructs a `FolderScoringService` per call, running `MailItemHelper.FromMailItemAsync` plus a `FolderPredictor` initialization against live Outlook COM for each candidate.
- When the master queue is empty and the `BackgroundWorker` producer is still busy, the gate waits the full `timeOut` (1000 ms) per empty poll, so it is additionally throttled by the serial `GetItemFromID` producer loop in `LoadRemainingEmailsToQueueAsync`.

Net effect: the pre-UI wait scales with `itemsPerIteration / (fraction of items above the confidence threshold)` serial COM-bound scoring operations, which is unbounded as the accepted fraction approaches zero.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `QfcStreamingDequeueConfidenceGate`, `QfcDatamodel` queue processing, `QfcHomeController.RunAsync` high-confidence path
- [ ] Integration scenario to retest: High Confidence launch with a low-yield folder (few items above threshold) and with a high-yield folder
- [ ] Manual verification notes: confirm the first screen appears within the agreed bound and that progress advances during the wait

Candidate directions for research to evaluate (not a decision):

- Bound the pre-UI wait with a deadline and present the first screen with whatever high-confidence items are available, continuing to fill in the background.
- Score candidates concurrently with a bounded degree of parallelism instead of strictly one at a time.
- Report incremental progress from inside the gate loop so the ProgressViewer reflects scanning progress.
- Avoid re-scoring: retain scores computed during queue admission so the gate does not repeat the work.
- Reduce the empty-queue poll interval, or signal on queue arrival instead of polling on a fixed 1000 ms delay.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
