# PowerShell Analyzer Baseline (Issue #283)

Timestamp: 2026-07-08T17-56
Command: `mcp__drm-copilot__run_poshqc_analyze` (scan_folders: `scripts/vscode`, `tests/scripts/vscode`)
EXIT_CODE: 1

Output Summary:
- PoshQC analyze (bundled PSScriptAnalyzer settings) reported 16 pre-existing issue(s) folder-wide across the two scanned folders. The scanned folders include multiple scripts NOT touched by this fix (e.g. `Install-RepoDotNetSdk.ps1`, `Invoke-VSBuild.ps1`, `TestProcessCleanup.ps1`, `Sync-PackageReferences.ps1`), which account for the bulk of the 16.
- Per-file breakdown for the THREE in-scope files (default PSScriptAnalyzer, pwsh7):
  - `scripts/vscode/Invoke-MSTest.ps1`: 2 pre-existing `PSAvoidUsingWriteHost` warnings at lines 119-120 (host status output in the top-level body; OUTSIDE the `Get-VsTestArgumentList` edit region at line ~54).
  - `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: 0 findings.
  - `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`: 0 findings.
- Baseline interpretation: the analyzer exit-1 is driven by pre-existing findings folder-wide, not by anything this fix will introduce. The final gate (P2-T6) acceptance is "no NEW analyzer findings on the touched scripts"; this baseline records the pre-existing count so any delta is attributable.
