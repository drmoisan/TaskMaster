# Batch A Tests With Coverage

Timestamp: 2026-07-19T02-05

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-a-coverage.cobertura.xml` (run after a normal `msbuild TaskMaster.sln /t:Build` so the test assemblies reference the updated UtilitiesCS.dll)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. Total time: ~36.7s. No regression versus baseline (AC3).
- Line coverage: 83.7932% (86576/103321); branch coverage: 76.3759% (19540/25584). Essentially identical to baseline (83.7787% / 76.3368%); the tiny positive delta reflects run-to-run instrumentation variance, not a coverage change. No regression.
- Cobertura XML: `evidence/regression-testing/batch-a-coverage.cobertura.xml`.
