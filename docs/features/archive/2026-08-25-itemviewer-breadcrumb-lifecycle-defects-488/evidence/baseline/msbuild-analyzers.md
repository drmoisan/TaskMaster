# Phase 0 — Baseline Analyzer Build ([P0-T10])

Timestamp: 2026-08-28T05-14

Command (run under `pwsh -NoProfile` from the worktree root, with the MSBuild path resolved in
`[P0-T4]`):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo
```

GATE: analyzer gate (`/t:Rebuild` against `TaskMaster.sln`)
EXIT_CODE: 0

## Headline counts

| Measure | Value |
| --- | --- |
| Total errors | **0** |
| Total warnings | **5** |
| `Skipping target "CoreCompile"` occurrences | **0** |
| `CoreCompile:` target executions | 67 |
| Build result | `Build succeeded.` |
| Elapsed | 00:00:11.31 |

## Non-vacuity proof

MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build`
returns exit 0 having skipped `CoreCompile` on every project and the analyzer gate cannot fail. Three
independent facts establish that this run was a real compile:

1. The target is `Rebuild`, which runs `Clean` before `Build`.
2. A fixed-string search of the build log for `Skipping target "CoreCompile"` returns **0**
   occurrences, while `CoreCompile:` target-execution headers appear **67** times.
3. Build output assemblies were rewritten by this run. `QuickFiler/bin/Debug/QuickFiler.dll`,
   `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, and `UtilitiesCS/bin/Debug/UtilitiesCS.dll` all
   carry modification timestamps within the minute preceding this artifact's write.

A count of `csc.exe` occurrences is deliberately not used as the non-vacuity signal: `CoreCompile`
runs the Roslyn `Csc` MSBuild task rather than spawning `csc.exe`, so that count is zero even on a
genuine compile.

## The 5 warnings, with attribution

All five are the same diagnostic, emitted by a NuGet-supplied targets file rather than by any C#
source file:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
Please migrate to PackageReference.
```

It is raised once per project that references System.Reactive 7.0.0 through `packages.config`, in the
`_RxCheckPackagesConfig` target, for these five projects:

| # | Project |
| --- | --- |
| 1 | `UtilitiesCS/UtilitiesCS.csproj` |
| 2 | `ToDoModel/ToDoModel.csproj` |
| 3 | `QuickFiler/QuickFiler.csproj` |
| 4 | `TaskMaster/TaskMaster.csproj` |
| 5 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |

None is attributable to a C# source file, and none is an analyzer diagnostic. All five are
pre-existing repository conditions this feature neither caused nor is permitted to fix.

## BASELINE PER-FILE ANALYZER WARNING COUNTS (comparison basis for [P8-T3])

| Owned production file | Baseline analyzer warnings |
| --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | **0** |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | **0** |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | **0** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | **0** |

Derived by searching the full build log for a warning line naming each of the four file names; the
combined search returns **0** matching lines. Every one of the five warnings is attributed above to a
`.targets` file and a `.csproj`, and none names a `.cs` file at all.

## Consequence for [P8-T3]

The baseline error count is **0**, which is the expected case this task's downstream gate `[P8-T3]`
describes. `[P8-T3]`'s acceptance therefore reduces to its absolute branch: the post-change analyzer
run must report `EXIT_CODE: 0` with zero errors, and the per-file analyzer warning count for each of
the four owned production files must remain at or below **0** — that is, must remain exactly zero. The
scope-escalation branch for a non-zero baseline is not triggered.

Output Summary: EXIT_CODE 0. `Build succeeded` with **0 errors** and **5 warnings**, all five being
the System.Reactive `packages.config` advisory raised from a NuGet targets file in five projects and
none attributable to a `.cs` file. Zero `Skipping target "CoreCompile"` occurrences against 67
`CoreCompile:` executions, so the gate was non-vacuous. Baseline analyzer warning count for each of
the four owned production files is 0.
