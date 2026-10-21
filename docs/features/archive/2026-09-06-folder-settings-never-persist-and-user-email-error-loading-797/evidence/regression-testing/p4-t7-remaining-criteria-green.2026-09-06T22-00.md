# P4-T7 — Remaining Automated Criteria Are Green (Issue #797, AC5, AC7, AC8)

Timestamp: 2026-09-07T09-54

Commands:

1. `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — exit 0.
2. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName Remaining -ResultsDirectory coverage/plan797-trx/p4`

EXIT_CODE: 0

Failed count: 0.

## PASS-AFTER correspondence

One line per test name that appeared as a `FAIL-BEFORE:` entry for AC5, AC7 or AC8 in the P1-T18
artifact. All seven are now passing.

- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithLeadingStorePrefix_RemovesIt
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_NullCurrent_SetsErrorLoadingText
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow
- PASS-AFTER: UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow

## Run totals, recorded as non-asserted observations

Total tests 17, passed 17, failed 0, elapsed 2.65 seconds. The scope also re-ran the retargeted
missing-implementation test, the AC5 argument-order test, the four AC7 trim cases that were green from
the moment they were written, and the four pre-existing relative-path tests, all of which passed.

## Implementation recorded by this evidence

- AC5: TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs declares the interface on the partial and
  adds an explicit implementation forwarding both arguments in the certain-then-potential order; the
  existing internal method keeps its accessibility and body, so the public surface of the globals
  type does not widen. UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs replaces the
  reflection lookup with a typed cast and logs at error level, naming the sink interface, when the
  cast fails. The file contains no `GetMethod` call and the `System.Reflection` using directive was
  removed, which is safe because the only other reflection reference in the file, in the static
  logger initialiser, is fully qualified. Analyzer cleanliness after these edits is proven in P5-T3.
- AC7: the trim helper removes a leading pair of backslash characters and returns every other input
  unchanged, including a null reference and the empty string, and is applied to the Inbox and Root
  Folder label assignments. The shared archive stem contract type was not modified.
- AC8: the four dereferences at the top of the populate method and the one in the relative-path
  helper are null-conditional, matching the null-conditional form the adjacent block already used.
  The six placeholder literals are unchanged apart from the user-email literal that AC6 replaced.
- The readability correction changes the single-ampersand operator in the live relative-path
  condition to the short-circuit form. It is behaviourally inert: both operands call a null-tolerant
  string extension and neither has a side effect. It does not repair a fault. The single-ampersand
  occurrence inside the commented-out block that moved with the populate method is dead commented
  text and was left untouched, so an occurrence count over the file is not used as the gate; the
  gate is that the four relative-path tests named in P4-T5 still pass, which they do.

Output Summary: The remaining criteria scope is green. Exit code 0, 17 of 17 passing, and all seven
AC5, AC7 and AC8 fail-before tests now pass.
