# quickfiler-high-confidence-cancel-teardown-and-deadline-defects (Issue #791)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-high-confidence-cancel-teardown-and-deadline-defects/ (Issue #791)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #791
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/791
- Last Updated: 2026-09-06
## Summary

Two defects observed while running QuickFiler in High Confidence mode on 2026-09-06 against the build of `7c8ac9ae`. (1) A High Confidence run whose first 12 seconds of scanning finds no item at or above the cutoff opens an empty dialog, and because scan order follows the Explorer view the same view produces the same empty dialog on every rerun. (2) The Cancel teardown does not shut QuickFiler down cleanly: the background queue loader outlives Cancel and crashes on fields that cleanup has already nulled, the keyboard-active flag and WebView2 focus are never reset on the Cancel path, the teardown chain has no `try`/`finally`, and the whole path emits no log output, which left a 37 minute unexplained gap during which the Outlook keyboard was locked.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler launched from the ribbon High Confidence button; `HighConfidenceThreshold` at the designer default 0.9 (never changed in any `user.config` on the machine); `HighConfidenceModeEnabled` toggled by the ribbon launch path
- Data source or fixture: live Outlook Inbox view; add-in loaded from `TaskMaster\bin\Debug` built 2026-09-06 08:51 from `7c8ac9ae`

## Steps to Reproduce

Defect 1 (deadline policy, deterministic for a given view):
1. Arrange an Explorer view whose first roughly 40 items in view order all score below 900 per-mille while later items score above it.
2. Launch QuickFiler via the High Confidence ribbon button.
3. Observe the dialog open with zero rows after roughly 20 seconds.
4. Cancel and relaunch via the same button; observe the same empty dialog.

Defect 2 (Cancel teardown, sporadic):
1. Launch QuickFiler via the High Confidence ribbon button and file one round of suggestions.
2. File a second round, then press Undo repeatedly (24 undo clicks were logged between 09:04:05 and 09:05:53).
3. Press Cancel.
4. Observe the Outlook keyboard is unusable in the native Outlook window. In a separate run the same Cancel left the background loader running until it crashed 4 seconds after the next launch.

## Expected Behavior

- A High Confidence run that has scored items but found none at or above the cutoff within the first-batch deadline keeps scanning until the first acceptance or until the candidate queue is exhausted, subject to a hard cap on scanned items, and reports progress. It never opens an empty dialog while unscanned candidates remain.
- The cutoff in effect and the scan progress are logged at launch and at every deadline decision.
- Cancel performs a complete, ordered teardown: cancellation is signalled, the background loader is stopped and awaited before any datamodel field is nulled, form and item keyboard handlers are unregistered before the item rows are removed, the keyboard-active flag is reset, WebView2 focus is parked and any open breadcrumb dropdown is cancelled (the same routine that `FormViewer_Deactivated` runs), and the ribbon release callback runs even if an earlier step throws.
- Every stage of the Cancel teardown writes a log line through the existing log4net pattern, including any exception, so a future sporadic occurrence can be read from the log.

## Actual Behavior

