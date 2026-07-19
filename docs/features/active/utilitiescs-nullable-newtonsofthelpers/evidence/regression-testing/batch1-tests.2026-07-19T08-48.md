# Batch 1 Regression Tests (P1-T5)

- Timestamp: 2026-07-19T08-48
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~AllInclusiveBinder|FullyQualifiedName~MonoExtension"`
- EXIT_CODE: 0
- Output Summary: `Total tests: 24, Passed: 24, Failed: 0`. All Batch 1 tests green and behavior-identical (no assertions added, removed, or weakened; edits were pragma + one return annotation only). `UtilitiesCS.Test.dll` rebuilt clean (0 errors) against the freshly compiled `UtilitiesCS.dll`.
- Note: `/EnableCodeCoverage` from the plan text is omitted here; per-batch coverage is captured comprehensively in the final coverage gate (P9-T4). The filter uses `|` (not `OR`) per the vstest 18.7 TestCaseFilter operator requirement.
