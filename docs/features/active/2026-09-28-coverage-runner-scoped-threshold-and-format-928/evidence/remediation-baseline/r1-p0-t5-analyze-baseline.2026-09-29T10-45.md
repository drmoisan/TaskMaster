# R1 P0-T5 Analyzer Baseline

Timestamp: 2026-09-29T10-45
Task: P0-T5 (remediation-plan.2026-09-29T10-00.md)
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root = <repo-root> and scan_folders = ["scripts/vscode"] (run A; the EXIT_CODE row). Runs B to F recorded on named lines below.
EXIT_CODE: 1
ExpectedExitCode: 1

## Run A: scan_folders = ["scripts/vscode"]

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 13 issue(s)."} (ANSI escape sequences removed)
- MCP_RESULT_OK_FLAG (A): false
- N_SCRIPTS_BASELINE: 13

## Run B: scan_folders = ["tests/scripts/vscode"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (B): true

## Run C: scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (C): true

## Run D: scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (D): true

## Run E: scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (E): true

## Run F: scan_folders = ["scripts/vscode/Invoke-MSTest.ps1"]

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 2 issue(s)."} (ANSI escape sequences removed)
- MCP_RESULT_OK_FLAG (F): false
- N_INVOKE_MSTEST_BASELINE: 2

Output Summary:
- N_SCRIPTS_BASELINE 13 (greater than 0; matches the original P0-T6 and P2-T2 figure).
- Runs B, C, D and E each ok true (zero findings on the test folder, the entry point, the Scope part file and the Scope test file).
- N_INVOKE_MSTEST_BASELINE 2 (matches the original measurement).
