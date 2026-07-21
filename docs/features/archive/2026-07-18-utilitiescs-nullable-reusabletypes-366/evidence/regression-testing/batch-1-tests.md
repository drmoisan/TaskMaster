# Batch 1 — Tests + Coverage (P1-T4)

Timestamp: 2026-07-19T09-14

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/evidence/regression-testing/batch-1-coverage.cobertura.xml`

EXIT_CODE: 0

## Output Summary

- Total tests: 5702; Passed: 5702; Failed: 0 (no regression — AC3)
- Total time: 56.80 s
- Coverage (whole run): line-rate 0.837913 = 83.79%, branch-rate 0.763641 = 76.36%
- Baseline was 83.79% line / 76.36% branch — unchanged within rounding, as expected for
  annotation-only changes (nullable annotations are compile-time metadata, IL-invariant).

Cobertura XML: `evidence/regression-testing/batch-1-coverage.cobertura.xml`.
