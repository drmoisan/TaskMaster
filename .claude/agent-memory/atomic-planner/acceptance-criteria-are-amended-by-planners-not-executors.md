---
name: acceptance-criteria-are-amended-by-planners-not-executors
description: "Never plan a task that has the EXECUTOR edit an acceptance criterion in spec.md or issue.md — an executor that rewrites its own gate is not gated; plan a read-only verification task instead"
metadata:
  type: feedback
---

When planning reveals that an acceptance criterion is unsatisfiable and must be amended, do NOT
author a task that makes the executor amend it. Report the needed amendment to the caller, let the
planning/scoping side apply it, and put a READ-ONLY verification task in the plan instead.

**Why:** `.claude/skills/acceptance-criteria-tracking/SKILL.md` places authorship of acceptance
criteria with planning and scoping agents. An executor free to rewrite the criterion it is judged
against is not gated by that criterion — the gate becomes self-issued. Caught on issue #825 round 2:
my round-1 plan had tasks P3-T1/T2/T3 amend AC6 and AC20; the orchestrator rejected the assignment,
applied the amendments itself during preparation, and required the three tasks be replaced by one
verification task.

**How to apply:**

1. In the plan's design-conflict section, state who amended the criteria and when, and keep the
   measurement and the rejected alternative so the choice stays reviewable.
2. Author the verification task as the FIRST task of the phase that depends on the amendment, so a
   stale checkout is caught before any source edit rather than after.
3. Give it machine-checkable assertions over the amended file: an amendment-marker occurrence count,
   a zero-count for the token the amendment deleted, a count of the added Write Set path in its
   backticked bullet form, and the AC inventory count.
4. Express box-state assertions against the state the task will ACTUALLY be in — earlier phases may
   already have checked criteria off. Prefer the box-state-independent inventory regex
   `^- \[[ x]\] \*\*AC[0-9]+\*\*` for the count, and state any checked count as a transition.
   See [[acceptance-edits-must-be-false-before-true-after]].
5. State explicitly in the task that a disagreement makes it BLOCKED, not repairable: the executor
   does not fix the mismatch by editing spec.md.
6. Downstream check-off tasks say "check off ACn in the form the orchestrator amended it into during
   preparation and <verification task> verified on disk", never "as amended by <task>".

Related: [[feedback_ac_checkoff_one_per_task]],
[[project_825_etl_deadline_mechanics_plan_seams]],
[[feedback_spec_corrections_sweep_sibling_sections]].
