# Phase 0 — Baseline Analyzer Gate (issue #440, plan task P0-T11)

Timestamp: 2026-08-29T06-24

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

`$msbuild` is the absolute path recorded by P0-T8. The command was issued through
`pwsh -NoProfile` from the repository root, never through the Bash tool, per
Global rule 2.

EXIT_CODE: 0 (expected 0)

## Output Summary

MSBuild summary lines:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.88
```

- `BaselineAnalyzerWarningCount`: **5**
- `BaselineAnalyzerErrorCount`: **0**

All five warnings are the same diagnostic, raised once per project that carries a
packages-config file and consumes System.Reactive 7.0.0:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference.
```

The five projects reporting it were enumerated by matching the trailing
`[<project>.csproj]` qualifier of every warning line in the captured output and
de-duplicating: QuickFiler.csproj, TaskMaster.csproj, ToDoModel.csproj,
UtilitiesCS.csproj and UtilitiesCS.Test.csproj. This is a pre-existing
repository-wide condition of the packages-config build style and is unrelated to this
change. It establishes the ceiling the P4-T3 warning count is compared against.

The command was run twice; both runs reported `5 Warning(s)`, `0 Error(s)` and
EXIT_CODE 0. The second run existed only to enumerate the warning projects
accurately, and it changed no figure.

No CS0006 diagnostic appeared, which confirms the P0-T7 analyzer provisioning
resolved the referenced-but-missing set completely.

The build is green, so the P4-T3 warning-count comparison has a usable baseline and
the phase advances. No `TOOLCHAIN-BLOCKER:` was recorded.
