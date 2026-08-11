---
name: model-routing-hook-reads-canonical-path-only
description: enforce-model-routing-receipt.ps1 hardcodes artifacts/orchestration/orchestrator-state.json, so a child-scoped checkpoint alone will not unblock a delegation — mirror it
metadata:
  type: project
---

`.claude/hooks/enforce-model-routing-receipt.ps1` reads the checkpoint through
`Get-ModelRoutingCheckpoint`, whose `$CheckpointPath` parameter **defaults to the literal
`artifacts/orchestration/orchestrator-state.json`** and is never overridden by the hook entrypoint.
It has no knowledge of a child-scoped checkpoint path. If `model_routing_receipts[]` exists only in
`artifacts/orchestration/orchestrator-state.<feature>.json`, every delegation to a gated subagent
(`atomic-planner`, `atomic-executor`, `feature-review`, `task-researcher`, `prd-feature`,
`pr-author`) is denied with `MODEL_ROUTING_RECEIPT_BLOCKED`.

**Why:** this collides head-on with [[parallel-preparation-children-shared-worktree]], which
prescribes a child-scoped checkpoint path precisely so concurrent siblings do not overwrite each
other's canonical file. The two pieces of advice are both correct but incomplete on their own.

**How to apply:** keep the child-scoped file authoritative (validate against it, record everything
there), then `cp` it to `artifacts/orchestration/orchestrator-state.json` before the first
delegation and after each routing-receipt change. Whether that is safe depends on the topology:

- **Isolated agent worktree** (`.claude/worktrees/agent-<id>/`, cwd == worktree): safe and required.
  `artifacts/` is gitignored and the directory is exclusively yours, so the canonical file there is
  not the shared one. Verified on the #512 preparation child (2026-08-10): `artifacts/orchestration/`
  did not exist at all in the fresh agent worktree. See [[agent-worktree-hooks-resolve-to-agent-cwd]].
- **Shared session worktree** (siblings in the same directory): mirroring races the siblings. Prefer
  ordering the delegation so the mirror is written immediately before the Agent call, and do not
  revert a sibling's later write to the canonical file.

Note the hook is presence-only — it checks that some `model_routing_receipts[]` entry has a matching
`agent`, never the recorded `model`. The MCP validator with `require_model_routing: true` is the
correctness gate. See [[orchestrator-state-flat-keys-and-enum]] for the receipt field set.
