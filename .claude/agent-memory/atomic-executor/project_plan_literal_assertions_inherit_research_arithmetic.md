---
name: plan-literal-assertions-inherit-research-arithmetic
description: Plan tasks that quote an exact expected string usually copy it from the research artifact verbatim — recompute the arithmetic during preflight, because a wrong literal is a guaranteed test failure
metadata:
  type: project
---

When a plan task specifies an exact expected literal for a test assertion (e.g. `",40,0.66,"`),
that literal is almost always copied verbatim from the feature's research artifact. Recompute it
from the production source during preflight rather than trusting it.

**Why:** #437 (F8 EFC home-controller coverage) task `[P5-T2]` asserted `",40,0.67,"`-worth of
output as `",40,0.66,"`. The production line is
`(duration / 60d).ToString("##0.00")` with `duration = 120 / 3 = 40`, so .NET emits `"0.67"`
(custom numeric format rounds 0.6666… away from zero). The same wrong value sat in
`research/EfcHomeController.Metrics.research.md` § 5 T2 — the planner faithfully copied a research
arithmetic error, and the plan would have mandated a test that cannot pass.

**How to apply:** during preflight, for every task quoting a literal expected string, open the
production formatter/interpolation and evaluate it by hand. Watch specifically for
`ToString("##0.00")`-style rounding, integer division, and `.Seconds` vs `.TotalSeconds`. Report a
wrong literal as a blocking executability defect with the corrected value; do not assume the
executor will "just fix it", because the plan text is the authority.

Two more literal classes in the same family, both confirmed wrong in #453 (F10 QfcItemController
coverage) after every one of that plan's ~200 `file:line` citations verified clean:

1. **Post-edit line-count projections.** `[P2-T4]`/`[P10-T1]` predicted
   `Initialization.cs` at "approximately 402" after three deletions; the real figure is ~372-373.
   The planner had subtracted only the two `public static` factories (466 − 64) and silently
   dropped the 29-line private `Initialize` overload. Sum every deletion range in the plan
   yourself, and remember csharpier collapses the double blank line a mid-file deletion leaves.
2. **"All N sites" enumerations.** `[P7-T1]` said "replace all **seven** duplicated inline guard
   blocks" and listed seven `:start-end` ranges; a grep for the guard's first line returned
   **eight**. `[P8-T1]` said "exactly five sites" then listed six; `[P10-T2]` said the `??=`
   pattern was "used seven times" where the block has eight. Always `Grep` the distinguishing
   token and compare the count AND the line set against the enumeration — the count word and the
   list are written at different times and drift apart. An "all N" that is really N+1 is blocking:
   the executor cannot reconcile "all" against a short list without replanning.

**Second class: aggregate counts.** The same defect appears in *count* literals, which are easy to
skip because they read as prose. Recompute all four of these mechanically at preflight:

1. **Enumeration counts of an external artifact** — e.g. #452 `[P0-T20]` said the committed
   `coverage-final.cobertura.xml` holds "71 distinct QuickFiler files"; both a distinct-`filename`
   prefix count and a `<package name="QuickFiler">`-scoped count give **70**.
2. **Requirement-list item counts** — e.g. #452 `[P7-T14]` said the `spec.md` Definition of Done has
   "Ten items" while the list holds **11** checkboxes and the task's own inline enumeration names 11.
3. **Test-inventory totals in the Test Plan section** — count the test-method-adding task IDs per
   phase and re-add them. #452 claimed 293 with a per-phase breakdown summing to 299/308, and stated
   a branch-B Phase-2 figure of 21 against 17 actual tasks.
4. **Per-phase task-range counts** — `last_id - first_id + 1` for each contiguous test block.
5. **Declaration count vs execution count when a `[DataTestMethod]` is present.** Executions =
   (plain `[TestMethod]` count) + (total `[DataRow]` count). The recurring error is adding the
   DataRows to the *declaration* total instead of substituting for the one data-driven declaration.
   #495 Phase 2 had 5 plain + 1 `[DataTestMethod]`×3 rows and claimed "6 declarations with 9
   executions"; the true figure is 8 (6 + 3 instead of 5 + 3). The same plan's Phases 4 and 5 did
   the substitution correctly, so a single phase can drift while its siblings are right — check
   every phase independently, then re-add the Test Plan grand total. Blocking: the scoped-run task
   asserts "N new executions passed" and vstest will report N−1.

Counts inside a task body are blocking (the executor must record them in an evidence artifact a
feature-reviewer later re-derives); counts in a summary section are still worth a delta because the
fix is one token.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_mstest_donotparallelize_overlaps_parallel_bucket]]
