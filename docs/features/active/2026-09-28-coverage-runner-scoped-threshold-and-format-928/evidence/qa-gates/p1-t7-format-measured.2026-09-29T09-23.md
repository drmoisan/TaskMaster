# P1-T7 Measured Format Pass

Timestamp: 2026-09-29T09-23
Task: P1-T7
Command: git hash-object --no-filters -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 scripts/vscode/Invoke-MSTest.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1; git status --porcelain -uall -- scripts/vscode tests/scripts/vscode; mcp__drm-copilot__run_poshqc_format with workspace_root = <repo-root> and scan_folders = ["scripts/vscode", "tests/scripts/vscode"]; the same hash command; the same porcelain command
EXIT_CODE: 0
MCP_RESULT_OK_FLAG: true

MCP payload (transcribed, workspace_root replaced):
- ok: true
- tool: run_poshqc_format
- workspace_root: <repo-root>
- summary: Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s).

HB (before CMD-FORMAT):
- scripts/vscode/Invoke-MSTestWithCoverage.ps1: d71b98ec641781702605e6806ca6527726539620
- scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1: 2b00d151502d979c52c5e3c11567ebc87d484c14
- scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1: 606ae6c881c7e634397f6b5f8635ea997874f29b

Porcelain before (verbatim):
```
 A scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 A tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

HA (after CMD-FORMAT):
- scripts/vscode/Invoke-MSTestWithCoverage.ps1: d71b98ec641781702605e6806ca6527726539620
- scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1: 2b00d151502d979c52c5e3c11567ebc87d484c14
- scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1: 606ae6c881c7e634397f6b5f8635ea997874f29b

Porcelain after (verbatim):
```
 A scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 A tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

FORMAT-REWROTE: none
FORMAT-OUT-OF-SCOPE-RESTORED: none (no porcelain line named a path outside the Write Set)

Output Summary:
- The MCP formatter returned ok true and rewrote none of the four Write Set PowerShell files: every HB entry equals its HA entry.
- Invoke-MSTest.ps1 was not rewritten, so it remains absent from the diff (D9: recorded, not repaired).
- Every porcelain line after the run names a Write Set path; no out-of-scope path was listed, so no restore was needed.
