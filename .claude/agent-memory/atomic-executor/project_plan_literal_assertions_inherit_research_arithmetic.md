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

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_mstest_donotparallelize_overlaps_parallel_bucket]]
