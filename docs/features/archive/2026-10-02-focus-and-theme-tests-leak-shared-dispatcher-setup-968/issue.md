# focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968)

- Date captured: 2026-10-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/focus-and-theme-tests-leak-shared-dispatcher-setup/ (Issue #968)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #968
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/968
- Last Updated: 2026-10-02
- Work Mode: full-bug

## Summary

Tests in `QuickFiler.Test` `QfcItemController_FocusAndThemeTests` set up the shared UI-thread dispatcher (through `UiThreadDispatcherFixture.EnsureDispatcher()`) and do not release that setup. Under the parallel test regime, another test class that releases or resets the shared dispatcher can leave a theme test running against a null dispatcher. The #950 preparation found the exposure: its transaction-test fix releases a dispatcher pin at the end of the test, and the same exposure already exists through two other tests in that file.

## Environment

- OS/version: Windows 11 (local) and windows-latest (CI)
- Python version: n/a (C# / MSTest, Workers=0, Scope=ClassLevel)
- Command/flags used: standard MSTest coverage route
- Data source or fixture: `UiThreadDispatcherFixture`

## Steps to Reproduce

1. Run `QuickFiler.Test` in parallel.
2. Have a class that resets the shared dispatcher run concurrently with `QfcItemController_FocusAndThemeTests`.
3. A theme test can observe a null dispatcher. This is intermittent.

## Expected Behavior

Each test class acquires and releases the shared dispatcher through a scoped, reference-counted pin, so no class can null it while another still depends on it.

## Actual Behavior

The theme tests depend on dispatcher state they do not own or pin.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #950 preparation report; its plan carries the stop marker `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

This is the same family as #950 and #882: raced static test-fixture state. It must not be fixed with `[DoNotParallelize]`, Workers=1 or retries. Find and own the shared state. Sequence it after #950 merges, because #950 introduces the pin.

## Proposed Fix / Validation Ideas

- [ ] Have the theme tests acquire and release the #950 dispatcher pin in class initialize and cleanup.
- [ ] Write a deterministic regression test that releases a competing pin mid-test, and show the theme test fails without the fix.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Coordinator Scope Amendment (2026-10-02T22-15, binding)

Recorded by the parent parallel-orchestrator (run `bugs-2026-09-28`, `/parallel-add 968` resume). This amendment supersedes the scope statements in the research record, `spec.md` and the plan wherever they conflict.

1. Issue #972 ("Bug: qfc-datamodel-950-review-residuals", promoted record `docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md`) is FOLDED INTO this item by maintainer direction. This item delivers all five #972 items, and its pull request must close both issues (`Closes #968` and `Closes #972` in the PR body):
   1. Consolidate the three duplicated `SynchronousBackgroundWorker` test helpers (`QfcDatamodelLivenessTests.cs`, `QfcDatamodelTeardownTests.cs`, `QfcInitEmailQueueZeroBatchTests.cs`) into one shared test-support helper.
   2. Reword the `_remainingLoadActive` comment to match the post-#950 behaviour.
   3. `QuickFiler/Controllers/QfcDatamodel.cs` is at 495 of 500 lines: confirm the apparently unused legacy members have no callers, then remove them (or move them) so the file sits well under the limit. Changed-line coverage must not drop.
   4. Dispose the `SynchronousBackgroundWorker` instances in the liveness and zero-batch tests.
   5. Wrap `transactionA` in test R4 in `try`/`finally` (already delivered here as D4; the spec must now record it as closing #972 item 5, not as an overlap for the coordinator to reconcile).
2. The 2026-10-02T05:37Z comment on #968 is also in scope: liveness test 1 in `QfcDatamodelLivenessTests` uses `fake.Advance` plus `Task.Yield` loops, which depend on scheduling. Replace them with a deterministic completion signal.
3. Consequence: this item now changes production code (`QuickFiler/Controllers/QfcDatamodel.cs` and the file declaring `_remainingLoadActive`). AC20 ("No production code change") must be amended to name exactly the production paths this scope requires and nothing else. This is a maintainer-directed widening, not a weakening.
4. All constraints stand: tests stay parallel (Workers=0, ClassLevel); no `[DoNotParallelize]`, Workers=1, retries, `Thread.Sleep`, `Task.Delay`, temporary files or timeout increases; MSTest, Moq, FluentAssertions; failing regression test first for any behaviour defect.
