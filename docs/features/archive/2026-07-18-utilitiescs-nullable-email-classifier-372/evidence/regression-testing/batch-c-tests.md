# Batch C — Test Run with Coverage

Timestamp: 2026-07-19T02-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/evidence/regression-testing/batch-c-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702. Passed: 5702. Failed: 0. Test Run Successful.
- Line coverage: 83.79% (line-rate 0.837939) vs baseline 0.837795 — no regression.
- Branch coverage: 76.36% (branch-rate 0.763641) vs baseline 0.763329 — no regression.
- The golden/property/characterization scoring suites and the subclass test doubles (SubBayesianClassifier/SubClassifierGroup/SubCorpus) pass unchanged; no changed-line coverage regression versus baseline (AC3, AC4).
