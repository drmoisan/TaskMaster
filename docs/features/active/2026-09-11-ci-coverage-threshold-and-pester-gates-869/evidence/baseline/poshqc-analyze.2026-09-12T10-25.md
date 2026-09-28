# Phase 0 — Baseline PowerShell analyzer state (P0-T12)

Timestamp: 2026-09-14T18-12

## MCP invocation

Tool: `mcp__drm-copilot__run_poshqc_analyze`
Workspace root: the item worktree root.
Scan folders passed, explicitly: `scripts/vscode` and `tests/scripts/vscode`.

Returned excerpt:

```
ok: false
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

Count from the MCP excerpt: **16**.

The MCP call returns `ok` false whenever the count is non-zero. That is the expected baseline shape rather than a tool failure: the analyzer wrapper raises on any reported issue, including Information-severity ones, so a tree with pre-existing diagnostics can never return `ok` true. A non-zero baseline count is recorded here, not treated as a failure of this task. Every later analyzer task in this plan is judged against the per-diagnostic baseline below, not against an absolute zero.

## Paired direct run

The paired direct run is load-bearing: the MCP payload carries a count only and no rule name, file path or line, so no per-diagnostic acceptance condition anywhere in this plan can be satisfied from it.

Command: `pwsh -NoProfile -Command '<worktree prologue>; Invoke-ScriptAnalyzer -Path scripts/vscode -Recurse; Invoke-ScriptAnalyzer -Path tests/scripts/vscode -Recurse'`
EXIT_CODE: 0

`Invoke-ScriptAnalyzer -Path tests/scripts/vscode -Recurse` returned no rows. Every diagnostic below is from `scripts/vscode`.

## Per-diagnostic baseline

| # | Severity | Rule name | File path | Line |
| --- | --- | --- | --- | --- |
| 1 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 59 |
| 2 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 79 |
| 3 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 106 |
| 4 | Information | `PSUseOutputTypeCorrectly` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 26 |
| 5 | Information | `PSUseOutputTypeCorrectly` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 36 |
| 6 | Information | `PSUseOutputTypeCorrectly` | `scripts/vscode/Install-RepoDotNetSdk.ps1` | 39 |
| 7 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Invoke-MSTest.ps1` | 210 |
| 8 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Invoke-MSTest.ps1` | 211 |
| 9 | Warning | `PSUseSingularNouns` | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 139 |
| 10 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Invoke-Restore.ps1` | 32 |
| 11 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Invoke-VSBuild.ps1` | 147 |
| 12 | Warning | `PSUseSingularNouns` | `scripts/vscode/Invoke-VSBuild.ps1` | 52 |
| 13 | Warning | `PSUseSingularNouns` | `scripts/vscode/Invoke-VSBuild.ps1` | 87 |
| 14 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Sync-PackageReferences.ps1` | 150 |
| 15 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Sync-PackageReferences.ps1` | 154 |
| 16 | Warning | `PSAvoidUsingWriteHost` | `scripts/vscode/Sync-PackageReferences.ps1` | 157 |

Row count: 16, which equals the count the MCP excerpt reported. The two sources agree.

## Rows inside the declared write set

Four of the sixteen rows name a path inside this plan's declared write set. They are the rows P10-T2 compares against:

- rows 11, 12 and 13, in `scripts/vscode/Invoke-VSBuild.ps1`: one `PSAvoidUsingWriteHost` and two `PSUseSingularNouns`;
- row 10, in `scripts/vscode/Invoke-Restore.ps1`: one `PSAvoidUsingWriteHost`.

No row names `scripts/vscode/Invoke-MSTestWithCoverage.ps1` or `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, the other two production files in the write set, and no row names any file under `tests/scripts/vscode`. Introducing a diagnostic in either of those two files, or in any test file, would therefore be a new row and would fail P10-T2.

Two consequences recorded for later tasks:

1. The `PSUseSingularNouns` rule is active in this repository and already fires twice in `scripts/vscode/Invoke-VSBuild.ps1`. The seam `Invoke-SyncPackageReferences` that P4-T1 adds ends in a plural noun, so without a suppression it emits a third row and raises the folder count above this baseline, which no batch analyzer acceptance in this plan can absorb. P4-T1 therefore requires a narrowly scoped `PSUseSingularNouns` suppression on that function.
2. `scripts/vscode/Invoke-Restore.ps1` carries no `PSUseSingularNouns` row today, so every function P4-T2 adds to it must carry a singular noun. The three names that task specifies, `Invoke-RestoreMain`, `Get-RestoreMSBuildPath` and `Invoke-RestoreMSBuildExe`, all satisfy that.

Output Summary: the baseline carries 16 analyzer diagnostics, all in `scripts/vscode` and none under `tests/scripts/vscode`. Thirteen are Warning severity and three are Information severity. Four of the sixteen name a path inside the declared write set. The MCP count and the direct-run row count agree at 16.
