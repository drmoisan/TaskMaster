# Fail-Before Evidence — F1, F2, F3, F4 against unmodified `Helpers.ps1` (P1-T7, P1-T8)

Timestamp: 2026-08-10T22-45

`CLAUDE.md` § Bugfix Workflow step 1 requires the regression tests to exist and be demonstrated
failing before any production change. This artifact records that demonstration. At the time of this
run, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is **unmodified** — proven below by
`git diff --name-only edf3d34c -- scripts` returning empty output.

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
$coverageXmlPath = Join-Path $root 'docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence\regression-testing\pester-coverage-fail-before.2026-08-10T22-45.xml'
$c = New-PesterConfiguration
$c.Run.Path                  = 'tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1'
$c.Run.PassThru              = $true
$c.CodeCoverage.Enabled      = $true
$c.CodeCoverage.Path         = 'scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1'
$c.CodeCoverage.OutputFormat = 'JaCoCo'
$c.CodeCoverage.OutputPath   = $coverageXmlPath
$c.Output.Verbosity          = 'Detailed'
$c.Should.ErrorAction        = 'Continue'
$r = Invoke-Pester -Configuration $c
```

EXIT_CODE: 1 (`PESTER_EXIT_CODE` from the direct run)

MCP payload (recorded verbatim, **non-probative**): `ok`: `false`; `summary`:
`Command exited with code 4.` The MCP tool returns no counts, no test names and no failure detail,
and it returns `ok:true` whether the suite is green or red; **none of the values below is attributed
to it.** Every figure comes from the direct `Invoke-Pester` run.

Output Summary:

```
Total=14 Passed=10 Failed=4 Skipped=0
Passed :: preserves backslash separators for nested Windows paths while making them workspace-relative
Passed :: strips active and stale TaskMaster roots while preserving already relative paths
Passed :: merges duplicate class entries that point to the same source file
Passed :: normalizes stale TaskMaster roots before merging duplicate production class entries
Passed :: excludes .Test packages from the report and from the aggregate covered/valid line totals
Failed :: counts each source line once when methods repeat the class-level rollup :: Expected: '3' / But was: '6' ; Expected: '2' / But was: '4'
Failed :: counts each branch line once when methods repeat the class-level rollup :: Expected: '2' / But was: '4' ; Expected: '1' / But was: '2'
Failed :: computes the merged per-file line-rate from the merged rollup alone :: Expected: '0.6' / But was: '0.75'
Failed :: deduplicates a repeated line number by taking the maximum hits value :: Expected: '1' / But was: '3' ; Expected: '1' / But was: '2'
Passed :: retains method-level lines when the class-level rollup element is absent
Passed :: preserves the primary class methods subtree and every hits value when merging
Passed :: excludes projects that resolve to a .Test assembly name
Passed :: retains non-test production projects in the allowlist
Passed :: applies the .Test exclusion to the project-file base-name fallback
PESTER_EXIT_CODE=1
```

## Verdict

| Metric | Required | Observed |
| --- | --- | --- |
| FailedCount | **4** | **4** |
| PassedCount | **10** | **10** |
| TotalCount | 14 (8 pre-existing + F1..F6) | 14 |

`TotalCount` is non-zero, so this is not the vacuous zero-discovery case. No other failure count was
observed, so the plan's halt condition is not triggered.

## Per-fixture pre-fix actuals

`$c.Should.ErrorAction = 'Continue'` was set, so every failed assertion inside each `It` is reported
rather than only the first. That is what makes the paired figures below observable.

| Fixture | Assertion | Expected (post-fix) | **Actual against unmodified `Helpers.ps1`** | Matches plan |
| --- | --- | --- | --- | --- |
| **F1** | `lines-valid` | `'3'` | **`'6'`** | yes (6/4) |
| **F1** | `lines-covered` | `'2'` | **`'4'`** | yes |
| **F2** | `branches-valid` | `'2'` | **`'4'`** | yes (4/2) |
| **F2** | `branches-covered` | `'1'` | **`'2'`** | yes |
| **F3** | merged class `line-rate` | `'0.6'` | **`'0.75'`** | yes |
| **F4** | `lines-valid` | `'1'` | **`'3'`** | yes (3/2) |
| **F4** | `lines-covered` | `'1'` | **`'2'`** | yes |

F1's `line-rate` assertion (`'0.666667'`) passed pre-fix, confirming the § Fixture-Design Trap
warning: a rate-only assertion is not a regression test. F1 and F2 are regression tests only because
they assert counts.

F5 (`retains method-level lines when the class-level rollup element is absent`) and F6 (`preserves
the primary class methods subtree and every hits value when merging`) both **pass** pre-fix, exactly
as the plan specifies; neither is tagged `[expect-fail]`. All **eight** pre-existing `It` blocks are
listed `Passed`.

Coverage report for this run:
`<FEATURE>/evidence/regression-testing/pester-coverage-fail-before.2026-08-10T22-45.xml`.

---

## P1-T8 — Fixtures written, no existing block modified, no production change yet

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff HEAD --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
git diff --name-only edf3d34c -- scripts
```

EXIT_CODE: 0

Output Summary:

```
--- git diff HEAD --numstat -- tests/... ---
167	0	tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
--- git diff --name-only edf3d34c -- scripts ---
--- end (empty above means unchanged) ---
```

| Check | Required | Observed | Verdict |
| --- | --- | --- | --- |
| numstat record count | exactly 1 (empty result **fails**) | 1 | pass |
| additions | >= 100 | **167** | pass |
| deletions | exactly 0 | **0** | pass |
| `git diff --name-only edf3d34c -- scripts` | empty | empty | pass |

The single numstat record proves the fixtures were actually written (an unmodified file emits no
record at all, which would satisfy a bare "0 deletions" reading while proving nothing). The `0`
deletions field proves no existing test block was modified. The `HEAD` operand makes both checks
immune to staging. The empty `scripts` diff proves no production file has changed at this point, so
the fail-before demonstration above is genuinely against unmodified code.

Test-file line count after the six insertions: **389** (from 222). Per-fixture line counts, all
within the plan's § Test-File Line Budget: F1 = 24 (<= 24), F2 = 28 (<= 28), F3 = 34 (<= 34),
F4 = 22 (<= 26), F5 = 19 (<= 24), F6 = 34 (<= 34).
