# P2-T1 Format Step, Iteration 2

Timestamp: 2026-09-29T10-53
Task: P2-T1 (remediation-plan.2026-09-29T10-00.md)
Iteration: 2
Command: git -C <repo-root> hash-object --no-filters -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 scripts/vscode/Invoke-MSTest.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1; git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode; mcp__drm-copilot__run_poshqc_format with workspace_root = <repo-root> and scan_folders = ["scripts/vscode", "tests/scripts/vscode"]; the hash and porcelain commands again
EXIT_CODE: 0

## Before (HB)

- scripts/vscode/Invoke-MSTestWithCoverage.ps1: 36f9595e5a806a750efb429c1d2963ce172dbc8d
- scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1: cb9b9a7466fe00635393b73d4d215178399a0d01
- scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1: 2bf36b2235c2796e902363868b3fee1a17719a60

Porcelain before:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

## CMD-FORMAT

- Payload: {"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true

## After (HA)

- All four hashes identical to HB.

Porcelain after: identical to porcelain before (the three Write Set files, modified). No out-of-scope path; P0-T4 FORMAT-DRIFT-SET was empty, so nothing was restored.

RESTART: no

Output Summary:
- PASS. Format ok true; HB equals HA for all four files (the AC5 idempotence observation on the edited tree: the entry point and Invoke-MSTest.ps1 are byte-identical across a format run); every porcelain line names a Write Set path.
