# Phase 0 — PoshQC Analyze Baseline (P0-T6)

Timestamp: 2026-09-02T21-50

Task: [P0-T6]

## Command 1 — MCP analyze run

Command: mcp__drm-copilot__run_poshqc_analyze
  workspace_root = the item worktree repository root for this run
  scan_folders = ["scripts/vscode", "tests/scripts/vscode"]

EXIT_CODE: 1

MCP payload:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <item worktree repository root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

The exit code of 1 is the tool's response to a non-empty diagnostic set. It reports a count
only, with no rule name, severity, file, or line, which is why the plan pairs it with the
direct run below.

## Command 2 — Direct per-file Invoke-ScriptAnalyzer, seven in-scope files

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and double-quoted inner
script, setting location to the item worktree root and calling
`Invoke-ScriptAnalyzer -Path <file>` once per file, then `exit 0`.
EXIT_CODE: 0

Verbatim output:

```
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | diagnostics=1
    PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line 141
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTest.ps1 | diagnostics=2
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 119
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 120
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | diagnostics=0
TOTAL DIAGNOSTICS: 3
```

## Command 3 — Direct folder-scoped Invoke-ScriptAnalyzer, reconciling the MCP count of 16

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and double-quoted inner
script, calling `Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse` and
`Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse`, then `exit 0`.
EXIT_CODE: 0

Verbatim output:

```
FOLDER-SCAN TOTAL: 16
  severity Information = 3
  severity Warning = 13
    PSAvoidUsingWriteHost | Warning | Install-RepoDotNetSdk.ps1 | line 59
    PSAvoidUsingWriteHost | Warning | Install-RepoDotNetSdk.ps1 | line 79
    PSAvoidUsingWriteHost | Warning | Install-RepoDotNetSdk.ps1 | line 106
    PSUseOutputTypeCorrectly | Information | Install-RepoDotNetSdk.ps1 | line 26
    PSUseOutputTypeCorrectly | Information | Install-RepoDotNetSdk.ps1 | line 36
    PSUseOutputTypeCorrectly | Information | Install-RepoDotNetSdk.ps1 | line 39
    PSAvoidUsingWriteHost | Warning | Invoke-MSTest.ps1 | line 119
    PSAvoidUsingWriteHost | Warning | Invoke-MSTest.ps1 | line 120
    PSUseSingularNouns | Warning | Invoke-MSTestWithCoverage.Helpers.ps1 | line 141
    PSAvoidUsingWriteHost | Warning | Invoke-Restore.ps1 | line 32
    PSAvoidUsingWriteHost | Warning | Invoke-VSBuild.ps1 | line 147
    PSUseSingularNouns | Warning | Invoke-VSBuild.ps1 | line 52
    PSUseSingularNouns | Warning | Invoke-VSBuild.ps1 | line 87
    PSAvoidUsingWriteHost | Warning | Sync-PackageReferences.ps1 | line 150
    PSAvoidUsingWriteHost | Warning | Sync-PackageReferences.ps1 | line 154
    PSAvoidUsingWriteHost | Warning | Sync-PackageReferences.ps1 | line 157
```

The direct folder scan totals exactly 16, matching the MCP tool's reported count, which
confirms the direct invocation reproduces the MCP tool's effective rule set. Thirteen of the
sixteen belong to files outside this plan's write set
(scripts/vscode/Install-RepoDotNetSdk.ps1, scripts/vscode/Invoke-Restore.ps1,
scripts/vscode/Invoke-VSBuild.ps1, scripts/vscode/Sync-PackageReferences.ps1) and are
pre-existing; this plan neither introduces nor is required to fix them.

## Baseline Diagnostic Set for the Seven In-Scope Files

This is the verbatim set P5-T2 compares against:

| Rule | Severity | File | Line |
|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 141 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 119 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 120 |

The four remaining in-scope files
(scripts/vscode/Invoke-MSTestWithCoverage.ps1,
scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1,
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1,
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, and
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1) report zero diagnostics at baseline.

The single Helpers.ps1 diagnostic, PSUseSingularNouns at line 141, is raised against the
existing function `Get-CoberturaLineConditionCoverageParts` (plural noun `Parts`). It is
pre-existing, is not one of the seven findings, and is out of this plan's scope to change.

## Output Summary

Diagnostic count by severity across both scan folders: 13 Warning, 3 Information, 16 total.
Diagnostic count within the seven in-scope files: 3, all Warning — one PSUseSingularNouns in
Invoke-MSTestWithCoverage.Helpers.ps1 and two PSAvoidUsingWriteHost in Invoke-MSTest.ps1. All
three are pre-existing. Zero diagnostics exist in any of the three in-scope test files.
