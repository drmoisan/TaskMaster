---
name: atomic-planner
description: 'Generate deterministic phased implementation plans with atomic [P#-T#] checkbox tasks. Use when Codex needs to turn a goal, issue, or remediation input into an executor-ready plan.'
---

# Atomic Planner

Planning-only workflow skill for generating executor-ready plans.

## Required Shared Skills

Always apply:
- `policy-compliance-order`
- `atomic-plan-contract`
- `execution-environment`

## Role

- Produce a phased plan that an executor can run without replanning.
- Write or update plan files only when explicitly asked or when a calling workflow provides the authoritative target path.
- Do not implement the plan.

## Plan Requirements

Use `atomic-plan-contract` as the source of truth for:
- phase and task formatting,
- Phase 0 requirements,
- baseline evidence tasks,
- final QA requirements,
- the design-level plan contract and the `## Pre-Mortem` section,
- single-pass preflight behavior.

Each task must be:
- atomic,
- binary in completion,
- independently verifiable,
- free of placeholder text.

## Scratch Location

Concurrent child runs in one session share one session scratchpad, so every plan places its scratch files in a directory unique to the child.

- `SCRATCH` resolves to `<session scratchpad>/issue-<issue-num>/`, where `<issue-num>` is the plan's canonical issue number. Every plan that uses scratch files defines `SCRATCH` this way.
- Every scratch file, helper script, staged copy, and coverage report a plan names lives under `SCRATCH`. Subdirectories such as `SCRATCH/stage/` are permitted.
- A plan never directs a scratch file to the scratchpad root or to a fixed shared name outside `SCRATCH`.
- Plans and evidence artifacts record the literal token `SCRATCH`, never a host path.
- If two runs for the same issue can execute at once, the run-unique form `issue-<issue-num>-<yyyy-MM-ddTHH-mm>` replaces `issue-<issue-num>`.

## Mandatory Preflight Loop

Before a plan is finalized:

1. Hand the current plan to `atomic-executor` in preflight-validation-only mode.
2. If preflight returns `PREFLIGHT: REVISIONS REQUIRED`, apply every listed `PREFLIGHT-DEFECT:` change to the same file in one revision.
3. A plan returns to the planner at most once. If the confirming preflight still reports a substantive defect, the orchestrator halts with `blocked_reason: "validator_failed"`; do not open another revision.

Do not finalize a plan before preflight clears it. Every plan follows `## Design-Level Plan Contract (Mandatory)` in `atomic-plan-contract`, including the `## Pre-Mortem` section and the final-phase self-review task.

### Enforced Handoff Contract

- The delegated preflight prompt MUST include the exact directive `DIRECTIVE: PREFLIGHT VALIDATION ONLY`.
- The delegated preflight route MUST remain `atomic-planner -> atomic-executor`.
- `atomic-executor` MUST return exactly one of:
  - `PREFLIGHT: ALL CLEAR`
  - `PREFLIGHT: REVISIONS REQUIRED`
- If revisions are required, `atomic-executor` MUST provide a precise plan delta that can be applied to the same plan file.
- Preflight is single-pass: trivial defects are fixed in place by `atomic-executor`, and only substantive defects return, at most once.
- Do not treat a partial summary as a valid substitute for the exact preflight signal.
- If the required `atomic-executor` handoff cannot be started or completed, stop and report blocked state; do not self-approve the plan.
- Before reporting completion, the target plan MUST pass the `validate_orchestration_artifacts` MCP tool with `artifact_type: "plan"` and `artifact_path: <plan-path>`.

## Write Scope

Allowed writes:
- create or update the plan file,
- normalize an existing plan template into executor-compatible structure.

Forbidden writes:
- source code,
- tests,
- configuration outside the plan file,
- workflow execution artifacts.

## Authoritative Plan Path Rule

When a caller provides an explicit target plan path:

- update that exact file in place,
- reuse that same file for the single permitted preflight revision,
- do not create additional sibling `plan.*.md` files during the same planning cycle.

## Determinism Gates

Reject and revise any plan that contains:
- bucket tasks,
- vague acceptance criteria,
- placeholder tokens,
- mixed discovery and implementation in one task,
- multi-outcome tasks that should be split.
