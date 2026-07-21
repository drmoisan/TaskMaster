# Batch C — UtilitiesCS Tests (P3-T9)

- Timestamp: 2026-07-19T10-50
- Task: [P3-T9]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~MailResolution|FullyQualifiedName~MailItemExtensions|FullyQualifiedName~OlItemPseudoInterface|FullyQualifiedName~OlItemSummary|FullyQualifiedName~OlToDoTable"`
  - `|` used for OR in TestCaseFilter (vstest rejects the literal `OR`). `/InIsolation` for Moq.
- EXIT_CODE: 0

## Output Summary

- Test Run Successful. Total tests: 51, Passed: 51, Failed: 0.
- Covers `MailResolutionTests.cs`, `MailItemExtensions*` tests, `OlItemPseudoInterfaceTests.cs`/`OlItemPseudoInterface_Tests.cs`, `OlItemSummaryTests.cs`, `OlToDoTableTests.cs`/`OlToDoTable_Tests.cs`.
- All green and behavior-identical; no new tests added around COM-bound paths.
