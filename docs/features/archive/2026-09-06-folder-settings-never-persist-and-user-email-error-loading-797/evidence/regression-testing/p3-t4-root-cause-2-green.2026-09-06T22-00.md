# P3-T4 — Root Cause 2 Automated Criterion Is Green (Issue #797, AC6)

Timestamp: 2026-09-07T09-50

Commands:

1. `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — exit 0.
2. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName RootCause2 -ResultsDirectory coverage/plan797-trx/p3`

EXIT_CODE: 0

Failed count: 0.

## PASS-AFTER correspondence

One line per test name that appeared as a `FAIL-BEFORE:` entry for AC6 in the P1-T18 artifact. All
five are now passing.

- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason

## Run totals, recorded as non-asserted observations

Total tests 20, passed 20, failed 0, elapsed 2.90 seconds.

## The two pre-existing tests re-derived against the new ordering

P3-T1 required the two pre-existing tests in the store wrapper test file to be re-derived against the
new fallback order. Both passed unchanged and no arrangement correction was required, so no declared
expectation change arises from Phase 3:

- `GetSmtpAddressFromStore_WhenExchangeUserIsUnavailable_ReturnsNull` — the Exchange user is null, so
  no primary address is produced; the address entry supplies no at-sign-bearing address because its
  address is unset on the mock; and the wrapper's display name is null. Every source therefore fails
  and the method still returns null.
- `GetSmtpAddressFromStore_WhenExchangeLookupThrowsComException_ReturnsNull` — the Exchange lookup
  throws, and the same two remaining sources yield nothing, so the method still returns null.

`GetSmtpAddressFromStore_WhenRootFolderIsNull_ShouldReturnNull` likewise passed unchanged, and the new
`RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow` test added under P3-T2
passed, which is the direct evidence that the retry entry point is safe when the root folder is null.

## Filter expression used

```text
(FullyQualifiedName~StoreWrapperTests&TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests)|(FullyQualifiedName~PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress&TestCategory!=LiveOutlook)|(FullyQualifiedName~PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup&TestCategory!=LiveOutlook)|(FullyQualifiedName~PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason&TestCategory!=LiveOutlook)
```

The four shell-icon test classes named in that expression are excluded from every local run in this
plan for environmental reasons unrelated to this change. CI covers them.

Output Summary: The root-cause-2 scope is green. Exit code 0, 20 of 20 passing, and all five AC6
fail-before tests now pass. The two pre-existing SMTP tests pass unchanged under the new ordering.
