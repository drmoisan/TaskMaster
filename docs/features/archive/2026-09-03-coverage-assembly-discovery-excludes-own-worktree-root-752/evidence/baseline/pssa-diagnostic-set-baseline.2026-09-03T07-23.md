# PSScriptAnalyzer Diagnostic-Set Baseline ([P0-T8])

Timestamp: 2026-09-03T11-53

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $d = @(); $d += Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse; $d += Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse; "PSSA TOTAL=$($d.Count)"; $d | ForEach-Object { "PSSA ITEM RuleName=$($_.RuleName) Severity=$($_.Severity) File=$(Split-Path $_.ScriptPath -Leaf) Line=$($_.Line)" }; exit 0'`

EXIT_CODE: 0

## Emitted lines, verbatim

```
PSSA TOTAL=16
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Install-RepoDotNetSdk.ps1 Line=59
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Install-RepoDotNetSdk.ps1 Line=79
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Install-RepoDotNetSdk.ps1 Line=106
PSSA ITEM RuleName=PSUseOutputTypeCorrectly Severity=Information File=Install-RepoDotNetSdk.ps1 Line=26
PSSA ITEM RuleName=PSUseOutputTypeCorrectly Severity=Information File=Install-RepoDotNetSdk.ps1 Line=36
PSSA ITEM RuleName=PSUseOutputTypeCorrectly Severity=Information File=Install-RepoDotNetSdk.ps1 Line=39
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Invoke-MSTest.ps1 Line=185
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Invoke-MSTest.ps1 Line=186
PSSA ITEM RuleName=PSUseSingularNouns Severity=Warning File=Invoke-MSTestWithCoverage.Helpers.ps1 Line=137
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Invoke-Restore.ps1 Line=32
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Invoke-VSBuild.ps1 Line=147
PSSA ITEM RuleName=PSUseSingularNouns Severity=Warning File=Invoke-VSBuild.ps1 Line=52
PSSA ITEM RuleName=PSUseSingularNouns Severity=Warning File=Invoke-VSBuild.ps1 Line=87
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Sync-PackageReferences.ps1 Line=150
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Sync-PackageReferences.ps1 Line=154
PSSA ITEM RuleName=PSAvoidUsingWriteHost Severity=Warning File=Sync-PackageReferences.ps1 Line=157
```

Output Summary: 16 diagnostics on the pre-change tree; the `PSSA TOTAL=16` figure equals the 16 `PSSA ITEM` lines recorded above, and it also equals the count the MCP analyzer reported in `poshqc-analyze-baseline.2026-09-03T07-23.md`. None of the 16 is in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and none is in any file under `tests/scripts/vscode`. This set is the comparison basis for the `NEW DIAGNOSTICS:` section of `[P3-T3]`.
