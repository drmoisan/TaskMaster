# Batch B Tests With Coverage

Timestamp: 2026-07-19T02-35

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/evidence/regression-testing/batch-b-coverage.cobertura.xml` (run after a normal `msbuild TaskMaster.sln /t:Build`)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. Total time: ~35.8s. No regression versus baseline (AC3).
- Line coverage: 83.7758%; branch coverage: 76.3407%. Consistent with baseline (83.7787% / 76.3368%); within run-to-run variance. No regression.
- Cobertura XML: `evidence/regression-testing/batch-b-coverage.cobertura.xml`.
