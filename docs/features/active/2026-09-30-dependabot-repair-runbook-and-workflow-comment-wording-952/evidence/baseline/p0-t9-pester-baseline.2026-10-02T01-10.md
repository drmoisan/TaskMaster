# P0-T9 Pester Static-Test Baseline

Timestamp: 2026-10-02T01-10
Command: CMD-POSHQC-TEST: mcp__drm-copilot__run_poshqc_test with workspace_root WORKTREE and scan_folders tests/scripts/dependencies/DependabotConfig.Tests.ps1 and tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1. CMD-PESTER (raw Invoke-Pester) was NOT run: the user directive prohibits raw Invoke-Pester, and the coordinator ruling substitutes the PoshQC MCP pass plus the Pester counts of the CI Pester job on the pull request head, to be read after the push.
EXIT_CODE: 0 (scoped to the CMD-POSHQC-TEST call; ok is true, so the call passed; the tool exposes no process exit code or test counts)
Output Summary:
PASSED=DEFERRED-TO-CI (CMD-PESTER substituted by the PoshQC MCP pass per the user directive)
TOTAL=DEFERRED-TO-CI (CMD-PESTER substituted by the PoshQC MCP pass per the user directive; expected 21 per plan fact 10)
BASELINE-FAILED-SET: DEFERRED-TO-CI (CMD-PESTER substituted by the PoshQC MCP pass per the user directive)
POSHQC-TEST-OK: true
POSHQC-TEST-PAYLOAD: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 2 selected scan folder(s)."}
SUBSTITUTION: CMD-PESTER replaced by CMD-POSHQC-TEST (ok is the gate and fails when the suite fails) plus the CI Pester job counts on the pull request head, read after the push (coordinator ruling). The PESTER RAN NOTHING stop does not apply because no raw run occurs.
