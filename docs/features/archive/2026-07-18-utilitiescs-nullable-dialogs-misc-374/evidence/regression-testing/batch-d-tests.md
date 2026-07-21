# Batch D — Regression Test Run with Coverage

- Timestamp: 2026-07-19T12-05
- Task: [P4-T5]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-dialogs-misc-374/evidence/regression-testing/batch-d-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Line coverage: 83.81% (line-rate 0.838071)
- Branch coverage: 76.35% (branch-rate 0.763524)

## Comparison to Baseline

Baseline: 5702 passed / 0 failed. Batch D: identical. No test regression (AC3).

## Concurrency Note

Several earlier attempts aborted with a test-host crash while a sibling-worktree agent ran its own
full coverage suite concurrently (shared vstest/dotnet-coverage/testhost tooling). The recorded
clean run was captured after polling for a quiet-machine window. This is environmental, not a code
defect; MyBox.cs changes are annotation-only.
