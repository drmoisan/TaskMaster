---
name: worktree-removal-gate-denies-epic-planner
description: enforce-epic-worktree-removal-gate.ps1 authorizes removal only from an epic-orchestrator or parallel-orchestrator checkpoint, neither of which epic-planner writes — so epic-planner can never clean up its own prep worktrees; leave them and say so
metadata:
  type: reference
---

Measured 2026-09-09. `git worktree remove -f <prep-worktree>` from `epic-planner` returns:

```
EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '-f' requires either an epic checkpoint
features[] record with merge_status in {merged, worktree_removed}, or a parallel-orchestrator
checkpoint with route_id == "parallel" whose matching items[] record (matched by worktree_path)
has merge_status in {merged, worktree_removed}. No checkpoint authorized this removal.
```

The gate reads `artifacts/orchestration/epic-orchestrator-state.json` or
`parallel-orchestrator-state.json`. `epic-planner` writes neither — its checkpoint is
`epic-planner-state.json`, and `merge_status` is not one of its fields. So the gate is
**unsatisfiable for epic-planner by construction**, not merely unsatisfied on a given run.

**Do not author an `epic-orchestrator-state.json` to unlock it.** That checkpoint is the execution
agent's authoritative state; `/epic-run` reads it on resume, and a synthetic one claiming features
are `merged` before execution has started is both hook evasion and a resume hazard. This is the
same reasoning that forbids seeding a decoy `orchestrator-state.json` to unlock the staging
exemption — see [[integration-commit-form-constraints]].

**How to apply.** Plan for the prep worktrees to outlive planning. Say so in the completion report,
name the paths, and note that `epic-orchestrator` removes child worktrees as it merges them during
execution, which is where the authorization legitimately exists.
`scripts/bash/cleanup-worktrees.sh` also exists but is outside the Bash allowlist and prompts.

The consequence that bites: uncommitted `.claude/agent-memory/` writes left by a child survive only
as long as its worktree does. Because `epic-planner` cannot commit that path either, and now also
cannot remove the worktree, the safe outcome is the default — but flag those worktrees explicitly so
a later cleanup does not silently discard the learnings. See
[[fan-in-the-feature-only-commit-agent-memory-conflicts]].
