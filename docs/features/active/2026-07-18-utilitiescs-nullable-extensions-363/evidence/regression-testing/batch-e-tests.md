# Batch E Tests With Coverage

Timestamp: 2026-07-19T04-45

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/evidence/regression-testing/batch-e-coverage.cobertura.xml` (run after a normal `msbuild TaskMaster.sln /t:Build`)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. No regression on the dataframe/async classes (AC3). The normal solution build compiled clean, confirming the `FromArray2D -> Frame<int,string>?`, `FromDefaultFolder -> Frame<int,string>?`, `FromDefaultFolderAsync -> Task<Frame<int,string>?>`, and `GetFirstNonNull -> object?` public changes break no existing caller.
- Line coverage: 83.7797%; branch coverage: 76.3485%. Consistent with baseline (83.7787% / 76.3368%). No regression.
- Cobertura XML: `evidence/regression-testing/batch-e-coverage.cobertura.xml`.
