# Coverage wrapper PoshQC test blocker retry

Timestamp: 2026-07-25T03-33Z

Command: `mcp__drm-copilot__run_poshqc_test(workspace_root="C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25", scan_folders=["tests/scripts/vscode"])`

EXIT_CODE: 4294967295

Output Summary: P8-T41 remains blocked. The required MCP command returned `ok: false` and exit code `4294967295`. The active and current registry version of `@danmoisan/drm-copilot-mcp` is 1.0.18. Its bundled Pester settings still include the drm-copilot-only coverage path `scripts/powershell/Publish-DrmCopilotExtension.ps1`. In the TaskMaster consumer workspace, Pester discovers 30 tests but fails during `RunStart` before executing them because that coverage path does not exist.

## Required MCP result

- MCP `ok`: `false`
- MCP summary: `Command exited with code 4294967295.`
- Scan folder: `tests/scripts/vscode`
- Test execution result: not reached
- P8-T41 result: `BLOCKED`

## Current package verification

- Active package: `@danmoisan/drm-copilot-mcp`
- Active package version: `1.0.18`
- Active package path: `C:\Users\DanMoisan\AppData\Local\npm-cache\_npx\bc9f2e765aac2c41\node_modules\@danmoisan\drm-copilot-mcp`
- `npm view @danmoisan/drm-copilot-mcp version`: `1.0.18`
- Bundled settings path: `resources/powershell/PoshQC/settings/pester.runsettings.psd1`
- Invalid consumer-workspace coverage input: `scripts/powershell/Publish-DrmCopilotExtension.ps1`

## Exact diagnostic reproduction

Diagnostic command:

`pwsh -NoProfile -ExecutionPolicy Bypass -File <active-package>\resources\templates\run-poshqc-test.ps1 -WorkspaceRoot C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 -ScanFoldersJson '["tests/scripts/vscode"]'`

Diagnostic result:

- Pester discovery: 30 tests in 4 files.
- Pester phase reached: `Starting code coverage`.
- Failure phase: `RunStart`.
- Error: `Resolve-CoverageInfo: Could not resolve coverage path 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\scripts\powershell\Publish-DrmCopilotExtension.ps1'`.
- Diagnostic process exit code: `-1`.

The diagnostic invocation reproduces the MCP failure from the same bundled script and configuration. It is not used as a substitute for the required MCP result.

## Repository integrity after failure

| Path | SHA-256 | Lines |
|---|---|---:|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `73E1A76E17C901D3E0A5BA254CA3025D4EFF1D0F5455921B6E5BA9CB6125D6B2` | 312 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `4FD01E3EF23A43F5B3E7FC304B96656F65FF8192267FECB94DD6343B6350DC93` | 328 |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | N/A |

- Scoped Git changes caused by the MCP and diagnostic invocations: `0`.
- Unexpected effective/derived runsettings files retained: `0`.
- Canonical `coverage.config` change: `0`.

## Required external correction

Publish a drm-copilot MCP release after 1.0.18 whose `run_poshqc_test` consumer-workspace path does not apply drm-copilot repository coverage inputs to TaskMaster, then restart the MCP connection so the new package is active. The current tool schema exposes only `workspace_root` and `scan_folders`, so this TaskMaster orchestration cannot supply an alternate Pester settings path.

P8-T39 through P8-T41 must be rerun from P8-T39 after the corrected MCP release is active. P8-T42, P8-T43, and Phase 9 remain blocked.
