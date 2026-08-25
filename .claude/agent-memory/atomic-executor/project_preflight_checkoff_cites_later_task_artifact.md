---
name: preflight-checkoff-cites-later-task-artifact
description: Preflight defect class - a check-off task cites an evidence artifact (or a GitHub issue number) that a LATER task produces, which is unsatisfiable in plan order; sweep producer-vs-consumer ordering for every check-off
metadata:
  type: project
---

Sweep every check-off task's cited artifact against the task that PRODUCES it and confirm the
producer's `P#-T#` sorts earlier in plan order. Three prior rounds on plan #488 swept criterion
prefixes, conjunctive citations, and unmodified-file evidence and never ran this check; round 4 found
two instances.

**Why:** the atomic-executor contract forbids reordering, and the fail-closed evidence rule leaves a
task unchecked when its cited artifact is missing. A check-off whose artifact arrives later is
therefore unsatisfiable at the moment it runs, and the plan deadlocks rather than failing loudly.
Round-3-style sweeps do not surface it because the citation names a real artifact path and a real
task ID — nothing is dangling, nothing is false; only the ORDER is wrong.

**How to apply:**

1. Build the pair list `(check-off task, producing task)` for the whole plan and flag any pair where
   producer > consumer. On #488 this caught `[P9-T2]` citing the fail-before index written by
   `[P9-T14]`.
2. Extend the check past artifacts to any *world-state side effect* a criterion needs. The second
   instance was `[P5-T6]`/`[P5-T11]` requiring a GitHub issue number that `[P7-T5]` opened two phases
   later. A "the later task back-fills this artifact" clause does not repair it — the Phase 5
   check-off still runs first.
3. Prefer the **body swap** over renumbering. Swapping the text of `[P9-T2]` and `[P9-T14]` fixed the
   order with zero ID churn and zero cross-reference edits; renumbering would have shifted
   `[P9-T4]`, `[P9-T12]`, `[P9-T13]` and forced edits in `[P0-T17]`, `[P8-T7]`, `[P8-T8]`,
   `[P8-T10]`. See [[project_plan_task_ids_digit_only_forces_renumbering]].
4. For a side effect, move the PRODUCING action into the phase that consumes it and demote the later
   task to "record what the earlier task already created; do not duplicate". Leaving both tasks
   authorized to create the issue invites a duplicate — see
   [[project_preexisting_issue_breaks_promotion_receipt]].

Related: [[project_preflight_conjunctive_criterion_citation_gap]] (the round-3 sweep this one sits
next to), [[feedback_confirmatory_preflight_proportionate_bar]].
