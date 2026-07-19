# Batch 8 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T10-05
- Task: [P8-T7]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Initializer|FullyQualifiedName~PropertyInitializer|FullyQualifiedName~FilePathHelper|FullyQualifiedName~PrettyPrint"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 120; Passed: 120; Failed: 0. Total time ~1.72s.
- All Batch-8 tests green and behavior-identical, including the FilePathHelper Newtonsoft converter tests, Initializer/PropertyInitializer, and PrettyPrint formatting; no assertions changed.
