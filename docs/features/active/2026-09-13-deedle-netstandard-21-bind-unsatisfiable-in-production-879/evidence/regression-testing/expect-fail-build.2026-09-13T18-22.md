# Phase 2 — Fail-Before Harness Build (Revision R2 re-run)

Timestamp: 2026-09-14T10-02

Build lock: ACQUIRED 879 at 2026-09-14T10:01:54, RELEASED by 879 at 2026-09-14T10:02:38.

Outlook gate: `@(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count` returned `0`
immediately before the lock was acquired, so no process held the add-in build output.

This is the Revision R2 re-run. `[P2-T1]`, `[P2-T5]` and `[P2-T6]` all changed compiled source after
the build the version 1.0 execution of this task recorded, so the artifact at this path is
overwritten.

Command:

```
pwsh -NoProfile -Command '
$d = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing"
if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force > $null }
Write-Output ("REGRESSION_EVIDENCE_DIR_PRESENT=" + (Test-Path -LiteralPath $d))
'
```

```
pwsh -NoProfile -Command '
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt"
$LASTEXITCODE
'
```

Both payloads additionally carry a leading `Set-Location` to the item worktree. The executor was
launched without worktree isolation, so pwsh starts in the coordinator session worktree and every
repository-relative path above would otherwise resolve in the wrong checkout. The statement changes
no measurement; it fixes the tree the measurement is taken against.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
REGRESSION_EVIDENCE_DIR_PRESENT=True
ZERO_ERROR_LINES=1
WARNING_SUMMARY_LINES=1
LAST_WARNING_SUMMARY=0 Warning(s)
LOG_LINES=11961
```

Console log tail:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Acceptance Condition: MET. The directory-creation span records
`REGRESSION_EVIDENCE_DIR_PRESENT=True`; `EXIT_CODE: 0`; and the console log at
`expect-fail-build-console.2026-09-13T18-22.txt` contains exactly one line matching
`^\s+0 Error\(s\)$`. The anchored pattern is asserted rather than a bare `error` substring count,
because a successful msbuild run prints the word in unrelated contexts dozens of times, and the
anchored form does not also match `10 Error(s)`.

This task is not tagged `[expect-fail]`. The seam is declaration-complete, so a successful build is
the expected outcome here; a compile failure would be a defect in the seam rather than the
fail-before signal. The deliberately failing observations belong to `[P2-T11]`.

The analyzer and nullable gates are deliberately not run in this phase. They would be evaluated
against a behaviour-empty seam whose ladder rungs all return `null` by design.
