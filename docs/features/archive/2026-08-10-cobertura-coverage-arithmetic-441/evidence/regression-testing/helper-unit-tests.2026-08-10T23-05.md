# Helper Unit Tests — Green Run (P3-T8)

Timestamp: 2026-08-10T23-05

Command:

```
# (1) MCP call — NON-PROBATIVE
mcp__drm-copilot__run_poshqc_test
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# (2) direct Pester capture — the actual verdict
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
Import-Module Pester -MinimumVersion 5.0 -Force
$c = New-PesterConfiguration
$c.Run.Path                  = 'tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1'
$c.Run.PassThru              = $true
$c.CodeCoverage.Enabled      = $true
$c.CodeCoverage.Path         = 'scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1'
$c.CodeCoverage.OutputFormat = 'JaCoCo'
$c.CodeCoverage.OutputPath   = '<FEATURE>\evidence\regression-testing\pester-coverage-helper-unit-tests.2026-08-10T23-05.xml'
$c.Output.Verbosity          = 'Detailed'
$c.Should.ErrorAction        = 'Continue'
$r = Invoke-Pester -Configuration $c
```

EXIT_CODE: 0 (`PESTER_EXIT_CODE` from the direct run)

MCP payload (recorded verbatim, **non-probative**): `ok`: `true`; `summary`:
`Ran bundled PoshQC test against 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a' with 2 selected scan folder(s).`

Output Summary:

```
Total=19 Passed=19 Failed=0 Skipped=0
Passed :: preserves backslash separators for nested Windows paths while making them workspace-relative
Passed :: strips active and stale TaskMaster roots while preserving already relative paths
Passed :: merges duplicate class entries that point to the same source file
Passed :: normalizes stale TaskMaster roots before merging duplicate production class entries
Passed :: excludes .Test packages from the report and from the aggregate covered/valid line totals
Passed :: counts each source line once when methods repeat the class-level rollup
Passed :: counts each branch line once when methods repeat the class-level rollup
Passed :: computes the merged per-file line-rate from the merged rollup alone
Passed :: deduplicates a repeated line number by taking the maximum hits value
Passed :: retains method-level lines when the class-level rollup element is absent
Passed :: preserves the primary class methods subtree and every hits value when merging
Passed :: still throws when the document has no packages node
Passed :: excludes projects that resolve to a .Test assembly name
Passed :: retains non-test production projects in the allowlist
Passed :: applies the .Test exclusion to the project-file base-name fallback
Passed :: retains the candidate condition-coverage when its total is greater
Passed :: retains the candidate condition-coverage when totals tie and its covered count is greater
Passed :: retains the existing condition-coverage when neither precedence condition holds
Passed :: returns zero totals for a class with neither a lines nor a methods element
PESTER_EXIT_CODE=0
INSTRUCTION: missed=23 covered=213
LINE: missed=19 covered=183
METHOD: missed=0 covered=8
CLASS: missed=0 covered=1
```

## Verdict

| Metric | Required | Observed |
| --- | --- | --- |
| FailedCount | 0 | **0** |
| PassedCount | **19** | **19** |
| TotalCount | **19** | **19** |

19 = eight pre-existing blocks + F1..F6 + the five blocks added by P3-T3..P3-T7. `TotalCount` is
non-zero, so the gate is not satisfied by a zero-discovery run.

### The five added blocks, listed Passed by name

| Task | `It` name | Branch exercised | Result |
| --- | --- | --- | --- |
| P3-T3 | retains the candidate condition-coverage when its total is greater | precedence: candidate `Total` (4) > existing `Total` (2) — candidate retained, so `TotalBranches` = 4, `CoveredBranches` = 2 | **Passed** |
| P3-T4 | retains the candidate condition-coverage when totals tie and its covered count is greater | precedence: `Total` equal (2), candidate `Covered` (1) > existing (0) — candidate retained, so 2 / 1 | **Passed** |
| P3-T5 | retains the existing condition-coverage when neither precedence condition holds | precedence: candidate `Total` (2) < existing (4) — existing retained, so 4 / 2 | **Passed** |
| P3-T6 | returns zero totals for a class with neither a lines nor a methods element | boundary: all four totals 0, and `Should -Not -Throw` | **Passed** |
| P3-T7 | still throws when the document has no packages node | error handling: `Get-CoberturaCoverageSummary` still throws `Cobertura XML does not contain a <packages> node.` | **Passed** |

P3-T7's block was added **inside** the existing `Describe 'ConvertTo-KoverageCoberturaXml'`, not
inside the new `Describe 'Get-CoberturaClassLineSummary'`, so it does not count against that
`Describe`'s 80-line budget.

## Coverage at this point

| Counter | missed | covered | total | percentage |
| --- | --- | --- | --- | --- |
| INSTRUCTION | 23 | 213 | 236 | 90.25% |
| **LINE** | **19** | **183** | **202** | **90.59%** |
| METHOD | 0 | 8 | 8 | 100.00% |
| CLASS | 0 | 1 | 1 | 100.00% |

Line coverage rose from the 88.48% baseline to 90.59%; `METHOD` rose from 7 to 8 covered, the new
function being the eighth. The authoritative post-change figures are captured by P4-T3.

Coverage report:
`<FEATURE>/evidence/regression-testing/pester-coverage-helper-unit-tests.2026-08-10T23-05.xml`.
