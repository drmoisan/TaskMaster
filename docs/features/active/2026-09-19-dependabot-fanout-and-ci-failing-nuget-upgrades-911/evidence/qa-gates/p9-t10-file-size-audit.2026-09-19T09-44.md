# P9-T10 — File size audit across the change footprint

Timestamp: 2026-09-20T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911"; foreach ($f in $paths) { (Get-Content -LiteralPath $f).Count }'
```

EXIT_CODE: 0

Output Summary: 17 files audited, 7 production PowerShell, 8 test PowerShell and 2 workflows. Every
count is at most 500. The largest is 498.

## Production PowerShell — 7 files

| File | Lines | At most 500 |
|---|---|---|
| `scripts/dependencies/PackageGraph.psm1` | 465 | yes |
| `scripts/dependencies/PackageCompatibility.psm1` | 172 | yes |
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 402 | yes |
| `scripts/dependencies/ProjectConsistency.psm1` | 331 | yes |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | yes |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 498 | yes |
| `scripts/vscode/Sync-PackageReferences.ps1` | 423 | yes |

## Tests — 8 files

| File | Lines | At most 500 |
|---|---|---|
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 487 | yes |
| `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | 124 | yes |
| `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 311 | yes |
| `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 453 | yes |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 275 | yes |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 382 | yes |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 335 | yes |
| `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | 185 | yes |

## Workflows — 2 files

| File | Lines | At most 500 |
|---|---|---|
| `.github/workflows/dependabot-repair.yml` | 123 | yes |
| `.github/workflows/_pester.yml` | 80 | yes |

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| Number of files listed | exactly 17 | 17 | PASS |
| Production PowerShell | 7 | 7 | PASS |
| Test PowerShell | 8 | 8 | PASS |
| Workflows | 2 | 2 | PASS |
| Every count is an integer at most 500 | yes | maximum 498 | PASS |

## Margins at the ceiling

Two files sit close to the 500-line cap and are named so the margin is visible to a reviewer:

| File | Lines | Margin |
|---|---|---|
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 498 | 2 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | 7 |

Neither file was rewritten by the Phase 9 formatter pass — P9-T1 iteration 2 recorded a rewrite count
of 0 across all 46 PowerShell files — so neither count moved during the final QA loop and no content
was removed to fit under the cap.

`tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` grew from 374 to 382 lines
when the P9-T2 analyzer findings were corrected, leaving a margin of 118.

Markdown documentation under the feature folder is exempt from the 500-line cap per
`.claude/rules/general-code-change.md` and is deliberately not in this list.
