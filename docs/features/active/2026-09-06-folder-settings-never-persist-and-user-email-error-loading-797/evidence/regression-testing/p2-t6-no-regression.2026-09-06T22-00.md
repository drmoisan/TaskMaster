# P2-T6 — No Regression Outside the Root-Cause-1 Set (Issue #797)

Timestamp: 2026-09-07T09-46

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName All -ResultsDirectory coverage/plan797-trx/p2-full`

EXIT_CODE: 1

ExpectedExitCode: 1

A non-zero exit is the expected outcome at this point in the plan: the AC5, AC6, AC7 and AC8 tests are
still red by design, because Phase 3 and Phase 4 have not run.

## Counts, read from the results file

- Total: 5261
- Passed: 5249
- Failed: 12
- Skipped: 0 (total minus executed)

The passed count rose from 5245 to 5249, which is exactly the four AC1, AC2 and AC4 tests that Phase 2
turned green.

PRE-EXISTING-FAILURES: NONE

The P1-T18 artifact recorded `PRE-EXISTING-FAILURES: NONE`, so no name is subtracted here and the
subtracted list is empty.

## The failing set is a proper subset of the P1-T18 FAIL-BEFORE set

Every one of the twelve names below appears in the `FAIL-BEFORE:` enumeration of the P1-T18 artifact,
and no failing test resides outside that set. The set is proper: the four AC1, AC2 and AC4 names in
the P1-T18 enumeration are absent here.

- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithLeadingStorePrefix_RemovesIt
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_NullCurrent_SetsErrorLoadingText
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow
- UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow

The existing test callers of the serializer that the research enumerated — across the serializer,
non-typed serializer, linked-list and stack test files in the UtilitiesCS test project and the two
application-globals test files in the TaskMaster test project — are all inside this run and none of
them appears above, so all of them continue to pass unchanged. That is the direct evidence that the
shared deserialize overload was not modified, per design decision D1.

Output Summary: Exit code 1 as declared. Twelve failures remain, all of them members of the P1-T18
fail-before set and all attributable to AC5, AC6, AC7 or AC8, which Phase 3 and Phase 4 address. No
test outside that set regressed.
