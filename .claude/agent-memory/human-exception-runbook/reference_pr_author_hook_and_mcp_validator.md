---
name: reference-pr-author-hook-and-mcp-validator
description: Where PR-creation gating lives (enforce-pr-author-skill.ps1) and the MCP validator that can substitute for the missing scripts/dev_tools python module
metadata:
  type: reference
---

The PreToolUse hook `.claude/hooks/enforce-pr-author-skill.ps1` gates every `gh pr create`/`gh pr
edit` call. Its `Invoke-OrchestratorStatePreflight` function (around lines 49-88) defaults to
invoking `python -m scripts.dev_tools.validate_orchestration_artifacts orchestrator-state <path>
--require-pr-creation-ready`. As of 2026-07-06, `scripts/dev_tools` does not exist as a Python
package in this repo (`ModuleNotFoundError`), so this preflight fails closed on every attempt,
producing `ORCHESTRATOR_STATE_PREFLIGHT_FAILED` even for well-formed `gh pr create --body-file
artifacts/pr_body_<N>.md` commands.

`.claude/settings.json` (line ~23) already registers `mcp__drm-copilot__validate_orchestration_artifacts`
as an allowed MCP tool, which performs equivalent checkpoint validation and is a plausible
replacement `$Invoker` target for the hook, instead of writing a new Python module from scratch.

**Related:** [[project-no-mcp-docs-tool]] — a separate, unrelated MCP-tooling gap (documentation
retrieval, not orchestrator-state validation).

Verify this is still accurate before reuse: confirm `scripts/dev_tools/validate_orchestration_artifacts`
still doesn't exist (glob it) and that the MCP tool name in `.claude/settings.json` hasn't changed,
since this was true as of the issue #240 PR-creation-blocker runbook (2026-07-06) and may have been
fixed since (see Option 1 of
`docs/features/active/2026-07-06-store-wrapper-launch-npe-240/runbooks/pr-creation-blocker.runbook.md`).
