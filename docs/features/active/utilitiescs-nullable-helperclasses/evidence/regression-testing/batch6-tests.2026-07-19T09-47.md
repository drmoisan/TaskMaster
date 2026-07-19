# Batch 6 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-47
- Task: [P6-T10]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ControlResizer|FullyQualifiedName~ControlPosition|FullyQualifiedName~ImageHelper|FullyQualifiedName~MouseDownFilter|FullyQualifiedName~OlvExtension|FullyQualifiedName~ScreenHelper|FullyQualifiedName~TableLayoutHelper"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 41; Passed: 41; Failed: 0. Total time ~1.94s.
- All Batch-6 tests green and behavior-identical; no assertions changed.
