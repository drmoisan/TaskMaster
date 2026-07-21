# Batch C Tests With Coverage

Timestamp: 2026-07-19T03-25

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/evidence/regression-testing/batch-c-coverage.cobertura.xml` (run after a normal `msbuild TaskMaster.sln /t:Build`)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5702. Passed: 5702. Failed: 0. No regression on the core-contract classes (IEnumerableExtensions, ArrayExtensions, IListExtensions, DictionaryExtensions) whose public signatures received nullable annotations (AC3). The normal solution build compiled clean, confirming the `Find<T> -> T?`, `TryFindMax out T?`, `UpdateOrRemove out TValue?`, and nullable-parameter changes break no existing caller.
- Line coverage: 83.7816%; branch coverage: 76.3368%. Consistent with baseline (83.7787% / 76.3368%). No regression.
- Cobertura XML: `evidence/regression-testing/batch-c-coverage.cobertura.xml`.
