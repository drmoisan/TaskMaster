# Phase 0 — PowerShell Toolchain Baseline (pre-edit)

Timestamp: 2026-06-12T19-22

Command:
```
# Analyze (PoshQC MCP): run_poshqc_analyze scoped to scripts/vscode + tests/scripts/vscode
# Per-file detail reproduced with bundled pssa.settings.psd1:
Invoke-ScriptAnalyzer -Path scripts/vscode -Recurse -Settings <bundled pssa.settings.psd1> -Severity Error,Warning,Information
# Pester (coverage mode) scoped to tests/scripts/vscode, coverage targeting the two in-scope scripts:
Invoke-Pester -Configuration <Run.Path=tests/scripts/vscode; CodeCoverage on Invoke-MSTest.ps1 + Invoke-MSTestWithCoverage.ps1>
```

EXIT_CODE: analyze = 1 (16 findings, pre-existing folder debt); Pester = 1 (1 pre-existing out-of-scope failure)

Output Summary:

## Analyzer baseline (PSScriptAnalyzer via PoshQC)

- `tests/scripts/vscode/` (the in-scope TEST folder): 0 findings — CLEAN.
- `scripts/vscode/` (folder): 16 findings total (pre-existing analyzer debt), distributed:
  - `Install-RepoDotNetSdk.ps1`: 6 (OUT OF SCOPE)
  - `Invoke-MSTest.ps1`: 2 — `PSAvoidUsingWriteHost` at lines 116, 117 (IN SCOPE file; these `Write-Host`
     calls are in the script body, NOT inside `Resolve-RunSettingsPath`; they are pre-existing and untouched by this change)
  - `Invoke-MSTestWithCoverage.Helpers.ps1`: 1 (OUT OF SCOPE)
  - `Invoke-Restore.ps1`: 1 (OUT OF SCOPE)
  - `Invoke-VSBuild.ps1`: 3 (OUT OF SCOPE)
  - `Sync-PackageReferences.ps1`: 3 (OUT OF SCOPE)
  - `Invoke-MSTestWithCoverage.ps1`: 0 findings (IN SCOPE file, clean)

Baseline analyzer finding count for the two IN-SCOPE production scripts: **2** (both in `Invoke-MSTest.ps1`,
rule `PSAvoidUsingWriteHost`, pre-existing). The final-QC gate (P2-T1) must show no net-new analyzer debt
versus this count.

## Pester baseline (coverage mode)

- Result: Passed=17, Failed=1, Total=18.
- The 1 failure is `Install-RepoDotNetSdk.Tests.ps1` -> "global.json SDK selection / pins the repository to the
  repo-local .NET 8 SDK path so dotnet format avoids the broken 10.0.200 host SDK". This is a PRE-EXISTING failure
  entirely OUTSIDE the five in-scope files (it concerns `Install-RepoDotNetSdk.ps1`, not the runsettings change).
- The IN-SCOPE test file `Invoke-MSTest.RunSettings.Tests.ps1`: all tests PASS at baseline.
- Numeric line-coverage headline for the two in-scope scripts
  (`Invoke-MSTest.ps1` + `Invoke-MSTestWithCoverage.ps1`): **77.06%** (84 / 109 commands covered).
  Uncovered commands are the script-body fail-fast `throw` paths and the executable-invocation seams
  (e.g., `& $VsTestPath @VsTestArgs`, `& dotnet-coverage @DotnetCoverageArgs`), which are not exercised by the
  argument-list unit tests by design.

This baseline establishes the no-net-new-analyzer-debt reference (2 in-scope) and the no-coverage-regression
reference (77.06% on changed-script lines) for AC7.
