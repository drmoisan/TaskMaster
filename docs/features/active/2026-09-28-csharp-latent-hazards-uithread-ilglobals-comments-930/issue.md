# csharp-latent-hazards-uithread-ilglobals-comments (Issue #930)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csharp-latent-hazards-uithread-ilglobals-comments/ (Issue #930)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #930
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/930
- Last Updated: 2026-09-28
- Work Mode: minor-audit

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

## Acceptance Criteria
Derived on 2026-09-28 from the Expected Behavior section above, because the promoted record carried no explicit Acceptance Criteria section. Each sub-item is independently verifiable.

- [x] AC1 (#889): A new MSTest regression test in the UtilitiesCS.Test project exercises the dispatcher exit of `UiThread.SynchronizationContextAwaiter.IsCompleted` with no captured UI dispatcher, a captured UI thread id equal to the executing thread's managed id, an awaiter context that is a `DispatcherSynchronizationContext` which is not the captured UI context, a non-null ambient context that differs from the awaiter context, and an executing thread that owns no WPF dispatcher. The test asserts `IsCompleted` is `false`, and it is recorded failing against the unmodified source before the fix is applied.
- [x] AC2 (#889): The dispatcher exit in `UtilitiesCS/Threading/UiThread.cs` requires `_dispatcher is not null` before the dispatcher reference comparison, matching the guard the captured-context exit already carries. The AC1 test passes after the fix, and every pre-existing `IsCompleted` test in the UtilitiesCS.Test project still passes.
- [x] AC3 (#863): `ILGlobals` in the SDIL Reader directory of UtilitiesCS no longer declares the `modules` field, and no longer exposes `Cache` as a public mutable static field (it is removed, or made private readonly). A repository-wide search confirms no remaining reference to either member, including by reflection or by string name, other than any retained private declaration.
- [x] AC4 (#863): The `ILGlobals.Cache` assertion in `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` is updated or removed consistently with AC3, and the ILGlobals test class passes.
- [x] AC5 (#862): The XML doc comments in `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` and `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` state no numeric line count for their primary partial-class file, while still explaining why the partial part exists.
- [x] AC6: The full C# toolchain passes in one clean pass in CLAUDE.md order (CSharpier check, analyzer Rebuild, TreatWarningsAsErrors Rebuild, MSTest with coverage), with test execution in the existing parallel regime: no new `DoNotParallelize` attribute, no worker-count reduction, and no retry. Every changed production line that remains executable is covered, and coverage does not regress.
- [ ] AC7: Committed evidence follows the CLAUDE.md "Committed Test Evidence Format" section (projections and summaries only, no raw trx or raw coverage collector document), and no committed file contains an absolute host path, the developer account name, or the host name.

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