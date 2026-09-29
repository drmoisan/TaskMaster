# P0-T5 Format Baseline with Liveness Control

Timestamp: 2026-09-29T09-01
Task: P0-T5
Command: (1) git status --porcelain -uall -- scripts/vscode tests/scripts/vscode; (2) git hash-object --no-filters -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTest.ps1; (3) mcp__drm-copilot__run_poshqc_format with workspace_root = <repo-root> and scan_folders = ["scripts/vscode", "tests/scripts/vscode"]; (4) repeat (1) and (2); (5) liveness control: Edit tool prepends four spaces to line 272 of scripts/vscode/Invoke-MSTestWithCoverage.ps1, git diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1, CMD-FORMAT again, Grep count mode over the entry point, git checkout -- scripts/vscode/Invoke-MSTestWithCoverage.ps1, repeat (1) and (2)
EXIT_CODE: 0

## Resume note

This is a fresh run of P0-T5 from step (1). A previous executor attempt stopped at step (5) when the pre-implementation gate denied the Edit; no P0-T5 artifact from that attempt existed under evidence/baseline. Before step (1), line 272 of the entry point read `$ErrorActionPreference = 'Stop'` with no leading whitespace and the porcelain over the two folders was empty, so the prior attempt left no indentation in the file and no restore was required.

## Step results

- Step (1) porcelain-before over scripts/vscode and tests/scripts/vscode: (empty)
- Step (2) H0:
  - scripts/vscode/Invoke-MSTestWithCoverage.ps1: d10e8f6c9db66305ff0e071ee11d872de37e5035
  - scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5
- Step (3) CMD-FORMAT payload: {"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (first CMD-FORMAT call)
- Step (4) porcelain-after over the two folders: (empty)
- Step (4) H1:
  - scripts/vscode/Invoke-MSTestWithCoverage.ps1: d10e8f6c9db66305ff0e071ee11d872de37e5035 (equals H0)
  - scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5 (equals H0)
- FORMAT-DRIFT-SET: (empty)
- FORMAT-DRIFT-RESTORED: not applicable (drift set empty)

## Liveness control (step 5)

- Edit: line 272 changed from `$ErrorActionPreference = 'Stop'` to four spaces followed by `$ErrorActionPreference = 'Stop'`.
- CONTROL_NUMSTAT_BEFORE: `1	1	scripts/vscode/Invoke-MSTestWithCoverage.ps1` (added 1, deleted 1)
- Second CMD-FORMAT payload: {"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
- MCP_RESULT_OK_FLAG: true (second CMD-FORMAT call)
- CONTROL_LINE_AFTER: indented form `^    \$ErrorActionPreference = 'Stop'` count 0; unindented form `^\$ErrorActionPreference = 'Stop'` count 1 (the formatter removed the indentation it was given)
- Restore: git checkout -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 (exit 0, no output). No FORMAT-DRIFT-SET paths to restore.
- Final porcelain over scripts/vscode and tests/scripts/vscode: (empty)
- Final hashes:
  - scripts/vscode/Invoke-MSTestWithCoverage.ps1: d10e8f6c9db66305ff0e071ee11d872de37e5035 (equals H0)
  - scripts/vscode/Invoke-MSTest.ps1: 9aec072f5255beeb7ff687291221dad4cc72fdd5 (equals H0)

Output Summary: PASS. The MCP PoshQC formatter rewrote nothing on the base tree over scripts/vscode and tests/scripts/vscode (FORMAT-DRIFT-SET empty; H1 equals H0 for both scripts). The liveness control showed the formatter is live on this tree: an injected four-space indent on entry-point line 272 (numstat 1/1) was removed by CMD-FORMAT (CONTROL_LINE_AFTER indented 0, unindented 1). Both CMD-FORMAT calls returned ok true. After restore, the porcelain over the two folders is empty and both hashes equal H0.
