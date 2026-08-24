---
name: drm-copilot-is-claude-governance-upstream
description: <user-profile>\repos\drm-copilot is the upstream source for TaskMaster's .claude governance surface (rules, skills, agents, hooks, orchestration libraries) and the MCP server
metadata:
  type: reference
---

`<user-profile>\repos\drm-copilot` is the upstream repository for TaskMaster's `.claude`
governance surface and for the `drm-copilot` MCP server itself.

Useful locations there when a TaskMaster governance file seems missing or stale:

- `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/`, `.claude/lib/` — the
  canonical copies that get distributed into consumer repos.
- `extensions/drm-copilot/resources/claude-customizations/.claude/**` — the packaged copy shipped
  by the VS Code extension.
- `extensions/drm-copilot/src/lib/validate/*.ts` — the TypeScript validators behind the MCP tool
  `validate_orchestration_artifacts`, including the `parallel-*` artifact types.
- `config/blast-radius.json` — the blast-radius truth table (drm-copilot-specific; see
  [[parallel-surface-partial-port]] for why it is not portable as-is).

**How to apply:** when a skill in TaskMaster references a rule file, config, or library that does
not exist locally, check drm-copilot before concluding the reference is wrong — it usually means the
port into TaskMaster is incomplete rather than that the skill is stale. Confirm whether the upstream
artifact is repo-specific before copying it across.
