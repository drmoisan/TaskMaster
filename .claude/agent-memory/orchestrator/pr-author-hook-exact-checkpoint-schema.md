---
name: pr-author-hook-exact-checkpoint-schema
description: The exact orchestrator-state shape enforce-pr-author-skill.ps1 demands before gh pr create - agents must be a LIST, each receipt needs 7 specific keys, and relativeFile is required but undocumented
metadata:
  type: project
---

`enforce-pr-author-skill.ps1` re-validates the checkpoint inside the PreToolUse hook and blocks
`gh pr create` with `ORCHESTRATOR_STATE_PREFLIGHT_FAILED`. Verified 2026-09-02 on issue #670, where
it took three attempts because the hook reports only ONE class of error per attempt.

**The three requirements, in the order the hook surfaces them:**

1. **`relativeFile` is a required top-level key.** `Get-OrchestratorStateBasePresenceError` in
   `.claude/lib/orchestrator-state/OrchestratorState.psm1` requires the full `REQUIRED_STATE_KEYS`
   set: `objective`, `change_budget_estimate`, `path_selected`, `promotion-type`, `short-name`,
   `relativeFile`, `long-name`, `issue-num`, `feature-folder`, `work-mode`, `plan-path`,
   `completed_steps`, `next_step`, `last_updated`, `step5_status`..`step10_status`,
   `delegation_receipts`, `blocked_reason`. `relativeFile` is the one no orchestrator skill
   documents; on a resumed item, populate it from `docs/features/potential/promoted/<slug>.md`.

2. **`delegation_receipts.agents` must be a LIST, not an object.** An object keyed by agent name
   (the obvious shape) fails with `delegation_receipts.agents must be a list`.

3. **Each receipt needs seven keys the skill never mentions**: `step`, `agent_name`, `agent_id`,
   `skill_source`, `started_at`, `result_signal`, `artifact_paths`. Note `agent_name` is required
   IN ADDITION to any `agent` field you were already carrying for the model-routing receipts, and
   `agent_id` is satisfied by the subagent id the Agent tool returns.

**PR-creation readiness is a separate, narrower predicate.** `Get-OrchestratorStatePrCreationReadinessError`
checks only: `step5_status`..`step8_status` are none of `pending`/`blocked`/`blocked_remediation_loop_limit`;
`blocked_reason` is absent, null, or `none`; and `local_execution_overrides` / `delegation_bypasses`
are empty lists when present. So `step8_status` must be at least `in_progress` BEFORE you create the
PR — leaving it `pending` blocks you. See [[pr-readiness-gate-bars-any-recorded-override]] for the
override clause.

**How to apply.** Run the predicate yourself before attempting `gh pr create`, which is far cheaper
than discovering the shape one blocked attempt at a time:

```
pwsh -NoProfile -Command 'Import-Module ./.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force; $r = Test-OrchestratorStatePrCreationReadiness -CheckpointPath "artifacts/orchestration/orchestrator-state.json"; "ExitCode=$($r.ExitCode)"; "Output=$($r.Output)"'
```

The parameter is `-CheckpointPath`, not `-Path`. ExitCode 0 with empty Output means the hook will
let the command through. Caution: this predicate passing does NOT mean the receipt-shape checks
above pass — those fire later in the hook, so a clean ExitCode 0 here can still be followed by a
`delegation receipt #0 missing key` block. Check both.

Related: [[pr-author-hook-blocks-gh-in-this-repo]],
[[checkpoint-receipt-namespaces-and-owner-race]], [[orchestrator-state-flat-keys-and-enum]].
