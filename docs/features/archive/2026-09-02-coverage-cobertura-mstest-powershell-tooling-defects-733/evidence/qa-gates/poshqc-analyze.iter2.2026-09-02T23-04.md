# P5-T2 — PoshQC analyze gate (Final QA Loop, iteration 2, final)

Timestamp: 2026-09-02T23-04

## Command 1 — MCP analyze run

Command: `mcp__drm-copilot__run_poshqc_analyze` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

EXIT_CODE: 1

MCP payload:

```
ok: false
tool: run_poshqc_analyze
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

EXIT_CODE 1 with 16 issues is byte-for-byte the P0-T6 baseline result. This tool exits 1 on any
non-empty diagnostic set at any severity, so its exit code is a constant across baseline and
post-change state and is not the gate signal. The gate signal is the per-file comparison below.

## Command 2 — Direct per-file Invoke-ScriptAnalyzer over all 13 write-set files

Command: `pwsh -NoProfile -Command` with a single-quoted outer wrapper and a double-quoted inner
script, calling `Invoke-ScriptAnalyzer -Path` once per file over the 6 production files and
7 test files in this plan's Phase 5 write set, then `exit 0`.

EXIT_CODE: 0

Verbatim output:

```
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | diagnostics=1
    PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line 137
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTest.ps1 | diagnostics=2
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 145
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 146
FILE: scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | diagnostics=0
TOTAL IN-SCOPE DIAGNOSTICS: 3
```

## Explicit comparison against the P0-T6 baseline set

| Rule | Severity | File | Baseline line | Iteration 2 line | Verdict |
|---|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 141 | 137 | present at baseline, still present |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 119 | 145 | present at baseline, still present |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 120 | 146 | present at baseline, still present |

Set difference against the baseline: empty in both directions. No diagnostic was newly
introduced by this plan, and no baseline diagnostic was silently resolved. The line-number shifts
are explained by this plan's own insertions and deletions above each site:
`Get-CoberturaLineConditionCoverageParts` moved up 4 lines when P1-T10 replaced the inline
per-class accumulation loop in `Get-CoberturaCoverageSummary` with a call to
`Get-CoberturaPackageLineSummary`; the two `Write-Host` calls moved down 26 lines when P4-T4
inserted `Get-MSTestAssemblyPathList` above them.

The `PSUseOutputTypeCorrectly` Information diagnostic observed at iteration 1
(`scripts/vscode/Invoke-MSTest.ps1` line 100) is resolved and does not appear on this run. It was
introduced by P4-T4's `[OutputType([System.Array])]` attribute and was replaced with
`[OutputType([System.Object[]])]`, which the analyzer accepts and which is the runtime-accurate
declaration for a `@(...)` result. That change also brings the whole-scan MCP count back from 17
to the baseline 16.

Six of the 13 write-set files did not exist at the P0-T6 baseline
(`Invoke-MSTestWithCoverage.PackageRate.ps1`, `Invoke-MSTestWithCoverage.Threshold.ps1`,
`Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`, `Invoke-MSTestWithCoverage.Merge.Tests.ps1`,
`Invoke-MSTestWithCoverage.Threshold.Tests.ps1`, `Invoke-MSTest.AssemblyDiscovery.Tests.ps1`).
All six report zero diagnostics, so this plan adds no new PSScriptAnalyzer debt in any new file.

## Output Summary

- MCP analyze: `ok` false, EXIT_CODE 1, 16 issues across both scan folders — identical to the
  P0-T6 baseline count of 16.
- Direct per-file scan over the 13 write-set files: 3 diagnostics, all Warning, all pre-existing
  at baseline, zero new.
- Zero diagnostics in every test file and in every file this plan created.
- No file was changed by this task on this iteration, so the loop does not restart.
