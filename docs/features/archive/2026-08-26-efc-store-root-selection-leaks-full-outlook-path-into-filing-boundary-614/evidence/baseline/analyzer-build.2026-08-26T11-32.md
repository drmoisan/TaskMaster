# [P0-T7] Baseline Analyzer Gate

Timestamp: 2026-08-26T11-32
Task: [P0-T7]
Issue: #614

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
Working directory: `<repo-root>`
Shell: `pwsh -NoProfile`
EXIT_CODE: 0

## Result counts

- `5 Warning(s)`
- `0 Error(s)`
- Time Elapsed 00:00:24.76

## Non-vacuity

`/t:Rebuild` forced compilation rather than an incremental up-to-date skip:

- 18 distinct projects entered the `Rebuild` target (QuickFiler, QuickFiler.Test, SVGControl,
  SVGControl.Test, Tags, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree, TaskTree.Test,
  TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS,
  UtilitiesCS.Test, VBFunctions, VBFunctions.Test).
- The build log contains 60 `CoreCompile:` target entries and 36 `csc.exe` references, proving the
  compiler (and therefore the analyzer set) actually ran.
- Build log line count: 11112.

## Warnings observed (all 5 identical in kind, pre-existing, unrelated to this change)

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

Emitted once each for: QuickFiler.csproj, QuickFiler.Test.csproj, TaskMaster.csproj,
TaskMaster.Test.csproj (and one sibling), UtilitiesCS.Test.csproj. These are MSBuild-target
warnings from a NuGet package's own `.targets` file, not analyzer diagnostics, and they are
pre-existing on the branch baseline. Zero analyzer diagnostics of `warning` or higher severity were
emitted.

Output Summary: Baseline analyzer gate PASSES with EXIT_CODE 0, 0 errors and 5 pre-existing
System.Reactive packages.config MSBuild warnings. Compilation was proven non-vacuous (18 projects
rebuilt, 60 CoreCompile entries, 36 csc references). No project file was modified for the #615
analyzer skew; the gitignored `packages\` backfill is sufficient.
