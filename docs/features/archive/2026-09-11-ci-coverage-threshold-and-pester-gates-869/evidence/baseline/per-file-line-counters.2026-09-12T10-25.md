# Phase 0 — Baseline per-file LINE counters affected by the seam defect (P0-T11)

Timestamp: 2026-09-14T18-10

Source document: `coverage/pester-coverage.xml`, written by the P0-T8 run.

Command: `pwsh -NoProfile -Command '<worktree prologue>; [xml]$j = Get-Content coverage/pester-coverage.xml -Raw; foreach ($c in @($j.SelectNodes("//class"))) { $lc = @($c.counter) | Where-Object { $_.type -eq "LINE" }; ... }'`
EXIT_CODE: 0

## The two rows this task requires

| File name read against | LINE covered | LINE missed |
| --- | --- | --- |
| `Invoke-VSBuild.ps1` | 36 | 7 |
| `Sync-PackageReferences.ps1` | 35 | 49 |

Both were read from the `class` element whose `sourcefilename` attribute carries the file name shown, using the LINE `counter` child of that element. The class names are `vscode/Invoke-VSBuild` and `vscode/Sync-PackageReferences` respectively.

These two rows are the comparison basis for the determinism proof in task P6-T6. The build script's 36 covered lines arise from the unguarded top-level body executing on dot-source; the sync script's 35 covered lines arise entirely from that body's call to the sync script at line 154 of the build script, because no test file targets the sync script directly.

## Full per-file table, recorded for context

| File | LINE covered | LINE missed |
| --- | --- | --- |
| `Install-RepoDotNetSdk.ps1` | 3 | 30 |
| `Invoke-MSTest.ps1` | 47 | 9 |
| `Invoke-MSTest.TrxSummary.ps1` | 40 | 2 |
| `Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 |
| `Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 |
| `Invoke-MSTestWithCoverage.Helpers.ps1` | 193 | 19 |
| `Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 |
| `Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 |
| `Invoke-MSTestWithCoverage.ps1` | 112 | 13 |
| `Invoke-MSTestWithCoverage.Threshold.ps1` | 14 | 1 |
| `Invoke-Restore.ps1` | 0 | 16 |
| `Invoke-VSBuild.ps1` | 36 | 7 |
| `Sync-PackageReferences.ps1` | 35 | 49 |
| `TestProcessCleanup.ps1` | 0 | 29 |
| **Total** | **662** | **177** |

The column totals reconcile exactly with the report-level LINE counter recorded in P0-T9, which reads covered 662 and missed 177. That reconciliation is the check that no class entry was omitted from this table.

Output Summary: the build script reads 36 covered of 43 LINE entries and the package-reference sync script reads 35 covered of 84. The per-file table sums to the report-level totals of 662 covered and 177 missed, confirming completeness.
