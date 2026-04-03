# Change Plan

## Objective

Continue the Codex runtime migration by moving the GitHub Copilot `orchestrator` workflow into the layered Codex runtime surface while preserving the existing separation between shared workflow skills, thin subagents, and prompt launchers.

## Current Migration Scope

- Keep repository-local reusable workflow rules under `.agents/skills/`
- Keep Codex subagent definitions under `.codex/agents/`
- Keep prompt launchers under `.codex/prompts/`
- Migrate Copilot workflow personas into shared Codex skills plus thin Codex agents

## This Increment

1. Migrate `.github/agents/orchestrator.agent.md` into:
   - a reusable shared orchestration workflow skill
   - a thin Codex-native `orchestrator` subagent that delegates to that skill
2. Migrate `.github/prompts/orchestrate-work.prompt.md` into a Codex prompt launcher that spawns the new subagent.
3. Preserve the original orchestration intent:
   - estimate change budget first
   - choose the correct small or large path
   - persist orchestration state and resume deterministically
   - continue through planning, execution, validation, and review until the selected path is complete
4. Adapt the workflow to the current Codex runtime:
   - route host-specific repo automation through `repo-automation-adapter`
   - reuse the existing `feature-promotion-lifecycle`, `atomic-planner`, `atomic-executor`, `feature-review`, and language budget-router skills
   - prefer currently migrated Codex subagents and keep direct-execution fallback only for steps whose specialist persona has not yet been migrated
5. Normalize any new canonical lifecycle rules into the existing shared skills rather than duplicating them in the new orchestrator files.

## Design Rules

1. Put reusable orchestration rules in one shared skill instead of duplicating them across agents or prompts.
2. Keep `.codex/agents/*.toml` concise and focused on bounded role behavior.
3. Preserve stable external names where practical:
   - agent name `orchestrator`
   - prompt name `orchestrate-work`
4. Keep canonical variable and lifecycle rules in existing shared skills when those rules already have a natural owner.
5. Do not hard-code `drmCopilotExtension.*` command execution in the new agent or prompt.

## Deliverables

- `change-plan.md`
- `.agents/skills/orchestrator-workflow/SKILL.md`
- `.codex/agents/orchestrator.toml`
- `.codex/prompts/orchestrate-work.md`
- updates to shared skill docs and any existing shared lifecycle skill that now owns canonical orchestration details

## Verification

- Confirm the new shared skill exists with valid Codex frontmatter.
- Confirm the new agent file exists and parses as TOML.
- Confirm the new prompt exists with valid Codex prompt frontmatter.
- Confirm the orchestrator skill references the existing shared lifecycle and routing skills instead of restating their rules.
- Confirm canonical `plan-path` rules live in one shared skill only.
