# Coverage wrapper PoshQC test gate blocker

- Timestamp: `2026-07-23T14:35:21Z`
- Required command: `mcp__drm-copilot__run_poshqc_test workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[tests/scripts/vscode]`
- Required-command result: `ok: false`
- Required-command EXIT_CODE: `4294967295`
- Gate status: `BLOCKED_EXTERNAL_TOOLING`
- TaskMaster test failures: `0`
- TaskMaster test skips: `0`

## Reproduction

The installed `@danmoisan/drm-copilot-mcp` package is version `1.0.17`.
Its bundled entry point was invoked directly with the same workspace and scan
folder:

`& '<package>\resources\templates\run-poshqc-test.ps1' -WorkspaceRoot (Get-Location).Path -ScanFolders 'tests/scripts/vscode' -DisableKoverageCopy`

The bundled path discovered all four Pester files and all 30 tests under
`tests/scripts/vscode`. It passed 30/30 with zero failures and zero skips. After
the tests completed, Pester coverage processing emitted:

`Could not resolve coverage path 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\scripts\powershell\Publish-DrmCopilotExtension.ps1': Cannot find path 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\scripts\powershell\Publish-DrmCopilotExtension.ps1' because it does not exist.`

PowerShell reports that terminal `-1` as the unsigned process result
`4294967295`.

## Root cause

| Bundled artifact | Version or SHA-256 |
|---|---|
| `@danmoisan/drm-copilot-mcp` | `1.0.17` |
| `resources/templates/run-poshqc-test.ps1` | `18CCDDD4A3099AFBFDEDDF5440705CD31C9D9329C512E14A3210EB40C50D198A` |
| `resources/powershell/PoshQC/settings/pester.runsettings.psd1` | `72ACA24A1BAC93F0108CB960026B21A805E26609599D579C4AB3A3E33944B47C` |

The required MCP schema exposes only `workspace_root` and `scan_folders`.
`scan_folders` replaces Pester `Run.Path`; it does not replace
`CodeCoverage.Path`. The bundled settings retain drm-copilot-specific coverage
paths, including `scripts/powershell/Publish-DrmCopilotExtension.ps1`, that are
not TaskMaster files. The wrapper does not expose the underlying
`Invoke-PoshQCTest -SettingsPath` parameter, and TaskMaster has no supported
repository-local override for the bundled coverage settings.

## Disposition

This is not a TaskMaster test or implementation failure. Adding unrelated
placeholder files, modifying the installed MCP package, or substituting direct
Pester for the required MCP gate would mask the defect and would not satisfy
P8-T41. The required correction is a released drm-copilot MCP runner that
supports repository-local Pester settings or filters publisher-specific
coverage paths for consumer workspaces.

Per the P8 restart rule, P8-T39 and P8-T40 are reset and must be rerun after the
external tool is corrected. P8-T41 remains incomplete. Phase 9 has not started.
The canonical `coverage.config` SHA-256 remains
`B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
