# uithread-synccontext-awaiter-always-posts-for-dispatcher-built-viewers (Issue #784)

- Date captured: 2026-09-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-synccontext-awaiter-always-posts-for-dispatcher-built-viewers/ (Issue #784)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #784
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/784
- Last Updated: 2026-09-05
## Summary

`UtilitiesCS.UiThread.SynchronizationContextAwaiter.IsCompleted` decides whether an `await context` continuation may run inline by comparing the awaited `SynchronizationContext` to `SynchronizationContext.Current` by reference. For any context captured inside a WPF dispatcher operation (which is how every pooled `ItemViewer` is constructed, see issue #781), the captured instance is never the UI thread's ambient context again, so `await viewer.UiSyncContext` always reports not-completed and always posts, even when the caller is already on the UI thread. The result is an unnecessary message-queue hop on every such await rather than a failure.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, Outlook VSTO add-in, .NET Framework 4.8
- Python version: n/a
- Command/flags used: any `await someItemViewer.UiSyncContext` (or `await UiThread.UiSyncContext` when that context was captured inside a dispatcher operation) executed on the UI thread
- Data source or fixture: n/a; reproducible with the runtime probe recorded in `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`

## Steps to Reproduce

1. Construct an `ItemViewer` through `ItemViewerQueue.Dequeue` (production path: `ViewerQueueCore.CreateWithPriority` inside `UiThread.Dispatcher.Invoke`), so `viewer.UiSyncContext` is a `DispatcherSynchronizationContext`.
2. On the UI thread, outside any dispatcher operation (ambient context is the persistent `WindowsFormsSynchronizationContext`), evaluate `viewer.UiSyncContext.GetAwaiter().IsCompleted`.
3. Observe `false`; `await viewer.UiSyncContext` therefore posts the continuation instead of continuing inline.

## Expected Behavior

An `await` on the UI thread's own synchronization context should continue inline when the caller already runs on the owning thread, regardless of which `SynchronizationContext` instance happens to be ambient at the call site.

## Actual Behavior

`IsCompleted` returns `false` whenever the ambient instance differs from the captured instance, so the continuation is always posted through `SynchronizationContext.Post`. No exception; one extra queued hop per await, and ordering relative to already-queued UI work changes.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `UtilitiesCS/Threading/UiThread.cs` line 100 (verified 2026-09-05): `public bool IsCompleted => _context == SynchronizationContext.Current;`. `SynchronizationContext` does not overload `==`, so this is a reference comparison. The probe cited above shows `Invoke ctx == outer ambient : False` on .NET Framework 4.8 STA.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Performance and ordering only; no functional failure. Recorded as out of scope by issue #781 and requested as a promoted follow-up by the #781 feature review (`code-review.2026-09-05T17-29.md`).

## Suspected Cause / Notes

Same root mechanism as issue #781: WPF installs a `DispatcherSynchronizationContext` for the duration of each dispatcher operation, so a context captured inside one is a different object from the thread's ambient context afterwards. Reference equality is therefore not a reliable "already on this context" test for dispatcher-captured contexts.

Related low-severity follow-ups from the #781 review that can ride along with this fix (same file family, same reviewer):

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, XML documentation on `ThrowIfOffUiBoundary` (added by #781): the remarks justify the guard with "managed thread ids are unique among live threads", but `Dispatcher.CheckAccess()` compares `Thread` object references, not ids. The guard is stronger than its stated rationale; reword so a future refactor does not weaken it to an id comparison (review finding CR-4).
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`: the private helper `DrainableSynchronizationContext.Drain()` is never called after the #781 test deletions and can be removed.

## Proposed Fix / Validation Ideas

- [ ] Change `IsCompleted` to a thread-identity test where the context exposes one: for a `DispatcherSynchronizationContext`, resolve its `Dispatcher` and use `CheckAccess()`; for `WindowsFormsSynchronizationContext`, compare the owning thread (or fall back to the existing reference comparison only for plain `SynchronizationContext` instances). Alternatively, have `ItemViewer` and `UiThread` capture the persistent WinForms context rather than the transient dispatcher context.
- [ ] Unit coverage: a test that captures a context inside `Dispatcher.CurrentDispatcher.Invoke(...)`, then evaluates `GetAwaiter().IsCompleted` on the same thread under the WinForms ambient context and expects `true`; a `Task.Run` case that expects `false`.
- [ ] Integration scenario to retest: QuickFiler launch in standard and high-confidence mode, confirming no behavior change and fewer posted continuations.
- [ ] Manual verification notes: none beyond the above.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
