# Coverage wrapper PoshQC analyzer gate

Timestamp: 2026-07-25T20-30Z

Command: `mcp__drm-copilot__run_poshqc_analyze(workspace_root="C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25", scan_folders=["scripts/vscode","tests/scripts/vscode"])`

EXIT_CODE: 0

Output Summary: The required MCP folder scan returned process exit code 1 because it reported 16 findings in unchanged files. P8-T36 recorded 22 findings, so the folder-level delta is -6 and introduces no new finding. A supplementary exact-file `Invoke-ScriptAnalyzer` check reported zero findings in both authorized files. Analysis caused no file change.

## Required MCP result

- MCP `ok`: `false`
- Process exit code: `1`
- Current folder finding count: `16`
- P8-T36 baseline finding count: `22`
- Finding delta: `-6`
- File changes caused by analysis: `0`

## Authorized-file verification

Supplementary command:

`$authorizedFiles = @('scripts/vscode/Invoke-MSTestWithCoverage.ps1','tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'); foreach ($authorizedFile in $authorizedFiles) { @(Invoke-ScriptAnalyzer -Path $authorizedFile).Count }`

| File | SHA-256 | Lines | Findings |
|---|---|---:|---:|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `73E1A76E17C901D3E0A5BA254CA3025D4EFF1D0F5455921B6E5BA9CB6125D6B2` | 312 | 0 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `4FD01E3EF23A43F5B3E7FC304B96656F65FF8192267FECB94DD6343B6350DC93` | 328 | 0 |

Result: PASS_WITH_BASELINE_FINDINGS for P8-T40.
