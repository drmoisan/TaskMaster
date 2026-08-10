# uithread-dispatcher-static-swap-no-restore (Issue #493)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-dispatcher-static-swap-no-restore/ (Issue #493)

- Work Mode: full-bug

- Issue: #493
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/493
- Last Updated: 2026-08-08
## Problem / Why

`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` (in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`) mutates the process-wide static `UtilitiesCS.Threading.UiThread._dispatcher` and never restores the prior value.

Because the static is process-wide and MSTest runs test classes in parallel by default, one class's mutation is visible to every other class in the same test host. The helper leaves the static pointing at whatever dispatcher the last caller installed, for the remainder of the run.

This violates the repository unit-test policy on two counts (`.claude/rules/general-unit-test.md`):

- **Independence** — "Tests must be able to run in any order without impacting each other."
- **Environment stability** — "Tests must not rely on mutable global state."

## Impact — this defect has already caused a failure

During execution of issue #230, the Phase 8 toolchain loop failed its first iteration with two `[Timeout]` expiries, one from each of the two test classes that swap this static. Under class-level parallelization, one class's restore reverted the static to a parked, never-pumped dispatcher while the other class's member was still awaiting a dispatcher operation, producing a deadlock.

That failure was diagnosed as a genuine isolation defect rather than a flake, and was fixed *locally* in the #230 fixture with a static `SemaphoreSlim(1,1)` gate held from fixture build through an idempotent restore.

The shared helper was not changed. Any future test that calls `EnsureUiThreadDispatcher` reintroduces the same hazard, and the next occurrence will present as an intermittent CI timeout — the most expensive failure mode to diagnose.

## Discovery Context

Found during execution of issue #230 (PR #479, WinForms message-pump test seam). Recorded as an out-of-scope finding; not caused by, and did not block, that change.

Reference implementation of the local fix: `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`.

## Proposed Behavior

Give the shared helper the same save/restore discipline the #230 fixture uses, so callers cannot leak the mutation:

1. Capture the prior `UiThread` dispatcher before installing a replacement.
2. Return a disposable scope (or equivalent) that restores the prior value idempotently.
3. Serialize concurrent callers so two classes cannot interleave install/restore against the same static.

Consider whether the cleaner long-term fix is an injectable seam on `UiThread` rather than a mutable static, which would remove the need for cross-class serialization entirely. That is a larger change and should be evaluated on its merits rather than assumed.

## Acceptance Criteria (early draft)

- [ ] `EnsureUiThreadDispatcher` restores the previous `UiThread` dispatcher value, idempotently.
- [ ] Concurrent callers from different test classes cannot interleave install and restore against the shared static.
- [ ] A regression test demonstrates the prior deadlock scenario is no longer reachable, with a bounded `[Timeout]` so a regression fails rather than hangs.
- [ ] The #230 fixture's local `SemaphoreSlim` workaround is removed or reduced to a call into the shared, now-safe helper — the fix is not duplicated in two places.
- [ ] No `Thread.Sleep`, `Task.Delay`, or wall-clock waits are introduced.

## Constraints & Risks

- The regression test must not itself be able to hang the suite; bound it with `[Timeout]`.
- Existing callers of `EnsureUiThreadDispatcher` must be audited — changing the return type to a disposable scope is a breaking change to the helper's contract within the test project.

## Test Conditions to Consider

- [ ] Two test classes both installing and restoring the dispatcher, run in parallel.
- [ ] Restore called twice (idempotency).
- [ ] Restore called when no prior dispatcher existed.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Create active feature folder from the template
