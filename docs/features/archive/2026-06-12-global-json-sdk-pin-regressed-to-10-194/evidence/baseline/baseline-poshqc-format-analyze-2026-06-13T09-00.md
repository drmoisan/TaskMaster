# Phase 0 — Baseline PoshQC Format + Analyze (Issue #194)

Timestamp: 2026-06-13T11-25

## Format
Command: mcp__drm-copilot__run_poshqc_format (scan_folders: tests/scripts/vscode, scripts/vscode)
EXIT_CODE: 0
Output Summary: Format ran successfully against 2 scan folders; ok=true. No format changes were reported for the regression test file or related scripts (formatter reported clean).

## Analyze
Command: mcp__drm-copilot__run_poshqc_analyze (scan_folders: tests/scripts/vscode, scripts/vscode)
EXIT_CODE: 1
Output Summary:
- PoshQC analyzer reported 16 pre-existing issue(s) total across the scanned folders.
- Severity breakdown: Warning 13, Information 3.
- All findings are in production scripts under scripts/vscode (and recursively scanned scripts), NOT in the regression test file tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1.
- Finding detail (RuleName | Severity | File:Line):
  - PSAvoidUsingWriteHost | Warning | Install-RepoDotNetSdk.ps1:59, :79, :106
  - PSUseOutputTypeCorrectly | Information | Install-RepoDotNetSdk.ps1:26, :36, :39
  - PSAvoidUsingWriteHost | Warning | Invoke-MSTest.ps1:119, :120
  - PSUseSingularNouns | Warning | Invoke-MSTestWithCoverage.Helpers.ps1:133
  - PSAvoidUsingWriteHost | Warning | Invoke-Restore.ps1:32
  - PSAvoidUsingWriteHost | Warning | Invoke-VSBuild.ps1:137
  - PSUseSingularNouns | Warning | Invoke-VSBuild.ps1:47, :78
  - PSAvoidUsingWriteHost | Warning | Sync-PackageReferences.ps1:150, :154, :157

Baseline interpretation:
- These 16 findings are the pre-change baseline. The only change in this plan is a single field in global.json (a JSON config file, not analyzed by PSScriptAnalyzer). The change cannot introduce or remove any PowerShell analyzer finding. Post-change analyze is expected to report the identical 16 baseline findings with zero new findings on changed/related files.
