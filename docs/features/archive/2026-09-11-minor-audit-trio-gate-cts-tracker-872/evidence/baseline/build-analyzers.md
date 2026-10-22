# Phase 0 — Analyzer Rebuild Baseline

Timestamp: 2026-09-13T14-54
Task: [P0-T6]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p0-t6-analyzers.txt;Verbosity=detailed"
EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: the rebuild printed `Build succeeded.`, `    0 Warning(s)` and `    0 Error(s)`, and
exited 0 after 19.10 seconds elapsed. The analyzer baseline is therefore clean: the post-merge tree
carries no analyzer diagnostic under EnableNETAnalyzers and EnforceCodeStyleInBuild.

## Derivation Of The Two Counts

Both counts are read from the MSBuild summary block with a start-anchored match on the whole line,
using the pattern `^\s+\d+ Warning\(s\)$` and the pattern `^\s+\d+ Error\(s\)$` against the detailed
file log. The whole-line anchor is required because a bare substring search for a zero-valued count
also matches a ten-valued one. The matched lines read `0 Warning(s)` and `0 Error(s)` after trimming,
and they agree with the console summary block quoted above.

## Non-Vacuity Observation

The detailed file log contains the csc command-line literals that MSBuild echoes under each project's
CoreCompile heading:

- `/out:obj\Debug\UtilitiesCS.dll` — 2 matching lines
- `/out:obj\Debug\UtilitiesCS.Test.dll` — 2 matching lines

Each count is at least one, so both projects genuinely compiled in this run. This is the observation
that distinguishes a real compilation from a build whose CoreCompile was skipped as up to date, and
the exit code cannot distinguish those two cases. The counts exceed one because the file logger
records the command line under both the task invocation and the CoreCompile echo; the acceptance is a
presence check, so a count above one satisfies it in the same way a count of one would.

Per D2 the target is `/t:Rebuild` and never `/t:Build`: MSBuild's up-to-date check does not invalidate
on a command-line property change, so a warm `/t:Build` returns exit 0 with CoreCompile skipped on
every project and runs no analyzer.

## Re-Run Note, Per D15

This artifact overwrites a superseded capture taken before the main branch carrying the fix for issue
#877 was merged into this branch. The merge changed `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, a
UtilitiesCS test source file, and added a shared source file under the repository-root TestSupport
directory, so the compiled population differs from the one the superseded capture measured. The
re-measured warning and error counts are both 0.

## Log Retention

Per D10 the detailed file log is written to `TestResults/msbuild/p0-t6-analyzers.txt`, which resolves
to the git-ignore pattern for the results directory class. It carries absolute host paths and is never
committed; only the transcribed counts and the two presence observations above are committed.

## Environment Note

Outlook was verified not running before this rebuild, so no add-in host held the build output
directory open. The build lock was held across this single command and released immediately
afterwards.
