# Batch 1 — PowerShell analyzer step (P2-T6)

Timestamp: 2026-09-14T18-38

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

Reported diagnostic count: **16**.

Baseline count recorded in the P0-T12 artifact: **16**.

16 is less than or equal to 16, so the count condition holds. The `ok: false` return is the expected shape for any non-zero count and is not a tool failure, as the P0-T12 artifact records.

## Paired direct run

Command: `pwsh -NoProfile -Command '<worktree prologue>; Invoke-ScriptAnalyzer -Path scripts/vscode -Recurse; Invoke-ScriptAnalyzer -Path tests/scripts/vscode -Recurse'`
EXIT_CODE: 0
Reported count: `COUNT=16`, which agrees with the MCP excerpt.

| # | Severity | Rule name | File | Line |
| --- | --- | --- | --- | --- |
| 1 | Warning | `PSAvoidUsingWriteHost` | `Install-RepoDotNetSdk.ps1` | 59 |
| 2 | Warning | `PSAvoidUsingWriteHost` | `Install-RepoDotNetSdk.ps1` | 79 |
| 3 | Warning | `PSAvoidUsingWriteHost` | `Install-RepoDotNetSdk.ps1` | 106 |
| 4 | Information | `PSUseOutputTypeCorrectly` | `Install-RepoDotNetSdk.ps1` | 26 |
| 5 | Information | `PSUseOutputTypeCorrectly` | `Install-RepoDotNetSdk.ps1` | 36 |
| 6 | Information | `PSUseOutputTypeCorrectly` | `Install-RepoDotNetSdk.ps1` | 39 |
| 7 | Warning | `PSAvoidUsingWriteHost` | `Invoke-MSTest.ps1` | 210 |
| 8 | Warning | `PSAvoidUsingWriteHost` | `Invoke-MSTest.ps1` | 211 |
| 9 | Warning | `PSUseSingularNouns` | `Invoke-MSTestWithCoverage.Helpers.ps1` | 139 |
| 10 | Warning | `PSAvoidUsingWriteHost` | `Invoke-Restore.ps1` | 32 |
| 11 | Warning | `PSAvoidUsingWriteHost` | `Invoke-VSBuild.ps1` | 147 |
| 12 | Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 52 |
| 13 | Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 87 |
| 14 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 150 |
| 15 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 154 |
| 16 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 157 |

## Comparison against the per-diagnostic baseline

The sixteen rows above are identical, row for row, to the sixteen rows recorded in the P0-T12 baseline artifact, in rule name, file and line.

Diagnostics naming a path inside the declared write set: rows 10, 11, 12 and 13. Each matches a row already present in the P0-T12 per-diagnostic baseline:

- row 10, `PSAvoidUsingWriteHost` in `scripts/vscode/Invoke-Restore.ps1` — present in the baseline;
- row 11, `PSAvoidUsingWriteHost` in `scripts/vscode/Invoke-VSBuild.ps1` — present in the baseline;
- rows 12 and 13, `PSUseSingularNouns` in `scripts/vscode/Invoke-VSBuild.ps1` — both present in the baseline.

No diagnostic naming a write-set path fails to match a baseline row, so this artifact lists none. In particular, the two production files this batch changed, `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, produce no diagnostic at all, and neither do the three test files this batch changed.

Output Summary: 16 diagnostics reported, equal to the baseline count of 16 and identical to it row for row. The batch introduced no new diagnostic in any path, and none of the four write-set rows deviates from the baseline.
