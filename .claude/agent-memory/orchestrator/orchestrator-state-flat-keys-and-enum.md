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
pass `require_model_routing=true`. Read `fable_policy` from `config/orchestration-routing.json` each
time — it has changed (it read `available` in early 2026-07, `preferred` by 2026-08-08) and a session
directive can override the file. Resolution: `disabled` clamps a `fable` cell to `opus` and records
`clamped_from: "fable"`; `available` uses the base `complexity_to_model` table as-is (C3 -> opus);
`preferred` redirects ONLY the C3 cell to `fable` and ONLY for `atomic-planner`, `prd-feature`,
`feature-review`, `task-researcher` — `atomic-executor` and `pr-author` stay at `opus` on C3 under
every policy. See [[orchestrator-state-validator-divergence]], [[pr-author-hook-blocks-gh-in-this-repo]],
and [[model-routing-scripts-absent-on-epic-integration-base]].
