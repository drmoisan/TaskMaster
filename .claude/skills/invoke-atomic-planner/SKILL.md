---
name: invoke-atomic-planner
description: 'Resolve the atomic-plan prompt via the drm-copilot MCP tool, then delegate to the atomic-planner subagent to create or edit a phased atomic plan. Use when a caller provides ${plan-path} and needs a new plan authored or an existing plan revised in place, including remediation-plan revision and preflight-delta routing.'
---

# Invoke Atomic Planner

Thin wrapper that resolves the atomic-plan prompt via the `drm-copilot` MCP tool and hands the resolved text to the `atomic-planner` subagent as kickoff directives. The resolved prompt is the authoritative instruction set for plan authoring; this skill does not duplicate its contents.

This skill is the canonical entry point for invoking `atomic-planner` to **create** a new atomic plan or **edit** an existing one in place (including revising a `remediation-plan.<entry-ts>.md` in response to a preflight `REVISIONS REQUIRED` delta).

## When to Use This Skill

Use this skill when:

- A new atomic plan must be authored at a target plan path.
- An existing atomic plan must be revised in place (for example, a preflight delta from `atomic-executor` routed back to the planner).
- The caller provides an explicit target plan file path (`${plan-path}`).
- The `drm-copilot` MCP server is registered and reachable.

Do not use this skill to execute a plan. Execution is owned by `atomic-executor` via the `execute-hard-lock` skill.

## Inputs

Required:

- `plan-path` — absolute or repo-relative path to the target plan-of-record markdown file. The planner updates this file in place across revision loops; it does not create sibling plan files.

Optional context to pass through to the subagent:

- Objective and expected outcome.
- Feature folder path and associated documents (`issue.md`, `spec.md`, `user-story.md`).
- Research artifact paths when available.
- Constraints, public APIs, and invariants to preserve.
- For a revision loop, the precise preflight delta returned by `atomic-executor`.

## Invocation Flow

### 1. Resolve the Atomic-Plan Prompt via MCP

Call the extension's resolver as the first action:

- Tool: `mcp__drm-copilot__resolve_atomic_plan_prompt`
- Parameters:
  - `target` (required): the target plan-of-record path (`${plan-path}`).
  - `workspace_root` (optional): the workspace root. Omit to default to the current working directory.

On success, the response contains an `artifacts` array whose first entry is the absolute path of a file containing the resolved atomic-plan prompt (produced by the extension passing `--output` and `--quiet` to the bundled resolver).

### 2. Read the Resolved Prompt

Use the `Read` tool on the path returned in `artifacts[0]`. Treat the file contents as the authoritative plan-authoring instruction set for this delegation.

### 3. Delegate to atomic-planner

Invoke the `atomic-planner` subagent via the Agent tool. Pass the resolved prompt text as the subagent's kickoff directive, followed by `${plan-path}` and any optional context inputs as session context.

The resolved prompt already instructs the subagent to apply the atomic plan contract (Phase 0 baseline capture, `### Phase N — <Title>` headings, `[P#-T#]` task IDs, final QA loop) and to validate the plan with `mcp__drm-copilot__validate_orchestration_artifacts`. This skill does not reissue those instructions locally — doing so would risk divergence from the canonical template.

## Abort Conditions

Stop immediately and report `BLOCKED: invoke-atomic-planner <cause>` in any of these cases. Do not reconstruct the atomic-plan prompt from any other source and do not delegate to `atomic-planner` without a successful read in step 2.

- MCP tool is not available or not permitted.
- MCP response has `ok: false` or is otherwise malformed.
- MCP response omits `artifacts` or `artifacts[0]`.
- `Read` on the artifact path fails (file missing, unreadable, empty).

## Equivalent Entry Points (Reference)

The entry points below produce the same resolved atomic-plan prompt for a given plan path. This skill always uses the MCP form:

- MCP (used by this skill): `mcp__drm-copilot__resolve_atomic_plan_prompt` with `target=<plan-path>`. The extension passes `--output` and `--quiet` to the bundled resolver.
- VS Code command: `@command:drm-copilot.resolveAtomicPlanPrompt` (interactive; writes to stdout + clipboard, no file artifact).

## Delegation Contract

The `atomic-planner` subagent at [../../agents/atomic-planner.md](../../agents/atomic-planner.md) is planning-only. It writes to `docs/**` and `artifacts/**` paths only, updates the target plan in place across revision loops, and returns the plan path plus the final preflight signal. It preloads the shared skills that support deterministic plan authoring:

- `policy-compliance-order` — mandatory policy reading order.
- `atomic-plan-contract` — plan format, Phase 0 requirements, task ID rules, validator gate, and final QA loop.
- `evidence-and-timestamp-conventions` — baseline and final-QA artifact paths and required fields.

## Relationship to the Remediation Loop

When this skill is used inside a remediation cycle, it implements the orchestrator -> `atomic-planner` handoff defined in `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`. In that context:

- The target plan path is `docs/features/active/<slug>/remediation-plan.<entry-ts>.md`.
- A preflight `REVISIONS REQUIRED` delta from `atomic-executor` is routed back through this skill so the planner revises the same plan file in place.
- The orchestrator, not this skill, owns preflight re-runs and the exit-gate evaluation.

## Prohibitions

- Do not proceed without a successful MCP resolver response AND a successful `Read` of the artifact.
- Do not reconstruct the atomic-plan contract from any other source (not from this file nor from prior session memory).
- Do not modify the resolved prompt text before passing it to `atomic-planner`.
- Do not execute the plan, run worker toolchains, or edit production files at this layer — the subagent owns plan authoring only, and execution belongs to `atomic-executor`.
- Do not create sibling plan files; the planner updates the supplied `${plan-path}` in place.
