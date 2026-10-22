# P4-T5 — Post-Change Physical Line Counts And The Committed Three-Dot Re-Verification

Timestamp: 2026-09-09T11-17
Task: [P4-T5]
EXIT_CODE: 0

## Command 1 — post-change line counts

Command: `pwsh -NoProfile -Command 'foreach ($f in @(Get-ChildItem -Recurse -File -Filter "*.ps1" -Path "scripts/vscode","tests/scripts/vscode")) { Write-Output ($f.FullName + "=" + (Get-Content -LiteralPath $f.FullName).Count) }'`
EXIT_CODE: 0

The command prints absolute paths. They are recorded below as repository-relative paths, because a
committed artifact must not carry an absolute host path. No count was altered in the transcription.

### Write Set files, before and after

| Path | Before (P0-T3) | After | Delta |
| --- | --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | absent | **162** | new file |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 469 | **470** | +1 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 350 | **351** | +1 |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 | 65 | unchanged |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` | absent | **271** | new file |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 494 | **494** | unchanged |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` | 70 | 70 | unchanged |

`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` are inside the spec's Write
Set but were not modified, because the new function calls `Get-CoberturaPackageLineSummary`
unchanged.

### Every `.ps1` under the two folders, after the change (24 files)

| Path | Physical lines |
| --- | --- |
| `scripts/vscode/Install-RepoDotNetSdk.ps1` | 111 |
| `scripts/vscode/Invoke-MSTest.ps1` | 202 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 |
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 162 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 470 |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 |
| `scripts/vscode/Invoke-Restore.ps1` | 39 |
| `scripts/vscode/Invoke-VSBuild.ps1` | 167 |
| `scripts/vscode/Sync-PackageReferences.ps1` | 159 |
| `scripts/vscode/TestProcessCleanup.ps1` | 70 |
| `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` | 28 |
| `tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1` | 79 |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 144 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 496 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 99 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | 486 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` | 271 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 494 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | 71 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` | 70 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | 15 |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 86 |

The largest count in the set is 496. **Every file, including both new files, is at or below 500.**
The file count rose from 22 to 24, the two additions being this feature's new production and test
files.

## Command 2 — the committed three-dot re-verification

Command: `git diff --numstat epic/review-residuals-2026-09-08-integration...HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 scripts/vscode/Invoke-MSTestWithCoverage.ps1`
EXIT_CODE: 0

```
1	0	scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
1	0	scripts/vscode/Invoke-MSTestWithCoverage.ps1
```

Each of the two paths shows **one added line and zero removed lines** against the committed merge
base. This is the three-dot re-verification that P2-T2 and P2-T3 defer to this task: those two tasks
ran before the edits were committed, when the three-dot form compares committed trees only and would
have printed nothing, so they used the two-dot form and this task carries the committed check.

## Command 3 — tree observation

Command: `git status --porcelain --untracked-files=all -- scripts/vscode`
EXIT_CODE: 0

```
(no output)
```

Nothing under `scripts/vscode` is uncommitted, so the numstat above describes the whole of this
feature's change to that folder.

Output Summary: `Invoke-MSTestWithCoverage.Helpers.ps1` is 470 (was 469) and
`Invoke-MSTestWithCoverage.ps1` is 351 (was 350), each showing exactly one added and zero removed
lines against the committed merge base. `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is unchanged at
494. Every `.ps1` file under `scripts/vscode` and `tests/scripts/vscode`, including the two new
files at 162 and 271 lines, is at or below the 500-line ceiling. AC11 is discharged.
