# Batch 7 — Regression Test Run with Coverage (P7-T4)

Timestamp: 2026-07-19T23-45

## Command

`pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-7-coverage.cobertura.xml`

EXIT_CODE: 0

## Output Summary

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 50.54 s

No regression: the 5702 passing count matches the P0-T6 baseline of 5702 passing. The Batch 7
annotation-only changes to the SmartSerializable family and config controller introduced no test
failures.

Whole-suite coverage headline (from `batch-7-coverage.cobertura.xml` `<coverage>` root):
- line-rate: 0.837875 (83.79%)
- branch-rate: 0.763407 (76.34%)

Cobertura XML written to
`docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-7-coverage.cobertura.xml`.
