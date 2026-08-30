---
name: orchestrator-override-does-not-satisfy-an-ac
description: An authorized orchestrator override lets execution proceed past a failed gate but does not make the acceptance criterion satisfied; a terminal N-of-N reconciliation task then becomes unsatisfiable and the run must stop for plan revision.
metadata:
  type: project
---

An orchestrator-authorized override (a recorded `local_execution_override`) authorizes **proceeding
past** a failed gate. It does not convert the underlying acceptance criterion into a satisfied one.
Check off the plan task if the override covers it; leave the AC checkbox unchecked.

**Why:** Observed on issue #644. `[P4-T6]`'s coverage gate reported a 0.01-point shortfall
(85.3194% vs an 85.3303% baseline). The orchestrator authorized proceeding under an override named
`p4_t6_comparison_clause_undecidable_at_measured_noise_floor`, on the ground that two runs over a
byte-identical tree produced 54793 and 54811 covered lines against an invariant 64221 denominator —
a ~0.03-point noise floor, roughly three times the shortfall. The evidence artifact itself said the
decisive thing: *"It is a documented deviation from the task's literal `>=` clause, not a
satisfaction of it."* `[P4-T6]` was checked off under the override; AC-16 was not, because
`acceptance-criteria-tracking` rule 4 requires unmet items to stay unchecked.

Two further traps in the same shape:

- **The AC's named figure source may produce no figure at all.** AC-16 read "the repository coverage
  figure from the AC-15 step-4 run", but that run is `vstest.console.exe /EnableCodeCoverage`, which
  emits a binary `.coverage` file and prints no percentage. The plan silently substituted a
  different command's Cobertura figure. A conjunctive AC fails if either conjunct fails, and a
  conjunct whose named source yields nothing is not satisfied.
- **A favorable run in the artifact is not a pass.** The artifact recorded a second run *above*
  baseline. Citing it would satisfy the plan task's verification clause as literally worded, but
  picking the passing run out of a set is the unfalsifiable-acceptance defect
  `.claude/rules/plan-acceptance-gates.md` exists to report. The artifact expressly disclaimed
  resting on it.

**How to apply:** When a plan task cites an evidence artifact that declares itself a deviation,
read the artifact's own self-assessment as authoritative over a favorable number buried in it.
Leave the AC unchecked. Expect the terminal reconciliation task (here `[P5-T19]`, demanding 18 of
18 checked) to become unsatisfiable, which blocks the final commit task too — that is a
plan-revision question for the caller, not something to resolve by checking the box. Complete every
other verifiable task first so the stop leaves a clean resume point. Relates to
[[exact-count-gate-vs-remediation-loop]] and
[[preflight-conjunctive-criterion-citation-gap]].
