# PowerShell QA MCP Test (R1, issue #985)

ITERATION: 1
Timestamp: 2026-10-09T15-27
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-junit.ps1 -WorkspaceRoot WORKSPACE-ROOT -Clear; mcp__drm-copilot__run_poshqc_test workspace_root = WORKSPACE-ROOT, scan_folders = [tests/scripts/dependencies]; pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-junit.ps1 -WorkspaceRoot WORKSPACE-ROOT
EXIT_CODE: 0
Output Summary:
- JUNIT-CLEARED: True
- MCP ok: true; summary (root substituted): "Ran bundled PoshQC test against 'WORKSPACE-ROOT' with 1 selected scan folder(s)."
- JUNIT tests=187 failures=0 errors=0 disabled=0 (baseline 178; +6 module tests, +3 entry-point tests)
- JUNIT-SUITE BindingRedirectSync.Tests.ps1 tests=24 failures=0
- JUNIT-SUITE Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 tests=9 failures=0
- Other suites unchanged from baseline: AnalyzerItemRepair 13, BindingRedirectVerification 16, ConsistencyVerifier 14, DependabotConfig 17, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31, RepositoryTreeConsistency 5; all failures=0.
