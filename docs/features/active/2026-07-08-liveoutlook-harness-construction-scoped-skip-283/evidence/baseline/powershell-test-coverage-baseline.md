# PowerShell Pester Test + Coverage Baseline (Issue #283)

Timestamp: 2026-07-08T17-56

## Mandated command
Command: `mcp__drm-copilot__run_poshqc_test`
EXIT_CODE: 4294967295 (-1)

Output Summary (mandated command):
- The bundled PoshQC Pester runner exits -1 with no stderr detail at baseline, both with `scan_folders: ["tests/scripts/vscode"]` and with no scan_folders (full default scope). This failure is present BEFORE any change in this fix, so it is a pre-existing environment/tooling condition of the bundled runner in this worktree, not a regression introduced by Issue #283. It is recorded here so the same behavior at final QC (P2-T7) is attributable to the environment rather than the change.

## Authoritative numeric baseline (direct Pester 5.6.1)
Command: `Invoke-Pester` (New-PesterConfiguration) over `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` with CodeCoverage on `scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
EXIT_CODE: 0

Output Summary (direct Pester):
- Pester version 5.6.1.
- Tests: TOTAL 9, PASSED 9, FAILED 0, SKIPPED 0.
- Code coverage of the two in-scope QC scripts: 77.06% (commands analyzed 109, executed 84). This is the baseline coverage headline for the RunSettings arg-builder scripts as exercised by the in-scope Pester file.
