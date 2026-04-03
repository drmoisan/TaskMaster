# Change Plan

## Objective

Continue the Codex runtime migration by restoring the repository change-plan document and migrating the GitHub Copilot `commit-steward` behavior into the layered Codex runtime surface.

## Current Migration Scope

- Keep repository-local reusable skills under `.agents/skills/`
- Keep Codex subagent definitions under `.codex/agents/`
- Migrate bounded agent personas from `.github/agents/` into `.codex/agents/`

## This Increment

1. Restore `change-plan.md` as the repository plan of record for the Codex migration.
2. Split `commit-steward` into:
   - a reusable shared skill for commit-message conventions
   - a thin Codex-native subagent definition that delegates to that skill
   - a thin Codex prompt launcher that spawns the subagent
3. Preserve the original purpose:
   - generate high-signal conventional commit messages
   - scope analysis to staged changes only
   - avoid speculative or noisy commit summaries
4. Adapt the behavior to Codex-native execution:
   - use the local workspace and git staging state directly
   - allow an explicitly provided context file when available
   - keep the agent output strict and copy-ready

## Design Rules

1. Put reusable commit-message rules in one shared skill instead of duplicating them across agents or prompts.
2. Keep `.codex/agents/*.toml` concise and focused on bounded role behavior.
3. Preserve stable naming where practical, but prefer Codex-friendly file names and agent names.
4. Prefer direct staged git inspection over Copilot-specific context assumptions when running inside Codex.

## Deliverables

- `change-plan.md`
- `.agents/skills/commit-message-conventions/SKILL.md`
- `.codex/agents/commit-steward.toml`
- `.codex/prompts/generate-commit-message-repo.md`

## Verification

- Confirm the new shared skill exists with valid Codex frontmatter.
- Confirm the new agent file exists and parses as TOML.
- Confirm the new prompt exists with valid Codex prompt frontmatter.
- Confirm the skill and agent together preserve the staged-only commit-message scope.
- Confirm the repository change-plan file exists again at the repo root.
