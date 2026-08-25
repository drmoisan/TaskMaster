# Hook Failures

## 2026-08-25T11:56:17-04:00

- Attempted action/path: delegated read-only research startup; `Get-Content` in the TaskMaster Issue #608 workspace.
- Hook: PreToolUse model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: agent 'task-researcher-c3' has model, reasoning, or profile drift from its persisted deployment receipt. Correct the receipt and relaunch the exact generated profile.`
- Impact: the S2 task-researcher could not read repository files or produce the required research artifact.
- Corrective action: record the failed attestation, resolve the specialist deployment profile again, and relaunch the generated `task-researcher-c3` profile without inherited runtime routing.

## 2026-08-25T11:56:17-04:00

- Attempted action/path: delegated S2 research shutdown after the blocked read-only startup in the TaskMaster Issue #608 workspace.
- Hook: Stop model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: 'task-researcher-c3' did not run under its recorded deployment model.`
- Impact: the initial research delegation is invalid and cannot supply a lifecycle receipt.
- Corrective action: preserve this attestation failure, record the failed launch receipt in the checkpoint, and relaunch `task-researcher-c3` using the resolved deployment runtime without inherited model routing.

## 2026-08-25T11:58:00-04:00

- Attempted action/path: re-launched delegated read-only S2 research with `fork_turns=none`; initial `Get-Content` in the TaskMaster Issue #608 workspace.
- Hook: PreToolUse model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: agent 'task-researcher-c3' has model, reasoning, or profile drift from its persisted deployment receipt. Correct the receipt and relaunch the exact generated profile.`
- Impact: the corrected-profile research launch could not access repository files or create the required research artifact.
- Corrective action: inspect the model-routing attestation hook and its receipt contract before any further launch; do not bypass the hook.

## 2026-08-25T11:58:00-04:00

- Attempted action/path: re-launched delegated S2 research shutdown after its blocked read-only startup in the TaskMaster Issue #608 workspace.
- Hook: Stop model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: 'task-researcher-c3' did not run under its recorded deployment model.`
- Impact: the re-launched research delegation is invalid and cannot supply a lifecycle receipt.
- Corrective action: preserve this failure and inspect the hook's expected receipt-to-runtime mapping before further launch attempts.

## 2026-08-25T13:32:00-04:00

- Attempted action/path: delegated cycle-3 atomic planning for Issue #608 at `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/correction-and-qa-plan.2026-08-25T13-32.md`.
- Hook: model-routing attestation (reported by the delegated planner).
- Exact failure output/reason: `The recorded deployment model does not match this agent’s runtime model. The plan must be delegated again using the exact atomic-planner-c3 deployment profile recorded in the routing receipt.`
- Impact: the cycle-3 planner made no repository changes and did not produce the required correction-and-QA plan.
- Corrective action: validate the checkpoint routing receipt, correct its placement or runtime mapping, and relaunch only the resolved `atomic-planner-c3` Terra/High profile.

## 2026-08-25T13:35:00-04:00

- Attempted action/path: validation-only cycle-3 atomic-executor preflight for `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/correction-and-qa-plan.2026-08-25T13-32.md`.
- Hook: routing attestation (delegated preflight).
- Exact failure output/reason: `The routing attestation mismatch prevented preflight validation. The parent must relaunch atomic-executor using its recorded deployment model before retrying.`
- Impact: the nested preflight result is invalid and cannot authorize cycle-3 execution.
- Corrective action: preserve the failed preflight receipt, resolve and persist the exact atomic-executor-c3 Terra/High deployment receipt at the top-level attestation-selected location, then launch a fresh validation-only preflight from the orchestrator.

## 2026-08-25T13:58:00-04:00

- Attempted action/path: configured feature-reviewer handoff for cycle-3 P3-T4 in `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608`.
- Hook: model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: model/reasoning/profile drift from the persisted deployment receipt.`
- Impact: the reviewer could not access the workspace; no audit artifacts were created and PR work cannot advance.
- Corrective action: persist the resolved `feature-reviewer-c3` Terra/High receipt in the top-level routing list and retry only the required P3-T4 handoff.

### Exact handoff receipt clarification

- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED` because the delegated feature-reviewer model/reasoning/profile did not match the persisted deployment receipt. The repository PreToolUse hook rejected it before any workspace read or write.

## 2026-08-25T14:13:00-04:00

- Attempted action/path: atomic-planner remediation handoff initiated by feature review for the AC-7 scope reconciliation in `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608`.
- Hook: PreToolUse model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED: agent 'atomic-planner' has model, reasoning, or profile drift from its persisted deployment receipt`.
- Impact: the nested planner was blocked before file read or mutation; no remediation plan validation or execution occurred.
- Corrective action: retain the review remediation inputs/plan targets, persist the exact top-level `atomic-planner-c3` Terra/High routing receipt, and perform the planner handoff from the orchestrator.

## 2026-08-25T14:38:00-04:00

- Attempted action/path: configured PR-body authoring handoff for cycle-3 P4-T1 using `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`.
- Hook: PreToolUse model-routing attestation.
- Exact failure output/reason: `MODEL_ROUTING_ATTESTATION_BLOCKED` because the `pr-author` model, reasoning, or profile drifted from its persisted deployment receipt.
- Impact: the delegate was rejected before source read or artifact write; no PR body, push, PR creation, or CI claim occurred.
- Corrective action: persist the exact `pr-author-c3` Terra/High receipt at the top-level attestation-selected location and rerun the restricted-source handoff.
