---
name: orchestrator-state-validator-divergence
description: The MCP orchestrator-state validator is stricter than the real SubagentStop hook; conform to the canonical JSON schema's remediation_loop, not the MCP enums
metadata:
  type: reference
---

`mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: orchestrator-state` enforces a stricter, legacy checkpoint shape
(demands `step7_status`, specific `step5..step10_status` enum values,
`delegation_receipts` as a list, and canonical keys like `promotion-type`,
`short-name`, `plan-path`). This diverges from both the actual gate and the
documented schema.

**Why:** The authoritative termination gate is the SubagentStop hook
`.claude/hooks/validate-orchestrator-output.ps1`, which only requires
`objective`, `completed_steps`, `next_step`, `last_updated` (objective
non-empty) plus a well-formed `remediation_loop` (its `Test-RemediationLoopShape`
checks `plan_path` non-empty, `exit_condition_met==true => blocking_count==0`,
and non-`not_started` execution requires `preflight.final_status=="clear"`). The
canonical `remediation_loop` contract is `.claude/schemas/orchestrator-state.schema.json`,
where each cycle's `audit_paths` is an OBJECT `{code_review, feature_audit,
policy_audit}` (not an array) and `exit_timestamp` is allowed.

**How to apply:** Make the checkpoint satisfy the SubagentStop hook and the
canonical JSON schema (especially the `remediation_loop` cycle shape — use the
object form for `audit_paths`). Do not contort the checkpoint to the MCP tool's
extra undocumented enum demands; treat that tool's orchestrator-state mode as
advisory/legacy. The plan/policy-audit/code-review/feature-audit artifact_types
of the MCP validator are reliable and should still be used.

**Confirmed 2026-07-07 (issue #253): rich `delegation_receipts[]` schema.** The MCP orchestrator-state validator requires every `delegation_receipts[]` entry to carry `step`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`, and `artifact_paths` (in addition to `agent_name`). The portable completion gate (`OrchestratorStateCompletion.psm1`) only needs `agent_name` + a matching `model_routing_receipts[].agent`; provide the rich shape to satisfy BOTH. Keep promotion MCP receipts under a separate key (e.g. `delegation_receipts_promotion`) so `delegation_receipts` can be the agent array the gates read. `config/orchestration-routing.json` DOES now exist (contra the stale note below), and `validate_orchestration_artifacts ... --require-model-routing` passes when each delegated agent has a matching `model_routing_receipts[]` + `complexity_assessments[]` entry.

**Confirmed 2026-07-07: portable step-status enum is wider.** `VALID_STEP_STATUS` in `.claude/lib/orchestrator-state/OrchestratorState.psm1` = {not-applicable, pending, delegated, verified, blocked, not_started, in_progress, completed}. `complete` and `passed` are NOT members (base-presence fails on them). For a passed S9 CI gate use `step9_status: verified` and record the real result in `ci_gate.conclusion: success`.

**Confirmed MCP enum demands (2026-06-23, cost two iterations to discover):**
- Non-complete MCP validation enforces enums: `step5..step10_status` ∈
  {not-applicable, pending, delegated, verified, blocked}; `blocked_reason` ∈
  {none, checkpoint_conflict, lifecycle_preconditions_missing,
  spawn_agent_unavailable, delegation_launch_failed, delegate_no_receipt,
  delegate_contract_incomplete, validator_failed, user_requested_stop,
  review_status_missing, commit_context_missing, no_staged_changes}. There is no
  "awaiting-human" blocked_reason — model human stop points via
  `human_interaction.requirements`, not a custom blocked_reason string.
- `human_interaction.requirements[]` each need `response` ∈ {scope_change,
  exception, halt}. `response: "exception"` additionally REQUIRES a non-empty
  `runbook_path`; `halt` and `scope_change` do not. For a plain stop-and-ask
  with no runbook, use `halt`.
- `--require-complete` additionally fails unless `step10_status != pending`,
  `blocked_reason == none`, AND `route_id` resolves to an entry in
  `config/orchestration-routing.json`. That routing file has never existed in
  this repo, so require-complete cannot pass; the real SubagentStop hook does not
  check route_id, step10, or blocked_reason. Enum source of truth:
  `.agents/skills/orchestrator-workflow/SKILL.md` (Status/Blocked-reason enums)
  and `.agents/skills/orchestrator-state/SKILL.md` (human_interaction invariants).

**Confirmed 2026-07-11 (epic child F4, issue #310): `--require-complete` unsatisfiable for a resumed-at-execution epic-child checkpoint.** When a child orchestrator resumes at atomic execution (promotion/research/spec/planning already done by `epic-planner` Preparation Mode in a *prior* session/checkpoint), `--require-complete` still demands `required_skills`/`required_mcp_tools` receipts for the current route's full matrix and rejects even after adding top-level `required_agents`/`required_skills`/`required_mcp_tools` arrays plus `delegation_receipts[]` entries (which DOES satisfy "missing required agent receipt" for task-researcher/prd-feature/atomic-planner/pr-author) — tried `skills_used`, `skill_receipts`, `mcp_receipts`, `mcp_tool_calls` as alias key names for the skill/MCP-tool checks and none satisfied "missing required skill receipt" / "missing successful MCP receipt". The exact expected shape for those two checks was not discoverable from outside the tool. Do not burn further iterations guessing it — confirm the REAL gate instead: run `.claude/hooks/validate-orchestrator-output.ps1 -CheckpointPath artifacts/orchestration/orchestrator-state.json` directly with `$env:CLAUDE_HOOK_INPUT = '{"output":"<final summary text>"}'` (note: payload key is top-level `.output`, not `.tool_response.output`); it only needs `objective`/`completed_steps`/`next_step`/`last_updated` plus a well-formed `remediation_loop`, and passes cleanly (exit 0) on a checkpoint that `--require-complete` still rejects.
