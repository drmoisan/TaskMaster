# Analyzer Build Remediation Baseline

Timestamp: 2026-07-21T18-53Z
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded in 1.73 seconds with 5 warnings and 0 errors. All warnings are the pre-existing System.Reactive packages.config compatibility diagnostic.

Diagnostic counts:

- Warnings: 5
- Errors: 0
- Roslyn analyzer findings: 0
- Pre-existing package compatibility warnings: 5

The repeated warning originates from `packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets(31,5)` for these projects:

- `UtilitiesCS/UtilitiesCS.csproj`
- `ToDoModel/ToDoModel.csproj`
- `QuickFiler/QuickFiler.csproj`
- `TaskMaster/TaskMaster.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

No suppression or project change was applied.
