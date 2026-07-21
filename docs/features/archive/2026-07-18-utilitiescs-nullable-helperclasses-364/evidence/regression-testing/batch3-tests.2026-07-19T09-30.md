# Batch 3 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-30
- Task: [P3-T7]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~DeepCompare|FullyQualifiedName~ObjectCopier|FullyQualifiedName~DispatchUtility|FullyQualifiedName~ReflectionHelper"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 33; Passed: 33; Failed: 0. Total time ~2.52s.
- All Batch-3 tests green and behavior-identical; no assertions changed.
