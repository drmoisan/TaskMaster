# P11-T11 PoshQC analysis

Timestamp: 2026-08-04T11-15

Command: `mcp__drm-copilot__run_poshqc_analyze(workspace_root='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25', scan_folders=['scripts/vscode', 'tests/scripts/vscode'])`

EXIT_CODE: 1

Output Summary: The bundled MCP folder scan reported the established 16 inherited findings and exited 1. The same PSScriptAnalyzer check scoped exactly to `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` reported zero findings for both paths. No suppression, configuration, filter, exclusion, threshold, or runsettings input changed.

Changed-path verification command: `Invoke-ScriptAnalyzer -Path scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Recurse; Invoke-ScriptAnalyzer -Path tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 -Recurse`

Changed-path result: `0` findings in each file.

Revalidated: 2026-08-04T11-18. The scoped MCP scan again reported the inherited 16 findings; direct analysis of both changed paths again reported zero findings.
