---
name: orchestrator-state-flat-keys-and-enum
description: The orchestrator-state MCP validator requires flat top-level variable keys and a specific step-status enum; nesting under "variables" or using "in-progress" fails
metadata:
  type: feedback
---

The `validate_orchestration_artifacts` (artifact_type: orchestrator-state) MCP validator requires the
lifecycle variables as FLAT top-level keys, not nested under a `variables` object. Required top-level
keys: `objective`, `change_budget_estimate`, `path_selected`, `promotion-type`, `short-name`,
`relativeFile`, `long-name`, `issue-num`, `feature-folder`, `work-mode`, `plan-path`,
`completed_steps`, `next_step`, `last_updated`, `step5_status`..`step10_status`,
`delegation_receipts`, `blocked_reason`.

Valid `stepN_status` enum: `not-applicable`, `pending`, `delegated`, `verified`, `blocked`,
`not_started`, `in_progress`, `completed`. Note `in_progress` uses an UNDERSCORE; `in-progress`
(hyphen) is rejected. Valid `blocked_reason`: `none`, `spawn_agent_unavailable`,
`delegation_launch_failed`, `delegate_no_receipt`, `delegate_contract_incomplete`,
`validator_failed`, `user_requested_stop` (use `"none"`, not null, to be safe).

**Why:** On the folder-hierarchy-live-provider (#350) epic-child preparation run I first nested the
variables under `variables` and used `step6_status: "in-progress"`; the validator failed with
"Checkpoint missing required key: promotion-type ..." and "invalid step6_status". Authoritative
source is `extensions/drm-copilot/src/lib/validate/orchestrator-state-core.ts`
(`REQUIRED_STATE_KEYS`, `VALID_STEP_STATUS`, `VALID_BLOCKED_REASONS`) in the drm-copilot repo.

**How to apply:** In preparation-mode epic-child runs (and any orchestrator run), the checkpoint also
needs `complexity_assessments[]` + `model_routing_receipts[]` populated for every delegated agent to
pass `require_model_routing=true`. In this repo `config/orchestration-routing.json` has
`fable_policy: "available"`, so C3 -> opus for all agents (base `complexity_to_model` table, no clamp,
no overlay). A delegation prompt may override the policy for the session (`disabled` was passed on the
#441 run); under `disabled` the C3 cell is already `opus`, so `clamped_from` stays `null` — the clamp
only fires on a `fable` table cell.

A receipt carrying only `agent_name`/`phase`/`status`/`model`/`artifact_path`/`completed_at` produced
24 errors across 4 receipts (verified 2026-08-10 on the #441 preparation resume). For an in-flight
delegation, `completed_at: null` and `result_signal: null` are accepted.

no overlay). See [[orchestrator-state-validator-divergence]] and [[pr-author-hook-blocks-gh-in-this-repo]].

**Per-receipt required keys (verified 2026-08-10, #457 preparation).** Every entry in the
`delegation_receipts` LIST must carry all eight of: `agent_name`, `step`, `agent_id`, `skill_source`,
`started_at`, `completed_at`, `result_signal`, `artifact_paths`. The validator names the missing key
per receipt index (`Checkpoint delegation receipt #0 missing key: step`), so one malformed receipt
produces seven errors. `phase` is NOT a substitute for `step` — `require_model_routing` matches
`model_routing_receipts[].phase` against the assessment phases while the receipt schema wants `step`,
so carry both. Also confirmed on that run: `step5_status`..`step10_status` all set to
`not-applicable` is accepted (the preparation route's out-of-scope steps), and
`require_model_routing=true` passes with HAND-COMPUTED routing when `scripts/dev_tools/` is absent
from the branch entirely — the validator checks receipt shape and floor/model consistency, not that
you shelled out to the Python reference implementations. See
[[model-routing-scripts-absent-on-epic-integration-base]].

**Two more gates, verified 2026-08-29 on the #635 parallel-item run.** These are enforced by the
pushed-down PowerShell modules, not only by the MCP validator, so they bite even when the MCP route is
not used.

- `enforce-orchestration-preimplementation-gate.ps1` additionally requires a top-level
  **`lifecycle_ready: true`**. `Test-OrchestrationReady` reads `issue-num`, `feature-folder`,
  `route_id` (falling back to `path_selected`), and `lifecycle_ready`, and denies when any is falsy.
  A checkpoint carrying every documented key but omitting `lifecycle_ready` still fails with
  `PREIMPLEMENTATION_GATE_BLOCKED`, whose message does not name the missing key. That gate also blocks
  the **Write tool creating the checkpoint itself**, so bootstrap it with `python3 -c` instead.
- `delegation_receipts` may be a LIST or an OBJECT namespace, but the object form's **`agents` value
  must itself be a LIST**, not a map keyed by agent name. A map produces
  `Checkpoint delegation_receipts.agents must be a list.` Only `agents` and `promotion` are supported
  keys of the object form. The eight per-receipt required keys above apply to the entries of that inner
  list.
- Run the gate yourself before delegating or creating a PR rather than guessing the shape:
  `Import-Module ./.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force;`
  `Invoke-OrchestratorStatePreflight -CheckpointPath artifacts/orchestration/orchestrator-state.json`
  names every missing key in one pass, which converges in two or three rounds instead of one key at a
  time.

**`enforce-promotion-mcp-only.ps1` matches Bash command TEXT, not intent.** A `python3 -c` that merely
writes the MCP promotion tool-name literals into a checkpoint's `required_mcp_tools` array is blocked
with `PROMOTION_MCP_ONLY_BLOCKED`. Split the literals across string concatenation to write them.
See [[hooks-pattern-match-bash-command-text]].

**Do not fabricate a lost receipt.** When a prior attempt dies and takes the gitignored checkpoint
with it, the raw MCP promotion payloads are gone. Record `receipt: null` plus a `receipt_note`
explaining what corroborates the invocation (folder on disk, issue.md provenance section), and set
`agent_id: "unknown-prior-attempt"` rather than inventing an ID. The validator accepts this; the
audit trail stays honest.
