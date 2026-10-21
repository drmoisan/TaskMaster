# P2-T7 — C# QC step 4, MSTest with coverage (iteration 2)

Timestamp: 2026-09-30T11-06
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .' (run detached with its combined output redirected to a session scratch log outside the repository; the script and its arguments are unchanged)
EXIT_CODE: 0
Output Summary:
- "Test Run Successful."
- "Total tests: 7346" / "Passed: 7346"
- First-party coverage: lines 56479/65736 (85.92%), branches 13657/17054 (80.08%)
- Line percentage 85.92; branch percentage 80.08
- MEETS-85: true (observation only, convention 10)
- Test-result summary (copied): total 7346, executed 7346, passed 7346, failed 0; skipped 0 (derived); error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0; failed tests: none
- Path-printing lines, with host prefixes replaced (convention 4):
  - "Using vstest.console: <program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
  - "Coverage output: <execution-worktree-root>\coverage\coverage.cobertura.xml"
  - "Coverage projection: <execution-worktree-root>\coverage\coverage.cobertura.jacoco.xml"
  - "Test-result summary: <execution-worktree-root>\coverage\test-results\mstest-coverage-run.summary.txt"
  - "Done. Coverage artifact: <execution-worktree-root>\coverage\coverage.cobertura.xml"

Copies:
- docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-coverage-projection.2026-09-28T20-01.jacoco.xml — 1467 bytes; root element report; 9 package elements; PROJECTION-RAW-ELEMENTS: 0
- docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt — 298 bytes

Reconciliation: the projection's package LINE covered sum is 56479, equal to the line numerator on the printed line.

The raw Cobertura and trx documents stay under the ignored coverage directory and are not copied.

Iteration note: iteration 1 of this step failed on one timing-dependent test (qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md); this iteration ran the same command on the same tree with no file changed between the two runs.
