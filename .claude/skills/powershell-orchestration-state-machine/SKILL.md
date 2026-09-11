---
name: powershell-orchestration-state-machine
description: Checkpoint schema and resume protocol for long-running PowerShell orchestration workflows.
---

# PowerShell Orchestration State Machine

Canonical checkpoint and resume behavior for orchestration agents running multi-step PowerShell delivery flows.

## When to Use This Skill

Use this skill when:
- A workflow spans multiple delegations and commands.
- Execution may be interrupted and must resume deterministically.
- An orchestrator must avoid repeating completed steps.

## Canonical Checkpoint Location

- `artifacts/orchestration/powershell-orchestrator-state.json`

## Required Checkpoint Fields

- `objective`
- `change_budget_estimate`
- `path_selected` (`small` or `large`)
- `promotion-type`
- `short-name`
- `relativeFile`
- `long-name`
- `issue-num`
- `feature-folder`
- `work-mode` (`minor-audit`, `full-feature`, or `full-bug`; normalize legacy `full` to `full-feature` before persistence)
- `plan-path` (minimal or full plan path)
- `completed_steps`
- `next_step`
- `last_updated`

For short-path runs, also persist:
- `small_path_qc_summary`
- `small_path_audit_artifacts`
- `bootstrap_mode` (`manual-bootstrap` or `auto-small-dev`)
- `phase0_execution_summary`
- `resume_after_manual_bootstrap` (next step token)

## Portable Prepared-State Projection

When the PowerShell delivery flow resumes from a provider-neutral prepared-state
handoff, the active destination projection is
`artifacts/orchestration/orchestrator-state.json`. Validate and retain:

- the destination provider, checkpoint expression, selected projector,
  `plan-path`, and exact recorded `next_step`;
- the portable handoff ID, envelope and latest-history SHA-256 values, selected
  adapter, source validator, identity, repository/workspace/branch binding,
  exact plan proof, lifecycle, capabilities, and scheduler context;
- the source checkpoint path/hash/archive facts and opaque historical receipt
  references without converting them into Claude receipts; and
- `destination_evidence: { status: "pending_first_delegation", receipts: [] }`
  until the first new Claude delegation after materialization.

Do not rediscover the plan or rerun any phase listed in
`portable_handoff.lifecycle.completed_phases`. Resume only the recorded
transition. Record Claude model, launch, worktree, and receipt evidence through
Claude-native fields for new work; functional parity does not require copying
Codex field names or launch representations.

For a scheduled parallel or epic child, validate the run/item, parent
checkpoint path/hash, scheduler and child owners, return contract, exact plan
hash, child checkpoint hash, and result hash before returning a result. The
ordinary child may update only its bounded status/result projection. Cohort or
wave order, barriers, fan-in, integration, cleanup, worktree lifecycle, and
parent completion remain owned by the parent scheduler.

## Update Protocol

- Write checkpoint after every completed orchestration sub-step.
- Treat checkpoint as source-of-truth for progress state.
- Never claim mission completion until checkpoint marks final state.

## Resume Protocol

On invocation:
1) Read checkpoint if it exists.
2) If incomplete, resume from `next_step` without re-running `completed_steps`.
3) If missing or completed, start at phase-0 intake.
4) If user explicitly requests restart, reset checkpoint and start phase-0.
