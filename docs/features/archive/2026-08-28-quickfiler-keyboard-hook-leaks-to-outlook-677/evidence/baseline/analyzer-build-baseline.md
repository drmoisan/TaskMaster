# Analyzer Baseline Build (P0-T7)

Timestamp: 2026-08-28T15-45
Command (CR-MSBUILD then CR-ANALYZE, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'
```

Resolved MSBuild: `<VS-install-root>\18\Community\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.94
```

- Errors: **0**
- Warnings: **5** — all five are the same uncoded advisory emitted by
  `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`
  ("The project contains a packages.config file, which is not supported by System.Reactive v7.0
  or later"), raised once each by QuickFiler, TaskMaster, ToDoModel, UtilitiesCS and
  UtilitiesCS.Test. No analyzer rule diagnostic (no `CAxxxx`, `Sxxxx`, `MAxxxx`, `RCSxxxx`,
  `ASYNCxxxx`, `RS0030`) is present. This is the analyzer-warning baseline for later comparison.
- All nine `*.Test.dll` assemblies produced, including
  `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` (required by P0-T11).

## Environment provisioning performed before this run (no tracked file modified)

The first execution of this exact command failed with `EXIT_CODE: 1`, 0 warnings and
**10 `error CS0006`** diagnostics, raised by `UtilitiesCS.csproj` and `VBFunctions.csproj`:

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll' could not be found
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll' could not be found
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll' could not be found
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll' could not be found
```

Root cause (pre-existing repository skew, not introduced by this plan): 16 first-party `.csproj`
files carry `<Analyzer Include>` HintPaths naming `Meziantou.Analyzer.3.0.156` and
`Roslynator.Analyzers.4.16.0`, while the same files' `<Import>`/`<Error Condition>` restore-check
lines and every `packages.config` name `3.0.174` and `4.16.1`. A clean-worktree restore therefore
installs only the new versions and the old folders the `<Analyzer Include>` items point at are
absent. `git status --porcelain -- '*.csproj' '*packages.config'` was empty before and after, so
the skew is committed repository state, not a local edit.

Remedy applied (environment provisioning only, gitignored target — `.gitignore:349` lists
`packages/`):

```
nuget install Meziantou.Analyzer   -Version 3.0.156 -OutputDirectory packages -DependencyVersion Ignore
nuget install Roslynator.Analyzers -Version 4.16.0  -OutputDirectory packages -DependencyVersion Ignore
```

Post-condition verified: `git status --porcelain -- '*.csproj' '*packages.config' packages` is
empty. No tracked file was modified, and no repository source, project or policy file was edited
to obtain the green baseline. The `EXIT_CODE: 0` recorded above is a genuine compile of every
project with analyzers enabled, not a skipped or degraded build. The durable fix (aligning the 16
`<Analyzer Include>` version strings with `packages.config`) is upstream repository work outside
this plan's scope.
