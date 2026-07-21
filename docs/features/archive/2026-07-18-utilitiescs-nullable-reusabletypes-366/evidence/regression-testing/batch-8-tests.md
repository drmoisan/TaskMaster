# Batch 8 — Regression Test Run with Coverage (P8-T4)

Timestamp: 2026-07-19T22-03

Supersedes the prior 2026-07-19T24-35 record (which reflected the reverted-constraint STOP
state). This record captures the run AFTER the three deferred constraint lines were applied
under the ratified four-file cross-child waiver (Option A'').

## Command

`pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/evidence/regression-testing/batch-8-coverage.cobertura.xml`
(pwsh 7).

EXIT_CODE: 0

## Output Summary

- Total tests: 5702
- Passed: 5702
- Failed: 0
- Skipped: 0
- Total time: ~32.5 s
- Overall line-rate: 0.837943 (83.79%)
- Overall branch-rate: 0.763446 (76.34%)
- Cobertura XML: `evidence/regression-testing/batch-8-coverage.cobertura.xml`
  (post-processed for Koverage compatibility).

Result: no test regression (AC3). The passing total matches the baseline (5702). The tree under
test carries all Batch 8 annotation-only changes plus the ratified `where TKey : notnull` on the
three truly generic bases (`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScDictionary`)
and the four cross-child NewtonsoftHelpers waiver consumers. The constraint is IL-metadata-only;
`ConcurrentDictionary` already rejects null keys at runtime, so no runtime behavior changed
(AC3/AC5). The serializable-wrapper classes exercised by the suite show no behavior change.
