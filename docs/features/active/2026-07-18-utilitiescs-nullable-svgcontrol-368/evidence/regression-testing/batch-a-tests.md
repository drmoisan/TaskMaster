# Batch A — Regression Test Run With Coverage

Timestamp: 2026-07-19T02-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-svgcontrol/evidence/regression-testing/batch-a-coverage.cobertura.xml`
(preceded by `msbuild SVGControl.Test/SVGControl.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU` to rebuild the test assembly against the updated `SVGControl.dll`)

EXIT_CODE: 0

Output Summary: Total tests: 37. Passed: 37. Failed: 0. No test regression (AC3). Overall
coverage headline unchanged from baseline: line-rate `0.266544` (26.65%), branch-rate
`0.322807` (32.28%). `RelativePath.cs` class-level coverage unchanged from baseline:
line-rate `0.567529` (56.75%), branch-rate `0.543544` (54.35%) — identical to
`evidence/baseline/baseline-coverage.cobertura.xml`, confirming no coverage regression on the one
file with a real automated baseline. The 4 newly-annotated Batch A files
(`ISvgResource.cs`, `ToggleSwitch.cs`, `SVGParser.cs`, `SvgRenderer.cs`) are not exercised by
`SVGControl.Test` (consistent with the plan's documented 0%-baseline posture for the 12
remediation-target files), so their own coverage numbers are unchanged (0%) and not a regression.
