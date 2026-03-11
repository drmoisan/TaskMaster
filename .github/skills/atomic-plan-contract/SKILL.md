---
name: atomic-plan-contract
description: 'Atomic plan format and toolchain contract shared by planning and execution agents. Use when generating, validating, or executing atomic plans with Phase 0, baseline capture, and final QA loops.'
---

# Atomic Plan Contract

Shared rules for atomic plan formatting, Phase 0 requirements, baseline capture, and final QA loops.

## When to Use This Skill

Use this skill when:
- Creating or validating atomic plans.
- Executing plans with strict format requirements.
- Enforcing Phase 0 policy reading + baseline capture and final QA loops.

## Canonical Plan Format

- Phase headings must be: `### Phase N — <Title>`
- Tasks must start with: `- [ ] [P#-T#]` (or `[x]` for completed)
- Task IDs must match their phase and be sequential per phase.

## Phase 0 Requirements

Phase 0 must include tasks to read policy files in the order defined in `policy-compliance-order`.

Phase 0 must also capture baseline toolchain results for the languages touched. Baseline artifact conventions (location + required fields) are defined in `evidence-and-timestamp-conventions`.

## Final QA Loop (Required for Code/Test Changes)

Run the full toolchain loop for each applicable language in order:
1) Formatting
2) Linting
3) Type checking (if applicable)
4) Testing

If any step fails or changes files, restart the loop from step 1 until a clean pass completes.

## Expect-Fail Test Tasks

Any regression test task expected to fail must be tagged with `[expect-fail]` and include an auditable evidence artifact per `evidence-and-timestamp-conventions`.

## Preflight Validation (Planner ↔ Executor)

When validating or handing off plans for execution:
- Use the directive line: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`.
- Require one of the exact signals:
	- `PREFLIGHT: ALL CLEAR`
	- `PREFLIGHT: REVISIONS REQUIRED`
- If revisions are required, provide a precise plan delta and repeat validation until all clear.

## Mode source precedence (Mandatory)

When a plan is generated or validated from a feature folder, resolve selected mode in this order:

1) Persisted marker in `issue.md` metadata block:
	- `- Work Mode: minor-audit`
	- `- Work Mode: full`
2) Explicit workflow override only when repo policy allows and only when reconciled against `issue.md`
3) Fail closed to `full` when marker is missing or malformed

## Mode-Specific Mandatory Plan Gates

- `minor-audit` plans MUST include baseline evidence tasks, targeted verification evidence tasks, and end-state evidence tasks.
- `minor-audit` plans MUST NOT treat missing `spec.md` or `user-story.md` as automatic blockers.
- `full` plans MUST enforce full-document expectations (`spec.md` + `user-story.md`) and full QA loop obligations.
