# Phase 2 — Targeted Build of the Behaviour-Empty Seam

Timestamp: 2026-09-14T11-25

This artifact was overwritten by the Revision R5 re-run of `[P2-T10]`, as that task directs. The re-run
is required because the Revision R5 amendments to `[P2-T5]` and `[P2-T6]` change compiled source after
the build the Revision R2 execution recorded. `[P2-T1]` is unchanged by Revision R5 and is not a reason
for this re-run.

This task is NOT tagged `[expect-fail]`. The seam is declaration-complete, so the expected outcome here
is a successful build; the deliberately failing observations belong to `[P2-T11]`. The analyzer and
nullable gates are deliberately not run in this phase, because they would be evaluated against a
behaviour-empty seam.

The build lock was acquired for item `879` before the spans below and released after them. The
Outlook-closed gate was re-confirmed before the rebuild: `OUTLOOK_PROCESS_COUNT=0`, with the window
closed rather than the process killed.

Command:

Span 1, directory creation:

```
pwsh -NoProfile -Command '
$d = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing"
if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force > $null }
Write-Output ("REGRESSION_EVIDENCE_DIR_PRESENT=" + (Test-Path -LiteralPath $d))
'
```

Span 2, rebuild:

```
pwsh -NoProfile -Command '
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt"
$LASTEXITCODE
'
```

Both payloads additionally carry a leading `Set-Location` to the item worktree, because the executor was
launched without worktree isolation and pwsh would otherwise resolve every repository-relative path in
the coordinator session worktree. That statement changes no measurement.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
REGRESSION_EVIDENCE_DIR_PRESENT=True
$LASTEXITCODE=0
ZERO_ERROR_LINE_COUNT=1
LOG_LINE_COUNT=11858
```

Console log summary block, reproduced from the tail of
`expect-fail-build-console.2026-09-13T18-22.txt`:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The raw console log is at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt`.
It contains absolute host paths in its per-project lines and is therefore left uncommitted; `[P5-T11]`
projects it and `[P5-T12]` removes it.

Acceptance Condition: MET. The directory-creation span recorded `REGRESSION_EVIDENCE_DIR_PRESENT=True`;
the rebuild exited 0, matching `ExpectedExitCode: 0`; and the console log carries exactly one line
matching the anchored pattern `^\s+0 Error\(s\)$`, which is the success signal this plan asserts and
which does not also match a line such as `10 Error(s)`. The seam compiles, so the fail-before signal
`[P2-T11]` looks for will be a runtime observation rather than a compile failure.
