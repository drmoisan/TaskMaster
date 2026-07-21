# Batch A — Test Run with Coverage

Timestamp: 2026-07-19T01-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-a-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702. Passed: 5702. Failed: 0. Test Run Successful.
- Line coverage: 83.78% (line-rate 0.837844) vs baseline 0.837795 — no regression (slightly higher).
- Branch coverage: 76.34% (branch-rate 0.763407) vs baseline 0.763329 — no regression.
- No test regression on the Batch A pure-data/contract-leaf classes; changed-line coverage does not regress versus the P0-T7 baseline (AC3, AC4). The `Prediction<T>` annotation change is annotation-only and does not add executable lines.
