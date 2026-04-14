Timestamp: 2026-04-13T22-58
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors
EXIT_CODE: 0
Output Summary:
- VSBuild wrapper used Visual Studio MSBuild 18.4.0.
- Initial repository warnings were emitted for unresolved SVGControl.Test dependencies and TaskMaster merge conflict markers during package sync.
- Final build summary: Build succeeded.
- Final counts: 0 warnings, 0 errors.
- Time Elapsed: 00:00:01.23.
