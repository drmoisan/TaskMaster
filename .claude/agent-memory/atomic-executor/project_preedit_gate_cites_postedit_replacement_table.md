---
name: preedit-gate-cites-postedit-replacement-table
description: A Phase-0 pre-edit citation gate that sources its literals from the plan's own "replacement text" table asserts post-edit strings before the edit, so it can only fail
metadata:
  type: project
---

A baseline citation-re-verification task that says "the N pre-edit tokens named in the R<k> table
each have count 1" is unsatisfiable when the R<k> table is the plan's *replacement* text table,
whose value column holds post-edit text. Before any edit those literals have count 0, so the gate
fails and the executor halts in Phase 0. The pre-edit literals normally live in a separate
"Currently reads" / verified-facts table earlier in the plan.

The companion signal is that the task's command block contains no command producing those N counts,
while the acceptance bullet demands them — an acceptance condition naming an observation the task
never makes.

**Why:** Plans that renumber or restate literals in two tables (current text and replacement text)
invite a cross-reference to the wrong one. Observed on the issue #469 comment-accuracy plan at
`[P0-T15]`, round 3, after the same text had survived two earlier preflight rounds.

**How to apply:** For every pre-edit / baseline assertion, check which internal table the plan cites
and confirm that table's value column holds the *pre*-edit spelling. Then check the task's command
block actually emits each asserted count. Related: [[project_preflight_gate_literal_extract_from_plan_not_retype]],
[[project_exact_count_gate_vs_remediation_loop]].
