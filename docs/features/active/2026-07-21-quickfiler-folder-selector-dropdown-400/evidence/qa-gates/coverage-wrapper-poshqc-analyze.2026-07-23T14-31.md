# Coverage wrapper PoshQC analyzer gate

- Timestamp: `2026-07-23T14:31:43Z`
- Command: `mcp__drm-copilot__run_poshqc_analyze workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[scripts/vscode,tests/scripts/vscode]`
- MCP EXIT_CODE: `1`
- MCP summary: `PSScriptAnalyzer reported 16 issue(s).`
- Baseline folder findings: `22`
- Final folder findings: `16`
- Folder finding delta: `-6`
- New findings in either authorized file: `0`
- File changes caused by analysis: `0`

The folder-level MCP command remains nonzero because of existing findings in
unchanged files. Its finding count did not increase from the P8-T36 baseline.
Every PowerShell file in the two scan folders other than the two authorized
files retained its P8-T39 pre/post hash, so those unchanged files cannot contain
a finding introduced by this batch.

Supplementary exact authorized-file command:

`$ErrorActionPreference='Stop'; $files=@('scripts/vscode/Invoke-MSTestWithCoverage.ps1','tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'); $findings=@(); foreach($file in $files){ $findings += @(Invoke-ScriptAnalyzer -Path $file) }; 'FINDINGS={0}' -f $findings.Count; if($findings.Count -gt 0){ exit 1 }`

Supplementary result:

| File | SHA-256 | Findings |
|---|---|---:|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `73E1A76E17C901D3E0A5BA254CA3025D4EFF1D0F5455921B6E5BA9CB6125D6B2` | 0 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `4FD01E3EF23A43F5B3E7FC304B96656F65FF8192267FECB94DD6343B6350DC93` | 0 |

The supplementary exact-scope command exited `0`. This gate therefore records
zero new analyzer findings against the baseline while preserving the MCP
folder-scan result without suppressing or changing existing findings.
