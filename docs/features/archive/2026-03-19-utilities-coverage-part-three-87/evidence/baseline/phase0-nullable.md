# Baseline: Nullable Build

Timestamp: 2026-03-28T00:00:00Z
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors
EXIT_CODE: 0

Output Summary:
Build succeeded. Errors: 0, Warnings: 0.
Non-fatal script-level WARNINGs: unresolved DLL references in SVGControl.Test (Castle.Core, FluentAssertions, Moq, MSTest, etc.) and one skipped file in TaskMaster due to merge conflict markers — these did not cause build failure.
