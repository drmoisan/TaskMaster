# Batch D — Regression Test Run With Coverage

Timestamp: 2026-07-19T03-30

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-svgcontrol/evidence/regression-testing/batch-d-coverage.cobertura.xml`
(preceded by rebuilding `SVGControl.Test/SVGControl.Test.csproj`)

EXIT_CODE: 0

Output Summary: Total tests: 37. Passed: 37. Failed: 0. No test regression (AC3).
`RelativePath.cs` class-level coverage unchanged: line-rate `0.567529` (56.75%), branch-rate
`0.543544` (54.35%) — identical to baseline, no regression.

Overall package headline shows a marginal, expected change: line-rate `0.266381` (26.64%) vs.
baseline `0.266544` (26.65%); `lines-covered` unchanged at `870`, `lines-valid` increased from
`3264` to `3266` (+2). This is the added `#nullable enable` pragmas/annotation lines in the
0%-baseline Batch D files (`SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`,
`SVGFileNameEditor.cs`) contributing a small number of additional instrumentable-but-uncovered
lines to the aggregate denominator — no previously-covered line became uncovered, so there is no
regression on any changed line.
