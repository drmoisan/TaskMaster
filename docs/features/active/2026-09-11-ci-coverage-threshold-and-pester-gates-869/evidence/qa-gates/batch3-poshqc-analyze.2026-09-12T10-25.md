# Batch 3 — PowerShell analyzer step (P5-T5)

Timestamp: 2026-09-14T19-41

## First execution, and the one new diagnostic it reported

Tool: `mcp__drm-copilot__run_poshqc_analyze`, scan folders `scripts/vscode` and `tests/scripts/vscode`.

```
ok: false
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 17 issue(s).
```

17 exceeds the P0-T12 baseline of 16, so the acceptance did not hold on that execution. The paired direct run identified the additional row precisely:

```
Warning | PSUseShouldProcessForStateChangingFunctions | TestProcessCleanup.Tests.ps1 | 16 | Function 'New-ProcessRecord' has verb that could change system state. Therefore, the function has to support 'ShouldProcess'.
```

The helper `New-ProcessRecord`, defined inside the setup block of the new `TestProcessCleanup.Tests.ps1`, builds an in-memory `[pscustomobject]` describing a process record. It changes no system state, but PSScriptAnalyzer classifies the `New-` verb as state-changing and therefore requires the function to declare `SupportsShouldProcess`.

## Correction applied

The helper was renamed to `ConvertTo-ProcessRecord`, which uses an approved verb from the Data category that the rule does not classify as state-changing, and all four call sites were updated. The rename is the accurate name for what the function does: it converts the four supplied field values into a process-record shape. No suppression attribute was added, and no `SupportsShouldProcess` declaration was added to a function that processes nothing.

Because the correction changed a file, the toolchain loop was restarted at the formatter rather than continued. `mcp__drm-copilot__run_poshqc_format` was re-run over both scan folders and returned `ok: true`.

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

The row set is the same sixteen rows recorded in the P0-T12 baseline and reproduced in full in the P4-T4 artifact: three `PSAvoidUsingWriteHost` and three `PSUseOutputTypeCorrectly` in `Install-RepoDotNetSdk.ps1`, two `PSAvoidUsingWriteHost` in `Invoke-MSTest.ps1`, one `PSUseSingularNouns` in `Invoke-MSTestWithCoverage.Helpers.ps1`, one `PSAvoidUsingWriteHost` in `Invoke-Restore.ps1`, one `PSAvoidUsingWriteHost` and two `PSUseSingularNouns` in `Invoke-VSBuild.ps1`, and three `PSAvoidUsingWriteHost` in `Sync-PackageReferences.ps1`.

A filtered query for rows naming any `*.Tests.ps1` path returned nothing after the correction, so all three files this batch touched are analyzer-clean and no new diagnostic was introduced in any path.

Output Summary: the first execution reported 17, one above baseline, a `PSUseShouldProcessForStateChangingFunctions` row on the `New-ProcessRecord` test helper. After renaming the helper to `ConvertTo-ProcessRecord` and restarting the loop at the formatter, the count returned to 16, equal to the baseline.
