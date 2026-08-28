---
name: uithread-dispatcher-restore-scope-493
description: "Issue #493 research: a single shared gate for both the Ensure helper and the pump-fixture swap is unsafe; the two-lock split, plus the editorconfig analyzer catch-all and the CI-vs-runsettings parallelization asymmetry"
metadata:
  type: project
---

Researched 2026-08-24 for issue #493 (`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` never
restores `UtilitiesCS.UiThread._dispatcher`). Four findings that are not visible from a single read
of the code:

1. **Do not put the "brief" helper and the long-lived pump-fixture swap on the SAME semaphore.**
   The obvious design (one shared `SemaphoreSlim`, briefly acquired by the helper) has three
   failure modes: the helper's two unowned call sites in `QfcItemController.FocusAndThemeTests.cs`
   carry no `[Timeout]`, so they block up to 60 s and hang FOREVER if a pump test expires before
   its restore; and any regression test that holds the gate and then calls the helper self-deadlocks
   (`SemaphoreSlim` is not reentrant). Correct split: a Monitor guarding each individual reflection
   read-modify-write (atomicity, taken by everyone) plus a separate `SemaphoreSlim(1,1)` held only by
   long transactions (mutual exclusion). Lock order gate -> field lock, never the reverse.
2. **The real #230 mechanism is a lost update on a check-then-act, not "two long transactions
   overlapping."** Making each mutation atomic is what makes it unrepresentable; the long gate alone
   was never the load-bearing part for the helper.
3. **`.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion` as a global
   catch-all** (only `MSTEST0032` is a warning). So `CA2000`/`CA1806`/`IDISP004` cannot break the
   toolchain, and changing a helper's return type from `void` to `IDisposable` is safe even at call
   sites that discard the result. Check this before proposing an IDisposable-returning API.
4. **CI and local coverage runs disagree about parallelization.** `QuickFiler.Test` has no
   `[assembly: Parallelize]`, and `.github/workflows/_mstest-coverage.yml` passes no `/Settings:`, so
   it runs SEQUENTIALLY in CI; but `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings`
   both force `Workers=0 / ClassLevel` on every assembly. A regression test for a class-parallelism
   race must therefore create its own threads — it cannot rely on MSTest scheduling two classes
   concurrently.

**Why:** these were derived while checking the orchestrator's design hypothesis for holes; the
hypothesis was sound in intent but would have converted a bounded pump-test failure into an
unbounded hang in a file the feature is forbidden to edit.

**How to apply:** reuse finding 1 whenever a test helper and a test fixture both mutate the same
process-wide static; reuse finding 3 before rejecting an `IDisposable` return type on analyzer
grounds; reuse finding 4 for any regression test targeting a parallelization race in this repo.

Related: `.claude/agent-memory/atomic-executor/project_uithread_dispatcher_static_swap_race.md`,
`.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md`.
