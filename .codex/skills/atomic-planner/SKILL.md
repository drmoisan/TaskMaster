---
name: atomic-planner
description: Produce deterministic phased implementation plans with atomic checkbox tasks, machine-verifiable acceptance criteria, and mandatory atomic-executor preflight clearance before finalization.
---

# Atomic Planner

## When to use
Use this skill when you need a phased implementation plan that can be executed exactly as written by `atomic-executor` without replanning.

## Shared skills to apply
Use these skills before and during execution when available:

- `policy-compliance-order`
- `atomic-plan-contract`

## Role
You are a planning-only workflow.

You may read code, plans, docs, and policy for context, but you do not implement the plan.

## Hard constraints
You must not:
- implement code
- change configuration
- write tests outside the plan document
- execute repo changes
- run mutating commands beyond writing or updating the plan document itself

Your only allowed writes are:
- creating a new Markdown plan file
- updating an existing Markdown plan file

## Required output
Always output:
1. a short overview
2. a phased plan composed of atomic checkbox tasks

The plan must be executable by `atomic-executor` without replanning.

## Plan structure rules
Follow the canonical structure defined by `atomic-plan-contract`.

Minimum requirements:
- phase headings must be canonical
- task IDs must use `[P#-T#]`
- tasks must be checkboxes
- task order must be deterministic
- acceptance criteria must be machine-verifiable

## Mandatory planning rules

### Phase 0 requirement
If the plan changes code or tests, it must include Phase 0 baseline capture tasks.

Phase 0 must include the required repository-policy reading and baseline evidence capture required by repo rules and `atomic-plan-contract`.

### Final QA requirement
If the plan changes code or tests, it must include a final QA phase that runs the full repo-standard toolchain loop for all impacted languages.

### Mode source precedence
When planning from a feature folder, resolve work mode in this order:
1. `issue.md` marker
2. legacy `- Work Mode: full` → `full-feature`
3. explicit workflow override only if repo policy permits it
4. otherwise fail closed to `full-feature`

Mode rules:
- `minor-audit` plans must include explicit baseline evidence tasks, targeted verification evidence tasks, and end-state evidence tasks
- `full-feature` plans must include full-document and full-QA obligations
- `full-bug` plans must include spec-driven documentation and full QA obligations

### Zero-placeholders gate
Do not output placeholder text.

Reject or revise the plan if it contains tokens such as:
- `<Phase Name>`
- `<Atomic task`
- `...`
- `TBD`
- `TODO`
- `(fill in`

### Atomicity gate
Every task must be atomic:
- binary in completion
- single-outcome
- short in duration
- unambiguous

Split any task that:
- contains multiple independent outcomes
- behaves like a bucket
- uses “and” to join independent deliverables
- would plausibly be partially complete

### Machine-verifiable acceptance gate
Acceptance criteria must be objectively verifiable.

Allowed examples:
- a named test passes
- a command exits with code 0 and contains an exact substring
- a file exists and contains an exact expected line

Forbidden examples:
- manual verification
- looks correct
- works in terminal
- manual inspection as a gating criterion

For expect-fail regression tasks:
- tag the task appropriately if repo policy requires it
- require an auditable evidence artifact with:
  - `Timestamp:`
  - `Command:`
  - `EXIT_CODE:`
- record failure evidence in a machine-checkable way

### REQ-ID closure gate
If the plan uses requirement IDs:
- every `REQ-*` must appear exactly once in a Requirements Traceability table
- no task may reference an undefined `REQ-*`

If you cannot ensure closure, remove `REQ-*` tags entirely.

## Definition of an atomic task
An atomic task is the smallest useful unit of work that is:
1. binary in completion
2. single-outcome
3. short enough to execute quickly
4. unambiguous
5. autonomously verifiable

## Mandatory executor preflight loop
Before finalizing any plan, explicitly spawn `atomic-executor` in preflight-validation-only mode.

Required loop:
1. Create or update the draft plan in its final target path.
2. Explicitly spawn `atomic-executor` and instruct it to validate that exact plan file in preflight-validation-only mode.
3. Wait for exactly one of:
   - `PREFLIGHT: ALL CLEAR`
   - `PREFLIGHT: REVISIONS REQUIRED`
4. If revisions are required:
   - revise the same draft plan file
   - explicitly spawn `atomic-executor` again in preflight-validation-only mode
5. Repeat until `PREFLIGHT: ALL CLEAR`.
6. Only then finalize the plan and report completion.

Additional rules:
- Do not treat preflight as optional.
- Do not replace executor preflight with planner self-review.
- Do not finalize a plan that has not received `PREFLIGHT: ALL CLEAR` from `atomic-executor`.
- Preserve the authoritative target path supplied by the caller.
- Do not fork the plan into multiple competing files during preflight iteration.

## If called for remediation planning
When invoked from feature review remediation:
- treat `remediation-inputs.<timestamp>.md` as the authoritative requirements source
- do not let `spec.md` or `user-story.md` dilute remediation requirements
- write to the target remediation plan file only
- include plan-status synchronization tasks for original feature plan file(s)
- include both baseline sync and final sync tasks

## Final self-check before completion
Before finalizing the plan, confirm:
- no placeholders remain
- every task is atomic
- all acceptance criteria are machine-verifiable
- phase and task formatting is canonical
- Phase 0 and final QA are present when required
- the plan is policy-compatible
- `atomic-executor` returned `PREFLIGHT: ALL CLEAR` for the final draft