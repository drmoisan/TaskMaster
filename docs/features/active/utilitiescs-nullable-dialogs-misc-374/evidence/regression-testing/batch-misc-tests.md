# Misc Batch — Regression Test Run with Coverage

- Timestamp: 2026-07-19T12-25
- Task: [P6-T4]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-misc-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Line coverage: 83.81% (line-rate 0.838071)
- Branch coverage: 76.36% (branch-rate 0.763563)

## Comparison to Baseline

Baseline: 5702 passed / 0 failed. Misc batch: identical. No test regression (AC3). Two earlier
attempts aborted with a test-host crash (2335 and 5701 passed, 0 failed) under concurrent
sibling-agent load; the recorded clean run was captured on a quiet-machine retry.
