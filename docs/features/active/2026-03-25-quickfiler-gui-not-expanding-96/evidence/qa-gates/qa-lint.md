# QA Gate: Lint / Analyzer Build

Timestamp: 2026-03-25T11:08:16.1573691-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 0

## Output Summary

Build succeeded.

- Warnings: 0
- Errors: 0
- Time Elapsed: 00:00:01.11

The analyzer-enabled build completed cleanly for `TaskMaster.sln`. The pre-build helper emitted
two known setup warnings before MSBuild started (`SVGControl.Test` package-resolution warnings and
`TaskMaster` merge-marker skip notice), but the actual analyzer build reported 0 warnings and
0 errors and did not require a QA-loop restart.
