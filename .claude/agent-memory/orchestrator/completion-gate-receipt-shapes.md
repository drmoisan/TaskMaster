---
name: completion-gate-receipt-shapes
description: Exact field shapes the MCP orchestrator-state completion gate requires for delegation, skill, and MCP receipts — guessing the key names wastes several validate cycles
metadata:
  type: project
---

The MCP `validate_orchestration_artifacts` completion gate (`require_complete: true`) rejects
plausible-looking receipt shapes. The authoritative source is
`extensions/drm-copilot/src/lib/validate/orchestrator-state-routing.ts` in the drm-copilot repo
(functions `receiptSkills`, `mcpTools`, `receiptAgents`). Read it instead of guessing.

**Why:** On #508 I burned three validate cycles guessing `skill_name`/`tool_name`/`mcp_receipts`.
The gate silently ignores a receipt that is missing any required field — it does not say *which*
field is wrong, only that the whole receipt is "missing".

**How to apply:** Use these exact shapes.

- `delegation_receipts` must be a **list** (not an object namespaced by phase). Each entry needs:
  `agent_name`, `step`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`,
  `artifact_paths`. If you also need promotion receipts, put them under a *separate* top-level key
  (for example `promotion_receipts`) — the list form is what supplies the delegated-agent set for
  the model-routing gate, so it cannot also be an object.
- `skill_receipts[]` needs exactly `{ skill: <string>, required: true, evidence: <non-empty string> }`.
  `required` must be the boolean `true`; a missing `required` silently drops the skill.
- MCP receipts live under **`mcp_call_receipts`**, not `mcp_receipts`, and need
  `{ tool: <string>, ok: true, evidence: <non-empty string> }`. `tool` must match the canonical
  name in `required_mcp_tools` — if you invoked a variant (for example `new_potential_bug_entry`
  for the `new_potential_entry` requirement), put the canonical name in `tool` and disclose the
  actual variant inside `evidence`.
- `ci_gate` needs `verified_at` in addition to `conclusion`.
- `local_execution_overrides` and `delegation_bypasses` must both be present and be **empty lists**.

Also required earlier, at PR-creation time: `relativeFile`, `long-name`, and `work-mode` (hyphenated)
as flat top-level keys, and steps 5-8 all non-pending. See [[orchestrator-state-flat-keys-and-enum]].

TaskMaster has no Python validator, so the PR-author hook uses the portable PowerShell path
(`Test-OrchestratorStatePrCreationReadiness` in `.claude/lib/orchestrator-state/OrchestratorState.psm1`).
Run it directly to preflight before `gh pr create` — it tells you exactly which step is pending.
