# P2-T2 Analyze Step, Iteration 2

Timestamp: 2026-09-29T10-53
Task: P2-T2 (remediation-plan.2026-09-29T10-00.md)
Iteration: 2
Command: mcp__drm-copilot__run_poshqc_analyze with workspace_root = <repo-root> and scan_folders = ["scripts/vscode"] (run A; the EXIT_CODE row). Runs B to F, the hash command and git -C <repo-root> status --porcelain -uall -- scripts tests are recorded on named lines below.
EXIT_CODE: 1
ExpectedExitCode: 1

## Run A: scan_folders = ["scripts/vscode"]

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 13 issue(s)."} (ANSI escape sequences removed)
- MCP_RESULT_OK_FLAG (A): false
- N_SCRIPTS_FINAL: 13
- N_SCRIPTS_BASELINE (P0-T5): 13
- Delta: 0 (not greater than baseline)

## Run B: scan_folders = ["tests/scripts/vscode"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (B): true (0 findings)

## Run C: scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (C): true (0 findings)

## Run D: scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (D): true (0 findings)

## Run E: scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1"]

- Payload: {"ok":true,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC analyze against '<repo-root>' with 1 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG (E): true (0 findings)

## Run F: scan_folders = ["scripts/vscode/Invoke-MSTest.ps1"]

- Payload: {"ok":false,"tool":"run_poshqc_analyze","workspace_root":"<repo-root>","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 2 issue(s)."} (ANSI escape sequences removed)
- MCP_RESULT_OK_FLAG (F): false
- N_INVOKE_MSTEST_FINAL: 2 (equals N_INVOKE_MSTEST_BASELINE 2; the file is unchanged)

## Tree check after the step

- Hashes: identical to the iteration-2 P2-T1 HA values (36f9595e..., cb9b9a74..., 9aec072f..., 2bf36b22...).
- git status --porcelain -uall -- scripts tests:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

Only Write Set paths; the step changed no file.

RESTART: no

Output Summary:
- PASS. scripts/vscode folder 13 findings, equal to baseline 13 (delta 0); zero findings on the entry point, the Scope part file, the Scope test file and the tests/scripts/vscode folder; Invoke-MSTest.ps1 2, equal to baseline.
- No suppression added; no file changed by the step.
