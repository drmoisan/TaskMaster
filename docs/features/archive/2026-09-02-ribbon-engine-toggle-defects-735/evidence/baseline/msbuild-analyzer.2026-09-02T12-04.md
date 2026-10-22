# Phase 0 — Analyzer Baseline (P0-T6)

Timestamp: 2026-09-03T01-20
Task: [P0-T6]
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

MSBuild resolved through vswhere to `<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`,
version 18.9.1+a81b43525 for .NET Framework. `/t:Rebuild` is used deliberately: a warm `/t:Build`
exits 0 with `CoreCompile` skipped on every project and the analyzers never loaded.

## Trailing counts printed by MSBuild

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.97
```

- Baseline warning count: **5**
- Baseline error count: **0**

P4-T5 is compared against these two numbers.

## Composition of the 5 warnings

All five are the same non-analyzer MSBuild warning, emitted once per project that consumes
System.Reactive 7.0.0 through `packages.config`:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

Emitting projects, one warning each:

1. `QuickFiler/QuickFiler.csproj`
2. `TaskMaster/TaskMaster.csproj`
3. `ToDoModel/ToDoModel.csproj`
4. `UtilitiesCS/UtilitiesCS.csproj`
5. `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

Note on counting method: the MSBuild file logger prints each warning twice, once inline during the
build and once in the trailing summary block. The distinct-warning enumeration above is taken from
the summary block, and the count of 5 is MSBuild's own trailing `5 Warning(s)` figure, not a raw
whole-log grep count.

**Zero Roslyn/.NET analyzer diagnostics were emitted at baseline.** None of the five warnings carries
an analyzer rule ID. Any analyzer diagnostic appearing in the P4-T5 run would therefore be new and
attributable to this change.

Output Summary: Analyzer rebuild succeeded with EXIT_CODE 0, 5 warnings and 0 errors. All five
warnings are the System.Reactive packages.config advisory, one per consuming project; there are no
analyzer rule diagnostics in the baseline.
