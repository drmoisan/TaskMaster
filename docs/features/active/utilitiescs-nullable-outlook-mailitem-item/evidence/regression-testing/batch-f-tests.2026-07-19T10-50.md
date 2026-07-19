# Batch F — UtilitiesCS Tests (P6-T6)

- Timestamp: 2026-07-19T10-50
- Task: [P6-T6]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ItemInfo|FullyQualifiedName~EmailDetails"`
- EXIT_CODE: 0

## Output Summary

- Test Run Successful. Total tests: 25, Passed: 25, Failed: 0.
- Covers `ItemInfoTests.cs`, legacy `ItemInfo_Tests.cs`, `EmailDetailsTests.cs`, `EmailDetailsWrapperTests.cs`.
- All green and behavior-identical. The `IEmailDetailsWrapper` seam over the static `EmailDetails` extension methods is preserved.
