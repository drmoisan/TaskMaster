# Batch A — Regression Test Run with Coverage

- Timestamp: 2026-07-19T11-20
- Task: [P1-T6]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-a-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 56.93 s
- Line coverage: 83.78% (line-rate 0.837829)
- Branch coverage: 76.35% (branch-rate 0.763524)

## Comparison to Baseline

Baseline (P0-T7): 5702 passed / 0 failed; line 83.80% / branch 76.35%. Batch A: 5702 passed /
0 failed; line 83.78% / branch 76.35%. No test regression (AC3): identical pass/fail counts. Line
coverage delta is -0.02 pp, attributable to non-deterministic denominator counting in the coverage
tool across runs (per known dotnet-coverage denominator behavior), not to any lost coverage on
changed lines; the annotation-only Batch A edits add no executable lines. Changed-line coverage is
assessed formally at P7-T5.
