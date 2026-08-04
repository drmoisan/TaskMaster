# Coverage wrapper PoshQC format gate

- Timestamp: `2026-07-24T16:50:13.3902652Z`
- Task: `P8-T39`
- Result: `PASS`

## Command

`mcp__drm-copilot__run_poshqc_format workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[scripts/vscode,tests/scripts/vscode]`

## Output Summary

The required bundled PoshQC formatter returned `ok: true` for both selected scan folders.

- PowerShell files inventoried before and after: `12`
- Pre-format manifest SHA-256: `F6F63BD78547C6D329BCA5C8A5B52162DBC0A8B74B22B83CC256A183229209EC`
- Post-format manifest SHA-256: `F6F63BD78547C6D329BCA5C8A5B52162DBC0A8B74B22B83CC256A183229209EC`
- Formatter file delta: `0`
- `git diff --check` exit code for both scan folders: `0`
- Existing changed files under the scan folders remain limited to:
  - `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
  - `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
- `Invoke-MSTestWithCoverage.ps1` physical lines: `312`
- `Invoke-MSTest.RunSettings.Tests.ps1` physical lines: `328`
- Files exceeding 500 physical lines: `0`

The remaining ten inventoried PowerShell files retained their exact pre-format hashes. The two authorized files also retained their exact pre-format hashes, so the formatter introduced no change.

## Exit Code

`EXIT_CODE: 0`
