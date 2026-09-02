---
name: parallel-add-needs-executing-run
description: /parallel-add is inapplicable to a planned-but-unstarted run; the fix is to re-plan with the extra item, not to start the run and then admit
metadata:
  type: project
---

`/parallel-add` operates only on a run that is already executing. Its one hard prerequisite is
`artifacts/orchestration/parallel-orchestrator-state.json`. A run that `parallel-planner` finished
has only `artifacts/orchestration/parallel-planner-state.json` (plus a kickoff artifact) and no
orchestrator checkpoint at all, so the add protocol has nothing to mutate: no `items[]` lifecycle,
no `mutations[]`, no `recolor_generation`, no pinned set.

The two checkpoints are different artifacts with different validator types
(`parallel-planner-state` vs `parallel-orchestrator-state`). Writing add-protocol fields into the
planner checkpoint is a schema violation, not a degraded-mode workaround.

**Why:** admission exists to protect in-flight work via the pinning invariant. With zero items
in flight there is nothing to pin, so the entire decision procedure (`ADMIT_CURRENT_COHORT` vs
`DEFER_AND_RECOLOR`) is vacuous. Starting the run first purely to satisfy the prerequisite is
strictly worse than re-planning: admission still has to run a full preparation-mode child
orchestrator for the new item, and it does so while peers are already burning worktrees and CI,
with quiesce and a `recolor_generation` bump as overhead. Re-planning seeds the new item into
generation 0 with the others and costs one mutation entry less than nothing.

**How to apply:** before acting on `/parallel-add`, check for the orchestrator checkpoint first.
If it is absent, stop and route the request to `/parallel-plan` (re-plan the slug including the new
item). Only recommend `/parallel-run` then `/parallel-add` when execution genuinely must begin
before the new item can be prepared.

Note the checkpoint is per-worktree and gitignored, so "absent" must be checked across the relevant
checkouts, not just the current cwd. See [[parallel-surface-taskmaster-caveats]].
