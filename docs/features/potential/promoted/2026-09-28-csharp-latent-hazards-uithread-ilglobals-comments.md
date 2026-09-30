# csharp-latent-hazards-uithread-ilglobals-comments (Issue #930)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csharp-latent-hazards-uithread-ilglobals-comments/ (Issue #930)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #930
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/930
- Last Updated: 2026-09-28
## Summary
Consolidates three small, low-risk latent defects in production C#. They have distinct root causes but no shared files. They are bundled into one item to cut orchestration overhead, as #872 (PR #893) did for #794, #840 and #841. Each is independently verifiable.

1. **#889:** `UtilitiesCS/Threading/UiThread.cs:197-201`. The dispatcher exit of `SynchronizationContextAwaiter.IsCompleted` evaluates `ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)` with no `_dispatcher is not null` guard.
   - When `_dispatcher` is null and the current thread owns no dispatcher, `null == null` evaluates `true`.
   - The captured-context exit above it (lines 183-187) received exactly this guard in PR #890.
2. **#863:** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`. Two public mutable static members remain after #824:
   - `public static Dictionary<int, object> Cache` (line 113). Its only reference is a test at `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:267`.
   - `public static Module[]? modules` (line 131). It has zero references.
3. **#862:** hard-coded line counts in two partial-class XML doc comments are stale:
   - `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:11` says 487; the file is 437 lines.
   - `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` says 481; the file is 497 lines, 3 lines under the ceiling.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C#, net48)
- Command/flags used: static inspection, `git grep -c "" HEAD -- <path>`, on `main` at `177b6d78e`
- Data source or fixture: files listed above

## Steps to Reproduce
Read the cited lines on `main`.

## Expected Behavior
1. `IsCompleted` returns `false` from the dispatcher exit when no dispatcher was captured.
2. `ILGlobals` exposes no public mutable static. Dead members are removed; a retained cache is private or readonly and safely published.
3. The comments state no line count at all, so they cannot drift again.

## Actual Behavior
As described in the Summary. All three were verified still present on 2026-09-28.

## Logs / Screenshots
- [x] Attached minimal logs or snippet
- Snippet: `return _context is DispatcherSynchronizationContext && ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher);`

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes
- #889 and #863 are sibling residuals of fixes (#816 and #824) that scoped out the adjacent member.
- #862 is comment drift.

## Proposed Fix / Validation Ideas
- [ ] #889: add the null guard, and a regression test that fails before the fix. The test constructs the awaiter state with a null captured dispatcher and a `DispatcherSynchronizationContext` ambient context.
- [ ] #863: delete `modules`. Delete `Cache` or make it private readonly, and update the one test assertion. Confirm there is no reflection-based consumer.
- [ ] #862: remove the numeric line counts from both comments.
- [ ] Run the full C# toolchain (CSharpier, analyzers, nullable, MSTest with coverage). Changed lines must be covered.

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Consolidates: #889, #863, #862.