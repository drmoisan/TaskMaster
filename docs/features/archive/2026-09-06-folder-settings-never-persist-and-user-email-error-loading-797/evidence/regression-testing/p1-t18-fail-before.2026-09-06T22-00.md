# P1-T18 — Fail-before Regression Run (Issue #797)

Timestamp: 2026-09-07T09-39

Commands:

1. `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — exit 0. The
   build must succeed: a compile error is not an acceptable fail-before signal. One compile error was
   encountered and corrected before this run, `CS0104: 'Action' is an ambiguous reference between
   'Microsoft.Office.Interop.Outlook.Action' and 'System.Action'` in the store controller test file,
   fixed by qualifying the type as `System.Action`.
2. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName All -ResultsDirectory coverage/plan797-trx/p1`

EXIT_CODE: 1

ExpectedExitCode: 1

## Counts, read from the results file

- Total: 5261
- Passed: 5245
- Failed: 16
- Skipped: 0 (total minus executed)

The baseline recorded 5237 tests. This run adds the 24 tests written in Phase 1: two for AC1, four in
the new serializer guard file for AC2 and AC4, two for AC5, four for AC6 fallback ordering, and twelve
in the new controller display partial for AC6 retry, AC7 and AC8.

PRE-EXISTING-FAILURES: NONE

The P0-T9 artifact recorded `BASELINE-FAILING-TESTS: NONE`, so the subtracted set is empty and every
failure below is new and attributable to this change's not-yet-implemented behaviour.

## FAIL-BEFORE enumeration

Every name below is absent from the `BASELINE-FAILING-TESTS:` set in the P0-T9 artifact.

AC1 — the fresh-build path does not yet adopt the loader configuration:

- FAIL-BEFORE: TaskMaster.Test.AppGlobals.AppOlObjectsCoverageTests.LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration

AC2 — the guard still returns silently and compares only against the empty string:

- FAIL-BEFORE: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer
- FAIL-BEFORE: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer

AC4 — the explicit-save entry point still forwards to the deferred path:

- FAIL-BEFORE: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer

AC5 — the reflection lookup still succeeds against the non-sink double:

- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke

AC6 — the single outer catch converts every failure to null, with no fallback, no captured reason and
no retry:

- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason

AC7 — the trim helper is still the declaration-only placeholder and no call site uses it:

- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithLeadingStorePrefix_RemovesIt
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix

AC8 — the four unguarded dereferences and the one in the relative-path helper still throw:

- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_NullCurrent_SetsErrorLoadingText
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow
- FAIL-BEFORE: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow

The enumeration contains at least one failing test attributable to each of AC1, AC2, AC4, AC5, AC6,
AC7 and AC8. AC3 is not represented because it is not automatable; its fail-before requirement is
discharged by the exception dossier authored in P1-T19.

## Tests added in Phase 1 that are green from the moment they were written

These are additive coverage, not fail-before signals: the AC1 key-absent negative case, the AC4
deferred-path-unchanged case, the AC5 argument-order case, the AC6 case in which the primary SMTP
address is present, the AC6 retry case in which the address is already populated, and the four AC7
trim cases whose input carries no leading backslash pair.

NEW-TEST-FILES-DISCOVERED:

The two new test files registered in P1-T12 and P1-T16 both compiled into their assembly and were
executed, so no compile entry is missing.

- From UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs:
  `UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer`
- From UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:
  `UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithLeadingStorePrefix_RemovesIt`

## Filter expression used

```text
TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
```

The four shell-icon test classes named in that expression are excluded from every local run for
environmental reasons unrelated to this change. CI covers them.

## Note on the helper

The first attempt at this run reported the same 16 failures but returned exit code 0, because the
helper's vstest wrapper returned the external command's captured console output joined with the exit
code, so the caller could not propagate a real exit code. The helper was corrected to send the
external command's output to the host and return only the integer, and the run above is the corrected
run. The two earlier green runs, P0-T9 and P1-T3, are unaffected in substance: both were genuinely
green in their results files, so their recorded exit code of 0 remains correct.

Output Summary: The build succeeds and the scoped run fails with exit code 1 and 16 failures, one or
more for each of AC1, AC2, AC4, AC5, AC6, AC7 and AC8, and none of them pre-existing. Both new test
files are discovered and executed.
