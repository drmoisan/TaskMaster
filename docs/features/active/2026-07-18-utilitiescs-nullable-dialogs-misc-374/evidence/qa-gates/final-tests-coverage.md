# Final QC — Full Test Suite with Coverage

- Timestamp: 2026-07-19T12-45
- Task: [P7-T4]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-dialogs-misc-374/evidence/qa-gates/final-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary (numeric post-change)

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Post-change line coverage: 83.82% (line-rate 0.838187; vs baseline 83.80%)
- Post-change branch coverage: 76.38% (branch-rate 0.763759; vs baseline 76.35%)
- Cobertura XML written to `evidence/qa-gates/final-coverage.cobertura.xml`.

## Comparison to Baseline (AC3)

Baseline (P0-T7): 5702 passed / 0 failed. Final: 5702 passed / 0 failed — identical; no test
regression. Post-change line/branch coverage are marginally above baseline (run-to-run denominator
noise; annotation-only edits add no executable lines). The full Final QC loop (CSharpier -> analyzer
build -> pragma gate -> tests) completed without any step changing files or failing, so no restart
was required.

## Concurrency Note

Three earlier attempts aborted with a test-host crash (5701, 522, 0 passed; 0 failures) under
concurrent sibling-agent coverage load on shared vstest/dotnet-coverage tooling; the recorded clean
run was captured on a quiet-machine retry.
