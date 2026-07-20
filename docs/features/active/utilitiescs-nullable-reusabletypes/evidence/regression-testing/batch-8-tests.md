# Batch 8 — Regression Test Run with Coverage (P8-T4)

Timestamp: 2026-07-19T24-35

## Command

`pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-8-coverage.cobertura.xml`
(run with `MSYS_NO_PATHCONV=1`; pwsh 7).

EXIT_CODE: 0

## Output Summary

- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: ~32.9 s
- Coverage artifact written:
  `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-8-coverage.cobertura.xml`
  (post-processed for Koverage compatibility).

Result: no test regression (AC3). The passing total matches the baseline (5702). The tree under test
carries all Batch 8 annotation-only changes plus the required `where TKey : notnull` on
`ScoDictionaryNew`; the `ScDictionary` constraint is reverted pending the epic waiver-extension
decision recorded in `evidence/qa-gates/batch-8-nullable-gate.md` (third-file CS8714 cascade into
`UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs`). The serializable-wrapper classes exercised by
the suite show no behavior change from the annotations.
