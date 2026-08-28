# Phase 0 — Baseline Nullable Build ([P0-T11])

Timestamp: 2026-08-28T05-15

Command (run under `pwsh -NoProfile` from the worktree root, with the MSBuild path resolved in
`[P0-T4]`):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo
```

GATE: nullable / type-check gate (`/t:Rebuild` against `TaskMaster.sln`)
EXIT_CODE: 0

## Headline counts

| Measure | Value |
| --- | --- |
| **Errors** | **0** |
| Warnings | 5 |
| `Skipping target "CoreCompile"` occurrences | **0** |
| `CoreCompile:` target executions | 52 |
| Build result | `Build succeeded.` |
| Elapsed | 00:00:11.80 |

## `/p:Nullable=enable` was NOT supplied

The command above is character-for-character the CI gate in
`.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"), except that
`/t:Rebuild` replaces CI's `/t:Build` and `/nologo` is added for log hygiene. Two properties are
load-bearing and were preserved:

- **`/p:Nullable=enable` is absent.** Nullable enforcement in this repository is per-file opt-in
  through the `#nullable enable` pragma, and `/p:TreatWarningsAsErrors=true` then promotes a
  participating file's `CS86xx` diagnostics to build errors. The solution-wide property would
  conscript every file that has never adopted the pragma; it is deliberately absent from CI and is
  forbidden by decision D-3 of this plan and by `.claude/rules/csharp.md`.
- **`/t:Rebuild`, not `/t:Build`.** CI can use `/t:Build` because a runner checkout is always cold. A
  local working tree is warm, and MSBuild's up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on every project
  and the gate could not fail.

## Non-vacuity proof

A fixed-string search of the build log for `Skipping target "CoreCompile"` returns **0** occurrences,
while `CoreCompile:` target-execution headers appear **52** times. Combined with the `Rebuild` target,
which runs `Clean` first, this establishes that every project was genuinely recompiled and that the
nullable-flow and compiler diagnostics actually ran.

## The 5 warnings

Identical to the five recorded in `[P0-T10]`: the System.Reactive `packages.config` advisory emitted
by `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)` in the
`_RxCheckPackagesConfig` target, once each for `UtilitiesCS.csproj`, `ToDoModel.csproj`,
`QuickFiler.csproj`, `TaskMaster.csproj`, and `UtilitiesCS.Test.csproj`. They are warnings rather than
errors under `/p:TreatWarningsAsErrors=true` because they are raised by an MSBuild `Warning` task in a
targets file rather than by the compiler, and `TreatWarningsAsErrors` promotes compiler warnings only.
None is attributable to a `.cs` file, and none to any file this feature owns.

## Consequence for [P8-T4]

The baseline error count is **0**, which is the expected case `[P8-T4]` describes and is consistent
with `main` being green on this command. `[P8-T4]`'s acceptance therefore reduces to its absolute
branch: the post-change nullable run must report `EXIT_CODE: 0` with zero errors. The
scope-escalation branch for a non-zero baseline is not triggered.

Output Summary: EXIT_CODE 0. `Build succeeded` with **0 errors** and 5 warnings, all five being the
pre-existing System.Reactive `packages.config` advisory. Zero `Skipping target "CoreCompile"`
occurrences against 52 `CoreCompile:` executions, so the gate was non-vacuous.
`/p:Nullable=enable` was not supplied and the target was `Rebuild`.
