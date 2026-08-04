# P11-T6 PoshQC analyzer gate

Timestamp: 2026-08-04T10-09

MCP inputs: `workspace_root = C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25`; `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`.

MCP result: `{"ok":false,"tool":"run_poshqc_analyze","summary":"Command exited with code 1.","stderr_excerpt":"PSScriptAnalyzer reported 16 issue(s)."}`

EXIT_CODE: 1

Output Summary: The bundled folder scan returned its known nonzero baseline of 16 findings. Per P11-T6, the formatter was rerun before this second analyzer attempt; it made no file modification. The current count equals the prior final folder-scan count of 16 and is lower than the P8-T36 baseline count of 22. A direct analyzer scan of the only two merge-base-changed PowerShell files returned zero findings. Therefore no analyzer regression was introduced; no suppression, configuration, threshold, filter, or exclusion was changed.

## Full folder-scan findings

| File | Line | Rule | Severity |
| --- | ---: | --- | --- |
| Install-RepoDotNetSdk.ps1 | 26 | PSUseOutputTypeCorrectly | Information |
| Install-RepoDotNetSdk.ps1 | 36 | PSUseOutputTypeCorrectly | Information |
| Install-RepoDotNetSdk.ps1 | 39 | PSUseOutputTypeCorrectly | Information |
| Install-RepoDotNetSdk.ps1 | 59 | PSAvoidUsingWriteHost | Warning |
| Install-RepoDotNetSdk.ps1 | 79 | PSAvoidUsingWriteHost | Warning |
| Install-RepoDotNetSdk.ps1 | 106 | PSAvoidUsingWriteHost | Warning |
| Invoke-MSTest.ps1 | 119 | PSAvoidUsingWriteHost | Warning |
| Invoke-MSTest.ps1 | 120 | PSAvoidUsingWriteHost | Warning |
| Invoke-MSTestWithCoverage.Helpers.ps1 | 146 | PSUseSingularNouns | Warning |
| Invoke-Restore.ps1 | 32 | PSAvoidUsingWriteHost | Warning |
| Invoke-VSBuild.ps1 | 47 | PSUseSingularNouns | Warning |
| Invoke-VSBuild.ps1 | 78 | PSUseSingularNouns | Warning |
| Invoke-VSBuild.ps1 | 137 | PSAvoidUsingWriteHost | Warning |
| Sync-PackageReferences.ps1 | 150 | PSAvoidUsingWriteHost | Warning |
| Sync-PackageReferences.ps1 | 154 | PSAvoidUsingWriteHost | Warning |
| Sync-PackageReferences.ps1 | 157 | PSAvoidUsingWriteHost | Warning |

## Changed-file verification

Command: `Import-Module PSScriptAnalyzer; Invoke-ScriptAnalyzer -Path scripts/vscode/Invoke-MSTestWithCoverage.ps1; Invoke-ScriptAnalyzer -Path tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

Result: `0` findings in each authorized merge-base-changed PowerShell file. Current SHA-256 values are `5ED27E29262271D572D1AFD2A837F53EE4C36D832B848B89054734E3E6AE746C` and `045CF51583D14E2CEA4A5324294DA30467FAE2CC5E2691090B897A316998487A`, respectively.
