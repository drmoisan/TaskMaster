# [P15-T3] Final QA loop, step 2 — .NET analyzers

Timestamp: 2026-08-26T16-45

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`

Emitted MSBuild command line (the wrapper's `Get-MSBuildBuildArguments` output, host paths replaced
with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m
```

This is the `CLAUDE.md` §C#1.2 policy command; the wrapper emits `/m` last rather than immediately
after `/t:Rebuild`, which is switch-order-equivalent. `/p:Nullable=enable` is deliberately absent.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

MSBuild version `18.8.2+ce25c0108 for .NET Framework`. `Sync-PackageReferences: All HintPaths are up
to date`, so the wrapper rewrote no `.csproj`.

| Metric | P0-T12 baseline | P15-T3 (this run) | Delta |
|---|---|---|---|
| Exit code | 0 | **0** | 0 |
| Errors | 0 | **0** | 0 |
| Warnings | 5 | **5** | 0 |
| Analyzer diagnostics | 0 | **0** | 0 |
| Distinct projects that executed `CoreCompile` | 18 | **18** | 0 |
| `Skipping target "CoreCompile"` occurrences | 0 | **0** | 0 |
| Wall time | 00:00:19.26 | 00:00:15.67 | — |

**No new diagnostic relative to the P0-T12 baseline.** Error count, warning count, and analyzer
diagnostic count are all unchanged, and the five warnings are the same five diagnostics on the same
five projects.

## Non-vacuity proof

The gate requires a non-zero count of projects that executed `CoreCompile`, because a warm
`/t:Build` returns exit 0 having skipped `CoreCompile` on every project and having run no analyzers.
Three independent measurements over the build log:

1. **`Skipping target "CoreCompile"` occurrences: 0.** No project's compilation was skipped. This is
   the direct form of the assertion.
2. **18 distinct `/out:` targets** across the log's `csc.exe` command lines, in 36 `csc.exe`
   references (each invocation appears both as the task's command line and in its echo):

   ```
   QuickFiler.dll          QuickFiler.Test.dll        SVGControl.dll        SVGControl.Test.dll
   Tags.dll                Tags.Test.dll              TaskMaster.dll        TaskMaster.Test.dll
   TaskTree.dll            TaskTree.Test.dll          TaskVisualization.dll TaskVisualization.Test.dll
   ToDoModel.dll           ToDoModel.Test.dll         UtilitiesCS.dll       UtilitiesCS.Test.dll
   VBFunctions.dll         VBFunctions.Test.dll
   ```

   Eighteen distinct outputs for the eighteen projects in `TaskMaster.sln`. Every project compiled.

3. **The analyzer assemblies were genuinely loaded**, not merely requested. Representative
   `/analyzer:` flags observed on the `csc.exe` command lines:

   ```
   Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
   Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll
   ```

   These are exactly the two versions P0-T8 back-filled. Had that back-fill been missing, these
   references would have produced `CS0006` rather than a clean compile.

The gate is therefore non-vacuous: 18 projects were genuinely recompiled with the analyzers loaded.

## The five warnings, enumerated

All five are the same diagnostic, emitted once per project that consumes System.Reactive 7.0.0
through a `packages.config`:

```
warning : The project contains a packages.config file, which is not supported by System.Reactive
v7.0 or later. Please migrate to PackageReference.
```

| # | Project |
|---|---|
| 1 | `QuickFiler/QuickFiler.csproj` |
| 2 | `TaskMaster/TaskMaster.csproj` |
| 3 | `ToDoModel/ToDoModel.csproj` |
| 4 | `UtilitiesCS/UtilitiesCS.csproj` |
| 5 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |

This is **pre-existing repository debt**, present identically at the P0-T12 baseline and tracked by
open issue #570 (`Bug: system-reactive-7-packages-config-unsupported`). It is not an analyzer
diagnostic, it is not attributable to this feature, and no file this feature touched appears in the
list — `QuickFiler.Test/QuickFiler.Test.csproj`, the one project file this feature edited, is absent.

Zero diagnostics were emitted by any of the wired analyzers: no `CA`, `IDE`, `MA`, `RCS`, or `SCS`
rule fired anywhere in the solution.

## Acceptance verification

| Clause | Status |
|---|---|
| `EXIT_CODE: 0` | met |
| a non-zero `CoreCompile` project count | met — **18**, with **0** `Skipping target "CoreCompile"` occurrences |
| no new diagnostic relative to the P0-T12 baseline | met — 0 errors, 5 warnings, 0 analyzer diagnostics, identical to baseline on all three counts |
