# Phase 0 — Analyzer Baseline

Timestamp: 2026-08-26T10-42
Task: [P0-T7]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

## Output Summary

Error count: **0**
Warning count: **5**

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:21.95
EXIT_CODE=0
```

All five warnings are the same pre-existing notice emitted once per `packages.config` project
by `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:
"The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later." The five projects are `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and
`UtilitiesCS.Test`. None originates from an analyzer rule and none is attributable to this
feature, whose diff is empty at baseline.

### Gate non-vacuity

`/t:Rebuild` is mandatory because MSBuild's incremental up-to-date check does not invalidate on
a command-line `/p:` change, so a warm `/t:Build` can exit 0 with `CoreCompile` skipped on every
project and run no analyzers at all. Non-vacuity was asserted directly against the build log:

- occurrences of `Skipping target "CoreCompile"`: **0**
- `csc.exe` references in the log: 36

Every project genuinely recompiled, so the zero-error result is a real analyzer result and not
an up-to-date no-op.

## Prerequisite resolved during this task

The first attempt failed with EXIT_CODE 1 and 10 errors, all `CSC : error CS0006: Metadata file
... could not be found`, naming
`packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll` and
the four `packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\*.dll` assemblies.

Cause: a pre-existing version skew on the integration branch, not a consequence of any change in
this feature. Each first-party `.csproj` carries hand-written `<Analyzer Include>` items whose
paths pin `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`, while the same
projects' `packages.config` entries (and the `<Import>` and `<Error>` lines in the same
`.csproj`) have been bumped to `Meziantou.Analyzer.3.0.174` and `Roslynator.Analyzers.4.16.1`.
A clean-worktree `nuget restore` installs only the `packages.config` versions, so the two older
package folders the `<Analyzer Include>` paths require are absent.

The six analyzer package folders referenced by `<Analyzer Include>` items across the solution
were enumerated and checked. Four were present after the [P0-T5] restore
(`AsyncFixer.2.1.0`, `Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0`, `MSTest.Analyzers.4.3.3`,
`SonarAnalyzer.CSharp.10.32.0.713`); exactly two were missing.

Resolution: the two missing versions were installed into the git-ignored `packages/` folder with
`nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages -DependencyVersion Ignore`
and
`nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages -DependencyVersion Ignore`,
both EXIT_CODE 0. No `.csproj`, `.props`, or `.targets` file was edited; correcting the skew in
those files is outside this feature's owned surface. `packages/` is git-ignored, confirmed with
`git check-ignore`, so this step is invisible to every ownership and changed-file gate.
