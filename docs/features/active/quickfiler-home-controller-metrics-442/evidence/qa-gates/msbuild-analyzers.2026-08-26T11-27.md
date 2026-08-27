# Phase 6 — Analyzer Gate

Timestamp: 2026-08-26T11-27
Task: [P6-T3]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

This is the exact command from [P0-T7].

## Output Summary

Error count: **0**
Warning count: 5

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:26.57
EXIT_CODE=0
```

### Comparison with the [P0-T7] baseline

| Metric | Baseline ([P0-T7]) | Post-change ([P6-T3]) | Delta |
| --- | --- | --- | --- |
| Errors | 0 | **0** | 0 |
| Warnings | 5 | 5 | 0 |

The warning population is unchanged in both count and content: the same pre-existing, code-less
`System.Reactive.PackagesConfigCheck` notice emitted once per `packages.config` project
(`UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test`). This feature
introduced no analyzer diagnostic and suppressed none.

### Gate non-vacuity

`/t:Rebuild` is mandatory. MSBuild's incremental up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzers at all: the gate could not fail.

Occurrences of `Skipping target "CoreCompile"` in the build log: **0**.

Every project genuinely recompiled, so the zero-error result is a real analyzer result. This also
confirms that the three `using` directives removed by [P5-T12] from `QfcHomeController.cs`
(`System.Collections.Concurrent`, `System.Timers`, `System.Linq`) produce no missing-type error
under a full recompile of every consuming project.

`/p:Nullable=enable` was deliberately not added; see [P6-T4].
