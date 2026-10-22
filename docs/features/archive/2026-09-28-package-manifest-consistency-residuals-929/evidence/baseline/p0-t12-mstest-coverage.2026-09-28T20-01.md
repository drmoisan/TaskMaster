# P0-T12 — C# test and coverage baseline (CMD-MSTEST-COVERAGE)

Timestamp: 2026-09-30T09-40
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .' (run detached with its combined output redirected to a session scratch log outside the repository, because the run exceeds a single tool-call window; the script and its arguments are unchanged)
EXIT_CODE: 0
Output Summary:
- "Test Run Successful."
- "Total tests: 7346" / "Passed: 7346"
- First-party coverage: lines 56475/65736 (85.91%), branches 13656/17054 (80.08%)
- Baseline line percentage L = 85.91; baseline branch percentage R = 80.08 (L at least 80 and R at least 75; the runner enforces both floors before printing the line)
- MEETS-85: true (observation only, convention 10)
- Test-result summary (copied): total 7346, executed 7346, passed 7346, failed 0; skipped 0 (derived); error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0; failed tests: none
- Path-printing lines, with host prefixes replaced (convention 4):
  - "Using vstest.console: <program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
  - "Coverage output: <execution-worktree-root>\coverage\coverage.cobertura.xml"
  - "Coverage projection: <execution-worktree-root>\coverage\coverage.cobertura.jacoco.xml"
  - "Test-result summary: <execution-worktree-root>\coverage\test-results\mstest-coverage-run.summary.txt"
  - "Done. Coverage artifact: <execution-worktree-root>\coverage\coverage.cobertura.xml"

Copies:
- docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t12-coverage-projection.2026-09-28T20-01.jacoco.xml — 1467 bytes; root element report; 9 package elements
- docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t12-test-results.2026-09-28T20-01.summary.txt — 298 bytes

Reconciliation: the sum of the projection's package LINE covered counters is 56475, equal to the line numerator A = 56475 on the printed line, so the copy describes this run.

PROJECTION-RAW-ELEMENTS: 0 (Select-String over the copy for "<class", "<sourcefile", "<method" and "<line" returns 0 lines; the CI hygiene guard's rule A classifies the copy as jacoco-projection)

The raw Cobertura document and the raw trx document stay under the ignored coverage directory and are not copied.
