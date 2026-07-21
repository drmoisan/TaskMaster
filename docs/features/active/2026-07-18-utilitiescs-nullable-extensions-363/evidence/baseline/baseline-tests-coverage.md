# Baseline Test Run With Coverage

Timestamp: 2026-07-19T01-20

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. Total time: ~39.2s.
- Repository line coverage (baseline): 83.7787% (lines-covered 86561 / lines-valid 103321).
- Repository branch coverage (baseline): 76.3368% (branches-covered 19530 / branches-valid 25584).
- Cobertura XML written to `evidence/baseline/baseline-coverage.cobertura.xml`. All 24 `UtilitiesCS/Extensions/` files (plus DfDeedle.FrameUtilities.cs partial) are present in the coverage set; this file is the authoritative baseline for the AC4 changed-line comparison at P6-T5.
