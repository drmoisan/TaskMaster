---
name: stale-citation-gate-literal-must-match-the-comments-legitimate-citations
description: A "no line-number citation" gate on a permanent source comment must be scoped to the citation SHAPE the comment could wrongly carry, because a blanket line-number gate is unsatisfiable when the same comment legitimately cites an unmoved file
metadata:
  type: project
---

When a plan task writes a permanent source comment and gates it against stale line
citations, the gate literal has to be chosen per comment, not copied between siblings.
Two shapes are in use and they are not interchangeable:

- `line [0-9]` — blocks a single-line citation. Use it when the comment's subject is a
  single declaration and the comment carries no other line citation at all.
- `lines? [0-9]+-[0-9]+` — blocks only a range citation. Use it when the comment
  legitimately carries a single-line citation into a file that no task in the plan moves.

**Why:** issue #825's plan hit both cases in one round. P7-T1's replacement class comment
names a test whose declaration every earlier phase moves, so `line [0-9]` is right and
satisfiable. P6-T1's budget comment must cite `TaskMaster/log4net.config line 4` — a file
outside the Write Set that no task edits — so a blanket `line [0-9]` there would be
unsatisfiable and the gate has to narrow to the range form. Copying one literal to the
other task breaks it in one direction or the other.

**How to apply:** at preflight, for each such gate, (1) confirm the comment's own required
content does not itself match the gate literal, and (2) confirm the literal actually covers
the citation shape the displaced subject would produce. A range-only gate leaves the
singular form open, so the task's instruction line must independently say "carries no
line-number citation"; treat the instruction as the binding requirement and the gate as one
machine-checkable slice of it. Related: [[project_preflight_gate_literal_extract_from_plan_not_retype]],
[[feedback_never_predict_an_observation_into_an_artifact]].
