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

Counts inside a task body are blocking (the executor must record them in an evidence artifact a
feature-reviewer later re-derives); counts in a summary section are still worth a delta because the
fix is one token.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_mstest_donotparallelize_overlaps_parallel_bucket]]
