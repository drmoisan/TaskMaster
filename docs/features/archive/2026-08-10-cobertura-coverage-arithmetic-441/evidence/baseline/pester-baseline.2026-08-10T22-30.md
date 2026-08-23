# Baseline Pester Run and Coverage (P0-T16)

Timestamp: 2026-08-10T22-30

Command:

```
# (1) MCP call — NON-PROBATIVE (carries no verdict, no counts, no coverage)
mcp__drm-copilot__run_poshqc_test
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# (2) direct Pester capture — the actual verdict and the numbers
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
Import-Module Pester -MinimumVersion 5.0 -Force
$coverageXmlPath = Join-Path $root 'docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence\baseline\pester-coverage-baseline.2026-08-10T22-30.xml'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $coverageXmlPath) | Out-Null
$c = New-PesterConfiguration
$c.Run.Path                    = 'tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1'
$c.Run.PassThru                = $true
$c.CodeCoverage.Enabled        = $true
$c.CodeCoverage.Path           = 'scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1'
$c.CodeCoverage.OutputFormat   = 'JaCoCo'
$c.CodeCoverage.OutputPath     = $coverageXmlPath
$c.Output.Verbosity            = 'Detailed'
$c.Should.ErrorAction          = 'Continue'
$r = Invoke-Pester -Configuration $c
```

EXIT_CODE: 0 (`PESTER_EXIT_CODE` from the direct run, not the MCP payload)

MCP payload (recorded verbatim, **non-probative**): `ok`: `true`; `summary`:
`Ran bundled PoshQC test against 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a' with 2 selected scan folder(s).`
It returns `ok:true` whether the suite is green or red and reports no counts, so it establishes
nothing about the baseline.

Output Summary:

```
Total=8 Passed=8 Failed=0 Skipped=0
Passed :: preserves backslash separators for nested Windows paths while making them workspace-relative
Passed :: strips active and stale TaskMaster roots while preserving already relative paths
Passed :: merges duplicate class entries that point to the same source file
Passed :: normalizes stale TaskMaster roots before merging duplicate production class entries
Passed :: excludes .Test packages from the report and from the aggregate covered/valid line totals
Passed :: excludes projects that resolve to a .Test assembly name
Passed :: retains non-test production projects in the allowlist
Passed :: applies the .Test exclusion to the project-file base-name fallback
PESTER_EXIT_CODE=0
INSTRUCTION: missed=22 covered=170
LINE: missed=19 covered=146
METHOD: missed=0 covered=7
CLASS: missed=0 covered=1
```

## Suite totals

| Metric | Value |
| --- | --- |
| TotalCount | **8** |
| PassedCount | **8** |
| FailedCount | **0** |
| SkippedCount | **0** |

All **eight** pre-existing `It` blocks in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` pass, and each is listed by name
above. This matches the plan's stated baseline of 8 passed / 0 failed exactly. `TotalCount` is
non-zero, so the green result is not the vacuous zero-discovery case.

## JaCoCo counters (concrete integers)

| Counter | missed | covered | total | percentage |
| --- | --- | --- | --- | --- |
| INSTRUCTION | 22 | 170 | 192 | 88.54% |
| **LINE** | **19** | **146** | **165** | **88.48%** |
| METHOD | 0 | 7 | 7 | 100.00% |
| CLASS | 0 | 1 | 1 | 100.00% |

**Derived baseline line-coverage percentage: 146 / 165 = 0.884848 = 88.48%** for
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. This is the no-regression reference for
P4-T6. No placeholder values appear anywhere in this artifact.

Coverage report path: `<FEATURE>/evidence/baseline/pester-coverage-baseline.2026-08-10T22-30.xml`.

## Negative-evidence claim — no BRANCH counter

Pester 5.6.1 emits **no `BRANCH` counter**. The counters actually present in the report are exactly
the four listed above: `INSTRUCTION`, `LINE`, `METHOD`, `CLASS`. That enumeration is the proof.

- SearchScope: `<FEATURE>/evidence/baseline/pester-coverage-baseline.2026-08-10T22-30.xml`
- SearchPatterns: `report/counter[@type='BRANCH']`
- SearchResult: `none`

Consequence: the `>= 75%` branch floor in `.claude/rules/general-unit-test.md` has no available
instrument for PowerShell in this repository. This is a property of the coverage tooling, is not
caused by this change, and is **not** grounds for altering any threshold. It is carried forward as
an auditable claim in P4-T6.
