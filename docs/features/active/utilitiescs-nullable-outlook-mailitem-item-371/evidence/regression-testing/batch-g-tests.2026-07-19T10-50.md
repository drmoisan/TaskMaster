# Batch G — UtilitiesCS Tests (P7-T10)

- Timestamp: 2026-07-19T10-50
- Task: [P7-T10]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~MailItemHelper|FullyQualifiedName~CidImageResolver|FullyQualifiedName~EmailDetails"`
  - The filter also re-covers the two files re-touched during Batch G cross-batch reconciliation (`CidImageResolver.cs`, `EmailDetails.cs`).
- EXIT_CODE: 0

## Output Summary

- Test Run Successful. Total tests: 125, Passed: 125, Failed: 0.
- Covers `MailItemHelperCoreTests.cs`, `MailItemHelperProjectionTests.cs`, `MailItemHelper_ExtendedTests.cs`, and `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` (legacy-named duplicate), plus `CidImageResolverTests.cs`, `EmailDetailsTests.cs`, `EmailDetailsWrapperTests.cs`.
- All green and behavior-identical. The test project recompiled clean against the new nullable public surface (`MailItemHelper.Sender`/`.FolderInfo`/`.AttachmentsInfo`/`.Globals` nullable), confirming source compatibility for downstream consumers.
