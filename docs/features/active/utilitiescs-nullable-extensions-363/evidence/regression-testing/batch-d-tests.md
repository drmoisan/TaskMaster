# Batch D Tests With Coverage

Timestamp: 2026-07-19T04-05

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-d-coverage.cobertura.xml` (run after a normal `msbuild TaskMaster.sln /t:Build`)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. No regression (AC3). The normal solution build compiled clean, confirming the `GetAncestor<T> -> T?`, `IsRegistered(this EventHandler?, ...)`, and TraceExtensions reflection-return nullable changes break no existing caller — important because WinFormsExtensions.cs `Clone<T>()` is the downstream #374 (dialogs-misc) contract, whose signatures are unchanged.
- Line coverage: 83.7826%; branch coverage: 76.3524%. Consistent with baseline (83.7787% / 76.3368%). No regression.
- Cobertura XML: `evidence/regression-testing/batch-d-coverage.cobertura.xml`.
