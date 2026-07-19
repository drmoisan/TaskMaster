# Batch 7 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-55
- Task: [P7-T9]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Theme|FullyQualifiedName~QfcTipsDetails|FullyQualifiedName~TipsController"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 65; Passed: 65; Failed: 0. Total time ~1.67s.
- All Batch-7 tests green and behavior-identical (Theme dispatcher, mail-label theming, breadcrumb theme-change routing, QfcTipsDetails, TipsController); no assertions changed.
