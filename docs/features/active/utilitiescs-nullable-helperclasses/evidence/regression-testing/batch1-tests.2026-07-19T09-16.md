# Batch 1 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-16
- Task: [P1-T10]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~GenericBitwise|FullyQualifiedName~MergeSort|FullyQualifiedName~ObjectSize|FullyQualifiedName~ParamArray|FullyQualifiedName~SimpleRegex|FullyQualifiedName~Tokenizer|FullyQualifiedName~SegmentStopWatch"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` was omitted for the per-batch targeted run (the coverage-enabled full measurement is captured at P0-T5 baseline and P9-T4 final). `/InIsolation` is required for the Moq-based assembly (per repo tooling). The test assembly was rebuilt (no TreatWarningsAsErrors) before the run so it links the updated UtilitiesCS.dll.

## Output Summary

- Test Run Successful.
- Total tests: 78; Passed: 78; Failed: 0. Total time ~2.16s.
- All Batch-1 tests green and behavior-identical; no assertions added, removed, or weakened.
