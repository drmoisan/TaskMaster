# Phase 0 — Analyzer Baseline with Non-Vacuity Proof (Issue #445)

Timestamp: 2026-08-22T09-21

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl '/flp:logfile=msbuild-analyzer-baseline.log;verbosity=detailed'
(Select-String -SimpleMatch -Pattern 'Skipping target "CoreCompile"' -Path msbuild-analyzer-baseline.log | Measure-Object).Count
(Select-String -SimpleMatch -Pattern 'CoreCompile:' -Path msbuild-analyzer-baseline.log | Measure-Object).Count
```
Run from `WS` = `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6e508cbcd1e0a79d` via `pwsh -NoProfile`. `/t:Rebuild` is used, never `/t:Build`, per Non-negotiable Command Constraint 1. No `/p:Nullable=enable` was added.

EXIT_CODE: 0

## Numeric results

| Measurement | Value |
|---|---|
| MSBuild verdict | `Build succeeded.` |
| Warning count (MSBuild summary) | 5 |
| Error count (MSBuild summary) | 0 |
| `Skipping target "CoreCompile"` occurrences in log | 0 |
| `CoreCompile:` occurrences in log | 96 |
| Elapsed | 00:00:21.09 |

## Non-vacuity proof

The `Skipping target "CoreCompile"` count is **0**, and the `CoreCompile:` target-start count is **96**, well above the floor of 9 (nine `*.Test.csproj` projects alone). Compilation therefore genuinely ran on every project and the analyzers genuinely executed. A warm `/t:Build` would have produced a non-zero skip count and a near-zero `CoreCompile:` count, so this gate is falsifiable and was not vacuous.

## The five warnings

All five are the same pre-existing, non-analyzer warning emitted by a third-party targets file, once per project that carries a `packages.config` and references System.Reactive 7.0.0:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
Please migrate to PackageReference.
```

Emitting projects: `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj`.

These carry no diagnostic ID (the text is `warning :` with no code), so they are not analyzer rule violations and are not addressable by `.editorconfig` severity configuration. They are pre-existing repository state, unrelated to issue #445, and are not remediated by this bugfix. This baseline count of 5 is the ceiling that P5-T4 must not exceed.

The log file is named `msbuild-analyzer-baseline.log`, which `.gitignore` already ignores, so it never appears in a Phase 4 scope-lock `git status` gate.

Output Summary: `Build succeeded.` with EXIT_CODE 0, 5 warnings and 0 errors. The non-vacuity proofs both hold: the `Skipping target "CoreCompile"` count is exactly 0 and the `CoreCompile:` count is 96, at least the required 9. All 5 warnings are the same pre-existing third-party System.Reactive `packages.config` advisory emitted once per affected project (`UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test`); none is an analyzer diagnostic and none is related to issue #445. Baseline warning ceiling for P5-T4 is therefore 5.
