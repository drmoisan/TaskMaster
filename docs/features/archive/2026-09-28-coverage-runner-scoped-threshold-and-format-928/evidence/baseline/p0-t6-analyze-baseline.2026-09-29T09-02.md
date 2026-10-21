# P0-T6 Analyzer Baseline

Timestamp: 2026-09-29T09-02
Task: P0-T6
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root = <repo-root>, run four times: (A) scan_folders = ["scripts/vscode"]; (B) scan_folders = ["tests/scripts/vscode"]; (C) scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.ps1"]; (F) scan_folders = ["scripts/vscode/Invoke-MSTest.ps1"]. The EXIT_CODE row is run (A), mapped per D15 (ok false = 1).
EXIT_CODE: 1
ExpectedExitCode: 1

## Run (A) scripts/vscode

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 13 issue(s)."} (ANSI color escape sequences removed from stderr_excerpt)
- MCP_RESULT_OK_FLAG: false
- N_SCRIPTS_BASELINE: 13

## Run (B) tests/scripts/vscode

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (read as 0 findings)

## Run (C) scripts/vscode/Invoke-MSTestWithCoverage.ps1

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (read as 0 findings; the entry point carries no finding at the base anchor)

## Run (F) scripts/vscode/Invoke-MSTest.ps1

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 2 issue(s)."} (ANSI color escape sequences removed from stderr_excerpt)
- MCP_RESULT_OK_FLAG: false
- N_INVOKE_MSTEST_BASELINE: 2

Output Summary: PASS (baseline recorded). N_SCRIPTS_BASELINE = 13 over scripts/vscode (integer greater than 0; matches the 13 measured on 2026-09-20). tests/scripts/vscode ok true (0 findings). Entry point scripts/vscode/Invoke-MSTestWithCoverage.ps1 ok true (0 findings). N_INVOKE_MSTEST_BASELINE = 2 for scripts/vscode/Invoke-MSTest.ps1 (matches the 2 measured on 2026-09-20). The tool exposes counts only; rule names are not printed by the MCP payload.
