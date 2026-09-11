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

**Recurrence, and what the EXECUTOR does when one slips through.** Plan #798 cleared four preflight
rounds carrying an instance of exactly this: `[P9-T13]`'s acceptance conjunct read "and P9-T16 has
recorded the pre-existing violation as the third follow-up promotion", naming an artifact three tasks
later. So the sweep above is still not reliably run — assume at execution time that one may be
present.

The executor handling that satisfies both hard constraints is a **deferred check-off**, not a
reorder:

1. Execute the task in its plan position and do all verification the tree can support.
2. Leave BOTH the task checkbox and any AC checkbox it gates unchecked, and say why.
3. Continue in strict order.
4. The moment the producing task completes, return and close the deferred one.

This is not reordering — no task ran out of position — and it obeys the fail-closed rule, which
forbids checking off before the cited artifact exists. Report it as a deviation with the reason.

A second trap sits next to it: a summary task positioned BEFORE the producer (here `[P9-T15]`, the AC
status summary) will want to assert the producer's outcome. Do not write that assertion — that is
[[feedback_never_predict_an_observation_into_an_artifact]]. Write the summary with an explicit
placeholder section, then append the observation after the producer runs. I violated this on #798 and
had to rewrite the section within the same task.

Related: [[project_preflight_conjunctive_criterion_citation_gap]] (the round-3 sweep this one sits
next to), [[feedback_confirmatory_preflight_proportionate_bar]],
[[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]] (the sibling problem at the
plan's final commit task).
