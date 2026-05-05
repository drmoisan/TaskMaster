Timestamp: 2026-05-05T13:11:51
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 0
Output Summary: Analyzer-enabled build completed successfully with 0 errors and 25 warnings. Warnings were existing build warnings including CS0618 usage warnings in production code and CS8632/CS0067 warnings in test projects.
