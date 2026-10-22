# P5-T2 — PoshQC analyze gate (Final QA Loop, iteration 3, final)

Timestamp: 2026-09-02T23-25

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

16 issues across both scan folders is byte-for-byte the P0-T6 baseline result and the iteration-2
result. This tool exits 1 on any non-empty diagnostic set at any severity, so its exit code is a
constant across baseline and post-change state and is not the gate signal. The gate signal is the
per-file comparison below.

## Command 2 — Direct per-file Invoke-ScriptAnalyzer over all 14 write-set files

Command: `pwsh -NoProfile -Command` with a single-quoted outer wrapper and a double-quoted inner
script, calling `Invoke-ScriptAnalyzer -Path -Severity Error,Warning,Information` once per file
over the 6 production files and 8 test files in this plan's Phase 5 write set as it stands after
the P5-T5 criterion (d) remediation, then `exit 0`.

EXIT_CODE: 0

Verbatim output:

```
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | diagnostics=1
    PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line 137
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTest.ps1 | diagnostics=2
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 185
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 186
FILE: scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | diagnostics=0
TOTAL IN-SCOPE DIAGNOSTICS: 3
```

## Explicit comparison against the P0-T6 baseline set

| Rule | Severity | File | Baseline line | Iteration 2 line | Iteration 3 line | Verdict |
|---|---|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 141 | 137 | 137 | present at baseline, still present |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 119 | 145 | 185 | present at baseline, still present |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 120 | 146 | 186 | present at baseline, still present |

Set difference against the baseline: empty in both directions. No diagnostic was newly introduced,
and no baseline diagnostic was silently resolved. The iteration-3 line shift of the two
`PSAvoidUsingWriteHost` sites, from 145/146 to 185/186, is explained by the remediation: the
entry-point body containing both `Write-Host` calls moved down into the new `Invoke-MSTestMain`
function, below the new `Get-VsTestConsolePath` seam. Both calls are textually unchanged, which is
required because the remediation is a refactor that must preserve the emitted messages exactly.

New surface introduced on this iteration, and its diagnostic result:

| New or changed surface | File | Diagnostics |
|---|---|---|
| `Invoke-MSTestMain` function | scripts/vscode/Invoke-MSTest.ps1 | 0 |
| `Get-VsTestConsolePath` seam | scripts/vscode/Invoke-MSTest.ps1 | 0 |
| dot-source-guarded top-level wiring | scripts/vscode/Invoke-MSTest.ps1 | 0 |
| whole new test file (11 It cases) | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | 0 |

`Get-VsTestConsolePath` deliberately carries no `[OutputType(...)]` attribute. The known
`PSUseOutputTypeCorrectly` interaction recorded on iteration 1 arose from declaring an output type
the analyzer could not reconcile with the function body; declaring none avoids reintroducing that
Information diagnostic, and the whole-scan MCP count stays at the baseline 16 rather than rising
to 17.

Seven of the 14 write-set files did not exist at the P0-T6 baseline
(`Invoke-MSTestWithCoverage.PackageRate.ps1`, `Invoke-MSTestWithCoverage.Threshold.ps1`,
`Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`, `Invoke-MSTestWithCoverage.Merge.Tests.ps1`,
`Invoke-MSTestWithCoverage.Threshold.Tests.ps1`, `Invoke-MSTest.AssemblyDiscovery.Tests.ps1`,
`Invoke-MSTest.Main.Tests.ps1`). All seven report zero diagnostics.

## Output Summary

- MCP analyze: `ok` false, EXIT_CODE 1, 16 issues across both scan folders — identical to the
  P0-T6 baseline count of 16 and to iteration 2.
- Direct per-file scan over the 14 write-set files: 3 diagnostics, all Warning, all pre-existing
  at baseline, zero new.
- Zero diagnostics in every test file and in every file this plan created.
- No file was changed by this task on this iteration, so the loop does not restart.