- `QfcStreamingDequeueConfidenceGate.DequeueAsync` returns `DeadlineExpired` with an empty accepted list when `accepted.Count == 0` after 12 seconds, and `QfcHomeController.RunAsync` loads zero rows. Three runs today logged `First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=38|44|42 Deadline=00:00:12` (09:43:54, 09:45:36, 10:08:06). Scores were real, not zero: those runs peaked at 928 and 960 after the deadline had already expired, while accepting runs peaked at 997 to 1000. The cutoff (900) is never logged.
- After Cancel, `QfcDatamodel.Cleanup()` cancels the token and calls `worker.CancelAsync()` but does not await `LoadRemainingEmailsToQueueAsync`, then nulls `_moveMonitor`, `_globals`, `_masterQueue`, and `_worker`. The still-running loader then throws at `QfcDatamodel.cs:355-358` (`new QfcRemainingQueueAdmission(_masterQueue.AddLast, _moveMonitor.HookItem, ...)`): `ERROR QfcDatamodel - LoadRemainingEmailsToQueue Error. Delegate to an instance method cannot have null 'this'.` followed by `Error in Worker_DoWork` (log 2026-09-06 10:08:10.910 and 10:08:10.985, the last two lines of the file).
- `ActionCancelAsync` (`QfcFormController.EventHandlers.cs:84-93`) calls `_parent?.TokenSource?.Cancel()`, awaits the UI sync context, hides the form, then `_groups?.Cleanup()` and `Cleanup()`. It does not reset `KbdActive` (the OK path does, `EventHandlers.cs:125-128`), does not call `ParkFocusOffWebView2()` or `CancelBreadcrumbSelector()` (both exist only in `QfcFormController.Deactivate.cs:26-58`, wired to `FormDeactivated`, which the Cancel path unsubscribes), and has no `try`/`finally`. `ButtonCancel_Click` is `async void`, so an exception escaping `ActionCancelAsync` is lost.
- `QfcFormController.Cleanup()` (`SetupDisposal.cs:213-261`) calls `UnregisterFormEventHandlers()` after `_groups.Cleanup()` has already removed the item rows from the table layout, so the recursive `Controls.ForAllControls` unsubscribe no longer reaches the item controls' `PreviewKeyDown`/`KeyDown` subscriptions added at `SetupDisposal.cs:156-168`. The guard at `:180-183` also returns early when `_formViewer?.Controls` or `_parent?.KeyboardHandler` is already null.
- `QfcHomeController.Cleanup()` (`QfcHomeController.cs:370-379`) calls `_datamodel.Cleanup()` and then `ParentCleanup.Invoke()` with no `try`/`finally`; if the datamodel cleanup throws, `RibbonController.ReleaseQuickFiler()` never runs, `_quickFilerLoaded` stays true, and both ribbon buttons become no-ops. `_tokenSource` is never disposed and `Worker_RunWorkerCompleted` is never detached.
- The Cancel path, `QfcDatamodel.Cleanup()`, and `ParkFocusOffWebView2()` contain no logging. After the 09:05:53 undo burst the log is silent for 37 minutes 39 seconds until the next launch at 09:43:32 (no restart; Outlook restarted only at 09:53:24).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (from `TaskMaster\bin\Debug\logs\debug_2026-09-06.log`):

```
2026-09-06 09:43:54,214 [44] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=38 Deadline=00:00:12
2026-09-06 09:45:36,727 [53] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=44 Deadline=00:00:12
2026-09-06 10:08:06,149 [29] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=42 Deadline=00:00:12
2026-09-06 10:08:10,910 [5] ERROR QuickFiler.Controllers.QfcDatamodel - LoadRemainingEmailsToQueue Error.
      Delegate to an instance method cannot have null 'this'.
   at System.MulticastDelegate.CtorClosed(Object target, IntPtr methodPtr)
   at QuickFiler.Controllers.QfcDatamodel.<TryQueueRemainingMailItemAsync>d__41.MoveNext() ... QfcDatamodel.cs:line 355
   at QuickFiler.Controllers.QfcDatamodel.<LoadRemainingEmailsToQueueAsync>d__40.MoveNext() ... QfcDatamodel.cs:line 330
2026-09-06 10:08:10,985 [5] ERROR QuickFiler.Controllers.QfcDatamodel - Error in Worker_DoWork Delegate to an instance method cannot have null 'this'.
```

Timeline evidence (same log): launches at 08:52:09 (accepted, rows at 08:53:39), 09:43:32 (Accepted=0), 09:45:14 (Accepted=0), 10:04:26 (accepted), 10:05:12 (accepted, relaunch 46 s after the previous), 10:07:45 (Accepted=0). Undo burst 09:04:05 to 09:05:53, then no log output until 09:43:32.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the deadline defect makes High Confidence mode unusable for any view whose top-scoring items are not near the front, with no message and no recovery other than filing items some other way. The teardown defect can leave the whole Outlook keyboard unusable until Outlook is restarted, and the surviving background loader crashes against the next launch's state.

