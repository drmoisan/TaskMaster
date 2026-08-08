# P0-T4 — Analyzer Baseline

Issue: #230
Task: [P0-T4]

- Timestamp: 2026-08-07T21-35
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  (invoked as `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -v:m`
  from git-bash using the VS 18 Community full-framework MSBuild at
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`;
  dash-switch form and `MSYS_NO_PATHCONV=1` are the git-bash-safe equivalents.)
- EXIT_CODE: 0
- Output Summary: Build succeeded. **0 errors, 6 warning lines** across the whole
  solution. All 20 projects produced output, including `TaskMaster.dll`,
  `QuickFiler.dll`, `QuickFiler.Test.dll`, `UtilitiesCS.Test.dll` and
  `TaskMaster.Test.dll` (the Office/VSTO runtime is present in this environment,
  so no CS0234 `ThisAddIn.Designer.cs` failures occurred).

## Warning inventory (pre-existing, not introduced by #230)

| Count | Diagnostic | Project(s) |
|---|---|---|
| 5 | `System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` | UtilitiesCS, ToDoModel, QuickFiler, TaskMaster, UtilitiesCS.Test |
| 1 | `CSC : warning CS2002: Source file '...UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` | UtilitiesCS.Test |

Both warning classes are pre-existing merge-base state, unrelated to this
feature, and are recorded here so the Phase 8 comparison is like-for-like. The
CS2002 duplicate-`Compile` entry in `UtilitiesCS.Test.csproj` is a known latent
defect outside #230's scope.
