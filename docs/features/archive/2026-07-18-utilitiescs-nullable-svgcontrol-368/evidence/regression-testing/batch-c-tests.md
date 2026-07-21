# Batch C — Regression Test Run With Coverage

Timestamp: 2026-07-19T03-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-c-coverage.cobertura.xml`
(preceded by rebuilding `SVGControl.Test/SVGControl.Test.csproj`)

EXIT_CODE: 0

Output Summary: Total tests: 37. Passed: 37. Failed: 0. No test regression (AC3). Coverage
headline unchanged from baseline: line-rate `0.266544` (26.65%), branch-rate `0.322807` (32.28%).
`RelativePath.cs` class-level coverage unchanged: line-rate `0.567529` (56.75%), branch-rate
`0.543544` (54.35%) — identical to baseline, no regression. `SvgImageSelector.cs` is not exercised
by `SVGControl.Test` (0%-baseline file, consistent with the plan's documented coverage posture);
this is unchanged after the `ImagePath`/`ResourceName` annotation work.
