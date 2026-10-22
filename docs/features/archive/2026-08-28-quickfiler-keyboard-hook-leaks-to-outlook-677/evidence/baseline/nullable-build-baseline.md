# Nullable / Type-Check Baseline Build (P0-T8)

Timestamp: 2026-08-28T15-46
Command (CR-MSBUILD then CR-NULLABLE, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'
```

Resolved MSBuild: `<VS-install-root>\18\Community\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.58
```

- Errors: **0**. No `CS86xx` nullable-flow diagnostic was promoted to an error, so every file
  currently carrying `#nullable enable` (including
  `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, the two files Phase 2 edits) is clean at
  baseline.
- Warnings: **5** — the identical `System.Reactive.PackagesConfigCheck.targets(31,5)`
  packages.config advisory recorded in the P0-T7 artifact, on QuickFiler, TaskMaster, ToDoModel,
  UtilitiesCS and UtilitiesCS.Test. It is uncoded (no `CSxxxx` identifier) and is not promoted by
  `/p:TreatWarningsAsErrors=true`.
- `/p:Nullable=enable` was **not** passed, per CLAUDE.md and `.claude/rules/csharp.md`: nullable
  enforcement in this repository is per-file opt-in via `#nullable enable`.
- Run order follows the mandated toolchain sequence (analyzer build first, then nullable), which is
  also what keeps `QuickFiler.Test` compiling under its real C# 7.3 language version.
