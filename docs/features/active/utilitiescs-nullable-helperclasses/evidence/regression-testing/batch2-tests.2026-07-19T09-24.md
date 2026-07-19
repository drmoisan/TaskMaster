# Batch 2 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-24
- Task: [P2-T7]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~DebugTextLogger|FullyQualifiedName~VerboseLogger|FullyQualifiedName~TraceUtility"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt (no TreatWarningsAsErrors) before the run.

## Output Summary

- Test Run Successful.
- Total tests: 37; Passed: 37; Failed: 0. Total time ~1.97s.
- All Batch-2 tests green and behavior-identical; no assertions changed.
