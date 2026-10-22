# P1-T11 — Tree consistency tests pass after the fix

Timestamp: 2026-09-30T10-17
Command: pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ
EXIT_CODE: 0
Output Summary:
- RUN-START: 2026-09-30T13:29:57.2124290Z
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
- JUNIT-WRITTEN=2026-09-30T13:30:27.3304471Z (later than RUN-START)
- JUNIT-ROOT tests=137 failures=0 errors=0 disabled=0 (131 plus 6)
- Exactly 8 JUNIT-SUITE lines:
  - AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
  - ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
  - DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
  - PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
  - PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
  - ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
  - Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
  - RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
- No JUNIT-NOTPASSED line.

Fail-before / pass-after pair: the fail-before run is recorded at docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md (RepositoryTreeConsistency.Tests.ps1 tests=4 failures=4); this run records the same four tests passing.

GATE-SUBSTITUTION: JUnit per-file counts stand in for a direct Pester run
