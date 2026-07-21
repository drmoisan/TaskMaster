# Batch 6 — Tests + Coverage (P6-T5)

Timestamp: 2026-07-19T19-37

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-6-coverage.cobertura.xml`

EXIT_CODE: 0

## Output Summary

- Total tests: 5702; Passed: 5702; Failed: 0 (no regression on the concurrent-observable bases — AC3)
- Total time: 34.21 s
- Coverage (root Cobertura element): line-rate 0.837866 = 83.79%, branch-rate 0.763407 = 76.34%
- Baseline 83.79% / 76.36% — unchanged within rounding (annotation-only + additive `where TKey : notnull`).

Cobertura XML: `evidence/regression-testing/batch-6-coverage.cobertura.xml`.
