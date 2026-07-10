# [P0-T13] Test + Coverage Baseline

Timestamp: 2026-07-10T06:06:59Z
Command: `vstest.console.exe TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll /InIsolation /Settings:coverage.runsettings`
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 106, Passed: 106.

## Baseline TaskVisualization line coverage (denominator #298 must keep >= 80%)

- lines-covered: 1032
- lines-valid: 1209
- **line-rate: 85.36%** (0.85359801488833742)

Measured with `coverage.runsettings` (TaskVisualization.dll only, Cobertura,
honoring `[ExcludeFromCodeCoverageAttribute]`).

Captured on the pre-#298 baseline ref `epic/winforms-testability-refactor-integration`
(`949dddd2`) in worktree `C:\Users\DanMoisan\repos\TaskMaster-wt\winforms-integration`
after `Invoke-Restore.ps1`.

## Interpretation

The 85.36% baseline reflects the #197 **class-level** `[ExcludeFromCodeCoverage]`
annotations still present on the #298 in-scope classes (`EditFilterController`,
`FlagTasks`, `AutoCreateProject`, `AutoAssignContext`, `AutoAssignPeople`), which
remove those classes from the denominator entirely at baseline. #298 removes those
class-level exemptions in favor of testable interface/delegate seams plus narrow
method-level exemptions at genuine Interop/UI boundaries, which **grows** the
measured denominator (previously-exempt logic becomes counted). The final QA
coverage measurement (`evidence/qa-gates/coverage-delta.md`,
`evidence/qa-gates/final-vstest-coverage.md`) reports the post-change number against
that grown denominator and must remain `>= 80%` overall with each new class `>= 90%`.

A single spurious collector message ("No code coverage data available. Profiler was
not initialized.") was emitted by a secondary isolated test host; the Cobertura
attachment contains complete instrumentation data (1209 valid lines), confirming
collection succeeded.
