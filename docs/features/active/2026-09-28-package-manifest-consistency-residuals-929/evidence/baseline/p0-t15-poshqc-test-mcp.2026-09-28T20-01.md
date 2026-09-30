# P0-T15 — PowerShell test baseline through the MCP route (CMD-POSHQC-TEST, CMD-JUNIT-READ)

Timestamp: 2026-09-30T09-45
Command: pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ; the PESTER-VERSION read command, appended to the same pwsh invocation as CMD-JUNIT-READ
EXIT_CODE: 0
Output Summary:
- RUN-START: 2026-09-30T13:17:15.3091663Z
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
- JUNIT-WRITTEN=2026-09-30T13:17:45.2888350Z (later than RUN-START; the document was written by this run)
- JUNIT-ROOT tests=131 failures=0 errors=0 disabled=0
- JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=12 failures=0 skipped=0
- JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
- JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
- JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
- JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
- JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
- Exactly 7 JUNIT-SUITE lines; no JUNIT-NOTPASSED line.
- PESTER-VERSION: 5.6.1

This route reports no coverage figure; P0-T16 reads the coverage baseline from CI.

GATE-SUBSTITUTION: JUnit per-file counts stand in for a direct Pester run
