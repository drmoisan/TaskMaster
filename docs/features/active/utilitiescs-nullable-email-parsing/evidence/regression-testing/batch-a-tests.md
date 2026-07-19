# Batch A Regression Test Run with Coverage

Timestamp: 2026-07-19T01-50

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-a-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 56.4288 seconds
- Overall line-coverage: 83.7892% (baseline: 83.7834%)
- Overall branch-coverage: 76.3720% (baseline: 76.3407%)

No test regression: pass/fail counts identical to the Phase 0 baseline (5702/5702 passed).
Overall coverage percentage did not regress (both line and branch coverage are marginally
higher than baseline, consistent with the annotation-only nature of this batch introducing no
new uncovered executable lines).
