# P0-T3 — Pre-Change Physical Line Counts

Timestamp: 2026-09-09T10-41
Task: [P0-T3]
Command: `pwsh -NoProfile -Command 'foreach ($f in @(Get-ChildItem -Recurse -File -Filter "*.ps1" -Path "scripts/vscode","tests/scripts/vscode")) { Write-Output ($f.FullName + "=" + (Get-Content -LiteralPath $f.FullName).Count) }'`
EXIT_CODE: 0

The command prints absolute paths. They are recorded below as repository-relative paths, because a
committed artifact must not carry an absolute host path. No count was altered in the transcription.

## Entries (22)

| Path | Physical lines |
| --- | --- |
| `scripts/vscode/Install-RepoDotNetSdk.ps1` | 111 |
| `scripts/vscode/Invoke-MSTest.ps1` | 202 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 469 |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 350 |
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
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 494 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | 71 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` | 70 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | 15 |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 86 |

Output Summary: 22 entries recorded. The five values the task names all match the plan's stated
expectations: `Invoke-MSTestWithCoverage.Helpers.ps1` 469, `Invoke-MSTestWithCoverage.ps1` 350,
`Invoke-MSTestWithCoverage.Helpers.Tests.ps1` 494, `Invoke-MSTest.RunSettings.Tests.ps1` 496 and
`Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` 486. The largest count in the set is 496, so no
entry exceeds the 500-line ceiling. Headroom against the ceiling is 31 lines for Helpers.ps1 and 6
lines for Helpers.Tests.ps1, which is what makes plan decision D1's new-file placement necessary.
