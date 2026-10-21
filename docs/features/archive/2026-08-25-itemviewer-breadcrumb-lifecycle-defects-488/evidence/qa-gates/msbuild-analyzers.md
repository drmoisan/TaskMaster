# Final QC Stage 2 — Analyzer Gate ([P8-T3])

Timestamp: 2026-08-28T06-21

Command (under `pwsh -NoProfile` from the worktree root, MSBuild path resolved in `[P0-T4]`):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo
```

GATE: analyzer gate (`/t:Rebuild` against `TaskMaster.sln`)
EXIT_CODE: 0

## Headline counts

| Measure | Baseline `[P0-T10]` | Now `[P8-T3]` | Comparison |
| --- | --- | --- | --- |
| **Errors** | 0 | **0** | not greater than baseline |
| Warnings | 5 | **5** | unchanged |
| `Skipping target "CoreCompile"` | 0 | **0** | non-vacuous |
| `CoreCompile:` executions | 67 | **83** | non-vacuous |
| Build result | `Build succeeded.` | **`Build succeeded.`** | |
| Elapsed | 00:00:11.31 | 00:00:14.13 | |

`[P0-T10]` recorded **zero** errors, which is the expected case this task describes, so the acceptance
reduces to its absolute branch: **`EXIT_CODE: 0` with zero errors**. That is what was observed. The
scope-escalation branch for a non-zero baseline is not triggered, and no error is attributable to any
file this feature changed because there are no errors at all.

## Per-file analyzer warning comparison

| Owned production file | Baseline `[P0-T10]` | Now | Result |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 0 | **0** | not greater |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 0 | **0** | not greater |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 0 | **0** | not greater |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 0 | **0** | not greater |

**The analyzer warning count attributable to each of the four owned production files is 0, equal to and
therefore no greater than its Phase 0 baseline of 0.**

Two independent searches establish this. A combined search of the build log for a warning line naming
any of the four file names returns **0** matching lines. A broader search for any warning line carrying
the compiler's `<file>.cs(line,col): warning` shape returns **0** matching lines across the whole
solution, so no warning in this build is attributable to **any** C# source file.

## The 5 warnings

Identical to the baseline five: the System.Reactive `packages.config` advisory emitted by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)` in the
`_RxCheckPackagesConfig` target, once each for `UtilitiesCS.csproj`, `ToDoModel.csproj`,
`QuickFiler.csproj`, `TaskMaster.csproj`, and `UtilitiesCS.Test.csproj`. All five are raised by a NuGet
targets file, none names a `.cs` file, and all are pre-existing repository conditions this feature
neither caused nor is permitted to fix.

## Non-vacuity proof

A warm `/t:Build` would return exit 0 having skipped `CoreCompile` on every project, because MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change, and the analyzer gate could not
then fail. Three facts establish that this run was a real compile:

1. The target is `Rebuild`, which runs `Clean` before `Build`.
2. `Skipping target "CoreCompile"` appears **0** times in the log.
3. `CoreCompile:` target-execution headers appear **83** times.

The `CoreCompile:` count is higher than the baseline's 67 because this build compiles the added test
file and the changed sources across more project configurations in one pass; the load-bearing figure is
the **zero** skips.

A `csc.exe` occurrence count is deliberately not used as the signal: `CoreCompile` runs the Roslyn `Csc`
MSBuild task rather than spawning `csc.exe`, so that count is zero even on a genuine compile.

Output Summary: EXIT_CODE 0. `Build succeeded` with **0 errors** and **5 warnings**, all five the
pre-existing System.Reactive advisory and none attributable to a `.cs` file. The analyzer warning count
for each of the four owned production files is **0**, no greater than its baseline of 0. Zero
`Skipping target "CoreCompile"` occurrences against **83** `CoreCompile:` executions, so the gate was
non-vacuous.
