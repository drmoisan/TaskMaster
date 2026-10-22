# P4-T11 TRX-derived test-result summary (final scoped runs)

Timestamp: 2026-10-01T22-03
ITERATION: 1
Command: CMD-TRX-SUMMARY (dot-sources scripts\vscode\Invoke-MSTest.TrxSummary.ps1, then Format-TrxRunSummary -Summary (Get-TrxRunSummary -TrxContent ...) for coverage\test-results\956\p4-t5\p4-t5.trx and coverage\test-results\956\p4-t6\p4-t6.trx); one pwsh -NoProfile -Command invocation beginning Set-Location to the item worktree
EXIT_CODE: 0
Output Summary:
SUMMARY-BEGIN p4-t5
Test run outcome: Completed
Total 26, executed 26, passed 26, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END p4-t5
SUMMARY-BEGIN p4-t6
Test run outcome: Completed
Total 7, executed 7, passed 7, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END p4-t6

Acceptance evaluation (P4-T11):
1. The p4-t5 block reads `Total 26, executed 26, passed 26, failed 0.`: HOLDS.
2. The p4-t6 block reads `Total 7, executed 7, passed 7, failed 0.`: HOLDS.
3. Both blocks state `Failed tests: none`: HOLDS.
4. No trx document is copied into FEATURE/: HOLDS (the two trx documents remain under the git-ignored coverage\test-results\956\ tree; this file is a projection of them).
