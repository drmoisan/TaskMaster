# Pass-After Evidence — F1..F6 against the fixed `Helpers.ps1` (P3-T1, P3-T2)

Timestamp: 2026-08-10T22-55

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
$c.CodeCoverage.OutputPath   = '<FEATURE>\evidence\regression-testing\pester-coverage-pass-after.2026-08-10T22-55.xml'
$c.Output.Verbosity          = 'Detailed'
$c.Should.ErrorAction        = 'Continue'
$r = Invoke-Pester -Configuration $c
```

EXIT_CODE: 0 (`PESTER_EXIT_CODE` from the direct run)

MCP payload (recorded verbatim, **non-probative**): `ok`: `true`; `summary`:
`Ran bundled PoshQC test against 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a' with 2 selected scan folder(s).`
This payload carries no verdict and establishes nothing; it returned the identical `ok:true` string
while the suite was red at P1-T7. Every figure below comes from the direct run.

Output Summary:

```
Total=14 Passed=14 Failed=0 Skipped=0
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
Passed :: excludes projects that resolve to a .Test assembly name
Passed :: retains non-test production projects in the allowlist
Passed :: applies the .Test exclusion to the project-file base-name fallback
PESTER_EXIT_CODE=0
INSTRUCTION: missed=27 covered=209
LINE: missed=23 covered=179
METHOD: missed=0 covered=8
CLASS: missed=0 covered=1
```

## P3-T1 verdict

| Metric | Required | Observed |
| --- | --- | --- |
| FailedCount | 0 | **0** |
| PassedCount | **14** | **14** |
| TotalCount | **14** | **14** |

14 = eight pre-existing blocks + F1..F6. The five helper unit tests added by P3-T3..P3-T7 do not
exist yet, so 19 is correctly **not** the expected figure at this point. `TotalCount` is non-zero,
so the gate is not satisfied vacuously by a zero-discovery run.

### F1..F6 individually listed Passed, with their post-fix values

| Fixture | `It` name | Post-fix assertion values (from `spec.md` § Test Strategy) | Result |
| --- | --- | --- | --- |
| **F1** | counts each source line once when methods repeat the class-level rollup | `lines-valid` = `'3'`, `lines-covered` = `'2'`, `line-rate` = `'0.666667'` (pre-fix 6 / 4) | **Passed** |
| **F2** | counts each branch line once when methods repeat the class-level rollup | `branches-valid` = `'2'`, `branches-covered` = `'1'` (pre-fix 4 / 2) | **Passed** |
| **F3** | computes the merged per-file line-rate from the merged rollup alone | merged `line-rate` = `'0.6'` (3/5); merged `<lines>` has exactly 5 children `12,13,56,57,58` ascending (pre-fix `'0.75'`, 6/8) | **Passed** |
| **F4** | deduplicates a repeated line number by taking the maximum hits value | `lines-valid` = `'1'`, `lines-covered` = `'1'` (pre-fix 3 / 2) | **Passed** |
| **F5** | retains method-level lines when the class-level rollup element is absent | `lines-valid` = `'2'`, `lines-covered` = `'1'` (unchanged before and after) | **Passed** |
| **F6** | preserves the primary class methods subtree and every hits value when merging | merged class keeps a `<methods>` element with exactly one `<method>` (`M`); merged line hits `12=0,13=0,56=1,57=1,58=1` (unchanged before and after) | **Passed** |

## P3-T2 — zero existing tests broken

All **eight** pre-existing `It` blocks are listed individually with `Result = Passed`, quoted
verbatim from the per-test output above:

```
Passed :: preserves backslash separators for nested Windows paths while making them workspace-relative
Passed :: strips active and stale TaskMaster roots while preserving already relative paths
Passed :: merges duplicate class entries that point to the same source file
Passed :: normalizes stale TaskMaster roots before merging duplicate production class entries
Passed :: excludes .Test packages from the report and from the aggregate covered/valid line totals
Passed :: excludes projects that resolve to a .Test assembly name
Passed :: retains non-test production projects in the allowlist
Passed :: applies the .Test exclusion to the project-file base-name fallback
```

This includes `normalizes stale TaskMaster roots before merging duplicate production class entries`,
the block carrying the `lines-valid | Should -Be '3'` assertion that `spec.md` § Why the existing
suite cannot detect either defect singles out.

Command:

```powershell
git diff HEAD --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
```

EXIT_CODE: 0

Output Summary:

```
167	0	tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
```

| Check | Required | Observed | Verdict |
| --- | --- | --- | --- |
| numstat record count | exactly 1 (empty result **fails**) | 1 | pass |
| deletions | exactly 0 | **0** | pass |
| additions | >= 100 | **167** | pass |

Zero deletions proves no existing block was edited.
