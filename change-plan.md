# Change Plan

## Objective

Align the TaskMaster Codex runtime with the published `drm-copilot` MCP bridge so repository automation now targets the semantic MCP server surface instead of treating those operations as mostly unavailable in Codex.

## Current Migration Scope

- Keep repository-local reusable workflow rules under `.agents/skills/`
- Keep Codex subagent definitions under `.codex/agents/`
- Keep prompt launchers under `.codex/prompts/`
- Migrate Copilot workflow personas into shared Codex skills plus thin Codex agents

## This Increment

1. Update `repo-automation-adapter` so the canonical Codex path is the published MCP server name `drmCopilotExtension`.
2. Add MCP dependency metadata for the adapter skill so the owning skill declares the external tool surface once.
3. Replace stale guidance that treated feature promotion and feature-folder initialization as unavailable in Codex when no repo-local script existed.
4. Update PR-context refresh guidance so it prefers the published `collect_pr_context` MCP tool before falling back to git reconstruction.
5. Update migration and authoring docs so future migrations target semantic MCP tools on `drmCopilotExtension`, not raw VS Code command IDs.

## Design Rules

1. Keep host-surface translation in `repo-automation-adapter`; do not restate MCP tool names across workflow skills.
2. Prefer semantic MCP tool names on server `drmCopilotExtension` over raw `drmCopilotExtension.*` VS Code command IDs.
3. Declare the MCP dependency once on the owning adapter skill instead of duplicating tool bindings across every caller.
4. Keep canonical PR-context artifact paths in `pr-context-artifacts`.
5. Preserve deterministic fallback rules only where the MCP surface is unavailable and a safe local fallback is still acceptable.

## Deliverables

- `change-plan.md`
- `.agents/skills/repo-automation-adapter/SKILL.md`
- `.agents/skills/repo-automation-adapter/agents/openai.yaml`
- `.agents/skills/pr-context-artifacts/SKILL.md`
- updates to `.agents/README.md` and `.agents/skills/README.md`

## Verification

- Confirm the adapter skill references server name `drmCopilotExtension` and the published semantic tool names.
- Confirm the new `agents/openai.yaml` exists under `repo-automation-adapter`.
- Confirm `pr-context-artifacts` now prefers the MCP collector before git fallback.
- Confirm migration and authoring docs instruct future Codex migrations to target semantic MCP tools rather than raw VS Code command IDs.
