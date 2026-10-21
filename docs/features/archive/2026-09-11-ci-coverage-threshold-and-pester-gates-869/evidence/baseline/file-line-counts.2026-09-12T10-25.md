# Phase 0 — Pre-change file line counts (P0-T4)

Timestamp: 2026-09-14T17-53

Command: Grep tool, count output mode, over the start-of-line pattern, run twice — once against `scripts/vscode` and once against `tests/scripts/vscode`, both addressed by absolute item-worktree paths.
EXIT_CODE: 0

## The eleven halt-set values

Each row is read independently by the Grep tool in count mode and compared for equality against the literal the task states. A file whose count differs by even one line makes that comparison unequal and halts the plan; a count that cannot be read at all is a missing row, which fails the same acceptance.

| Path | Required | Measured | Match |
| --- | --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 | 56 | yes |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 438 | 438 | yes |
| `scripts/vscode/Invoke-VSBuild.ps1` | 167 | 167 | yes |
| `scripts/vscode/Invoke-Restore.ps1` | 39 | 39 | yes |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 498 | 498 | yes |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 86 | 86 | yes |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | 15 | 15 | yes |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 106 | 106 | yes |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | 71 | 71 | yes |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 146 | 146 | yes |
| `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` | 28 | 28 | yes |

All eleven match. The halt branch did not fire.

## Twelfth informational row (not part of the halt set)

| Path | Measured |
| --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 471 |

This confirms the measured count of 471 rather than the 470 the specification's non-goals list still carries. Task P9-T1 corrects that numeral in the specification.

## Write-set members that do not yet exist

Three declared write-set paths are absent from the tree at this point and are therefore not measured here. They are created by later tasks:

- `tests/scripts/vscode/Invoke-Restore.Tests.ps1` (created by P3-T2)
- `tests/scripts/vscode/TestProcessCleanup.Tests.ps1` (created by P5-T1)
- `.github/workflows/_pester.yml` (created by P7-T3)

## Full directory listings as measured

`scripts/vscode` (15 entries, of which 14 are `.ps1`):

```
TestProcessCleanup.ps1:70
Invoke-MSTestWithCoverage.PackageRate.ps1:65
TaskMaster.cli.runsettings:9
Invoke-MSTestWithCoverage.Helpers.ps1:471
Sync-PackageReferences.ps1:159
Invoke-MSTestWithCoverage.FirstParty.ps1:162
Invoke-VSBuild.ps1:167
Invoke-MSTestWithCoverage.ClosureFilter.ps1:413
Invoke-Restore.ps1:39
Invoke-MSTestWithCoverage.Threshold.ps1:56
Invoke-MSTest.TrxSummary.ps1:150
Invoke-MSTestWithCoverage.ps1:438
Invoke-MSTestWithCoverage.Projection.ps1:197
Install-RepoDotNetSdk.ps1:111
Invoke-MSTest.ps1:262
```

`tests/scripts/vscode` (16 entries):

```
Invoke-VSBuild.Tests.ps1:86
Invoke-MSTestWithCoverage.Threshold.Tests.ps1:15
Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1:268
Invoke-MSTestWithCoverage.Projection.Tests.ps1:495
Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:70
Invoke-MSTestWithCoverage.Merge.Tests.ps1:71
Invoke-MSTest.ResultsDirectory.Tests.ps1:119
Invoke-MSTest.RunSettings.Tests.ps1:498
Invoke-MSTest.AssemblyDiscovery.Tests.ps1:79
Install-RepoDotNetSdk.Tests.ps1:28
Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:486
Invoke-MSTest.TrxSummary.Tests.ps1:193
Invoke-MSTest.Main.Tests.ps1:146
Invoke-MSTestWithCoverage.Helpers.Tests.ps1:494
Invoke-MSTestWithCoverage.FirstParty.Tests.ps1:271
Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:106
```

Output Summary: all eleven halt-set line counts match the plan's stated literals exactly, so the plan's citations describe this tree. The informational twelfth row reads 471. The production directory now holds 14 PowerShell scripts rather than the 12 the research record enumerated on 2026-09-12; `Invoke-MSTest.TrxSummary.ps1` and `Invoke-MSTestWithCoverage.Projection.ps1` are the two additions, both arriving with the merged evidence-projection item. That count is recorded here because it enlarges the PowerShell coverage denominator relative to every prior-basis figure; the Phase 0 re-measurement in P0-T8 through P0-T10 is taken against the current denominator, as the plan requires, and no prior-basis figure is carried forward.
