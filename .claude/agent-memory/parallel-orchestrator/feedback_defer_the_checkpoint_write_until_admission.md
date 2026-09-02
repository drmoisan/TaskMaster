---
name: defer-the-checkpoint-write-until-admission
description: In /parallel-add, do not write the candidate into items[] at proposed-state entry — the schema cannot express it, and concurrent adds race on the one checkpoint file; do the whole write after preparation returns
metadata:
  type: feedback
---

`/parallel-add` step 1 says "enter `proposed`: add the item to `items[]`". Do not do that
literally. Keep the candidate out of the checkpoint until preparation returns a real
declared blast radius and the admission decision has produced a cohort index, then apply
the item record, the cohort assignment, and the `mutations[]` entry in one fresh
read-modify-write.

**Why:** Two independent reasons, and the second is the dangerous one.

1. *The schema cannot represent a proposed item.* Invariant 9 lets `modules`,
   `shared_surfaces`, and `contracts` be empty but not `paths`, and invariant 13 requires
   every non-withdrawn item to sit in exactly one current-generation cohort — `withdrawn`,
   `merged`, and `blocked` are the only exempt states, and `proposed`/`admitted` are not
   among them. So an honest early write is invalid, and the alternatives are both
   fabrications: a placeholder radius, or a placeholder cohort index that asserts a
   schedule position before the admission decision computed one. Observed 2026-08-29: a
   concurrent add wrote item 637 at `admitted` with a one-path placeholder radius and no
   cohort entry, leaving the shared checkpoint failing invariant 13 for as long as its
   preparation ran.
2. *Concurrent adds race.* Two `/parallel-add` invocations can be in flight at once, and
   they read-modify-write the same `artifacts/orchestration/parallel-orchestrator-state.json`.
   `next_step` and `completed_steps` are single-writer fields that both operations want,
   so an early write held across a long preparation step is a wide window for a lost
   update. The `mutations[]` monotonic-`recolor_generation` rule exists to make such a
   lost update detectable AFTER the fact — it does not prevent one.

The constraint in the skill already sanctions the deferral: "a failed preparation appends
no entry and leaves `items[]` without the candidate."

**How to apply:** Re-derive durable state up front, launch preparation, touch nothing
while it runs. On return, re-read the checkpoint fresh (another add may have landed items,
recolored, or bumped `recolor_generation` meanwhile), recompute conflict edges over ALL
items including any that arrived during preparation, then decide and write. If the Edit
tool reports the checkpoint changed since your read, that is the race announcing itself —
re-read and recompute the decision, never re-apply the stale edit. See
[[parallel-run-execution-playbook]].

**The admission DECISION itself can invert while preparation runs, not just the checkpoint
around it.** Preparation takes tens of minutes, and an in-flight sibling can reach merge in
that window. Observed 2026-09-01 on `/parallel-add 646`: at add time item 647 was
`in_flight` with PR 712 open, and 647's declared radius already named both files 646 must
edit, so the decision on that reading was `DEFER_AND_RECOLOR` at `recolor_generation` 2.
PR 712 merged mid-preparation, which emptied the pinned set and left no non-terminal member
in any current-generation cohort — so the correct decision became `ADMIT_CURRENT_COHORT`
with NO recolor and the generation stamped unchanged. Had the pre-preparation verdict been
applied, the run would carry a spurious generation bump and a needlessly deferred item.

Two consequences worth internalizing:

- **Never carry a pre-preparation verdict forward.** Recompute `in_flight`, `current_cohort`,
  `current_cohort_members`, and `highest_pinned_cohort` from `git`/`gh` AFTER preparation
  returns. The pre-check reading is what justified spending the preparation cycle; it is not
  an input to the decision.
- **A conflicting neighbour that merged does not stop being an edge.** Record the edge anyway
  — 646~647 is real and stays in `conflict_edges[]`. It still constrains cohort PLACEMENT: the
  candidate cannot join the merged neighbour's index, because a cohort must remain an
  independent set, so it takes the next index even on the no-recolor branch. Edge existence and
  barrier satisfaction are separate questions.

