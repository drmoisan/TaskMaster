---
name: 584-review-residuals
description: "#584 UiThread.Dispatcher guard review — ACCEPT/0 blocking, 7/7 AC; the census that mattered was the one the work itself was forced to add after a real 8-of-1312 regression"
metadata:
  type: project
---

Issue #584 (`bug/uithread-dispatcher-null-race-progresstrackerasync-584`, base `87cb4df3`): reviewed
2026-09-04, **ACCEPT, 0 blocking, 7 of 7 AC PASS**. Six files, one production
(`UtilitiesCS/Threading/UiThread.cs`).

**Why:** several residuals here are not derivable from the merged code, and the blast-radius pattern
generalises to any "throw where we used to return null on a public static" change.

**How to apply:** when a similar accessor-hardening change appears, reuse the three-route census below
rather than trusting a single grep.

Residuals and non-obvious facts:

- **Three read routes, not one.** A public static property can be read via (1) the qualified
  expression `Type.Member`, (2) reflection (`GetProperty("Member")`), (3) `using static`. The plan's
  original census covered only route 1, missed route 2, and that miss *materialised* as 8 of 1312
  failing in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — `PropertyInfo.GetValue`
  propagates a throwing getter as `TargetInvocationException` from `[TestInitialize]`/`[TestCleanup]`.
  Route 3 was enumerated by neither the plan nor P0-T14; I closed it (`using static .*UiThread` = 0
  hits). Check all three.
- **The repair idiom:** retarget the reflective snapshot from the property to the private backing
  field. Reading the field observes the same state without invoking the guard.
- **`[DoNotParallelize]` census scope gap** — see [[donotparallelize-census-misses-lazy-init-writers]].
- **`ProgressTracker_Tests.cs` is 514 lines at BASE and stayed 514.** The executor deliberately wrote
  `[TestClass, DoNotParallelize]` as a combined attribute list rather than a second line, to avoid
  515. Pre-existing overage, branch delta 0 -> PARTIAL non-blocking per the #324 severity split.
- **`artifacts/pr_context.*` in this worktree were stale and foreign** — they describe issue #565,
  branch `bug/invoke-mstestwithcoverage-threshold-before-setcontent-565`, base `87233f86`, and the
  appendix pointer names a different worktree. Carried in from main by another cohort item. Confirms
  [[pr-context-artifacts-are-tracked-not-gitignored]].
- **Raw coverage was 70.736% line / 46.79% branch** (unstripped `dotnet-coverage` over the whole
  `UtilitiesCS.Test` process). Recorded FAIL non-blocking on both rows; baseline was 70.733%, so
  pre-existing, and changed-line coverage was 100% (8 of 8). Canonical `artifacts/csharp/coverage.xml`
  absent; `coverage/p4-t5.cobertura.xml` + `coverage/p0-t10.cobertura.xml` were the substitutes.
- **Owed at merge:** `spec.md` "Rollout & Follow-up" item 1 (synchronize
  `ProgressTrackerAsync_Tests.cs`'s reflective mutation of `UiThread._dispatcher`, a #493-shaped
  concern in a different assembly) exists only as feature-folder prose and dies with the folder.
  Item 2 (the `IUiDispatcher` seam conversion) is already durable on the issue thread.
- `UiThread.cs:122` still carries `_uiSyncContext!` — the same suppression class this change removed
  from `_dispatcher`, untouched and a candidate for identical treatment.
