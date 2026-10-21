# P0-T10 PowerShell test baseline (issue #973)

Timestamp: 2026-10-03T10-48
Command: CMD-JUNIT-DELETE (pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue'); MCP mcp__drm-copilot__run_poshqc_test workspace_root <execution-worktree-root> scan_folders ["tests/scripts/dependencies"]; CMD-JUNIT-READ over <execution-worktree-root>/artifacts/pester/pester-junit.xml
EXIT_CODE: 0
Output Summary: test payload ok true; JUnit root tests=151 failures=0; nine suites, BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0; no Failed testcase. No PESTER-BASELINE-RED.

JUNIT-DELETE: exit 1 recorded (no prior document; not gated)

Payload (C3, C4):
{"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}

JUNIT-ROOT tests=151 failures=0 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
JUNIT-NOTPASSED: none (the `<testsuites ` read matched exactly one line, line 2)

BASELINE-SUITE AnalyzerItemRepair.Tests.ps1=13
BASELINE-SUITE BindingRedirectVerification.Tests.ps1=14
BASELINE-SUITE ConsistencyVerifier.Tests.ps1=14
BASELINE-SUITE DependabotConfig.Tests.ps1=17
BASELINE-SUITE PackageCompatibility.Tests.ps1=8
BASELINE-SUITE PackageGraph.Tests.ps1=32
BASELINE-SUITE ProjectConsistency.Tests.ps1=18
BASELINE-SUITE Repair-PackageManifestConsistency.Tests.ps1=31
BASELINE-SUITE RepositoryTreeConsistency.Tests.ps1=4

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure
