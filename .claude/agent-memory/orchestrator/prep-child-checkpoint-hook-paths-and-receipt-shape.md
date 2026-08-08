---
name: prep-child-checkpoint-hook-paths-and-receipt-shape
description: Preparation-mode epic children must mirror their child-scoped checkpoint to the canonical orchestrator-state.json (hooks hard-code that path), and delegation_receipts entries need a seven-key shape once the array is non-empty
metadata:
  type: project
---

Two checkpoint mechanics bit during epic child F6 preparation (#435, epic #136) and will bite every
preparation-mode child.

**1. The canonical path is hook-mandatory even when the delegation prompt forbids it.**

`epic-planner` instructs preparation children to persist to
`artifacts/orchestration/orchestrator-state.<child-slug>.json` and to never touch the shared canonical
`artifacts/orchestration/orchestrator-state.json`. But two hooks hard-code the canonical path:

- `.claude/settings.json` SubagentStop, matcher `orchestrator` → `validate-orchestrator-output.ps1`,
  whose `-CheckpointPath` defaults to `artifacts/orchestration/orchestrator-state.json`. A missing file
  blocks termination.
- `.claude/hooks/enforce-model-routing-receipt.ps1` (PreToolUse) reads the same default path and denies
  any delegation to a gated subagent when it finds no `model_routing_receipts` entry for that agent.

So the child-scoped file alone means you cannot delegate and cannot stop. **Why:** the prohibition
exists to stop siblings clobbering one shared file; in an isolated `.claude/worktrees/agent-<id>`
worktree there is no sharing, and `artifacts/` is gitignored, so a local mirror is invisible and
harmless. **How to apply:** keep the child-scoped file authoritative, `cp` it to the canonical path
after every write, and say plainly in the report that you wrote a path you were told not to.

**2. `delegation_receipts` flips to a strict schema once populated.**

An empty `delegation_receipts.agents` array validates. The moment it holds an entry, the MCP validator
requires seven keys per entry — `step`, `agent_id`, `skill_source`, `started_at`, `completed_at`,
`result_signal`, `artifact_paths` — in addition to `agent_name`. A shape carrying only
`agent_name`/`phase`/`model`/`scope` produces one error per missing key per entry (7 receipts → 49
errors). **How to apply:** write the full seven-key shape from the first receipt; record in-flight
delegations only after they return, so `completed_at` and `result_signal` are real.

Also confirmed again here: `potential_to_issue` reports a `destination_path` under
`docs/features/potential/promoted/` that it does not actually create. The issue and the active folder
are real; recreate the promoted markdown for the audit trail rather than treating it as a failure. See
[[promotion-potential-md-may-not-persist]].

Related: [[orchestrator-state-flat-keys-and-enum]], [[agent-worktree-hooks-resolve-to-agent-cwd]],
[[parallel-preparation-children-shared-worktree]]
