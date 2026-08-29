---
name: planned-run-worktrees-mimic-inflight
description: A seeded parallel-orchestrator checkpoint plus item branches/worktrees do NOT prove a run is executing; only an open per-item PR does. Reject mutation ops against a planned-but-not-started run.
metadata:
  type: project
---

`parallel-planner` creates one pushed feature branch per item (and preparation-mode child
worktrees) during preparation. Those artifacts survive planning, so a fully planned but
never-started run looks identical to an executing run if you judge by `git worktree list` and
`git branch` alone.

**Correction (confirmed 2026-08-29, run `bugs-635-440`).** The existence of
`artifacts/orchestration/parallel-orchestrator-state.json` is ALSO not proof of execution. The
orchestrator seeds that checkpoint before launching anything: the observed state had
`next_step: launch_cohort_0_batch`, `completed_steps` ending at `checkpoint_seeded`, both items
`state: scheduled` / `merge_status: not_started`, zero item worktrees, and
`gh pr list --head <branch> --state all` returning `[]` for both.

The single decisive signal is the **open per-item pull request**. No item can be past
`worktree_created` without one, so:

- `gh pr list --head <item-branch> --state all` empty for every item => nothing is pinned.
- A planner checkpoint whose `next_step` is `PARALLEL_EXECUTION_READY` means planning finished.
- An orchestrator checkpoint whose `next_step` is `launch_cohort_0_batch` means execution has
  been set up but has NOT begun.

**Why:** The mutation protocol (`/parallel-add`, `/parallel-remove`, `/parallel-close`) exists to
protect PINNED in-flight work. Applied to a not-yet-started run it protects nothing, increments
`recolor_generation` for no reason, and writes a `mutations[]` entry claiming a mid-flight
admission that never happened mid-flight — corrupting the audit log the protocol exists to keep
honest.

**How to apply:** Before any mutation operation, re-derive durable state and confirm at least one
item has an open PR. If none does, REJECT the mutation, append no `mutations[]` entry, change no
state, and recommend re-seeding the generation-0 coloring over the full item set instead. Items
already carrying `preparation_status: prepared` + `PREFLIGHT: ALL CLEAR` in the planner checkpoint
do not need re-preparation; only the new item does.
