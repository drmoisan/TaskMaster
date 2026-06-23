---
name: repo-automation-adapter
description: 'Centralize required drm-copilot MCP execution for Codex repo automation. Use when a workflow needs promotion, PR context, commit context, customization publishing, hard-lock resolution, or orchestration validation.'
---

# Repo Automation Adapter

Use this skill to keep host-specific workflow execution rules in one place.

## Canonical Rule

The `drm-copilot` MCP server is the only approved execution surface for canonical repository automation covered by this skill.

There is no fallback. If the server is unavailable, the required tool is unavailable, or the MCP call fails, the workflow must stop, persist blocked state when an orchestrator checkpoint is active, and report the missing dependency or failed MCP operation.

Do not replace a required MCP operation with local scripts, git reconstruction, direct filesystem synthesis, VS Code command IDs, or best-effort behavior.

## When to Use This Skill

Use this skill when:

- a migrated workflow previously depended on `drmCopilotExtension.*` commands,
- a workflow needs PR-context collection, issue promotion, feature-folder creation, commit-context collection, customization publishing, hard-lock prompt resolution, or orchestration-artifact validation,
- multiple skills need the same MCP-required execution rule.

## Published Codex Automation Surface

The required Codex automation dependency for this repo is the published MCP server:

- `drm-copilot`

Downstream Codex skills must depend on the MCP server name `drm-copilot`, not on raw VS Code command IDs.

## Published MCP Tool Surface

Use these semantic MCP tools when the corresponding operation is required:

- `collect_commit_context`
- `collect_pr_context`
- `push_down_copilot_customizations`
- `push_down_codex_and_agents_customizations`
- `new_potential_bug_entry`
- `new_potential_entry`
- `potential_to_issue`
- `new_active_feature_folder`
- `resolve_execute_hard_lock_prompt`
- `resolve_atomic_plan_prompt`
- `validate_orchestration_artifacts`
- `run_poshqc_format`
- `run_poshqc_analyze`
- `run_poshqc_analyze_autofix`
- `run_poshqc_test`

Legacy VS Code command IDs are historical source material only and must not be invoked:

- `drmCopilotExtension.collectCommitContext`
- `drmCopilotExtension.collectPrContext`
- `drmCopilotExtension.pushDownCopilotCustomizations`
- `drmCopilotExtension.pushDownCodexAndAgentsCustomizations`
- `drmCopilotExtension.newPotentialBugEntry`
- `drmCopilotExtension.newPotentialEntry`
- `drmCopilotExtension.potentialToIssue`
- `drmCopilotExtension.newActiveFeatureFolder`
- `drmCopilotExtension.resolveExecuteHardLockPrompt`

## Adapter Preconditions

Before starting any workflow that depends on this skill, the orchestrator must assume these prerequisites are mandatory:

- the Codex project is trusted so project `.codex/config.toml` loads,
- the Codex client is configured with MCP server name `drm-copilot`,
- the `drm-copilot` MCP server is active,
- the required MCP tool is exposed by the active server,
- an open workspace folder exists for workspace-targeted operations.

If any prerequisite is missing, stop before mutating workflow state.

## Execution Order

For any host-specific workflow step:

1. Identify the required `drm-copilot` MCP tool.
2. Call that MCP tool.
3. Validate the MCP response according to the calling workflow's contract.
4. If the tool is unavailable, the call fails, or the response does not satisfy the contract, stop and record blocked state. Do not execute a replacement path.

## Current Adapter Guidance

### PR context collection

- Required tool: `collect_pr_context`.
- When the caller already resolved a base branch, pass that base explicitly.
- For orchestrator remediation loops, PR-context refresh is mandatory after each remediation commit and before each re-review.
- If the tool is unavailable or fails, stop. Do not reconstruct PR context from local git commands.

### Commit context collection

- Required tool: `collect_commit_context`.
- For orchestrator remediation loops, collect commit context only after `git add -A` and only when staged changes exist.
- Do not continue to commit-message generation without an on-disk commit-context artifact path produced by the MCP tool.
- If the tool is unavailable or fails, stop. Do not replace it with staged-diff inspection.

### Feature promotion and active feature folder creation

Required tools:

- `new_potential_entry`
- `new_potential_bug_entry`
- `potential_to_issue`
- `new_active_feature_folder`

Execute these lifecycle operations as one ordered chain:

1. Create the potential entry.
2. Promote with `potential_to_issue`.
3. Capture numeric issue number from promotion output.
4. Create or check out `${promotion-type}/${short-name}-${issue-num}`.
5. Create the active feature folder with `new_active_feature_folder`.

`new_active_feature_folder` is not an allowed bootstrap substitute for missing promotion state. If `${issue-num}` is missing, non-numeric, or placeholder text, stop. Do not synthesize GitHub issue state, active-folder scaffolding, or placeholder lifecycle variables.

### Customization publishing and hard-lock resolution

Required tools:

- `push_down_copilot_customizations`
- `push_down_codex_and_agents_customizations`
- `resolve_execute_hard_lock_prompt`
- `resolve_atomic_plan_prompt`

If the required tool is unavailable or fails, stop.

### Orchestration artifact validation

Required tool:

- `validate_orchestration_artifacts`

Use this tool for canonical validation of plans, policy audits, code reviews, feature audits, and orchestrator state. For orchestrator completion, call it with `artifact_type: "orchestrator-state"` and `require_complete: true`.

If the tool is unavailable or fails, stop. Do not substitute direct CLI validation for canonical orchestrator completion.

### PowerShell quality gates

Required tools:

- `run_poshqc_format`
- `run_poshqc_analyze`
- `run_poshqc_analyze_autofix`
- `run_poshqc_test`

When a workflow requires PoshQC execution through MCP, use only these tools. If the required tool is unavailable or fails, stop.

## Output Requirements

When this skill is used, the calling workflow must report:

- which MCP operation was required,
- which `drm-copilot` tool was called,
- whether the MCP response satisfied the calling contract,
- the blocked-state reason when the MCP dependency is unavailable or fails.
