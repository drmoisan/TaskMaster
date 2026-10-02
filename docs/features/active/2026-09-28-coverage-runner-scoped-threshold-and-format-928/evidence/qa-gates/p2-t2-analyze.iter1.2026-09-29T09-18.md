# P2-T2 Analyze Step, Iteration 1

Timestamp: 2026-09-29T09-18
Task: P2-T2
Iteration: 1
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root = <repo-root>, run six times: (A) scan_folders = ["scripts/vscode"]; (B) scan_folders = ["tests/scripts/vscode"]; (C) scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.ps1"]; (D) scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1"]; (E) scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1"]; (F) scan_folders = ["scripts/vscode/Invoke-MSTest.ps1"]. The EXIT_CODE row is run (A), mapped per D15 (ok false = 1).
EXIT_CODE: 1
ExpectedExitCode: 1

## Run (A) scripts/vscode

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 13 issue(s)."} (ANSI color escape sequences removed from stderr_excerpt)
- MCP_RESULT_OK_FLAG_A: false
- N_SCRIPTS_FINAL: 13
- N_SCRIPTS_BASELINE (P0-T6): 13
- Delta: 0 (not greater than baseline)

## Run (B) tests/scripts/vscode

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG_B: true (0 findings)

## Run (C) scripts/vscode/Invoke-MSTestWithCoverage.ps1

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG_C: true (0 findings)

## Run (D) scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG_D: true (0 findings)

## Run (E) tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG_E: true (0 findings)

## Run (F) scripts/vscode/Invoke-MSTest.ps1

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 2 issue(s)."} (ANSI color escape sequences removed from stderr_excerpt)
- MCP_RESULT_OK_FLAG_F: false
- N_INVOKE_MSTEST_FINAL: 2
- N_INVOKE_MSTEST_BASELINE (P0-T6): 2 (equal, D10 loop gate met)

## D16 change observation

- Write Set hashes after the six runs equal the P2-T1 iter1 HA values (d71b98ec..., 2b00d151..., 9aec072f..., 606ae6c8...); `git status --porcelain -uall -- scripts tests` is empty. The step changed no file.

RESTART: no

Output Summary: PASS, RESTART: no. N_SCRIPTS_FINAL 13 equals N_SCRIPTS_BASELINE 13 (delta 0). Runs (B) test folder, (C) entry point, (D) new part file and (E) new test file are each ok true (zero findings). Run (F) Invoke-MSTest.ps1 reports 2, equal to N_INVOKE_MSTEST_BASELINE 2 (D10). No suppression was added.
