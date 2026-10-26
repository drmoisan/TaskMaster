# Baseline Toolchain Analyzers (Issue #1007)

Timestamp: 2026-10-23T13-40
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (MSBuild resolved through vswhere; console logger /v:minimal /clp:Summary; log kept under SCRATCH)
EXIT_CODE: 0
Output Summary:
- `0 Warning(s)`, `0 Error(s)`.
- Warnings whose path is a PROD-MODIFY file: none.
