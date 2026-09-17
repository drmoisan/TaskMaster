# Batch 2 — PowerShell test step (P4-T5)

Timestamp: 2026-09-14T19-22

## MCP invocation

Tool: `mcp__drm-copilot__run_poshqc_test`
Workspace root: the item worktree root.
Scan folders: omitted, so the scan set resolved from `config/poshqc-scan.json`.

Returned payload:

```
ok: true
summary: Ran bundled PoshQC test against '<repo-root>'.
```

The payload carries no counts and no test names, so this task's acceptance is judged on the printed `PESTER Passed=` line from the paired direct run below.

## Paired direct run

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Skipped=" + $r.SkippedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

Printed result line, verbatim:

```
PESTER Passed=160 Failed=0 Skipped=0 Total=160
```

Failed count: **0**.

The exit code is not used as the pass signal, because Pester does not exit non-zero on a failing test case.

## Recorded total, for later phases to compare against

**Total count: 160.** Every later phase compares against this figure. It is reached from the P2-T7 total of 143 as follows:

- `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` moved from 6 cases to 16, a gain of 10: the one entry-point guard case, the seven `Invoke-VSBuildMain` scenario cases, and the two wrapper-seam cases. Its six pre-existing pure-helper cases are retained unchanged.
- `tests/scripts/vscode/Invoke-Restore.Tests.ps1` is new and contributes 7: the one entry-point guard case, the four `Invoke-RestoreMain` scenario cases, and the two wrapper-seam cases.

143 + 10 + 7 = 160, which reconciles exactly.

## Pass-after half of the Phase 3 fail-before evidence

Both guard cases pass. They were identified in the result object by name and by their containing block:

```
GUARDPASS: Invoke-Restore.ps1 entry-point guard / defines functions only and performs no work on dot-source
GUARDPASS: Invoke-VSBuild.ps1 entry-point guard / defines functions only and performs no work on dot-source
```

These are the pass-after half of the fail-before evidence recorded in P3-T1 and P3-T2. On the pre-fix tree each case failed on the same assertion, `$topLevelStatements.Count | Should -Be 3`, reporting 17 for the build script and 12 for the restore script. Both now report 3, which is the measured confirmation that each production file's entry-point body has moved into a function behind an invocation guard.

## Observed behaviour change in the run's own console output

The batch-1 whole-suite run in P2-T7 printed these two lines, produced by the unguarded build-script body executing on dot-source:

```
Using MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
Sync-PackageReferences: All HintPaths are up to date
```

Neither line appears in this run. That is the direct observation that dot-sourcing `scripts/vscode/Invoke-VSBuild.ps1` no longer launches `vswhere.exe` and no longer executes `Sync-PackageReferences.ps1` against the real repository root. It is reported here as a console-output observation, not as this task's acceptance; the acceptance is the failed count and the two guard-case passes above.

## Pre-fix side effect

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- '*.csproj'`
EXIT_CODE: 0
Output verbatim: empty.

No project file was modified by the suite run.

Output Summary: 160 tests, 0 failures, 0 skips. The total reconciles exactly with the 143 recorded in P2-T7 plus the 17 cases this batch added. Both entry-point guard cases pass, and the build script's side-effecting console output is gone from the run.
