# Final PowerShell analyzer step (P10-T2)

Timestamp: 2026-09-14T21-04

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

16 is less than or equal to 16, so the count condition holds. The `ok: false` return is the expected shape for any non-zero count, as the P0-T12 artifact records.

## Paired direct run

The paired direct run is required because the per-diagnostic comparison below cannot be read from the MCP payload, which carries a count only.

Command: `pwsh -NoProfile -Command '<worktree prologue>; Invoke-ScriptAnalyzer -Path scripts/vscode -Recurse; Invoke-ScriptAnalyzer -Path tests/scripts/vscode -Recurse'`
EXIT_CODE: 0
Reported count: `COUNT=16`, which agrees with the MCP excerpt.

## Diagnostics naming a path inside the declared write set

The four production files in the write set are `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-VSBuild.ps1` and `scripts/vscode/Invoke-Restore.ps1`; the test files in the write set all carry the `.Tests.ps1` suffix. A filtered query over that set returned exactly four rows:

| Severity | Rule name | File | Line |
| --- | --- | --- | --- |
| Warning | `PSAvoidUsingWriteHost` | `Invoke-Restore.ps1` | 101 |
| Warning | `PSAvoidUsingWriteHost` | `Invoke-VSBuild.ps1` | 245 |
| Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 52 |
| Warning | `PSUseSingularNouns` | `Invoke-VSBuild.ps1` | 87 |

The P0-T12 baseline carries exactly four such rows: three in `scripts/vscode/Invoke-VSBuild.ps1` and one in `scripts/vscode/Invoke-Restore.ps1`. The four rows above match that baseline **by rule name and file**, one for one, with no row added and none removed.

Two line numbers moved, both inside files this delivery restructured and both for the same pre-existing `Write-Host` call that was relocated into an extracted function: `Invoke-Restore.ps1` from line 32 to line 101, and `Invoke-VSBuild.ps1` from line 147 to line 245. The comparison this task specifies is by rule name and file, not by line number, so both remain byte-identical matches under it. Neither is a new diagnostic: each is the same statement in the same file, moved by the extraction.

Removing a pre-existing diagnostic is permitted and none was removed. Introducing a new one is not permitted and none was introduced. No row names any file under `tests/scripts/vscode`, so all ten test files this delivery created or modified are analyzer-clean, and no row names `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` or `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

The remaining twelve rows are the pre-existing diagnostics in `Install-RepoDotNetSdk.ps1`, `Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1` and `Sync-PackageReferences.ps1`, none of which is in the declared write set and none of which this delivery touched.

Output Summary: 16 diagnostics, equal to the baseline count of 16. The four diagnostics naming write-set paths match the baseline by rule name and file exactly; two line numbers moved as a consequence of the main-function extraction and the comparison is unaffected. No new diagnostic was introduced in any path.
