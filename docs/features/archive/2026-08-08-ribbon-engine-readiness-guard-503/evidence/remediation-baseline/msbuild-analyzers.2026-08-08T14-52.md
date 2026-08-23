# Phase 0 — Analyzer Build Baseline (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T9]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true"`
EXIT_CODE: 0

## Output Summary

```text
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.32
```

- **Errors: 0.**
- **Warnings: 6.**

### Baseline warning set (the comparison basis for P3-T4)

| Count | Diagnostic | Projects |
|---|---|---|
| 5 | `System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` | emitted once per project that references `System.Reactive 7.0.0`, including `QuickFiler.csproj`, `TaskMaster.csproj`, and `UtilitiesCS.Test.csproj` |
| 1 | `CSC : warning CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` | `UtilitiesCS.Test.csproj` |

Both diagnostics are pre-existing and untouched by this cycle. `CS2002` is the duplicate `<Compile Include>` entry routed to issue **#510** and is explicitly out of scope; it must not be fixed here. The `System.Reactive` packages.config advisory is a build-infrastructure notice unrelated to this change.

P3-T4 passes on `EXIT_CODE: 0` with no diagnostic absent from this set.

Binary outcome satisfied: `EXIT_CODE: 0`.
