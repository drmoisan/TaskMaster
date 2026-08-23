# winforms-message-pump-test-seam (Issue #230)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Active -> docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/ (Issue #230)
- Type: feature (test infrastructure)

- Issue: #230
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/230
- Last Updated: 2026-08-07
- Work Mode: full-feature

> Promotion note: GitHub issue #230 was created ahead of this orchestration run as a
> deferred follow-up from issue #227 cycle 5. The local potential entry was created via
> `mcp__drm-copilot__new_potential_entry` and the active folder via
> `mcp__drm-copilot__new_active_feature_folder`. `mcp__drm-copilot__potential_to_issue`
> was deliberately not invoked because it unconditionally creates a new GitHub issue and
> would have produced a duplicate of #230.

## Problem / Why

Across issue #227's remediation cycles, `QfcItemController`'s coverage-exemption boundary was
reduced from 103 to 19 members. The remaining 9 residuals in that boundary are all blocked by the
same structural gap: they `await itemViewer.UiSyncContext` (or an equivalent continuation posted
through the ambient `WindowsFormsSynchronizationContext`), and this repo has no WinForms analogue of
the WPF `Dispatcher.Run()`-on-a-background-thread test seam it already built for `IUiDispatcher`
(see `UtilitiesCS.Test/Threading/*` and `QfcItemController.TestSupport.cs`'s `StartRunningDispatcher()`).

Awaiting a continuation captured by a `WindowsFormsSynchronizationContext` on a thread-pool MSTest
async-test thread (no message pump) can hang indefinitely — documented in this exact repo at
`UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs:362-374`.

## Affected members (QuickFiler/Controllers/QfcItemController.*.cs)

- `Initialize(bool async)`, `InitializeAsync`, `InitializeGraphicsAsync`, `InitializeSequentialAsync`
- `CreateAsync`, `CreateSequentialAsync` (static factories; barrier inherited from the above)
- `ResolveControlGroupsAsync(ItemViewer)` (the async half; the synchronous half was de-exempted in
  #227 cycle 5 via headless `ItemViewer` construction)
- `InitializeWebViewAsync` (also has a residual concrete-`ItemViewer` accessor barrier, tracked
  separately)

## Proposed Behavior

A WinForms `Application.Run()`-on-a-dedicated-background-thread test seam, analogous to the existing
`StartRunningDispatcher()` WPF pattern, that lets a test:

1. Start a real WinForms message pump on a background thread.
2. Marshal a headless `ItemViewer`'s continuations through it.
3. Await completion deterministically without hanging the test thread.

## Constraints & Risks

- Unit tests must remain deterministic and must not hang. A pump seam that fails to shut down
  cleanly converts a test failure into a CI timeout.
- No temporary files (repository unit-test policy).
- The seam must dispose its pump thread and `SynchronizationContext` deterministically so tests
  remain independent and order-insensitive.
- Removing `[ExcludeFromCodeCoverage]` from the affected members changes the coverage denominator;
  the repository line-coverage floor must not regress.

## Scope note

This was evaluated during #227 cycle-5's research and explicitly deferred as "a materially larger,
distinct piece of test infrastructure ... out of scope to build inside this research task." It is
tracked here as its own initiative, analogous to how #197 tracks the repo-wide coverage uplift. Not
a blocker for #227's merge.

## References

- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`
- `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
- `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`
