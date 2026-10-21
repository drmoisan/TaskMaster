# C# Test Gate with Coverage (P5-T5)

Timestamp: 2026-10-09T14-31
Command: pwsh -NoProfile -File CMDDIR\985-mstest.ps1 -WorkspaceRoot WORKSPACE-ROOT -EvidencePrefix WORKSPACE-ROOT\FEATURE\evidence\qa-gates\mstest-coverage-final.2026-10-09T14-31
EXIT_CODE: 0
ITERATION: 1
Output Summary:
- SHELL-ICON-EXCLUSION: APPLIED ShellUtilities_Tests ShellUtilitiesStatic_Tests SysImageListHelperTests OSBrowser_Tests (route D3; CI runs the full set)
- Discovered 9 test assemblies.
- First-party coverage: lines 56495/66143 (85.41%), branches 13709/17173 (79.83%)
- RUNNER_RESULT=COMPLETED (runner floors line 80 and branch 75 enforced inside the runner; C7 flaky rule did not apply)
- TRX-SUMMARY: Test run outcome: Completed
- TRX-SUMMARY: Total 7427, executed 7427, passed 7427, failed 0.
- TRX-SUMMARY: Skipped 0, derived as total minus executed rather than reported by the test platform.
- TRX-SUMMARY: Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
- TRX-SUMMARY: Failed tests: none
- PROJECTION-COPIED; copies present: mstest-coverage-final.2026-10-09T14-31.summary.txt and mstest-coverage-final.2026-10-09T14-31.jacoco.xml (beside this file)
- FINAL-CS-LINE: 85.41
- FINAL-CS-BRANCH: 79.83
- Result: PASS.
