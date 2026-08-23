# Coverage wrapper PoshQC analyzer gate

- Timestamp: `2026-07-24T16:51:00Z`
- Task: `P8-T40`
- Result: `PASS_WITH_BASELINE_FINDINGS`

## Required Command

`mcp__drm-copilot__run_poshqc_analyze workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[scripts/vscode,tests/scripts/vscode]`

## Output Summary

- Required MCP result: `ok: false`
- Required MCP exit code: `1`
- Required MCP finding count: `16`
- P8-T36 baseline folder finding count: `22`
- Folder finding delta: `-6`
- File changes caused by analysis: `0`

The required folder scan remains nonzero because of findings in unchanged files, but its finding count decreased from the recorded P8-T36 baseline.

## Authorized-file verification

Supplementary command:

`$files=@('scripts/vscode/Invoke-MSTestWithCoverage.ps1','tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'); foreach($file in $files){ Invoke-ScriptAnalyzer -Path $file }`

| File | SHA-256 | Lines | Findings |
|---|---|---:|---:|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `73E1A76E17C901D3E0A5BA254CA3025D4EFF1D0F5455921B6E5BA9CB6125D6B2` | 312 | 0 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `4FD01E3EF23A43F5B3E7FC304B96656F65FF8192267FECB94DD6343B6350DC93` | 328 | 0 |

The supplementary exact-scope command exited `0`. No analyzer finding was introduced in either authorized file, and the folder-level finding count did not regress.

## Exit Code

`EXIT_CODE: 0`
