# Batch 4 — Tests + Coverage (P4-T4)

Timestamp: 2026-07-19T09-53

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/evidence/regression-testing/batch-4-coverage.cobertura.xml`

EXIT_CODE: 0

## Output Summary

- Total tests: 5702; Passed: 5702; Failed: 0 (no regression on the timed-action classes — AC3)
- Total time: 35.08 s
- Coverage (root Cobertura element): line-rate 0.837942 = 83.79%
  (lines-covered 86577 / lines-valid 103321), branch-rate 0.763759 = 76.38%
  (branches-covered 19540 / branches-valid 25584)
- Baseline 83.79% / 76.36% — unchanged within rounding (annotation-only).

Note: an initial invocation of this step was aborted after ~23 minutes with exit 1 because two
`vstest`/`dotnet-coverage` collection processes ran concurrently in the worktree and collided
(environmental, not a code failure; the annotations are compile-time-only and IL-invariant). All
stale test processes were terminated and the step was re-run cleanly to the passing result above.

Cobertura XML: `evidence/regression-testing/batch-4-coverage.cobertura.xml`.
