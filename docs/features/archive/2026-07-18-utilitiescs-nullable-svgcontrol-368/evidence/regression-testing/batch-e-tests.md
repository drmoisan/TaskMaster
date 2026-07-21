# Batch E — Regression Test Run With Coverage

Timestamp: 2026-07-19T04-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-e-coverage.cobertura.xml`
(preceded by rebuilding `SVGControl.Test/SVGControl.Test.csproj`)

EXIT_CODE: 0

Output Summary: Total tests: 37. Passed: 37. Failed: 0. No test regression (AC3).
`RelativePath.cs` class-level coverage unchanged: line-rate `0.567529` (56.75%), branch-rate
`0.543544` (54.35%) — identical to baseline. `lines-covered` unchanged at `870` (same as Batch D),
`lines-valid` unchanged at `3266` (Batch E's edits to `ButtonSVG.cs`/`PictureBoxSVG.cs` did not
add net-new instrumentable lines beyond what Batch D introduced). No coverage regression.
