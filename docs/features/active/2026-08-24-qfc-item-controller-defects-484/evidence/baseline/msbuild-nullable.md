# Phase 0 — Baseline Nullable Build

Timestamp: 2026-08-26T08-33
Task: [P0-T11]

Command (run under `pwsh -NoProfile` with the `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

`/p:Nullable=enable` was **not** added, per decision D3 and `.claude/rules/csharp.md`. Nullable
enforcement in this repository is per-file opt-in via a `#nullable enable` directive; the solution-wide
property would conscript every file that has never adopted the pragma and is deliberately omitted by CI.

EXIT_CODE: 0

## Counts

| Metric | Value |
|---|---|
| Errors | **0** |
| Warnings | 5 |

## Log summary

```
Build succeeded.
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.43
```

The 5 warnings are the same pre-existing `System.Reactive` `packages.config` compatibility notices
recorded in `msbuild-analyzers.md`. They originate in an imported `.targets` file rather than in a
compiled source file, so `/p:TreatWarningsAsErrors=true` does not promote them.

Output Summary: `/t:Rebuild` nullable gate passes at the baseline with exit code 0 and **0 errors**.
