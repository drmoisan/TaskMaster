# Baseline — Analyzer Build

Timestamp: 2026-06-09T11-31
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
(executed as: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -m)
EXIT_CODE: 0

Output Summary:
- Build succeeded.
- 0 Warning(s)
- 0 Error(s)
- The five-package first-party analyzer stack (Meziantou, SonarAnalyzer.CSharp, Roslynator,
  AsyncFixer, BannedApiAnalyzers) is wired; baseline build is analyzer-clean.
