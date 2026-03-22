---
description: 'Route a C# request through budget-based orchestration: direct small-scope delivery or full feature/bug lifecycle with promotion, research, plan/execution, and review.'
argument-hint: 'Provide: request summary, likely affected files (optional), feature or bug classification (optional), constraints (optional).'
---

# C# Orchestration Command

Invoke the `csharp-orchestrator` skill to run an end-to-end C# workflow.

## Objective

Coordinate the user request from intake to completion using the correct path based on rough change budget.

## Inputs

- **Request summary (required):** clear objective and expected outcome
- **Likely affected files (optional):** any known production/test files
- **Initial classification hint (optional):** `feature` or `bug`
- **Constraints (optional):** APIs/paths/behavior that must remain unchanged

## Required orchestration behavior

1. Estimate rough change budget first (production C# files and test C# files).
2. If budget is **1–3 production C# files** (+ corresponding tests):
   - Execute enforced small-path lifecycle:
     1) Scope/create potential entry and promote with `--work-mode minor-audit`
     2) Create branch and active feature folder with `--work-mode minor-audit`
     3) Delegate plan creation to `atomic-planner` with directive `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED`
     4) Require `atomic-executor` preflight until `PREFLIGHT: ALL CLEAR`
     5) Delegate to `atomic-executor` to execute **Phase 0 only**
     6) Branch behavior:
        - manual bootstrap: save state and stop for manual resume
        - non-bootstrap small development: delegate constrained implementation to `csharp-typed-engineer`
     7) Delegate post-delivery validation to `atomic-executor` for validation against issue.md and persist checklist/doc updates
     8) Run reduced small-path audit and remediation loop until ready-to-merge
3. If budget is **>3 production C# files** or **>3 test C# files**:
   - Execute full large-path lifecycle:
     1) Scope and create potential entry (`feature` vs `bug`, short-name creation)
     2) Promote to GitHub issue and capture issue metadata
     3) Create branch and active feature folder
     4) Delegate research + spec/story completion
     5) Delegate plan creation to `csharp-atomic-planner` (which delegates to `atomic-planner`, then validates preflight via `csharp-atomic-executor` until `PREFLIGHT: ALL CLEAR`)
     6) After all-clear, delegate implementation + QA directly to `csharp-atomic-executor`
     7) Delegate post-implementation feature review to `feature-reviewer`

## Persistence and resume

- Maintain orchestration state in `artifacts/orchestration/csharp-orchestrator-state.json`.
- Resume from the next incomplete step if interrupted.
- Do not end early while required downstream steps remain.

## Output expectations

On completion, report:

- Selected route (`small` or `large`)
- Branch name (if created)
- Key captured variables (`promotion-type`, `short-name`, `issue-num`, `feature-folder` when applicable)
- Created/updated artifact paths
- Final readiness summary and any remaining action items

For small path also report:
- approved `plan-path`
- preflight result (`PREFLIGHT: ALL CLEAR`)
- whether `manual bootstrap` branch was taken
- Phase 0 execution evidence and stored `next_step` for resume
