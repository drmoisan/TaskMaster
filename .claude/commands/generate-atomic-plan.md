---
description: 'Generate or update an atomic implementation plan file. Produces a phased, machine-executable plan and validates it via preflight loop until PREFLIGHT: ALL CLEAR.'
argument-hint: 'Provide: plan name, plan file path, work mode, spec path, user-story path, research path.'
---

# Generate Atomic Plan Command

Invoke the `atomic-planner` skill to generate or update an implementation plan.

## Primary Directive

Generate or update the implementation plan file based on new or updated requirements. Output must be machine-readable, deterministic, and structured for autonomous execution by other AI systems.

## Execution Context

This command is designed for AI-to-AI communication and automated processing. All instructions must be interpreted literally and executed systematically.

## Mode Context (Deterministic)

- Selected work mode: resolve from `issue.md` marker first; fail closed to `full-feature` when marker is missing or malformed.
- Fallback reason: document when fallback is applied.

## Core Requirements

- Generate implementation plans that are fully executable by AI agents.
- Plan must deliver all requirements detailed in the spec and user-story documents.
- Plan must leverage research artifacts when provided.
- Use deterministic language with zero ambiguity.
- Structure all content for automated parsing and execution.
- Ensure complete self-containment with no external dependencies for understanding.

## Plan Structure Requirements

Plans must consist of discrete, atomic phases containing executable tasks. Each phase must be independently processable without cross-phase dependencies unless explicitly declared.

## Phase Architecture

- Each phase must have measurable completion criteria.
- Tasks within phases must be executable in parallel unless dependencies are specified.
- Unit testing must be written in TDD manner: no production code without corresponding test verification.
- All task descriptions must include specific file paths, function names, and exact implementation details.
- No task should require human interpretation or decision-making.

## AI-Optimized Implementation Standards

- Use explicit, unambiguous language with zero interpretation required.
- Structure all content as machine-parseable formats (tables, lists, structured data).
- Include specific file paths, line numbers, and exact code references where applicable.
- Define all variables, constants, and configuration values explicitly.
- Provide complete context within each task description.
- Use standardized prefixes for requirements and constraints (e.g., REQ-, SEC-, CON-).
- Use `[P#-T#]` identifiers for tasks (canonical).
- Include validation criteria that can be automatically verified.

## Mandatory post-output preflight validation loop

After fully generating or updating the plan file, initiate a **validate-only** preflight validation via the Agent tool targeting `atomic-executor`.

Purpose: Ensure the plan produced is ingestible by the executor without replanning.

Hard constraints:
- The executor MUST perform **preflight checks only** (no task execution).
- Iterate until the executor returns an all-clear signal.

Required handoff directive (exact text):

`DIRECTIVE: PREFLIGHT VALIDATION ONLY`

Required validation result signals (exact text; one must be present):

- `PREFLIGHT: ALL CLEAR`
- `PREFLIGHT: REVISIONS REQUIRED`

Loop protocol (MANDATORY):
1. Hand off the plan file to `atomic-executor` via Agent tool with the directive above.
2. If executor returns `PREFLIGHT: REVISIONS REQUIRED`, apply the executor's plan delta (preserving task IDs and executor-compatible formatting), then hand off again.
3. Repeat until executor returns `PREFLIGHT: ALL CLEAR`.
4. Only then return control to the calling system, including the final `PREFLIGHT: ALL CLEAR` signal verbatim.

## Template Validation Rules

- All front matter fields must be present and properly formatted.
- All section headers must match exactly (case-sensitive).
- All identifier prefixes must follow the specified format.
- Tables must include all required columns.
- No placeholder text may remain in the final output.

## Status

The status of the implementation plan must be clearly defined in the front matter and must reflect the current state: `Completed`, `In progress`, `Planned`, `Deprecated`, or `On Hold`.
