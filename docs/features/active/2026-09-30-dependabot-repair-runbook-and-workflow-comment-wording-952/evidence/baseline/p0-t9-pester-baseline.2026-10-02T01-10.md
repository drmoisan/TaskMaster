# P0-T9 Pester Static-Test Baseline (PARTIAL: CMD-PESTER substituted, acceptance not fully observable)

Timestamp: 2026-10-02T01-10
Command: CMD-POSHQC-TEST only: mcp__drm-copilot__run_poshqc_test with workspace_root WORKTREE and scan_folders tests/scripts/dependencies/DependabotConfig.Tests.ps1 and tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1. CMD-PESTER (raw Invoke-Pester) was NOT run: the delegation directive prohibits raw Invoke-Pester and substitutes the PoshQC pass plus the CI Pester job on the pull request head.
EXIT_CODE: 0 (scoped to the CMD-POSHQC-TEST call; the tool reports ok true and exposes no process exit code or test counts)
ExpectedExitCode: 0
Output Summary:
PASSED=NOT OBSERVED (CMD-PESTER substituted; the PoshQC payload carries no counts)
BASELINE-FAILED-SET: NOT OBSERVED (CMD-PESTER substituted)
TOTAL=NOT OBSERVED (expected 21 per plan fact 10)
POSHQC-TEST-OK: true
POSHQC-TEST-PAYLOAD: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 2 selected scan folder(s)."}
