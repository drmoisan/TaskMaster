# P0-T16 PowerShell analyzer baseline (helper test file)

Timestamp: 2026-09-29T09-08
Command: pwsh -NoProfile -Command 'Import-Module PSScriptAnalyzer; $d = @(Invoke-ScriptAnalyzer -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"); "DIAGNOSTICS=" + $d.Count; foreach ($x in $d) { "RULE| " + $x.RuleName + " | " + $x.Severity + " | line " + $x.Line }'; mcp__drm-copilot__run_poshqc_analyze with scan_folders ["tests/scripts/vscode"]
EXIT_CODE: 0
Output Summary:
- DIAGNOSTICS=0 (no RULE| lines).
- MCP channel: POSHQC MCP AVAILABLE ok=true (summary: "Ran bundled PoshQC analyze ... with 1 selected scan folder(s).").

BASELINE-HELPER-TEST-DIAGNOSTICS: 0