## Suspected Cause / Notes

- Gate loop `QfcStreamingDequeueConfidenceGate.cs:168-237`: the deadline is checked only while `accepted.Count == 0`; `scanned++` at `:205` runs only after `_scoreLoader` returns, so `Scanned=N Accepted=0` means N real scores all below `_cutoff` (`:129`, per-mille). Scan order is `_masterQueue.TryTakeFirst()` (`QfcDatamodel.QueueProcessing.cs:185`), populated from the Explorer view (`QfcDatamodel.FrameBuilding.cs:13-67`), so the outcome is a function of view order and scoring throughput (about 2 to 3 items per second observed). Rejected items are dropped from the queue for the session (`:211-222`).
- The 12 second first-batch deadline was introduced by #424 and adjusted by #446 and #608; those changes handled the post-UI iteration and the undersized-batch cases, not the zero-accepted first batch.
- `Worker_DoWork` (`QfcDatamodel.cs:175-213`) is `async void`; `BackgroundWorker.IsBusy` goes false at its first await while production continues, and `LoadRemainingEmailsToQueueAsync` observes the token only at `:322` and `:324`.
- Keyboard mechanism: no `SetWindowsHookEx`, `AddMessageFilter`, or `KeyPreview` exists anywhere in the repo (confirmed again today). #677 identified WebView2 focus retention and an open breadcrumb `ToolStripDropDown` as the mechanism and fixed it on the `Form.Deactivate` path only. `AlwaysOnKeyActionsAsync` (`KeyboardHandler.cs:155-160`) suppresses keys regardless of `KbdActive`.
- The breadcrumb WebView2 failed to initialize twice today (`WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed ... 0x8007139F` at 08:55:22 and 10:06:51). That is a separate defect, filed as its own potential entry, and is not in scope here.
- Related closed issues: #424, #446, #608 (deadline lineage), #677 (Deactivate focus fix), #731 (controller lifecycle disposal), #737 (breadcrumb keyboard navigation). All are closed and their fixes are on `7c8ac9ae`.
- Unknown: whether the 09:05 keyboard lock cleared on Escape, on focus change, or only on restart. The user could not reproduce it.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: gate behavior when the deadline expires with zero accepted and candidates remain (continue, hard cap, exhaustion); cutoff and progress logging; `ActionCancelAsync` ordering (token, loader awaited, handlers unregistered before rows removed, `KbdActive` reset, focus parked, breadcrumb selector cancelled, release callback invoked under exception); `QfcDatamodel.Cleanup()` awaiting the loader before nulling fields; `QfcHomeController.Cleanup()` invoking `ParentCleanup` under a `finally`.
- [ ] Integration scenario to retest: High Confidence launch against a view whose first 40 items score below cutoff; Cancel after an undo burst; relaunch after Cancel.
- [ ] Manual verification notes: record a live-Outlook evidence note as was done for #677; confirm the new Cancel-path log lines appear and that no `null 'this'` error follows a Cancel.

## Acceptance Criteria

- [ ] AC1: A High Confidence run that has found no item at or above the cutoff when the first-batch deadline expires continues scanning until the first acceptance or until the candidate queue is exhausted, subject to a hard cap on items scanned, and never opens an empty dialog while unscanned candidates remain. The cutoff in effect and the scanned/accepted counts are logged at launch and at each deadline decision. Covered by deterministic MSTest regression tests using a fake time provider.
- [ ] AC2: The Cancel teardown completes cleanly and in order: the background loader is stopped and awaited before any datamodel field is nulled; form and item keyboard handlers are unregistered before item rows are removed; the keyboard-active flag is reset; WebView2 focus is parked and any open breadcrumb dropdown is cancelled on the Cancel path; the ribbon release callback runs under a `finally`; and every stage, including any exception, is logged. Covered by deterministic MSTest regression tests, plus a manual live-Outlook evidence note.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
