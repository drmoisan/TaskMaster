# Phase 0 — Nullable Rebuild Baseline

Timestamp: 2026-09-13T14-57
Task: [P0-T7]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults/msbuild/p0-t7-nullable.txt;Verbosity=detailed"
EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: the rebuild printed `Build succeeded.`, `    0 Warning(s)` and `    0 Error(s)`, and
exited 0 after 17.87 seconds elapsed. Under this gate a warning is promoted to an error, so the zero
error count is the operative signal and the nullable baseline is clean.

## No Nullable Property, Per D3

The command line contains no Nullable property. No project in this repository carries a Nullable
element and there is no solution-wide opt-in, so supplying it would conscript every file that has
never adopted the per-file pragma. The CI nullable workflow omits it deliberately and this command is
character-for-character the CI form. Nullable enforcement here is per-file opt-in: a file participates
when it carries the enable directive, and TreatWarningsAsErrors then promotes its CS86xx diagnostics
to build errors.

## Derivation Of The Two Counts

Both counts are read from the MSBuild summary block with a start-anchored match on the whole line,
using the pattern `^\s+\d+ Warning\(s\)$` and the pattern `^\s+\d+ Error\(s\)$` against the detailed
file log. The whole-line anchor is required because a bare substring search for a zero-valued count
also matches a ten-valued one. The matched lines agree with the console summary block quoted above.

## Non-Vacuity Observation

The detailed file log contains the csc command-line literals that MSBuild echoes under each project's
CoreCompile heading:

- `/out:obj\Debug\UtilitiesCS.dll` — 2 matching lines
- `/out:obj\Debug\UtilitiesCS.Test.dll` — 2 matching lines

Each count is at least one, so both projects genuinely compiled under this property set rather than
being skipped as up to date. Per D2 the target is `/t:Rebuild` for exactly that reason.

## Re-Run Note, Per D15

This artifact overwrites a superseded capture taken before the main branch carrying the fix for issue
#877 was merged into this branch. The re-measured warning and error counts are both 0.

## Log Retention

Per D10 the detailed file log is written to `TestResults/msbuild/p0-t7-nullable.txt`, which resolves to
the git-ignore pattern for the results directory class. It carries absolute host paths and is never
committed.

## Environment Note

Outlook was verified not running before this rebuild. The build lock was held across this single
command and released immediately afterwards.
