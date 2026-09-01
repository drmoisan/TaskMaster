---
name: checkpoint-gate-exact-key-requirements
description: The two checkpoint gates that block a resumed run - the pre-implementation gate needs a top-level lifecycle_ready boolean, and the PR-creation preflight needs a 6-key delegation-receipt shape
metadata:
  type: project
---

Two hooks read `artifacts/orchestration/orchestrator-state.json` and block on exact key shapes that the surrounding prose does not spell out. Both were hit on a single resumed run (#663, 2026-09-01) and both are cheap to satisfy once the key is known.

**1. `PREIMPLEMENTATION_GATE_BLOCKED` needs a top-level `lifecycle_ready: true` boolean.** `Test-OrchestrationReady` in `.claude/hooks/enforce-orchestration-preimplementation-gate.ps1` requires four things and nothing else: a non-empty `issue-num`, a non-empty `feature-folder` that **starts with the literal `docs/features/active/`**, a non-empty `route_id` (falling back to `path_selected`), and a `lifecycle_ready` property that is truthy. A nested `lifecycle_readiness: { ready_for_execution: true }` object does NOT satisfy it — the check is `$Payload.lifecycle_ready`, top level and flat. The deny message names "lifecycle readiness" without naming the key, so it reads as though a richer structure is wanted.

**2. `Invoke-OrchestratorStatePreflight` rejects a delegation receipt missing any of six keys.** Before `gh pr create` the hook re-runs the PR-creation-readiness validator itself, and every element of `delegation_receipts.agents[]` must carry all of: `step`, `agent_name`, `agent_id`, `skill_source`, `result_signal`, `artifact_paths`. A receipt written with the intuitive `{agent, phase, model, status}` shape fails with one error line per missing key per receipt. Note this is a DIFFERENT required set from the completion gate's, which wants `evidence` — see [[completion-gate-receipt-shapes]].

**Why:** both gates fail closed with messages that describe the intent rather than the key, so the natural response is to add more plausible-looking structure and re-fail. Knowing the literal key names turns two or three blocked attempts into one edit.

**How to apply:** when bootstrapping a checkpoint for a resumed run, write `lifecycle_ready: true` at the top level from the start, and give every delegation receipt the six-key shape even if you also keep your own descriptive fields alongside them (extra keys are ignored). Then run the preflight directly before delegating or before `gh pr create`:

`pwsh -NoProfile -Command 'Import-Module <abs>/.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force; Invoke-OrchestratorStatePreflight -CheckpointPath artifacts/orchestration/orchestrator-state.json'`

It prints `HasErrors=False` when the hook will allow the call, and enumerates every missing key when it will not. The module needs an ABSOLUTE path and a `Set-Location` into the worktree; a relative module path silently resolves to nothing.

**Related trap on the same run:** setting `step8_status`, `step9_status`, or `step10_status` to `completed` fires `COMPLETION_CONSISTENCY_BLOCKED` unless a `ci_gate` object is already present. Use `verified` for step 8 while the PR is still being opened — it satisfies the PR-creation readiness check (which only rejects `pending`, `blocked`, and `blocked_remediation_loop_limit`) without asserting completion. See [[step-status-completed-write-locks-checkpoint]].
