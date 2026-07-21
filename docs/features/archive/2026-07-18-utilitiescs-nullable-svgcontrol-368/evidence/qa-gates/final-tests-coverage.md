# Final QC — Test Run With Coverage

Timestamp: 2026-07-19T04-35

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-coverage.cobertura.xml`
(preceded by rebuilding `SVGControl.Test/SVGControl.Test.csproj`, since the prior `TaskMaster.sln
/t:Rebuild` (P6-T3) cleaned `SVGControl`'s `bin`/`obj` outputs)

EXIT_CODE: 0

Output Summary: Total tests: 37. Passed: 37. Failed: 0. No test regression (AC3).

Post-change numeric coverage headline (`SVGControl` package, the only package instrumented by
this test project): line-rate `0.266381` (26.64%), branch-rate `0.322807` (32.28%);
`lines-covered="870"`, `lines-valid="3266"`, `branches-covered="368"`, `branches-valid="1140"`.

`RelativePath.cs` class-level coverage (the one file in scope with a real automated baseline):
line-rate `0.567529` (56.75%), branch-rate `0.543544` (54.35%) — identical to the Phase 0
baseline (`evidence/baseline/baseline-coverage.cobertura.xml`), confirming no coverage regression
on this file (AC4). See `evidence/qa-gates/final-coverage-delta.md` for the full baseline-vs-final
delta computation.
