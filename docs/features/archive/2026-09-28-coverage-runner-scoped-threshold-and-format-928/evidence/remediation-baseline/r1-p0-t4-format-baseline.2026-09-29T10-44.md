# R1 P0-T4 Format Baseline with Liveness Control

Timestamp: 2026-09-29T10-44
Task: P0-T4 (remediation-plan.2026-09-29T10-00.md)
Command: git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode; git -C <repo-root> hash-object --no-filters -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 scripts/vscode/Invoke-MSTest.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1; mcp__drm-copilot__run_poshqc_format with workspace_root = <repo-root> and scan_folders = ["scripts/vscode", "tests/scripts/vscode"] (CMD-FORMAT, twice); Edit-tool perturbation of line 272; git -C <repo-root> diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1; Grep count mode; git -C <repo-root> checkout -- scripts/vscode/Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0

## Steps 1 to 4 (baseline run)

- Porcelain before (scripts/vscode, tests/scripts/vscode): empty.
- H0:
  - scripts/vscode/Invoke-MSTestWithCoverage.ps1: d71b98ec641781702605e6806ca6527726539620
  - scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1: 2b00d151502d979c52c5e3c11567ebc87d484c14
  - scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5
  - tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1: 606ae6c881c7e634397f6b5f8635ea997874f29b
- CMD-FORMAT payload 1: {"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (run 1)
- Porcelain after: empty.
- H1: identical to H0 for all four files.
- FORMAT-DRIFT-SET: (empty)

## Step 5 (liveness control)

- Edit tool: four spaces prepended to line 272 (`$ErrorActionPreference = 'Stop'`) of scripts/vscode/Invoke-MSTestWithCoverage.ps1.
- CONTROL_NUMSTAT_BEFORE: `30	3	scripts/vscode/Invoke-MSTestWithCoverage.ps1` (committed 29 and 2 plus the one perturbed line)
- CMD-FORMAT payload 2: {"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (run 2)
- CONTROL_LINE_AFTER: `^    \$ErrorActionPreference = 'Stop'` count 0; `^\$ErrorActionPreference = 'Stop'` count 1 (the formatter removed the indentation it was given).
- Restore: git checkout -- scripts/vscode/Invoke-MSTestWithCoverage.ps1. FORMAT-DRIFT-SET is empty, so no other path was restored.
- Final porcelain (scripts/vscode, tests/scripts/vscode): empty.
- Final hashes: identical to H0 for all four files.

Output Summary:
- Both CMD-FORMAT runs returned ok true; the base tree carries no format drift in the two folders (FORMAT-DRIFT-SET empty, H1 equals H0).
- Liveness confirmed: the perturbed line (numstat 30/3) was re-indented to column 0 by the formatter.
- The tree was restored: final porcelain empty and final hashes equal H0.
