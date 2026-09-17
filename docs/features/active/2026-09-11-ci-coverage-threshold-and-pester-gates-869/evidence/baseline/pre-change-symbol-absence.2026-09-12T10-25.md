# Phase 0 — Pre-change absence of the new symbols (P0-T15)

Timestamp: 2026-09-14T18-17

Purpose: prove that the eight identifiers this delivery introduces are absent from the production and test trees before any implementation task runs, so that the existence assertions in Phase 2 and Phase 4 are not vacuous.

## Grep-tool search over the two code directories

Tool: Grep, count output mode.
Pattern: the eight identifiers as a single alternation.
Paths searched: `scripts/vscode` and `tests/scripts/vscode`, each addressed by an absolute item-worktree path.

Result over `scripts/vscode`: `No matches found. Found 0 total occurrences across 0 files.`
Result over `tests/scripts/vscode`: `No matches found. Found 0 total occurrences across 0 files.`

## Per-identifier counts

Confirmed independently with a per-identifier literal search over every file under both directories, so that a single malformed alternation cannot be the reason the count is zero.

Command: `pwsh -NoProfile -Command '<worktree prologue>; $ids = @(...eight identifiers...); $files = @(Get-ChildItem -Path "scripts/vscode","tests/scripts/vscode" -Recurse -File); foreach ($i in $ids) { $n = @($files | Select-String -SimpleMatch -Pattern $i).Count; ... }'`
EXIT_CODE: 0

| Identifier | Matches across `scripts/vscode` and `tests/scripts/vscode` |
| --- | --- |
| `Assert-CoberturaBranchCoverageThreshold` | 0 |
| `Invoke-VSBuildMain` | 0 |
| `Invoke-RestoreMain` | 0 |
| `Get-MSBuildPath` | 0 |
| `Invoke-SyncPackageReferences` | 0 |
| `Invoke-MSBuildExe` | 0 |
| `Get-RestoreMSBuildPath` | 0 |
| `Invoke-RestoreMSBuildExe` | 0 |

All eight are zero.

## Control search over the feature folder

The same search over `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869` returns non-zero for every identifier, which confirms the search mechanism itself is working and that a zero result over the code directories is a genuine absence rather than a broken search.

| Identifier | Matches across the feature folder |
| --- | --- |
| `Assert-CoberturaBranchCoverageThreshold` | 14 |
| `Invoke-VSBuildMain` | 14 |
| `Invoke-RestoreMain` | 6 |
| `Get-MSBuildPath` | 8 |
| `Invoke-SyncPackageReferences` | 9 |
| `Invoke-MSBuildExe` | 9 |
| `Get-RestoreMSBuildPath` | 4 |
| `Invoke-RestoreMSBuildExe` | 4 |

The Grep-tool alternation over the same folder likewise returned `Found 32 total occurrences across 4 files`, naming `spec.md`, the research record, the plan document and the P0-T12 evidence artifact.

Output Summary: all eight identifiers record a zero match count across `scripts/vscode` and `tests/scripts/vscode`, confirmed by two independent search mechanisms. The same identifiers return non-zero over the feature folder, confirming the search mechanism works. Every later existence assertion in this plan is therefore falsifiable.
