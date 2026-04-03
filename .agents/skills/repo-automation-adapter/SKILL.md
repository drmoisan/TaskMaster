---
name: repo-automation-adapter
description: 'Centralize host-surface and repo-automation differences for Codex. Use when a migrated workflow previously depended on GitHub Copilot or VS Code extension commands and needs a single Codex-compatible execution or fallback rule.'
---

# Repo Automation Adapter

Use this skill to keep host-specific workflow translation in one place.

## When to Use This Skill

Use this skill when:
- a migrated skill previously depended on `drmCopilotExtension.*` commands,
- a workflow needs PR-context collection, issue promotion, or feature-folder creation,
- the same fallback behavior would otherwise be repeated across multiple skills.

## Canonical Rule

Do not encode host-specific execution details in multiple workflow skills. Put them here and have the calling skills reference this skill.

## Codex Capability Model

Codex in this repo can reliably use:
- repository files,
- shell commands,
- git,
- local scripts that already exist in the repository,
- configured MCP tools when available.

Codex in this repo should not assume direct access to:
- `vscode/runCommand`,
- `drmCopilotExtension.*` command execution,
- GitHub issue or PR mutation unless an explicit connector or script is available.

## Execution Order

For any host-specific workflow step:

1. Prefer a repo-native script, CLI, or MCP tool that Codex can invoke directly.
2. If no direct adapter exists, determine whether a deterministic git/filesystem fallback is sufficient.
3. If a deterministic fallback is sufficient, use it and record that the result is a fallback artifact rather than a canonical tool-produced artifact.
4. If no direct adapter or safe fallback exists, stop and report the missing automation dependency instead of inventing behavior.

## Current Adapter Guidance

### PR context collection

- Preferred: use the repository's direct PR-context collector if one becomes available to Codex.
- Current fallback: use deterministic git commands to reconstruct equivalent context when review workflows only need base/head, merge-base, commits, and changed files.
- When using fallback, record the provenance in the generated review artifact.

### Feature promotion and active feature folder creation

- Preferred: use a direct repository script, CLI, MCP tool, or future Codex-facing adapter.
- Current rule: if no such adapter exists, treat the step as unavailable in Codex and surface a precise dependency gap.
- Do not synthesize GitHub issue state or feature-folder scaffolding unless the user explicitly requests a best-effort local-only fallback.

## Output Requirements

When this skill is used, the calling workflow should report:
- which operation required host adaptation,
- which direct adapter or fallback path was selected,
- whether the result is canonical or fallback-only,
- what dependency is missing when the step is blocked.
