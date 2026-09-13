# P0-T11 — PowerShell Analyzer Baseline (diagnostic set, not an exit code)

Timestamp: 2026-09-13T05-00
Task: [P0-T11]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders`.

## Step 1 — MCP analyzer invocation

Tool: mcp__drm-copilot__run_poshqc_analyze
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root set to this worktree and
scan_folders set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: false
EXIT_CODE: 1
MCP result summary: `Command exited with code 1.`
MCP stderr excerpt: `Exception: PSScriptAnalyzer reported 16 issue(s).`
MCP_REPORTED_ISSUE_TOTAL: 16

An exit code of 1 is a legitimate baseline value here: the MCP analyzer tool exits 1 on any Warning,
and the two script folders already carry unsuppressed Warning-severity diagnostics, including the
plural-noun warning in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Every analyzer gate in
this plan is therefore a diagnostic-set comparison against the set recorded below, never an exit code.

## Step 2 — Paired direct run, unconditional

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; Import-Module PSScriptAnalyzer; Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse; Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse'

The direct run is paired unconditionally with the MCP invocation because the MCP tool returns only an
ok flag and a prose summary and carries no per-diagnostic detail. No repository-local
PSScriptAnalyzer settings file exists (`PSScriptAnalyzerSettings*` and
`scripts/powershell/PoshQC/settings/*` both resolve to no file), so the direct run uses the default
rule set, and its total agrees exactly with the MCP tool's reported total of 16.

Verbatim diagnostic lines, each carrying severity, rule name, file leaf name and line number:

```
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36
DIAG| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79
DIAG| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 185
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 186
DIAG| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 138
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 52
DIAG| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 87
DIAG| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 147
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154
DIAG| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157
POWERSHELL_ANALYZER_TOTAL: 16
```

## Output Summary

MCP_RESULT_OK_FLAG: false. EXIT_CODE: 1. POWERSHELL_ANALYZER_TOTAL: 16.

Composition: 13 Warning-severity and 3 Information-severity diagnostics, across 6 files, all of them
under `scripts/vscode`. No diagnostic is reported against any file under `tests/scripts/vscode`.

Facts a later gate depends on:
- The plan's cited pre-existing plural-noun warning is confirmed: `PSUseSingularNouns` on
  `Invoke-MSTestWithCoverage.Helpers.ps1` line 138.
- Every later analyzer gate compares its diagnostic set against these 16 lines. The gate is
  satisfied when no entry appears whose file leaf name is that of a file this delivery creates or
  edits and which is absent from this list.

EXIT_CODE: 1
