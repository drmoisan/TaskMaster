---
name: planner-mcp-validator-not-in-tool-surface
description: The atomic-planner subagent is sometimes launched with a file-only tool surface (Read/Grep/Glob/Edit/Write), so mcp__drm-copilot__validate_orchestration_artifacts cannot be run despite being a required output
metadata:
  type: project
---

When `atomic-planner` is launched as a subagent, its tool surface may be file-only (`Read`, `Grep`, `Glob`, `Edit`, `Write`) with no Bash and no `mcp__drm-copilot__*` tools — even when the delegation prompt names `mcp__drm-copilot__validate_orchestration_artifacts` as a required output. Observed 2026-08-04 on the #418 revision-pass-2 delegation.

**Why:** The validator gate in `.claude/skills/atomic-plan-contract/SKILL.md` is mandatory before a plan may be treated as approved, so an unavailable validator cannot simply be skipped or silently claimed.

**How to apply:** Do not claim the validator passed. Instead (a) perform a structural self-check against the contract's machine-checkable constraints — exact `### Phase N — <Title>` headings (see [[plan-validator-phase-heading-constraint]]), digit-only sequential task IDs per phase (see [[plan-validator-task-id-sequential-constraint]]), canonical `<FEATURE>/evidence/<kind>/` paths, three-phase minor-audit shape — and (b) report `VALIDATOR NOT RUN: tool unavailable in this agent's tool surface` and ask the caller to run the MCP validator or route the plan through `atomic-executor` preflight. Report the self-check result separately from the validator signal so the two are never conflated.

**Correction (2026-08-06, #424 cycle):** `mcp__drm-copilot__validate_orchestration_artifacts` IS available on the *orchestrator's* tool surface in this TaskMaster worktree — the orchestrator ran it against the #424 plan and it passed structural validation. So the earlier "belongs to mix-calculator, not TaskMaster" conclusion is wrong at the orchestrator level; the unavailability is specific to the planner subagent's file-only surface. Also per the same coordinator message: the planner must NOT emit `PREFLIGHT: ALL CLEAR` on its own behalf — that signal belongs to the atomic-executor preflight; return `plan-path` plus a revision summary instead. Structural validity is necessary but not sufficient: the #424 pass-1 plan passed the MCP validator yet drew nine semantic/precondition findings from executor preflight (unrunnable commands, broken pinned tests, non-deterministic fail-before mechanics).
