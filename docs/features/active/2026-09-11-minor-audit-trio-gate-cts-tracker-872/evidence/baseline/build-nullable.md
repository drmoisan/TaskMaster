# Phase 0 — Nullable Rebuild Baseline

Timestamp: 2026-09-13T05-06
Task: [P0-T7]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults/msbuild/p0-t7-nullable.txt;Verbosity=detailed"
EXIT_CODE: 0
WarningCount: 0
ErrorCount: 0

MSBuild was resolved through vswhere at the explicit installer path, per D1. `/t:Rebuild` was used
rather than `/t:Build`, per D2.

Output Summary: the build succeeded. MSBuild printed `Build succeeded.` followed by the summary lines
`0 Warning(s)` and `0 Error(s)`, and `Time Elapsed 00:00:17.28`. Both counts were read from the summary
lines ending in `Warning(s)` and `Error(s)` by the same start-anchored whole-line match P0-T6 used.
Exactly one line matched each pattern, and the two matched lines read:

```
0 Warning(s)
0 Error(s)
```

Nullable enforcement in this repository is per-file opt-in: a file participates when it carries a
`#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes that file's CS86xx
diagnostics to build errors. Zero errors therefore means no opted-in file carries a nullable-flow
diagnostic at the base commit.

## The Nullable Property Is Not Supplied, Per D3

The command line above contains no `Nullable` property. A search of the detailed log for the literal
`/p:Nullable=enable` returned 0 matches, which confirms the property reached neither the command line
nor any project through it. No project in this repository carries a `<Nullable>` element and there is no
solution-wide properties file, so supplying the property would conscript every file that has never
adopted the per-file pragma. The CI nullable workflow omits it deliberately and this command matches
that workflow.

## Non-Vacuity Observation

The detailed file log carries both csc command-line literals:

- `/out:obj\Debug\UtilitiesCS.dll` — 2 matching lines
- `/out:obj\Debug\UtilitiesCS.Test.dll` — 2 matching lines

Each count is at least one, so CoreCompile ran on both projects rather than being skipped as up to date.
Per D10 the detailed log remains at the git-ignored transient path and is not committed.

## Build Lock

The cross-item build lock was held across the MSBuild invocation only. Acquired 2026-09-13T05:06:22,
released 2026-09-13T05:06:51. Outlook was confirmed not running, so no test host held the build output
and no MSB3021 occurred.
