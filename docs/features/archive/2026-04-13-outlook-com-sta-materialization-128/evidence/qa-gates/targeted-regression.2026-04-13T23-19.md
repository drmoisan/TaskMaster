# Targeted Regression Verification

Timestamp: 2026-04-13T23-19
Source Artifact: `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-mstest-coverage.2026-04-13T23-19.md`
Source Log: `artifacts/outlook-com-sta-materialization-128-tests-2026-04-13T23-19.log`

## Verified Test Files

- `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticSenderResolverTests.cs`

## Verified Test Names

### STA materialization path

- `ToIItemInfo_WhenCreatingMailHelper_UsesCallingThreadForMaterialization` — Passed
- `ToMinedMail_WhenCreatingMailHelper_UsesCallingThreadForMaterialization` — Passed
- `CreateMailItemHelperAsync_WithMockMailItem_UsesBaseHelperFactory` — Passed
- `FromMailItemAsync_MaterializesTokenizationDependenciesBeforeBackgroundTokenAccess` — Passed

### Sender and recipient fallback path

- `GetSenderName_ForMailItemWhenGetExchangeUserReturnsNull_FallsBackToMailSenderName` — Passed
- `GetSenderName_ForMailItemWhenExchangeLookupThrowsAndAddressEntryNameThrows_FallsBackToSenderName` — Passed
- `GetSenderAddress_ForMailItemWhenSenderAddressThrows_UsesPropertyAccessorFallback` — Passed
- `GetRecipientInfo_WhenExchangeLookupFails_UsesSafeRecipientFallbacks` — Passed
