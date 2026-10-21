# Remediation cycle 1, P0-T7: PowerShell test baseline (unchanged tree)

Timestamp: 2026-10-06T20-29
Command: pwsh -NoProfile -Command 'Remove-Item -LiteralPath "<execution-worktree-root>/artifacts/pester/pester-junit.xml" -ErrorAction SilentlyContinue; "JUNIT-DELETED"'; MCP mcp__drm-copilot__run_poshqc_test workspace_root=<execution-worktree-root> scan_folders=["tests/scripts/dependencies"]; Grep over <execution-worktree-root>/artifacts/pester/pester-junit.xml for `<testsuites `, `<testsuite ` and `status="Failed"` (with -n); git -C <execution-worktree-root> hash-object --no-filters -- scripts/dependencies/BindingRedirectVerification.psm1 tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
EXIT_CODE: 0

CMD-JUNIT-DELETE output: JUNIT-DELETED

Payload (verbatim, worktree root replaced):
{"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}

JUNIT-ROOT tests=153 failures=0 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=16 failures=0 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
JUNIT-NOTPASSED: none

HASH scripts/dependencies/BindingRedirectVerification.psm1 51d6664b281cad0b6c8cd01e78c6bc8491a75862
HASH tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 bd93c01d5b587b8fe67f16f1efa3562b3f89ee7d
MODULE-HASH-UNCHANGED: True (equals evidence/qa-gates/poshqc-format.md line 15)
COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure

Output Summary:
- PoshQC test ok true with the 1-folder summary literal; JUnit root (line 2) 153 tests, 0 failures, 0 errors, 0 disabled.
- Nine suites, totals 13+16+14+17+8+32+18+31+4 = 153; BindingRedirectVerification.Tests.ps1 16 tests, 0 failures, 0 skipped.
- No `status="Failed"` match; the root query matched exactly one line, so JUNIT-NOTPASSED: none is valid.
- No local coverage figure exists: the bundled route does not instrument scripts/dependencies; CI measures it.
- No BASELINE-TEST-RED.
