# Coverage wrapper PoshQC test gate retry blocker

- Timestamp: `2026-07-24T16:52:00Z`
- Task: `P8-T41`
- Result: `BLOCKED_EXTERNAL_TOOLING`

## Required Command

`mcp__drm-copilot__run_poshqc_test workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[tests/scripts/vscode]`

## Output Summary

- Required MCP result: `ok: false`
- Required MCP exit code: `4294967295`
- Source or test file changes caused by this command: `0`
- Active `@danmoisan/drm-copilot-mcp` version: `1.0.17`
- Active runner SHA-256: `18CCDDD4A3099AFBFDEDDF5440705CD31C9D9329C512E14A3210EB40C50D198A`
- Current MCP schema parameters: `workspace_root`, `scan_folders`
- Repository-local Pester settings override: unavailable

The retry reproduced the same external result recorded in `coverage-wrapper-poshqc-test-blocker.2026-07-23T14-35.md`. That prior reproduction ran all 30 tests under `tests/scripts/vscode` successfully with zero failures and zero skips, then failed during coverage-path resolution because the bundled runner referenced the drm-copilot-only path `scripts/powershell/Publish-DrmCopilotExtension.ps1`, which does not exist in TaskMaster.

## Restart disposition

P8-T39 and P8-T40 were rerun successfully before this retry:

- `evidence/qa-gates/coverage-wrapper-poshqc-format.2026-07-24T16-50.md`
- `evidence/qa-gates/coverage-wrapper-poshqc-analyze.2026-07-24T16-51.md`

The P8 restart rule supersedes those successful attempts because P8-T41 did not complete. P8-T39 through P8-T43 remain unchecked. A corrected drm-copilot MCP runner must be activated, and the sequence must restart at P8-T39 with a new evidence suffix. Direct Pester execution is not substituted for the mandatory MCP gate.

## Exit Code

`EXIT_CODE: 4294967295`
