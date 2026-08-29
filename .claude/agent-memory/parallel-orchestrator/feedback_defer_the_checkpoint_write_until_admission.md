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
