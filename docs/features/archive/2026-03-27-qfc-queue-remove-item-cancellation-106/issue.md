# qfc-queue-remove-item-cancellation (Issue #106)

- Date captured: 2026-03-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-queue-remove-item-cancellation/ (Issue #106)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #106
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/106
- Last Updated: 2026-03-27
- Work Mode: minor-audit

## Summary

`QfcQueue.RemoveItem` throws an unhandled `System.OperationCanceledException` when the instance-level cancellation token has been cancelled before the move-monitor callback fires. `JobsToFinish` calls `token.ThrowIfCancellationRequested()` unconditionally, causing the exception to propagate through `RemoveItem` and surface from within the `EnqueueAsync` move-monitor lambda.

## Environment

- OS/version: Windows / Outlook VSTO add-in
- Command/flags used: N/A (runtime crash)
- Data source or fixture: Mail item moved to Junk E-mail folder while cancellation token is set

## Steps to Reproduce

1. Open Outlook with the TaskMaster VSTO add-in loaded.
2. Have a mail item enqueued in `QfcQueue` (via `EnqueueAsync`) with a move-monitor hook active.
3. Cancel the add-in's `CancellationToken` (e.g., closing the pane or shutting down).
4. Move the mail item to Junk E-mail (or any folder) triggering the move-monitor callback.
5. `RemoveItem` is invoked → calls `JobsToFinish(_token)` → `_token.ThrowIfCancellationRequested()` → `OperationCanceledException` is thrown unhandled.

## Expected Behavior

When the cancellation token is already cancelled during a move-monitor–triggered `RemoveItem`, the method should exit gracefully (no-op or log and return) rather than propagating an unhandled exception.

## Actual Behavior

```
System.OperationCanceledException
  HResult=0x8013153B
  Message=The operation was canceled.
  Source=mscorlib
  StackTrace:
   at System.Threading.CancellationToken.ThrowOperationCanceledException()
   at System.Threading.CancellationToken.ThrowIfCancellationRequested()
   at QuickFiler.Controllers.QfcQueue.<JobsToFinish>d__14.MoveNext() in QfcQueue.cs:line 269
   at QuickFiler.Controllers.QfcQueue.<RemoveItem>d__12.MoveNext() in QfcQueue.cs:line 170
   at QuickFiler.Controllers.QfcQueue.<>c__DisplayClass13_0.<<EnqueueAsync>b__2>d.MoveNext() in QfcQueue.cs:line 217
```

## Logs / Screenshots

- [x] Stack trace captured above
- Snippet: See Actual Behavior above

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`QfcQueue.JobsToFinish(int pollInterval, CancellationToken token)` calls `token.ThrowIfCancellationRequested()` in its polling loop. When `RemoveItem` is triggered via the move-monitor callback (an `EmailMoveMonitor` hook registered in `EnqueueAsync`), the instance-level `_token` may already be cancelled. `RemoveItem` passes `_token` directly to `JobsToFinish`, so any cancellation immediately throws instead of allowing the cleanup to complete or abort gracefully.

Secondary concern: `ConversationResolver.LoadConversationInfoAsync()` previously re-entered the lazy `ConversationInfo` getter before assigning `ConversationInfo = pair`, causing a synchronous `LoadConversationInfo()` to run and throw on items in Junk E-mail where `Count.Expanded == 0`. Verify this path is fully fixed in the current codebase.

## Proposed Fix / Validation Ideas

- [x] In `QfcQueue.RemoveItem`, catch `OperationCanceledException` from `JobsToFinish` and return (log at debug level) when the instance token is cancelled — removal is moot at that point.
- [x] Alternatively, pass `CancellationToken.None` to `JobsToFinish` in the `RemoveItem` path so that token cancellation does not abort a pending cleanup.
- [x] Add a regression unit test in `QuickFiler.Test/Controllers/QfcQueueTests.cs` verifying that `RemoveItem` does not throw when the cancellation token is pre-cancelled.
- [x] Verify `ConversationResolver.LoadConversationInfoAsync` assigns `ConversationInfo = pair` before calling `UpdateUI`.
- [x] Manual retest: move a mail item while addin is shutting down.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch