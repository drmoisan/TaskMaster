# Baseline — .NET Analyzers (P0-T4)

- **Timestamp:** 2026-07-11T12-52
- **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (MSBuild.exe from VS18 Community; `MSYS_NO_PATHCONV=1`)
- **EXIT_CODE:** 0
- **Output Summary:** `Build succeeded. 76 Warning(s), 0 Error(s).` Warnings are pre-existing, all in test projects: CS8632 (nullable annotation outside a `#nullable` context) and CS0067 (unused event). No analyzer errors. This build does not treat warnings as errors, so warnings are non-fatal at baseline.
