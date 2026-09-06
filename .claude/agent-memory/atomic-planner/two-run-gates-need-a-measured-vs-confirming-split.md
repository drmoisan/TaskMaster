---
name: two-run-gates-need-a-measured-vs-confirming-split
description: "When an AC is verified by two runs (pre-commit staged-index sweep and post-commit diff sweep), name exactly one as measured and the other as confirming, and never state an unmeasured scale figure as an order-of-magnitude claim"
metadata:
  type: feedback
---

Two authoring rules learned on the #752 remediation plan (rounds 3-4), both reported as preflight
defects rather than found by the planner.

**1. A criterion verified by two runs must declare which run measures it.** The #752 plan verified
AC-R3/AC-R4 with a `[P2-T5]` step-3 `Index` sweep (merge base vs staged index, recorded in a
committed artifact) and a step-8 `Diff` sweep over `<merge-base>..HEAD` after the commit. AC-R3 said
"measured by step 3, confirmed by step 8"; step 8's own task text said "this is the run AC-R3 and
AC-R4 are measured against". Both readings are defensible in isolation, so an executor can satisfy
the AC from either run and the recorded evidence may not be the run that was judged.

**Why:** the two runs observe different trees — the staged index and the post-commit tip — so they
are not interchangeable, and only one of them produces the artifact the AC cites. Leaving the choice
open lets the executor pick the evidence it is graded on, which is the failure the atomic-plan
contract's "fix the evidence in the plan" rule names.

**How to apply:** for any AC with more than one verifying run, write **measured** on exactly one run
and **confirming** on the rest, state which artifact records the measured values, and declare
disagreement between them a BLOCKED outcome. Then grep the plan for every other sentence that names
either run — the contradicting claim is usually in the task text, not in the AC.

**2. A non-gated scale figure is still a fact and must be measured and proportionate.** The same plan
carried a whole-tree residue count of "1,268 markdown files ... three orders of magnitude larger",
recorded explicitly as an observation nothing gates on. The reviewer's re-measurement got 956, and
956 against a 4-file in-scope result is ~2.4 orders, so the magnitude claim also breached
`.claude/rules/tonality.md`'s hyperbole prohibition.

**Why:** "nothing gates on it" exempts a figure from the acceptance contract, not from being true.
A wrong control figure in the rationale section undermines the scope argument it exists to support.

**How to apply:** state the measurement method and the moment it was taken ("measured at preflight
against the current branch tip, by counting distinct markdown files carrying any of the five token
classes"), use the arithmetically correct magnitude, and add a sentence saying later drift in the
figure is not a plan defect — otherwise a future round re-measures it and reports a mismatch.

See [[acceptance-edits-must-be-false-before-true-after]],
[[observation-scope-must-match-blast-radius]], [[diff-gates-need-a-commit-task]],
[[project_752_relative_path_anchor_plan_seams]].
