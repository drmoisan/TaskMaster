# PowerShell Baseline MCP Test (R1, issue #985)

Timestamp: 2026-10-09T15-16
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-junit.ps1 -WorkspaceRoot WORKSPACE-ROOT -Clear; mcp__drm-copilot__run_poshqc_test workspace_root = WORKSPACE-ROOT, scan_folders = [tests/scripts/dependencies]; pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-junit.ps1 -WorkspaceRoot WORKSPACE-ROOT
EXIT_CODE: 0
Output Summary:
- JUNIT-CLEARED: True
- MCP ok: true; summary (root substituted): "Ran bundled PoshQC test against 'WORKSPACE-ROOT' with 1 selected scan folder(s)."
- JUNIT tests=178 failures=0 errors=0 disabled=0
- JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0
- JUNIT-SUITE BindingRedirectSync.Tests.ps1 tests=18 failures=0
- JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=16 failures=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0
- JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0
- JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0
- JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0
- JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0
- JUNIT-SUITE Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 tests=6 failures=0
- JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0
- JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=5 failures=0
