# Final QC Stage 3 — Nullable / Type-Check Gate ([P8-T4])

Timestamp: 2026-08-28T06-22

Command (under `pwsh -NoProfile` from the worktree root, MSBuild path resolved in `[P0-T4]`):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo
```

GATE: nullable / type-check gate (`/t:Rebuild` against `TaskMaster.sln`)
EXIT_CODE: 0

## Target and property, as the acceptance requires them recorded

- **The target is `Rebuild`.** `/t:Build` was not substituted. MSBuild's up-to-date check does not
  invalidate on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile`
  skipped on every project and this gate could not fail.
- **`/p:Nullable=enable` was NOT supplied.** Nullable enforcement in this repository is per-file opt-in
  through the `#nullable enable` pragma, and `/p:TreatWarningsAsErrors=true` then promotes a
  participating file's `CS86xx` diagnostics to build errors. The solution-wide property is deliberately
  absent from `.github/workflows/ci.yml`; supplying it would conscript every file that has never adopted
  the pragma. Decision D-3 and `.claude/rules/csharp.md` both forbid it.

Apart from `/t:Rebuild` replacing CI's `/t:Build` and the added `/nologo`, this is character-for-character
the CI gate command.

## Headline counts

| Measure | Baseline `[P0-T11]` | Now `[P8-T4]` | Comparison |
| --- | --- | --- | --- |
| **Errors** | 0 | **0** | not greater than baseline |
| Warnings | 5 | **5** | unchanged |
| `CS86xx` diagnostics | 0 | **0** | none |
| `Skipping target "CoreCompile"` | 0 | **0** | non-vacuous |
| `CoreCompile:` executions | 52 | **76** | non-vacuous |
| Build result | `Build succeeded.` | **`Build succeeded.`** | |
| Elapsed | 00:00:11.80 | 00:00:12.48 | |

`[P0-T11]` recorded **zero** errors, which is the expected case and is consistent with `main` being
green on this command. The acceptance therefore reduces to its absolute branch: **`EXIT_CODE: 0` with
zero errors**, which is what was observed. No error is attributable to any file this feature changed
because there are no errors. The scope-escalation branch is not triggered.

## Nullable diagnostics specifically

A search of the build log for the `CS86` prefix returns **0** occurrences. This matters because one of
the four owned production files, `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
carries `#nullable enable` on its first line and therefore does participate in nullable analysis under
this gate.

That file's D2 edit initially raised **CS8604** — "Possible null reference argument for parameter
'theme'" — because the .NET Framework 4.8.1 reference assemblies carry no `NotNullWhen` post-condition
on `string.IsNullOrWhiteSpace`, so a bare `if (!string.IsNullOrWhiteSpace(_retainedTheme))` established
no non-null flow state. That was found and fixed during `[P2-T5]`, before this gate ran, by capturing
the field into a local and testing `retained != null` alongside the whitespace check. No null-forgiving
`!` operator and no warning suppression was used, and the guard still checks both null and whitespace.
The zero `CS86` count here confirms the fix holds under the real gate command.

## The 5 warnings

Identical to the baseline five: the System.Reactive `packages.config` advisory from
`System.Reactive.PackagesConfigCheck.targets(31,5)`, once each for `UtilitiesCS.csproj`,
`ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, and `UtilitiesCS.Test.csproj`. They remain
warnings rather than errors under `/p:TreatWarningsAsErrors=true` because they are raised by an MSBuild
`Warning` task in a targets file; `TreatWarningsAsErrors` promotes compiler warnings only.

## Non-vacuity proof

`Skipping target "CoreCompile"` appears **0** times; `CoreCompile:` target-execution headers appear
**76** times; and the target is `Rebuild`, which cleans first. Every project was genuinely recompiled,
so the compiler and nullable-flow diagnostics actually ran.

Output Summary: EXIT_CODE 0. `Build succeeded` with **0 errors** and 5 warnings, all five the
pre-existing System.Reactive advisory. **Zero `CS86xx` nullable diagnostics.** The target was `Rebuild`
and `/p:Nullable=enable` was **not** supplied. Zero `Skipping target "CoreCompile"` occurrences against
**76** `CoreCompile:` executions, so the gate was non-vacuous.
