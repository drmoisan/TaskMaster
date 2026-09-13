# P7-T2 — Final PowerShell Analyzer Step

Timestamp: 2026-09-13T06-23
Task: [P7-T2]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders`.

## Step 1 — MCP analyzer invocation

Tool: mcp__drm-copilot__run_poshqc_analyze
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root set to this worktree and
scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt, with terminal colour escapes removed: `Exception: PSScriptAnalyzer reported 16
issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

The exit code is not the gate. The MCP analyzer tool exits 1 on any Warning and
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` already carried an unsuppressed
`PSUseSingularNouns` warning at Phase 0, so an exit code of 1 is the baseline value. The gate is the
diagnostic-set comparison in Step 3.

## Step 2 — Paired direct enumerating run, unconditional

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; Import-Module PSScriptAnalyzer; $d = @(); $d += Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse; $d += Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse; foreach ($i in $d) { "DIAG| " + $i.Severity + " | " + $i.RuleName + " | " + (Split-Path -Leaf $i.ScriptPath) + " | " + $i.Line }; "POWERSHELL_ANALYZER_TOTAL: " + $d.Count'

DIRECT_RUN_EXIT_CODE: 0

The direct run issues no explicit exit statement, so its process exit code is not a verdict; its
value here is the verbatim diagnostic list, which the MCP tool does not supply.

Verbatim diagnostic lines:

```
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 210
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 211
DIAG| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 139
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 147
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 52
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 87
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157
POWERSHELL_ANALYZER_TOTAL: 16
```

## Step 3 — Diagnostic-set comparison against the P0-T11 baseline

Compared by the tuple of rule name, file leaf name and severity, as the task specifies. Line numbers
are deliberately not part of the tuple, because this delivery inserts lines into two of the scanned
files and a line-number shift in an unchanged diagnostic is not a new diagnostic.

| Rule | File leaf | Severity | Baseline count | Final count |
|---|---|---|---|---|
| PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | Information | 3 | 3 |
| PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | Warning | 3 | 3 |
| PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | Warning | 2 | 2 |
| PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | Warning | 1 | 1 |
| PSAvoidUsingWriteHost | Invoke-Restore.ps1 | Warning | 1 | 1 |
| PSUseSingularNouns | Invoke-VSBuild.ps1 | Warning | 2 | 2 |
| PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | Warning | 1 | 1 |
| PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | Warning | 3 | 3 |
| **Total** | | | **16** | **16** |

ENTRIES_ABSENT_FROM_BASELINE: 0

No entry appears in the final set whose tuple is absent from the baseline set. In particular no
diagnostic is reported against any of the five files this delivery creates —
`scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1`,
`scripts/vscode/Invoke-MSTest.TrxSummary.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` and
`tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` — and none against any file under
`tests/scripts/vscode`.

Line-number movements within unchanged diagnostics, recorded so the difference from the baseline
text is accounted for rather than unexplained:

- `Invoke-MSTest.ps1` `PSAvoidUsingWriteHost` moved from lines 185 and 186 to lines 210 and 211,
  consistent with the lines Phase 4 added to that entry point.
- `Invoke-MSTestWithCoverage.Helpers.ps1` `PSUseSingularNouns` moved from line 138 to line 139,
  consistent with the single dot-source line Phase 1 task T2 added at the top of that file.

## Pass 2 — re-run after the P7-T7 toolchain restart

Timestamp: 2026-09-13T07-10

The loop restarted from P7-T1 because P7-T7's coverage gate failed on its first measurement and the
remediation edited `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`. This task
was re-run over the formatted tree that carries that edit.

MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt, colour escapes removed: `Exception: PSScriptAnalyzer reported 16 issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

Verbatim diagnostic lines from the paired direct run, pass 2:

```
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 210
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 211
DIAG| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 139
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 147
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 52
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 87
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157
POWERSHELL_ANALYZER_TOTAL: 16
```

The pass-2 list is byte-identical to the pass-1 list, including every line number. The two tests the
remediation added introduced no diagnostic of any severity.

ENTRIES_ABSENT_FROM_BASELINE: 0

## Output Summary

MCP_RESULT_OK_FLAG: false with EXIT_CODE: 1, which matches the Phase 0 baseline and is not the gate.
POWERSHELL_ANALYZER_TOTAL: 16, identical to the baseline total, with every tuple and every tuple
multiplicity identical to the P0-T11 baseline set and zero entries absent from it. The gate passes.
Both passes produced the same 16 diagnostics, so the pass-2 result is the operative one and it too
introduces no entry absent from the baseline.
