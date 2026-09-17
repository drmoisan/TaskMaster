# P0-T8 — Baseline Nullable and Type-Check State

Timestamp: 2026-09-17T02-15

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\p0-t8.msbuild.log;Verbosity=normal"`
(MSBuild resolved through vswhere)

No `/p:Nullable=enable` and no `/t:Build`. Nullable enforcement in this repository is per-file
opt-in through a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` promotes the
`CS86xx` diagnostics of an opted-in file to build errors. Adding `/p:Nullable=enable` would conscript
every file that has never adopted the pragma, and `/t:Build` would let MSBuild's up-to-date check
skip `CoreCompile` and exit 0 without type-checking anything.

EXIT_CODE: 0

NULLABLE-BASELINE-EXIT: 0

CHANNEL: COMMAND

## Output Summary

WARNINGS: 0

ERRORS: 0

Recorded verbatim from the build summary as `    0 Warning(s)` and `    0 Error(s)`.

ZERO_ERRORS_LINES: 1

CSC_OUT_LINES: 2

At least 1, so the test project was compiled by this rebuild rather than skipped.

TEST-DLL-EXISTS: True

`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` exists after the command. This is the precondition
for P0-T9, which runs `vstest.console.exe` against that assembly; `vstest.console.exe` never
compiles, so the assembly must be produced by a build step before any test task.

The log file exists under `coverage/`, which is git-ignored, and is not copied into the feature
folder.

## Note on the analyzer skew

The analyzer HintPath skew diagnosed and resolved in P0-T7 affected this gate identically before the
remedy, because `<Analyzer Include>` is unconditional and does not depend on the
`/p:EnableNETAnalyzers` or `/p:EnforceCodeStyleInBuild` properties. With the missing package version
provisioned into the git-ignored `packages/` tree, this gate was run once and returned 0 on the
first attempt. No tracked file was edited to obtain that result.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the rebuild and released
immediately after it completed.
