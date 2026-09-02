---
name: pr-creation-readiness-exact-requirements
description: The exact checkpoint shape Invoke-OrchestratorStatePreflight demands before gh pr create — including the 8-key delegation-receipt schema and the step-status value that satisfies it WITHOUT tripping the completion-consistency hook
metadata:
  type: project
---

Run the gate directly instead of guessing; it names every missing key in one pass:

```powershell
Import-Module ./.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force
$r = Invoke-OrchestratorStatePreflight -CheckpointPath 'artifacts/orchestration/orchestrator-state.json'
$r.HasErrors; $r.ErrorText
```

Verified against #648 on 2026-09-01. Three requirements are not obvious from any doc:

**1. `relativeFile` is required** and is not in the commonly-copied key set. It is the
`docs/features/potential/promoted/<slug>.md` path. On a resumed item nobody re-runs promotion, so
read it off disk rather than inventing it.

**2. `delegation_receipts.agents` must be a LIST, and each element needs all eight of:**
`step`, `agent_name`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`,
`artifact_paths`. An object (`{}`) fails with "must be a list". A receipt carrying the intuitive
`agent`/`phase`/`model`/`result` names fails with eight separate missing-key lines — those names are
not aliases and are simply ignored.

**3. Use `verified`, not `completed`, for `step7_status` and `step8_status`.**
This is the trap. PR-creation readiness rejects `pending`, so the reflex is `completed` — but a
`completed` step status makes the checkpoint *assert completion*, and the write is then blocked by
`COMPLETION_CONSISTENCY_BLOCKED` demanding a `ci_gate` with `conclusion == "success"` and a non-empty
`head_sha`, which cannot exist before the PR is even open. `verified` is in `VALID_STEP_STATUS`, is
not in `COMPLETION_BLOCKING_STEP_STATUS`, and satisfies the readiness gate. Move to `completed` only
after CI is green and you can supply real `ci_gate` and `pr_gate` evidence.

The vocabulary is `VALID_STEP_STATUS` in `.claude/lib/orchestrator-state/OrchestratorState.psm1`:
`not-applicable`, `pending`, `delegated`, `verified`, `blocked`, `not_started`, `in_progress`,
`completed`. `step9_status` additionally accepts `passed`, `failed_remediation_required`,
`blocked_ci_loop_limit`; `step6_status` additionally accepts `blocked_remediation_loop_limit`.

Related: [[step-status-completed-write-locks-checkpoint]],
[[bootstrapping-orchestrator-state-json-first-write]], [[orchestrator-state-json-is-tracked-in-git]],
[[pr-author-hook-blocks-gh-in-this-repo]].
