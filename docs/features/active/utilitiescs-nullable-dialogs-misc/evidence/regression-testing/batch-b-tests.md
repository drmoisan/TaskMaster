# Batch B — Regression Test Run with Coverage

- Timestamp: 2026-07-19T11-35
- Task: [P2-T6]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-b-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 34.33 s
- Line coverage: 83.81% (line-rate 0.838071)
- Branch coverage: 76.36% (branch-rate 0.763602)

## Comparison to Baseline

Baseline: 5702 passed / 0 failed; line 83.80% / branch 76.35%. Batch B: identical pass/fail counts;
line/branch within run-to-run denominator noise. No test regression (AC3). Annotation-only edits add
no executable lines.
