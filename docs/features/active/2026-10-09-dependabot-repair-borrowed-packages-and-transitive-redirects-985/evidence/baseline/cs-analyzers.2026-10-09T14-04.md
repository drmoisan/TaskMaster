# C# Baseline Analyzers (P0-T11)

Timestamp: 2026-10-09T14-04
Command: pwsh -NoProfile -File CMDDIR\985-msbuild.ps1 -WorkspaceRoot WORKSPACE-ROOT -Gate Analyzers -LogName 985-baseline-analyzers.log (msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true)
EXIT_CODE: 0
Output Summary:
- EXIT_CODE: 0
- OUTPUT-ASSEMBLIES: 18
- SUMMARY: 0 Warning(s)
- SUMMARY: 0 Error(s)
- ERROR-LINE-COUNT: 0
- PATH-LENGTH-SIGNATURE-LINES: 0
- BASELINE-OUTPUT-ASSEMBLIES: 18
- Log kept at coverage/985-baseline-analyzers.log (ignored directory).
- Result: analyzer baseline green; no CS-BASELINE-RED.
