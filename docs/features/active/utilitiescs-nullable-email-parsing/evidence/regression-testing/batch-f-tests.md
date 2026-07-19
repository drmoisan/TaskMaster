# Batch F Regression Test Run with Coverage

Timestamp: 2026-07-19T05-55

## Environment note: pre-existing full-suite parallelism flakiness (not a regression)

As with prior batches, `scripts/vscode/TaskMaster.cli.runsettings` was temporarily edited to
`Workers: 4` to obtain a deterministic coverage-instrumented pass (see
`project_utilitiescs_test_parallelism_flakiness.md`), then restored to its original content.
`git diff` and MD5 checksum `214be06fbfaf1aee387da41e907f4fb4` confirm zero net change before
and after.

Command (as actually run, after the temporary `Workers: 4` edit): `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-f-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 40.1057 seconds
- Overall line-coverage: 83.8051% (baseline: 83.7834%)
- Overall branch-coverage: 76.3641% (baseline: 76.3407%)

No test regression: pass/fail counts identical to the Phase 0 baseline (5702/5702 passed),
including `EmailDataMiner_Tests.cs`, `EmailDataMiner_Additional_Tests.cs`,
`EmailDataMiner_FolderExtractionCoverage_Tests.cs`, and `EmailDataMiner_TestSupport.cs`.
Overall coverage percentages remain at or above baseline.
