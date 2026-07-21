# Batch D Regression Test Run with Coverage

Timestamp: 2026-07-19T04-25

## Environment note: pre-existing full-suite parallelism flakiness (not a regression)

As with Batch C (see `batch-c-tests.md`), the standard coverage command crashed
non-deterministically at default parallelism (`EXIT_CODE: 127`, no test failures, terminated
mid-run in `QuickFiler.Test`/`TaskVisualization.Test`). This is the same documented pre-existing
environment condition (agent memory `project_utilitiescs_test_parallelism_flakiness.md`),
unrelated to this batch's code changes. `scripts/vscode/TaskMaster.cli.runsettings` was
temporarily edited to `Workers: 4`, the coverage script was run once successfully, and the file
was restored to its original content (`git diff` and MD5 checksum
`214be06fbfaf1aee387da41e907f4fb4` confirm zero net change before and after).

Command (as actually run, after the temporary `Workers: 4` edit): `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/evidence/regression-testing/batch-d-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 38.4861 seconds
- Overall line-coverage: 83.8080% (baseline: 83.7834%)
- Overall branch-coverage: 76.3641% (baseline: 76.3407%)

No test regression: pass/fail counts identical to the Phase 0 baseline (5702/5702 passed),
including the two `EmailFiler_Tests.cs` duplicate-named test files. Overall coverage percentages
remain at or above baseline.
