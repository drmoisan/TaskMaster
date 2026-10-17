---
name: atomic-executor
description: 'Execute an atomic-planner plan exactly as written. Use when a plan with [P#-T#] tasks already exists and Codex must carry it out task-by-task without replanning.'
---

# Atomic Executor

Execution-only workflow skill for running an approved atomic plan.

## Required Shared Skills

Always apply:
- `policy-compliance-order`
- `atomic-plan-contract`
- `acceptance-criteria-tracking`
- `execution-environment`

## Role

- Execute the plan of record exactly as written.
- Preserve phase headings, task IDs, checkbox state, and task order.
- Verify each task before checking it off.
- Do not create or revise the plan after execution starts.

## Preflight Rules

Before executing the first unchecked task:

1. Read repository policy in the order defined by `policy-compliance-order`.
2. Validate the plan format and Phase 0 / QA requirements using `atomic-plan-contract`.
3. If the plan is incomplete, non-atomic, or conflicts with repo policy, stop at preflight and produce a precise plan delta.

Blocking is allowed only during preflight.

### Validation-Only Mode Contract

If the incoming handoff includes the exact directive `DIRECTIVE: PREFLIGHT VALIDATION ONLY` (the directive belongs in the delegation prompt, never in a plan file):

1. Perform preflight validation only, in one pass over the entire plan, under `### Single-Pass Preflight` in `atomic-plan-contract`.
2. Do not execute tasks, establish execution state, or run implementation commands.
3. Fix trivial defects in place in task text only (never add, remove, or reorder tasks), record each fix in `<FEATURE>/evidence/other/preflight-fixes.<timestamp>.md`, and report it as a `PREFLIGHT-FIX:` line with a `preflight-fix-record:` path.
4. Return exactly one of:
   - `PREFLIGHT: ALL CLEAR`
   - `PREFLIGHT: REVISIONS REQUIRED`
5. If revisions are required, include a precise plan delta that `atomic-planner` can apply to the same plan file, with one `PREFLIGHT-DEFECT:` line per substantive defect and every substantive defect listed.
6. Return the result to the caller. A plan returns to the planner at most once; the orchestrator routes the defect list for one revision and runs one confirming preflight.
7. If the required handoff cannot be started or completed, stop and report blocked state; do not convert the failure into `PREFLIGHT: ALL CLEAR`.

## Execution Rules

For each task:

1. Announce the exact task ID being executed.
2. Perform only the micro-actions needed for that task.
3. Verify the task acceptance criteria before marking it complete.
4. Check off the plan file on disk immediately after verification passes.
5. Check off satisfied acceptance criteria in the authoritative requirement source file per `acceptance-criteria-tracking`.

## Scratch Location

Concurrent child runs in one session share one session scratchpad, so every scratch file a run writes lives in a directory unique to the child.

- `SCRATCH` resolves to `<session scratchpad>/issue-<issue-num>/`, where `<issue-num>` is the plan's canonical issue number.
- Every scratch file, helper script, staged copy, and coverage report a plan names lives under `SCRATCH`. Subdirectories such as `SCRATCH/stage/` are permitted.
- Create `SCRATCH` before the first scratch write. Never write a scratch file to the scratchpad root or to a fixed shared name outside `SCRATCH`.
- When a plan written before this rule defines `SCRATCH` as the bare session scratchpad, treat that definition as `<session scratchpad>/issue-<issue-num>/`. This is a mechanical micro-action, not replanning.
- Plans and evidence artifacts record the literal token `SCRATCH`, never a host path.
- If two runs for the same issue can execute at once, the run-unique form `issue-<issue-num>-<yyyy-MM-ddTHH-mm>` replaces `issue-<issue-num>`.

## Non-Negotiable Constraints

- Do not invent new phases or tasks.
- Do not reorder tasks.
- Do not substitute an in-session todo list for the plan file.
- Treat the plan file on disk as the only authoritative checklist.
- Do not claim success without verification.
- After execution starts, do not stop mid-plan for replanning.
- Use the `validate_orchestration_artifacts` MCP tool with `artifact_type: "plan"` and `artifact_path: <plan-path>` as an additional fail-closed validation signal during preflight.

## Resume Rule

On resume:
- reload the plan of record,
- find the next unchecked task,
- continue from there without replanning.
