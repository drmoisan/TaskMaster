# Phase 0 — Baseline Test Run with Coverage

- Timestamp: 2026-07-19T10-53
- Task: [P0-T7]
- Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-coverage.cobertura.xml`
- EXIT_CODE: 0

## Output Summary (numeric headline)

- Test Run Successful.
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Skipped: 0
- Total time: 36.50 s
- Test assemblies discovered: 8
- Baseline line coverage: 83.80% (line-rate 0.838032; lines-covered 86795 / lines-valid 103570)
- Baseline branch coverage: 76.35% (branch-rate 0.763485; branches-covered 19533 / branches-valid 25584)
- Cobertura XML written to `evidence/baseline/baseline-coverage.cobertura.xml` (post-processed for Koverage compatibility).

## Concurrency Note (environmental, flagged)

A separate agent was concurrently running its own full MSTest+coverage suite in a sibling worktree
(`C:\Users\DanMoisan\repos\TaskMaster-wt\utilitiescs-nullable-outlook-folder-store-365`). Source is
isolated (distinct worktrees), but the two runs share global test tooling (vstest.console,
dotnet-coverage, testhost) and the machine's CPU. Two earlier attempts aborted with
"Test host process crashed" (partial results: 522 and 927 passed, 0 failed) purely from that
resource contention — not from any test failure and not from any code change (this is the baseline;
no source is yet modified). The run above completed cleanly once the sibling agent's test processes
finished and the machine was quiet. Subsequent per-batch and final coverage runs are executed under
the same quiet-machine discipline. The default runsettings uses `<Workers>0</Workers>` (auto = 24
logical processors); this contention sensitivity is a flagged environmental observation, not a code
defect.
