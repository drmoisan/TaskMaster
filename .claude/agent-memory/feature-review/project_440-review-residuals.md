---
name: 440-review-residuals
description: "#440 breadcrumb Left walk-to-root: PASS/0 blocking; the reusable trick for spotting a 'corrected' test that is defect-neutral rather than defect-detecting; AC-15 rate-vs-uncovered-count nuance when a fix deletes covered code; post-440 coverage baseline"
metadata:
  type: project
---

Issue #440 (Qfc breadcrumb Left walks the ancestor chain to the root), reviewed 2026-08-29T07-01.
Cycle 1 verdict PASS, 15/15 AC, **0 blocking**. One-conjunct production deletion
(`activeIndex.Value == row.Chain.Count - 1`) from `BreadcrumbStateModel.LeftArrow()`; the root
boundary is delegated to `BreadcrumbStateRow.ActivateSegment`, which already refuses
`segmentIndex < 0` and `segmentIndex >= Chain.Count - 1`.

## Reusable: spotting a defect-NEUTRAL "corrected" test (CR-1 class)

When a bugfix corrects an existing test that *encoded* the defect, check whether the corrected test
would still pass against the unfixed code. It often would, because removing the wrong assertion is
not the same as adding a right one.

**Cheap detector:** read the fail-before evidence artifact's `Total tests:` count and the names it
lists. A corrected test that is absent from the fail-before set never went red, so it is
defect-neutral. On #440 the fail-before run recorded `Total tests: 2` naming only the two NEW
state-level tests; the corrected router test
`Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` was absent, which is exactly consistent
with it passing both before and after.

**The shape to look for:** unasserted Arrange presses. The corrected router test issues two
`await ArrowAsync(router, "left")` calls whose results are discarded, then asserts only that the
third press yields `UnhandledArrowMessage`. Pre-fix the sequence is handled/unhandled/unhandled, so
the third press still yields exactly that. Enumerate every call site of the helper in the file
(`grep -n 'ArrowAsync(router, "left")'`) to confirm no other test pins the second press.

Judged Major but NOT blocking: the AC only required the test to stop encoding the defect and to walk
to the root first, and the behavior is pinned at the state level with a real fail-before/pass-after
pair. Do not inflate this to blocking unless an AC actually demands router-level regression cover.

## AC-15 rate-vs-uncovered-count nuance

When a fix DELETES already-covered lines, per-file coverage **rates** can fall while nothing
regressed. Here: baseline 119/121 lines and 41/44 branches; final 118/120 and 39/42. Rates dip
(98.3471 -> 98.3333 line; 93.1818 -> 92.8571 branch) but uncovered counts are invariant at 2 and 3.
The `&&` conjunct removal drops exactly 2 conditions (`condition-coverage 100% (8/8)` -> `100% (6/6)`).
Judge such an AC on uncovered counts plus changed-region hits, and **record both readings** in the
artifact so the dip is not discovered later as concealed. Related:
[[csharp-coverage-constants-nondeterministic]].

## Post-#440 repo-wide baseline

Executor run 85.3026 line / 79.2558 branch (54760/64195, 13036/16448). Reviewer's independent
full-suite reproduction the same session: 85.2870 / 79.2376 (54750/64195, 13033/16448) — identical
denominators, ~0.016 pp jitter. Margin above the 85 floor is only ~0.29 pp. Full suite 6859 tests.

## Gotcha: partial-class test files defeat filename-shaped FQN filters

`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` is a `partial` of class
`BreadcrumbStateModelTests`, so every test in it has the FQN prefix
`...Folder.BreadcrumbStateModelTests`. A `/TestCaseFilter:FullyQualifiedName~BreadcrumbStateModelSequenceTests`
silently matches ZERO tests and the run still exits 0. Filter on method names, or on the real class
name, and always sanity-check the reported total.

## Residual observations owed (all non-blocking)

- OB-1 analyzer version skew on `main` (Meziantou 3.0.174 pinned vs 3.0.156 referenced; Roslynator
  4.16.1 vs 4.16.0) — fresh worktrees fail every msbuild with CS0006 until the packages dir is
  hand-provisioned. Durable fix needs its own issue; #440 correctly worked around it in the
  gitignored packages dir only.
- OB-2 AC-8's Moq clause was amended unconditional -> conditional by an **orchestrator**, not the
  maintainer. Amendment predates execution (verified by diffing spec.md between the prep commit and
  head: the executor changed only the 15 `- [ ]` markers). No maintainer ratification in-tree.
- OB-3 Right-descent commit asymmetry + single-level Right descent limit: spec non-goals, owed
  their own issues before this feature folder is archived.
- OB-6 no router-level test asserts a second consecutive Left is handled (see CR-1 above).

Host-identity scan across all 45 changed files: **clean**. TRX/Cobertura/msbuild logs all stayed
under gitignored `coverage/` with scoped `LogFileName=` — the recurring leak class did not recur here.
Related: [[_shared_no_absolute_host_paths]], [[review-worktree-differs-from-session-cwd-mirror-artifacts]].
