# Baseline Test Run With Coverage (P0-T6)

Timestamp: 2026-07-19T11-08

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/evidence/baseline/baseline-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: **4511**, Passed: **4511**, Failed: **0**, Skipped: 0.
- Coverage (dotnet-coverage instrumentation via `UtilitiesCS.Test`, whole-loaded-assembly denominator):
  - Line coverage: **65.30%** (`line-rate="0.652952"`, lines-covered 67621 / lines-valid 103562).
  - Branch coverage: **61.32%** (`branch-rate="0.613196"`, branches-covered 15688 / branches-valid 25584).
- Cobertura XML written to `evidence/baseline/baseline-coverage.cobertura.xml` (~9.7 MB).

Note: These absolute figures come from running only the `UtilitiesCS.Test` assembly; dotnet-coverage
instruments all loaded assemblies, so first-party assemblies not exercised by this single test project
appear at low/zero coverage, deflating the aggregate below the true repo-wide figure. For this
annotation-only feature the operative gate is AC4 (no coverage regression on changed lines), which is
threshold-independent; the absolute aggregate is recorded only as a comparison reference for P12-T5.
