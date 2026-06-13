# Phase 0 — Baseline Pester (fail-before) Evidence (Issue #194)

Timestamp: 2026-06-13T11-23

Command: mcp__drm-copilot__run_poshqc_test (scan_folders: tests/scripts/vscode); detail captured via `Invoke-Pester -Path tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1 -Output Detailed`
EXIT_CODE: 1

Output Summary:
- Pester v5.6.1. Discovery found 2 tests.
- [+] Get-RepoDotNetSdkDownloadUrl: returns the deterministic .NET 8 SDK archive URL — PASS
- [-] global.json SDK selection: pins the repository to the repo-local .NET 8 SDK path — FAIL
  - Assertion: `$globalJson.sdk.version | Should -Be '8.0.205'` at Install-RepoDotNetSdk.Tests.ps1:22
  - Expected: '8.0.205'  But was: '10.0.200'
- Counts: Passed 1, Failed 1, Skipped 0.
- This is the expected fail-before state for AC2 (version assertion fails against regressed 10.0.200). The MCP run_poshqc_test gate returned non-zero (code 1), consistent with the failing assertion.

EXPECT_FAIL: This task is tagged [expect-fail]; a failing `global.json SDK selection` test is the intended baseline outcome prior to the global.json revert.
