# Coverage wrapper PoshQC analyzer baseline

- Timestamp: `2026-07-23T14-15Z`
- Command: `mcp__drm-copilot__run_poshqc_analyze workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[scripts/vscode,tests/scripts/vscode]`
- EXIT_CODE: `1`
- Output Summary: `The folder scan reported 22 existing PSScriptAnalyzer issues. A supplementary direct scan of the two authorized files reported zero findings.`

| File | SHA-256 | Lines | Direct findings |
|---|---|---:|---:|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `4782C4E3F00CEA7F852AC884387AE9FDD15615F888F132CB7E71F2F1D9868E26` | 186 | 0 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `835D3F4890C7D896B09D43330F414A815ACB7670AD0A385CC042F33720EE7F5E` | 169 | 0 |

The final P8-T40 gate must introduce zero findings in either authorized file and must
not increase the existing folder-scan count.
