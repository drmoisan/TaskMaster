# P5-T5 — Final Scoped Test Run (Issue #797)

Timestamp: 2026-09-07T10-05

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName All -ResultsDirectory coverage/plan797-trx/p5`

The helper resolves vstest.console.exe through vswhere and invokes it over the two explicitly named
test assemblies UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and
TaskMaster.Test/bin/Debug/TaskMaster.Test.dll, never by directory discovery, with `/InIsolation`.

EXIT_CODE: 0

ExpectedExitCode: 0

The declared expectation equals the exit code this run actually produced. Because the exit code is 0
the failed count is 0, and the alternative branch — exit code 1 with every failing test a member of
`BASELINE-FAILING-TESTS:` — is not entered. The P0-T9 artifact recorded
`BASELINE-FAILING-TESTS: NONE`, so no baseline failing name needed to be checked for reproduction, and
`PRE-EXISTING-IN-WRITE-SET:` is empty.

## Counts, read from the results file

- Total: 5262
- Passed: 5262
- Failed: 0
- Skipped: 0 (total minus executed)

The skipped count is derived from the results-file counters as total minus executed, not from console
text, because a green run prints no `Skipped` line and the results file writes its not-executed
counter as zero.

The baseline recorded 5237 tests. This change adds 25: two for AC1, four in the new serializer guard
file for AC2 and AC4, two for AC5, five in the store wrapper file for AC6 fallback ordering and the
retry entry point's null-root-folder safety, and twelve in the new controller display partial for AC6
retry, AC7 and AC8.

## Filter expression used

```text
TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
```

Four shell-icon test classes are excluded from this run for environmental reasons unrelated to this
change: `HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests` and `EmailIntelligence.OSBrowser_Tests`. CI covers them.

The results file itself is not committed, because it carries `runUser` and `computerName` attributes.
Only the sanitized counts above are recorded here.

## PASS-AFTER correspondence, complete rather than sampled

The run executed 5262 tests with zero failures, so every one of the sixteen names recorded as a
`FAIL-BEFORE:` entry in the P1-T18 artifact passed. They are enumerated here in full.

- PASS-AFTER: TaskMaster.Test.AppGlobals.AppOlObjectsCoverageTests.LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperTests.GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithLeadingStorePrefix_RemovesIt
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_NullCurrent_SetsErrorLoadingText
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow

No name was carried under `PRE-EXISTING-FAILURES:` in the P1-T18 artifact, so no name is excluded from
that correspondence and none is listed separately.

PRE-EXISTING-IN-WRITE-SET: NONE

Note on the intermittent failure recorded by the caller: the test
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`,
tracked as a nondeterministic timing race under issue 803 and outside this item's Write Set, passed in
this run as it did at baseline. No re-run was required.

Output Summary: The final scoped test run is green. 5262 total, 5262 passed, 0 failed, 0 skipped, exit
code 0, and all sixteen fail-before tests pass.
