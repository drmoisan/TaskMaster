# Phase 2 — Final QA Pester (pass-after) (Issue #194)

Timestamp: 2026-06-13T11-32

Command: mcp__drm-copilot__run_poshqc_test (scan_folders: tests/scripts/vscode); detail captured via `Invoke-Pester -Path tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1 -Output Detailed`
EXIT_CODE: 0

Output Summary:
- MCP gate result: ok=true (PoshQC test gate passed).
- Pester v5.6.1. Discovery found 2 tests.
- [+] Get-RepoDotNetSdkDownloadUrl: returns the deterministic .NET 8 SDK archive URL — PASS
- [+] global.json SDK selection: pins the repository to the repo-local .NET 8 SDK path — PASS
  - version 8.0.205, rollForward latestFeature, allowPrerelease false, paths contains '.dotnet-sdk' and '$host$' — all assertions pass.
- Counts: Passed 2, Failed 0, Skipped 0.
- Pass-after evidence for AC2 confirmed: the previously failing version assertion (expected 8.0.205, was 10.0.200) now passes after the global.json revert.

## Coverage Headline
- PoshQC coverage report: artifacts/pester/powershell-coverage.xml (JaCoCo) and powershell-coverage.koverage.xml.
- The coverage instrument scoped its measurement to `.claude/hooks` scripts (LINE: 0 covered / 284 total = 0.0%). These hook scripts are NOT the regression suite under test and are NOT changed/related code for this issue.
- The change in this issue is a single field in global.json (a JSON config file). There is no PowerShell production-code change; therefore no PowerShell line coverage can regress on changed lines (zero PowerShell lines changed).
- The reported 0% is an artifact of PoshQC's default global coverage scope picking up unexercised hook scripts, not a coverage regression caused by this change. The regression test asserting the config value passes.

## Notes on referenced config path
- The plan references `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`. This path does not exist in the repository working tree; PoshQC is bundled inside the drm-copilot MCP server and uses its own internal Pester/coverage configuration. The MCP `run_poshqc_test` gate was used as the authoritative test runner (ok=true), and the suite result was reconfirmed with a direct Invoke-Pester run for assertion/count detail.
