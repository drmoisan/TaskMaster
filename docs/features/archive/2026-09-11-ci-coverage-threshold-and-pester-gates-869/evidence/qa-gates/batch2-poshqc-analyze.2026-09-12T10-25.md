# Batch 2 — PowerShell analyzer step (P4-T4)

Timestamp: 2026-09-14T19-18

## First execution, and the two new diagnostics it reported

Tool: `mcp__drm-copilot__run_poshqc_analyze`, scan folders `scripts/vscode` and `tests/scripts/vscode`.

```
ok: false
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 18 issue(s).
```

18 exceeds the P0-T12 baseline of 16, so the acceptance did not hold on that execution. The paired direct run identified the two additional rows precisely:

```
Warning | PSReviewUnusedParameter | Invoke-Restore.Tests.ps1  | 135 | The parameter 'latest' has been declared but not used.
Warning | PSReviewUnusedParameter | Invoke-VSBuild.Tests.ps1  | 257 | The parameter 'latest' has been declared but not used.
```

Both are in the vswhere stand-in helper functions that P3-T1 and P3-T2 define inside their `Invoke-VSBuild.ps1 wrapper seams` and `Invoke-Restore.ps1 wrapper seams` setup blocks. Each stand-in declares `[switch]$latest` so that the seam's own argument list binds against it, but neither read the switch in its body.

## Correction applied

Each stand-in now reads the switch, using the `$null = <parameter>` idiom this repository already uses for deliberately-bound-but-unread parameters, for example in the `Invoke-VsWhereExe` mock in `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`. The switch cannot be removed: deleting it would leave `-latest` unbound and the seam body's call would fail to bind its own argument list, which is precisely the contract these two cases exist to exercise. No suppression attribute was added.

Because the correction changed two files, the toolchain loop was restarted at the formatter rather than continued. `mcp__drm-copilot__run_poshqc_format` was re-run over both scan folders and returned `ok: true`.

## Second execution — the recorded result

Tool: `mcp__drm-copilot__run_poshqc_analyze`, scan folders `scripts/vscode` and `tests/scripts/vscode`.

```
ok: false
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

Reported diagnostic count: **16**.
Baseline count recorded in the P0-T12 artifact: **16**.

16 is less than or equal to 16, so the acceptance holds.

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
| 10 | Warning | `PSAvoidUsingWriteHost` | `Invoke-Restore.ps1` | 101 |
| 11 | Warning | `PSAvoidUsingWriteHost` | `Invoke-VSBuild.ps1` | 245 |
| 12 | Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 52 |
| 13 | Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 87 |
| 14 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 150 |
| 15 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 154 |
| 16 | Warning | `PSAvoidUsingWriteHost` | `Sync-PackageReferences.ps1` | 157 |

## Comparison against the per-diagnostic baseline

The row set is identical to the P0-T12 baseline in rule name and file for all sixteen rows. Two line numbers moved, both inside files this batch restructured and both for the same pre-existing `Write-Host` call:

- row 10, `PSAvoidUsingWriteHost` in `scripts/vscode/Invoke-Restore.ps1`, moved from line 32 to line 101, because the statement now sits inside the extracted `Invoke-RestoreMain` function;
- row 11, `PSAvoidUsingWriteHost` in `scripts/vscode/Invoke-VSBuild.ps1`, moved from line 147 to line 245, for the same reason.

P10-T2 compares write-set rows by rule name and file, not by line number, so both rows remain byte-identical matches to their baseline entries under that comparison. No diagnostic was introduced and none was removed.

The `PSUseSingularNouns` suppression P4-T1 added to `Invoke-SyncPackageReferences` is effective: that function produces no row, while the two pre-existing plural-noun rows in the same file, `Get-MSBuildBuildArguments` at line 52 and `Get-RequestedMSBuildProperties` at line 87, are unchanged. The three new seams in `scripts/vscode/Invoke-Restore.ps1` produce no `PSUseSingularNouns` row, confirming their singular names.

No row names `tests/scripts/vscode` at all, so both new test files are analyzer-clean.

Output Summary: the first execution reported 18, two above baseline, both `PSReviewUnusedParameter` on the `$latest` switch of the two vswhere stand-ins. After reading the switch in each stand-in body and restarting the loop at the formatter, the count returned to 16, equal to the baseline and identical to it by rule name and file.
