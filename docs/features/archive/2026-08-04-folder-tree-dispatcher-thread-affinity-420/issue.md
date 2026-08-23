# folder-tree-dispatcher-thread-affinity (Issue #420)

- Date captured: 2026-08-04
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-tree-dispatcher-thread-affinity/ (Issue #420)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #420
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/420
- Last Updated: 2026-08-04
- Work Mode: full-bug

## Summary

The shared Outlook folder-tree service can invoke `WpfDispatcherYield` from a dispatcher-free worker after `EmailDataMiner` starts a cold cache build through `Task.Run`. `Dispatcher.Yield` then raises `InvalidOperationException`, and a fallback that only uses `Task.Yield` would not restore the required Outlook STA traversal contract.

## Environment

- OS/version: Windows with Outlook VSTO runtime
- Application: TaskMaster Outlook add-in
- Trigger: Cold folder-tree cache build or refresh that reaches a deadline-controlled yield point
- Data source or fixture: Outlook folder hierarchy through the live COM adapter

## Steps to Reproduce

1. Start the email-mining path while the shared folder-tree cache requires a build or refresh.
2. `EmailDataMiner.MineEmails` invokes `ExtractOlFolderChunks` through `Task.Run`.
3. The folder-tree reader reaches `WpfDispatcherYield.YieldAsync` after the deadline clock requests a yield.

## Expected Behavior

Live Outlook COM traversal and cooperative yielding remain on the captured Outlook STA dispatcher, and the caller receives a folder snapshot without a thread-affinity exception.

## Actual Behavior

`WpfDispatcherYield` invokes `Dispatcher.Yield(DispatcherPriority.Background)` on a worker thread that has no current dispatcher. WPF raises `InvalidOperationException` indicating that the calling thread does not have a current Dispatcher.

## Logs / Screenshots

- [x] Captured stack evidence reviewed
- Snippet: `EmailDataMiner -> OutlookFolderTreeService -> FolderTreeSnapshotBuilder -> OutlookFolderHierarchyReader -> WpfDispatcherYield -> Dispatcher.Yield`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

PR #215 added the shared folder-tree service and unconditionally wires `WpfDispatcherYield`. The earlier `EmailDataMiner` worker boundary remains. Issue #214 requires live Outlook COM traversal on the Outlook STA and dispatcher-backed cooperative yielding. `ConfigureAwait(false)` suppresses context capture and can move a continuation off the STA; it does not preserve one worker thread.

## Proposed Fix / Validation Ideas

- [ ] Add deterministic regression coverage for a cold request initiated from a non-STA caller.
- [ ] Verify service construction, COM hierarchy access, and post-yield continuations execute on the captured Outlook STA dispatcher.
- [ ] Assess other background cold-build entry points, including SubjectMap and scrape paths, before selecting the production repair.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
