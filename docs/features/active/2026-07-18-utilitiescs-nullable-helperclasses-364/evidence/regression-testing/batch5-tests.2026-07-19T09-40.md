# Batch 5 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-40
- Task: [P5-T9]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ShellUtilities|FullyQualifiedName~SysImageListHelper|FullyQualifiedName~ComStreamWrapper|FullyQualifiedName~DvgForm"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 29; Passed: 29; Failed: 0. Total time ~1.96s.
- All Batch-5 tests green and behavior-identical; no assertions changed.