Also expect the checkpoint to have moved on independently: the same run's parent advanced 647
from `pr_open` to `worktree_removed` while this add was computing, so the read taken before
preparation was two transitions stale by the time of the write.

**The verdict flips in BOTH directions, so neither pre-preparation reading is safe to carry.** The
646 case above is DEFER becoming ADMIT because a pinned sibling merged. The opposite is just as
common on a busy run: a sibling ADD landing mid-preparation turns ADMIT into DEFER, because the
newly admitted item joins the current cohort as an unstarted `scheduled` member the candidate can
contend with. Observed twice on run `bugs-638-644-647` — `/parallel-add 656` began when every item
was terminal (ADMIT) and had to defer once 646 landed, and `/parallel-add 285` began at generation 1
and had to be decided against generation 2 after 656 landed. Three of the four adds on this run
overlapped another mutation, so treat concurrency as the normal case rather than the exception.

**A deferred recolor MOVES previously-admitted unstarted items, and can place the new candidate
AHEAD of them.** This looks wrong the first time and is correct. The pinning invariant protects
`in_flight` items only; every unstarted item (`proposed`/`admitted`/`prepared`/`scheduled`) is a
vertex of the recolored subgraph and may change index. Observed on `/parallel-add 285`: the
unstarted set `{285, 646, 656}` was a triangle, `compute-cohorts.sh` returned the singleton classes
in ascending-key order `[[285],[646],[656]]`, and with the offset that put the NEW item at
`current_cohort` (4) and pushed 646 to 5 and 656 to 6. Write it verbatim anyway. "Defer" names the
branch, not a guaranteed position, and the safety property is unaffected: conflicting items land in
distinct cohorts, so the per-edge barrier still prevents concurrency. Re-basing the indices to keep
incumbents first would be the actual defect.

**The candidate's cohort position is predictable: it is its rank by ASCENDING KEY within the
unstarted set.** Where the unstarted subgraph is complete — the normal case on a C# run, since the
mandated coverage-script citation makes every pair contend — Welsh-Powell returns singleton classes
in ascending key order, so the candidate lands at `current_cohort + (its rank among unstarted keys)`.
A high-numbered issue therefore appends last and disturbs nobody, while a LOW-numbered issue admitted
late displaces every incumbent above it. Confirmed 2026-09-01 on `/parallel-add 287`, where the new
item took index 5 and pushed 633, 646, 656, 670 and 678 each up one, while 285 kept index 4. Use this
to predict the churn before the write, and do not read a displacement as a bug: the five moved items
were all unstarted, and conflicting items still land in distinct cohorts so the per-edge barrier is
intact. The earlier runs where the new item happened to land last were a property of their keys, not
a rule.

**The inversion runs BOTH ways, and the second direction is the unsafe one.** A sibling
reaching merge flips `DEFER` to `ADMIT`; a sibling being ADMITTED flips `ADMIT` to `DEFER`.
Observed 2026-09-01 on `/parallel-add 656`, immediately after the 646 case above: at add time
every item in the run was terminal and the pinned set was empty, so the verdict on that reading
was `ADMIT_CURRENT_COHORT` with no recolor. `/parallel-add 646` landed while 656's preparation
ran, adding a fifth item at `scheduled` in cohort 4, four new conflict edges, and advancing
`current_cohort` from 3 to 4. Item 656 conflicts with 646, so the correct verdict became
`DEFER_AND_RECOLOR` at generation 2.

Note the asymmetry in what a stale verdict costs. Carrying a stale `DEFER` forward costs a
spurious generation bump and a needlessly deferred item — wasteful but safe. Carrying a stale
`ADMIT` forward puts two CONFLICTING items in one cohort, which breaks the independent-set
property the whole surface rests on, and `max_concurrency` 2 would then have launched them
together. It also fails Layer 2's structural reading of the barrier. So the re-read is not
housekeeping; in this direction it is the only thing standing between the run and a real
concurrency violation.

**`current_cohort_members` must include `scheduled` members, not just pinned ones.** That is
exactly what made 656 defer: 646 was never in flight, only admitted and waiting. An admission
check written against the `in_flight` subset alone would have missed it and admitted 656
alongside it.
