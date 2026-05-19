Timestamp: 2026-04-08T11-39
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 0
Output Summary: Analyzer-enabled baseline build succeeded with 46 warnings and 0 errors in 00:00:05.74.
Raw Highlights:
- 46 Warning(s)
- 0 Error(s)
- Time Elapsed 00:00:05.74
- Sample warnings included obsolete async enumerable API usage in `TaskMaster.csproj`, MSTEST0032 in `QuickFiler.Test`, and nullable-annotation-context warnings in `UtilitiesCS.Test`.
