---
name: gate-cites-a-baseline-count-the-baseline-task-never-records
description: A delta gate ("count equals 2, one more than the single occurrence P0-T18 recorded") can cite a baseline figure the cited baseline task's acceptance never requires; check the baseline task's own list, not the citing task's claim
metadata:
  type: project
---

A plan gate expressed as a delta ("`...FromThread(Thread.CurrentThread)` count equals 2, one more than
the single occurrence P0-T18 recorded") names a figure in an earlier artifact. Verify that the earlier
task's acceptance condition actually enumerates that figure. On issue 816 preflight round 2, P0-T18
enumerated five counts — four exit-shape patterns plus `_dispatcher is not null` — and no
`Dispatcher.FromThread(Thread.CurrentThread)` count, while P2-T1 asserted a delta against it. The
sibling clause in the very same acceptance condition (`_dispatcher is not null` equals 1, "up from the
zero P0-T18 recorded") did have its control, which is what makes the miss easy to read past.

**Why:** two failures ride together. The prose claim about the earlier artifact's content is false, and
the delta gate's positive control is unmeasured, so if the pre-change count were already the asserted
post-change value the gate would be vacuous and nothing in the plan would reveal it.

**How to apply:** when a gate says "up from"/"one more than"/"identical to the <task> baseline", open
that baseline task and match the cited figure against the list its acceptance requires. Prefer fixing
the baseline task (add the count with its expected value and a FAIL clause) over deleting the
attribution: that keeps the control and makes the citing clause true without touching the gate.
Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_checkoff_cites_later_task_artifact]].
