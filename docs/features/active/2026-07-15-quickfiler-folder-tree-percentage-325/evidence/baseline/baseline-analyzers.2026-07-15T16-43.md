# Baseline — .NET Analyzers (P0-T3)

Timestamp: 2026-07-16T09-08
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build SUCCEEDED. 0 Error(s), 76 Warning(s). Warnings are pre-existing baseline noise, predominantly CS8632 (nullable annotation outside a #nullable annotations context) and CS0067 (event never used) in UtilitiesCS.Test. No analyzer errors. This baseline build followed a `nuget.exe restore TaskMaster.sln` (169 packages installed) required in this fresh worktree.

Note: Since analyzer diagnostics are configured at `suggestion` severity per .claude/rules/csharp.md, they do not appear as warnings under the plain analyzer build.
