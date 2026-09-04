# PSScriptAnalyzer Diagnostic-Set Comparison, Iteration 1 ([P3-T3])

Timestamp: 2026-09-03T12-12

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

## NEW DIAGNOSTICS:

NONE

Output Summary: The post-change diagnostic set is line-for-line identical to the baseline set recorded in `evidence/baseline/pssa-diagnostic-set-baseline.2026-09-03T07-23.md`: same total of 16, same 16 `PSSA ITEM` lines in the same order, same rules, severities, files, and line numbers. No diagnostic is present here and absent there, so `NEW DIAGNOSTICS:` reads `NONE`. In particular the edited production file `Invoke-MSTestWithCoverage.ps1` and the new test file `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` contribute no diagnostic. This is a diagnostic-set difference rather than an exit-code check, because the MCP analyzer's exit code carries no rule, file, or severity information.
