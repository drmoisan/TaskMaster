# qfcformcontroller-cleanup-disposal-ordering (Issue #621)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfcformcontroller-cleanup-disposal-ordering/ (Issue #621)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #621
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/621
- Last Updated: 2026-08-26
## Summary


`QfcFormController.Cleanup()` tears down state the undo consumer is still using, without first stopping that consumer. `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:216-220` disposes `_undoQueue` and nulls `_globals` and `_groups` while never cancelling or awaiting `_undoConsumerTask`. A consumer iteration still in flight can therefore observe a disposed queue or a null global, producing an `ObjectDisposedException` or a `NullReferenceException` on a background task during teardown.

## Environment

- OS/version:
- Python version:
- Command/flags used:
- Data source or fixture:

## Steps to Reproduce


1. Start the undo consumer so `_undoConsumerTask` is running and blocked on a take.
2. Invoke `Cleanup()` while that iteration is in flight.
3. Observe the consumer touching `_undoQueue`, `_globals` or `_groups` after `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:216-220` has disposed or nulled them.

## Expected Behavior


Cleanup cancels the consumer, awaits `_undoConsumerTask` to a terminal state, and only then disposes the queue and releases `_globals` and `_groups`.

## Actual Behavior


Cleanup disposes and nulls first and never awaits the task, so teardown races the consumer.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:

## Impact / Severity


- [x] Medium

Race confined to teardown, but it can surface as an unobserved background task exception.

## Suspected Cause / Notes


Discovered during issue #446 (`docs/features/active/quickfiler-bug-family-446`), which fixed the consumer loop's own termination and its `finally` reset but did not change `Cleanup()`. `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` was outside #446's owned file set.

## Proposed Fix / Validation Ideas


- [ ] Cancel the consumer's token, then await `_undoConsumerTask` (with a bounded timeout) before disposing `_undoQueue`
- [ ] Unit coverage driving Cleanup() against an in-flight consumer, asserting no exception escapes and the task reaches a terminal state
- [ ] Confirm Cleanup() remains safe when the consumer was never started

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
