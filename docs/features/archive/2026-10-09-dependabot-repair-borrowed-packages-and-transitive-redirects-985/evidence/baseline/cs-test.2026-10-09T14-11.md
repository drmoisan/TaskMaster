# C# Baseline Tests with Coverage (P0-T13)

Timestamp: 2026-10-09T14-11
Command: pwsh -NoProfile -File CMDDIR\985-mstest.ps1 -WorkspaceRoot WORKSPACE-ROOT -EvidencePrefix WORKSPACE-ROOT\FEATURE\evidence\baseline\mstest-coverage-baseline.2026-10-09T14-11
EXIT_CODE: 0
Output Summary:
- SHELL-ICON-EXCLUSION: APPLIED ShellUtilities_Tests ShellUtilitiesStatic_Tests SysImageListHelperTests OSBrowser_Tests (route D3; CI runs the full set)
- Discovered 9 test assemblies.
- First-party coverage: lines 56496/66143 (85.41%), branches 13709/17173 (79.83%)
- RUNNER_RESULT=COMPLETED (C7 flaky rule did not apply)
- TRX-SUMMARY: Test run outcome: Completed
- TRX-SUMMARY: Total 7427, executed 7427, passed 7427, failed 0.
- TRX-SUMMARY: Skipped 0, derived as total minus executed rather than reported by the test platform.
- TRX-SUMMARY: Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
- TRX-SUMMARY: Failed tests: none
- PROJECTION-COPIED; copies present: mstest-coverage-baseline.2026-10-09T14-11.summary.txt and mstest-coverage-baseline.2026-10-09T14-11.jacoco.xml (beside this file)
- BASELINE-CS-LINE: 85.41
- BASELINE-CS-BRANCH: 79.83
- Result: baseline green; no BASELINE-TEST-RED.
