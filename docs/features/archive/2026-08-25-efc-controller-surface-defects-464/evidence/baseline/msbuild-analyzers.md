# Phase 0 — baseline analyzer build

Timestamp: 2026-08-27T23-22
Task: [P0-T10]
Command: `& "<resolved MSBuild.exe>" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:n` under `pwsh -NoProfile` from the worktree root, with output redirected to a log file
EXIT_CODE: 0

The MSBuild path is the one resolved in `[P0-T4]`.

## Result

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.58
```

BASELINE_ANALYZER_ERRORS: 0
BASELINE_ANALYZER_WARNINGS: 5

## BASELINE_ANALYZER_IDS

```
BASELINE_ANALYZER_IDS: (none)
```

Cardinality: **0**.

The set is empty because none of the five warnings carries a diagnostic identifier. All five are emitted
by the `System.Reactive.7.0.0` `PackagesConfigCheck` MSBuild target as a bare `warning :` with no code:

> The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
> Please migrate to PackageReference.

One warning per project, from five projects: `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`,
`UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`. These are pre-existing repository conditions unrelated to
this feature: no Roslyn analyzer produced any diagnostic at this baseline.

## Non-vacuity proof

Count of `Skipping target "CoreCompile"` lines in the build log: **0**. The build genuinely compiled —
36 `csc.exe` invocations appear in the log — so the analyzer gate was actually exercised rather than
short-circuited by MSBuild incrementality. `/t:Rebuild` was used, never `/t:Build`.

## How `[P10-T4]` consumes this

`[P10-T4]` compares as a **set**, not only as a count: any diagnostic identifier observed in the final
analyzer build that is absent from `BASELINE_ANALYZER_IDS` is a new diagnostic, which the cross-cutting
criterion beginning "The analyzer build introduces" forbids. Because the baseline set is empty, the final
build must emit **no identifier-bearing diagnostic at all**, and its warning count must not exceed 5.

Output Summary: Baseline analyzer build exits 0 with 0 errors and 5 warnings, all five being the
identifier-less System.Reactive packages.config advisory. BASELINE_ANALYZER_IDS is the empty set.
Non-vacuity confirmed by zero `Skipping target "CoreCompile"` lines.
