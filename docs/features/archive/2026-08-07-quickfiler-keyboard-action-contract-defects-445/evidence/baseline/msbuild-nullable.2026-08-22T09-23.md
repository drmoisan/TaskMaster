# Phase 0 — Nullable / Type-Check Baseline (Issue #445)

Timestamp: 2026-08-22T09-23

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl '/flp:logfile=msbuild-nullable-baseline.log;verbosity=detailed'
(Select-String -SimpleMatch -Pattern 'Skipping target "CoreCompile"' -Path msbuild-nullable-baseline.log | Measure-Object).Count
```
Run from `WS` = `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6e508cbcd1e0a79d` via `pwsh -NoProfile`. This is character-for-character the command in `.github/workflows/ci.yml` except for the deliberate `/t:Rebuild` substitution and the file logger. **No `/p:Nullable=enable` was added**, per Non-negotiable Command Constraint 2.

EXIT_CODE: 0

## Numeric results

| Measurement | Value |
|---|---|
| MSBuild verdict | `Build succeeded.` |
| Error count (MSBuild summary) | 0 |
| Warning count (MSBuild summary) | 5 |
| `Skipping target "CoreCompile"` occurrences in log | 0 |
| `CoreCompile:` occurrences in log | 130 |
| Elapsed | 00:00:21.33 |

## Non-vacuity proof

The `Skipping target "CoreCompile"` count is **0**, so compilation genuinely ran on every project under `/p:TreatWarningsAsErrors=true` and the type-check gate was not vacuous. The `CoreCompile:` count of 130 corroborates this independently. A warm `/t:Build` would have skipped `CoreCompile` and returned exit 0 without type-checking anything.

## Interpretation of the 5 surviving warnings under TreatWarningsAsErrors

The same five third-party System.Reactive `packages.config` advisories recorded in the P0-T12 artifact appear here and are NOT promoted to errors. The reason is that they are emitted by an MSBuild task in a third-party `.targets` file and carry no diagnostic code (the text is `warning :`, with no `CS`/`MSB` identifier). `/p:TreatWarningsAsErrors=true` promotes compiler warnings; it does not promote a codeless MSBuild task warning. They are pre-existing repository state and are unrelated to issue #445.

## Nullable enforcement model

Nullable enforcement in this repository is per-file opt-in: a file participates only when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes that file's `CS86xx` diagnostics to build errors. None of the five files this plan edits carries the pragma today, and this plan does not add one, so no new nullable surface is created. Forcing `/p:Nullable=enable` would conscript every unannotated file in the solution and is deliberately omitted here and in CI.

Output Summary: `Build succeeded.` with EXIT_CODE 0, 0 errors and 5 warnings. The non-vacuity proof holds: the `Skipping target "CoreCompile"` count is exactly 0 (with `CoreCompile:` at 130), so the type-check gate genuinely compiled every project under `/p:TreatWarningsAsErrors=true`. The 5 warnings are the same pre-existing codeless third-party System.Reactive `packages.config` advisories seen in P0-T12; being codeless MSBuild task warnings rather than compiler warnings, they are not promoted to errors. No `/p:Nullable=enable` was added and no `#nullable enable` pragma exists in any of the five in-scope files.
