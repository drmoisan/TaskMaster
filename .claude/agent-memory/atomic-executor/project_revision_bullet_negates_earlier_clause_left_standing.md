---
name: revision-bullet-negates-earlier-clause-left-standing
description: A planner revision that adds a correcting bullet often leaves the superseded clause standing in an earlier bullet of the SAME task, producing an internal contradiction that no pass reviewed
metadata:
  type: project
---

When a preflight round asks the planner to add a rule, the planner typically appends a new
bullet rather than editing the bullet the new rule contradicts. The result is one task carrying
two incompatible statements about the same object.

Worked instance (#798 plan, P2-T1): the Arrange bullet said the adder "blocks on a
`ManualResetEventSlim` released in the assert phase"; the newly appended bullet said "The
barrier's gate is released in a `finally`, **not in the assert phase**." Same object, opposite
instruction. The added bullet was explicit enough to be self-resolving, so it was reported as
non-blocking.

**Why:** the planner's revision pass re-derives the citations it *touched*. The prose it
supersedes elsewhere in the same task is a sibling it does not re-read, which is the same
sibling-invalidation mechanism `atomic-plan-contract` names — it just fires inside one task
rather than across files.

**How to apply:** on any confirming round, do not read the new text in isolation. Read the
WHOLE task containing it and look specifically for a clause the new text negates by name
(phrasings like "not in the X phase", "never `Y`", "rather than Z" are the tell). Then judge:
- If the new bullet explicitly negates the old clause, the plan resolves itself — report it as
  a non-blocking residual with a one-phrase fix, do not spend a round on it.
- If the two clauses merely differ without one negating the other, the executor has no
  deterministic resolution and it IS blocking.

Related: [[feedback_confirmatory_preflight_proportionate_bar]],
[[project_preflight_recurring_csharp_plan_defect_classes]].
