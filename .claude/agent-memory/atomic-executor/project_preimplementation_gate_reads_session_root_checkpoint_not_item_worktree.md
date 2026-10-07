---
name: preimplementation-gate-reads-session-root-checkpoint-not-item-worktree
description: enforce-orchestration-preimplementation-gate.ps1 reads artifacts/orchestration/orchestrator-state.json by RELATIVE path, i.e. from the session checkout; a checkpoint seeded only in the item worktree admits no .ps1/.cs Write and no git add/commit from that session
metadata:
  type: project
---

The gate's `$script:CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'` (hook line 31)
is read with `Test-Path`/`Get-Content` against the hook process's current directory, which is the
**session** checkout, not the execution worktree. Observed 2026-09-29, issue 882 Phase 0: the
orchestrator seeded a complete checkpoint (issue-num, feature-folder, route_id, lifecycle_ready) in
the item worktree, P0-T2 verified it, and the next `.ps1` Write into that worktree was still denied
with the generic `PREIMPLEMENTATION_GATE_BLOCKED` text. The session checkout had no
orchestrator-state.json at all (only epic/parallel planner and orchestrator states).

Second trigger in the same run: the command leg's staging detector fails closed on an opaque
`pwsh -Command` payload that contains both the word `git` (e.g. `git ls-files`) and the word `add`
(e.g. a `$list.Add(` call). The payload staged nothing but was refused.

**Why:** a plan's "P0-T2 reads the checkpoint and it is ready" check verifies the wrong file, so the
first implementation-classified write (`.py .ps1 .psm1 .ts .js .cs .json .yml` outside
docs/features/active/) blocks mid-Phase-0, typically at a helper-script task.

**How to apply:** at preflight or P0 start, check that the checkpoint exists at the SESSION root's
artifacts/orchestration/ as well as in the item worktree; if not, report it before execution starts.
Do not route the write through pwsh Set-Content to avoid the gate. Avoid `.Add(` in pwsh payloads
that also run git (use `+=`). Related: [[planner-and-executor-observe-different-worktrees]],
[[batch-budget-hook-discards-out-of-root-powershell-writes]].
