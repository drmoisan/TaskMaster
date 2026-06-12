# Phase 0 — Baseline Pester (in-scope test directory)

Timestamp: 2026-06-12T18-24

Command: mcp__drm-copilot__run_poshqc_test (workspace_root=c:\Users\DanMoisan\repos\TaskMaster, scan_folders=["tests/scripts/vscode"]); cross-checked with `pwsh -NoProfile -Command "Invoke-Pester -Path 'tests/scripts/vscode' -Output Detailed"`

EXIT_CODE: 1

Output Summary:
Passed: 8, Failed: 1, Skipped: 0, Total: 9.
The single failure is pre-existing and environment-dependent:
`Install-RepoDotNetSdk.Tests.ps1` asserts `global.json` SDK version `8.0.205`,
but the local machine resolves `10.0.200`. This failure is unrelated to the
runner-parity change and is out of scope (no `Install-RepoDotNetSdk.*` file is
touched by this work).

Baseline line-coverage headline for the in-scope changed scripts:
- `Invoke-MSTest.ps1`: 0% — the script currently contains no extracted/testable
  argument-construction function and is not exercised by any baseline Pester test
  (it executes external tooling at top level).
- `Invoke-MSTestWithCoverage.ps1`: 0% — same; the top-level script body is not
  covered by baseline tests (only its Helpers file `ConvertTo-Koverage*` functions
  are covered, which are out of scope for the new argument-seam work).

This 0% baseline for the new argument-construction code paths is the reference
against which the >= 90% new-code coverage target (AC7) is measured in P2-T5.
