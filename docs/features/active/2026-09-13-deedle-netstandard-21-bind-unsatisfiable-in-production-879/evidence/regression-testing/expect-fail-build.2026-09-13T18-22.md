# Phase 2 — Targeted Rebuild of the Declaration-Complete Seam

Timestamp: 2026-09-13T23-36

Build lock: ACQUIRED 879 at 2026-09-13T23:32:22, held across the CSharpier and msbuild
invocations of this task.

Command:

```
pwsh -NoProfile -Command '
$d = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing"
if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force > $null }
Write-Output ("REGRESSION_EVIDENCE_DIR_PRESENT=" + (Test-Path -LiteralPath $d))
'
```

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

with all streams redirected to
`evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt`.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
REGRESSION_EVIDENCE_DIR_PRESENT=True
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:24.83
```

Lines in the console log matching the anchored pattern `^\s+0 Error\(s\)$`: 1. Occurrences of
`warning CS` in the console log: 0.

This task is not tagged `[expect-fail]`. The seam is declaration-complete, so a successful
build is the expected outcome here, and a compile failure would have been a defect in the
seam rather than the fail-before signal. The deliberately failing observations belong to
`[P2-T11]`. The analyzer and nullable gates are deliberately not run in this phase: they
would be evaluated against a behaviour-empty ladder.

Console log length after sanitisation: 11765 lines. Residual occurrences of the account name:
0. The absolute worktree path and the absolute user-profile path were replaced with
`<repo-root>` and `<user-profile>`.

## Formatting Applied to the Five New Files

CSharpier was run over the three new `Bootstrap` directories before the build, scoped so it
could reach only the five files this phase created. The scoping matters because two of the
project files this phase edits are shared with a concurrently executing item; `.csharpierignore`
lines 12 to 14 already exclude `*.csproj`, `*.props` and `*.targets`, and the directory
scoping is a second independent guard.

```
dotnet tool run csharpier check UtilitiesCS/Bootstrap UtilitiesCS.Test/Bootstrap TaskMaster.Test/Bootstrap
  => Error on all 5 files, "Was not formatted.", exit 1
dotnet tool run csharpier format UtilitiesCS/Bootstrap UtilitiesCS.Test/Bootstrap TaskMaster.Test/Bootstrap
  => Formatted 5 files in 2969ms, exit 0
dotnet tool run csharpier check UtilitiesCS/Bootstrap UtilitiesCS.Test/Bootstrap TaskMaster.Test/Bootstrap
  => Checked 5 files in 729ms, exit 0
```

The read-only `check` was run both before and after, so the pair of observations distinguishes
a clean run from a repairing one rather than relying on the write-mode command's exit code.
Every acceptance literal asserted by `[P2-T1]`, `[P2-T3]`, `[P2-T5]`, `[P2-T6]`, `[P2-T7]` and
`[P2-T8]` was re-verified against the formatted text and all still hold at their required
counts.

## One Nullable Diagnostic Found and Fixed Within the Seam

The first build of this task exited 0 but reported one warning:

```
UtilitiesCS\Bootstrap\AssemblyBindingFallback.cs(164,24): warning CS8603: Possible null reference return.
```

The `[P0-T7]` nullable baseline is 0 warnings and 0 errors, and the nullable gate promotes
`CS86xx` to errors, so leaving this for Phase 5 would have turned a clean baseline into a
failing gate. The cause was a null check that traced the miss and then fell through to a
shared return, which left the local in a maybe-null state on the remaining path. The fix
returns from inside the null branch instead, so the trailing return is reached only in the
not-null state. No suppression and no annotation change was introduced, the file's public
surface is unchanged, and the edit is confined to
`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`, which is item 1 of the authorised write
set. The build was then re-run and the summary above is from that re-run.
