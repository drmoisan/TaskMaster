# iteratequeueasync-deadline-closes-queue-early (Issue #446)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/iteratequeueasync-deadline-closes-queue-early/ (Issue #446)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #446
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/446
- Last Updated: 2026-08-08
## Summary

`QfcHomeController.IterateQueueAsync` treats an empty dequeue result as proof that the mail source is exhausted and irreversibly closes the QuickFiler UI queue. Since issue #424 introduced a first-batch deadline on the underlying dequeue, an empty result can now mean "the deadline expired" rather than "no items remain", so a slow high-confidence scan can close the queue for the rest of the session while items are still available.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox with a low proportion of items above the confidence threshold, so scoring is slow relative to the deadline

## Steps to Reproduce

1. Enable High Confidence mode with a threshold that few messages clear.
2. Launch QuickFiler against a folder large enough that scoring a batch exceeds the first-batch deadline.
3. File the first batch so `IterateQueueAsync` runs for the next batch.
4. Observe whether further batches are ever presented.

## Expected Behavior

An empty batch caused by a scan deadline should leave the queue open so later iterations can continue supplying items. The queue should be closed only when the mail source is genuinely exhausted.

## Actual Behavior

The empty result routes to `QfcQueue.CompleteAddingAsync`, which calls `BlockingCollection.CompleteAdding()`. That operation is irreversible, so no further items can be enqueued for the remainder of the session even though unscanned items remain.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured; the failure presents as QuickFiler ending the session early rather than as an error.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: in High Confidence mode with a low-yield folder the user can be denied the remainder of their queue for the session, with no error shown and no way to recover short of relaunching QuickFiler.

## Suspected Cause / Notes

Found during read-only research for epic child F7 (`quickfiler-qfc-home-controller-coverage`, issue #433) under parent epic #136. Report-only; deliberately not fixed inside a coverage child. Evidence is recorded in
`docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/research/QfcHomeController.Iteration.cs.research.2026-08-07T20-50.md` (finding LD3).

- `QfcHomeController.Iteration.cs:21` calls the two-argument `_datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000)`.
- Issue #424 changed what that overload does. `QfcDatamodel.QueueProcessing.cs:66-76` now makes the two-argument overload delegate to the deadline-bearing path using `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` (12 s, `QfcStreamingDequeueConfidenceGate.cs:22`). The post-UI iteration call site therefore inherited a deadline it was not written for.
- `QfcHomeController.Iteration.cs:32` infers from `listObjects.Count == 0` that the source is exhausted and calls `QfcQueue.CompleteAddingAsync(Token, 10000)`, which reaches `_queue.CompleteAdding()` at `QfcQueue.cs:59`. `BlockingCollection.CompleteAdding()` cannot be undone.
- #424's `spec.md` states the post-UI iteration call site was "left unchanged". That is true of the file text and false of the resulting behavior, which is why the interaction was not caught during that change.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `IterateQueueAsync`'s empty-batch branch, distinguishing a deadline-expired empty result from a genuine end-of-source result. This likely needs the dequeue contract to report which of the two occurred.
- [ ] Integration scenario to retest: High Confidence launch against a low-yield folder large enough to exceed the deadline; confirm later batches still arrive.
- [ ] Manual verification notes: confirm the queue is closed exactly once, and only on genuine exhaustion.

Note that a fix likely requires a contract change on `IQfcDatamodel` (owned by epic child F5) so the caller can distinguish the two empty-result causes. Coordinate accordingly.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
