# Batch C Regression Test Run with Coverage

Timestamp: 2026-07-19T03-45

## Environment note: pre-existing full-suite parallelism flakiness (not a regression)

The standard command `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput
docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-c-coverage.cobertura.xml`
crashed non-deterministically on 3 consecutive attempts (`EXIT_CODE` -1/127, no test failures —
the process terminated mid-run at a different, unrelated test each time: once in
`TaskVisualization.Test`, once in a COM-reflection-helper test). This matches a documented
pre-existing environment condition (agent memory
`project_utilitiescs_test_parallelism_flakiness.md`): the full 5702-test suite under the
tracked `scripts/vscode/TaskMaster.cli.runsettings` default `Workers: 0` (processor-count
parallelism, observed as 24 workers) combined with `dotnet-coverage` instrumentation is
flaky in this environment; a diagnostic no-coverage `vstest.console.exe` run at the same
default parallelism also crashed (mid-run, no failures), while an identical run with
`Workers: 4` passed all 5702 tests cleanly (`EXIT_CODE: 0`). This crash is unrelated to this
batch's code changes — none of the crashing runs reported a single test **failure**, only a
mid-run process termination, and the crash points differed between runs (nondeterministic).

To obtain a deterministic coverage-instrumented artifact, `scripts/vscode/TaskMaster.cli.runsettings`
was temporarily edited to `Workers: 4` (from `Workers: 0`), the coverage script was run once
successfully, and the file was then restored to its original content. `git diff` and an MD5
checksum (`214be06fbfaf1aee387da41e907f4fb4`, both before and after) confirm zero net change to
the tracked file — no unrelated file was left modified by this diagnostic step (AC6 preserved).

Command (as actually run, after the temporary `Workers: 4` edit): `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-c-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 38.5512 seconds
- Overall line-coverage: 83.8066% (baseline: 83.7834%)
- Overall branch-coverage: 76.3641% (baseline: 76.3407%)

No test regression: pass/fail counts identical to the Phase 0 baseline (5702/5702 passed).
Overall coverage percentages remain at or above baseline.
