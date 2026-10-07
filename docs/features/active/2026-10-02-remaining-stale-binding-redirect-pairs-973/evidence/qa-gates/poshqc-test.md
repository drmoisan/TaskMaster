# P4-T3 PowerShell test (final PowerShell pass, iteration 1)

Timestamp: 2026-10-06T18-22
Command: CMD-JUNIT-DELETE (pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue'; exit 0, recorded not gated), then MCP mcp__drm-copilot__run_poshqc_test workspace_root=<execution-worktree-root> scan_folders=["tests/scripts/dependencies"], then CMD-JUNIT-READ over <execution-worktree-root>/artifacts/pester/pester-junit.xml
EXIT_CODE: 0
Output Summary: ok true with the 1-folder summary literal; JUnit root tests=153 failures=0 errors=0 disabled=0; nine suites, all failures 0 and skipped 0; BindingRedirectVerification.Tests.ps1 tests=16 failures=0 skipped=0; no Failed test. Ran directly after P4-T2 with no file change between steps.

## MCP payload (C3, C4)

    {"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}

## CMD-JUNIT-READ

(a) matched exactly one line (line 2):
JUNIT-ROOT tests=153 failures=0 errors=0 disabled=0

(b)
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=16 failures=0 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0

(c) Grep status="Failed": no match
JUNIT-NOTPASSED: none

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure
CI-COVERAGE-JOB: pester (Run Pester suite with coverage, windows-latest, .github/workflows/_pester.yml) runs tests/scripts/dependencies and measures scripts/dependencies, scripts/hygiene and scripts/vscode together at an 80 percent line floor; the module is unchanged by this item
