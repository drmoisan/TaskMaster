# emailmovemonitor-cross-thread-com (Issue #228)

- Date captured: 2026-06-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/emailmovemonitor-cross-thread-com/ (Issue #228)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #228
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/228
- Last Updated: 2026-06-30
- Work Mode: full-bug

## Summary

`EmailMoveMonitor.UnhookItem(MailItem)` accesses thread-affine Outlook COM objects (`mail.Parent`, `Folder.EntryID`) from a ThreadPool thread because `QfcDatamodel.DequeueNextItemGroupAsync` invokes the unhook path inside `await Task.Run(...)`. Cross-thread access to STA-bound Outlook interop objects throws `System.Runtime.InteropServices.COMException: "The operation failed."`.

## Environment

- OS/version: Windows, Outlook VSTO add-in host
- Python version: N/A (C# / .NET Framework VSTO add-in)
- Command/flags used: Triggered during QuickFiler queue processing (dequeue/unhook)
- Data source or fixture: Live Outlook mail items hooked via `EmailMoveMonitor.HookItem`

## Steps to Reproduce

1. Run QuickFiler with items hooked into the move monitor.
2. Trigger queue dequeue via `QfcDatamodel.DequeueNextItemGroupAsync`, which runs `TryUnhookOrReplace` -> `_moveMonitor.UnhookItem(node)` inside `await Task.Run(...)`.
3. Observe `COMException` ("The operation failed.") surfaced via `ExceptionDispatchInfo.Throw()` from the Outlook interop call on the background thread.

## Expected Behavior

Unhooking items from the move monitor completes without COM exceptions; all Outlook COM access remains on the owning/Outlook STA thread.

## Actual Behavior

`COMException: "The operation failed."` is thrown when `mail.Parent`/`Folder.EntryID` are evaluated on a ThreadPool thread. The displayed `ExceptionDispatchInfo.Throw()` frame is only a rethrow of the original background-thread interop failure.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `System.Runtime.InteropServices.COMException: The operation failed.` rethrown via `System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Cross-thread access to STA/thread-affine Outlook COM objects. `EmailMoveMonitor` is live and consumed by four production files: `QfcQueue.cs`, `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcCollectionController.cs`. Files to inspect:
- `QuickFiler\Helper Classes\EmailMoveMonitor.cs` (failing line 48-50)
- `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` (`Task.Run` unhook path, lines 33, 70-105)
- `UnhookItemAsync` and `GetParentFolderAsync` also wrap COM access in `Task.Run` and are not safe alternatives.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: unhook bookkeeping logic separated from COM access via a seam, tested with Moq + FluentAssertions (MSTest).
- [ ] Integration scenario to retest: queue dequeue/unhook on the Outlook thread without COMException.
- [ ] Manual verification notes: confirm no COM access executes on a ThreadPool thread.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
