# Pass-After Regression Tests (P8-T8)

Timestamp: 2026-10-06T18-00
ITERATION: 3
SUPERSEDES: b1367e2a05aac5435cd701e8250a687d940bbe92
WRITTEN-BY: P8-T8
Command: (1) CMD-VSTEST (ASSEMBLY-UCT, FILTER-SORTEMAIL, TASKID p8-t8): vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\959\p8-t8" "/Logger:trx;LogFileName=p8-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; (2) CMD-VSTEST (ASSEMBLY-QFT, FILTER-EFC-CLEANUP, TASKID p8-t8-cleanup): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll with "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelFilerCleanupTests" and the same settings, isolation, results directory coverage\test-results\959\p8-t8-cleanup and trx logger p8-t8-cleanup.trx; (3) CMD-VSTEST (ASSEMBLY-QFT, FILTER-EFC-ARCHIVE, TASKID p8-t8-archive): the same with "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelArchiveRootTests", coverage\test-results\959\p8-t8-archive and p8-t8-archive.trx; each resolved through vswhere and run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (the SortEmail run's VSTEST_EXIT_CODE)
Output Summary: on the merged tree (merge commit 9163994569e24c5c539a285724f9c8f9f6fd8a0e of origin/main f8ea1b5dcc6514bc0088bc80965c188bfd717557) all fifty-six rows of the five SortEmail test classes passed (COUNTERS total=56 executed=56 passed=56 failed=0); a mechanical set comparison of the fifty-six TRX row names with the fifty-six P7-T10 RESULT rows reported zero differences (SET-DIFFERENCES: 0); the EfcDataModel cleanup class passed 3/3 and the archive-root class 11/11. Every runsettings hash equals P0-T4, no sandbox root existed before or after any run and no sequence file was written. P8-T3 recorded an empty MAIN-TOUCHED-WRITE-SET-CODE and P8-T1's MAIN-TOUCHED-TEST-DIRS names no SortEmail_ file and not EfcDataModelArchiveRootTests.cs, so the fixed expectations apply. This rewrite supersedes the ITERATION 2 content written by P7-T10, which remains in git history.

Execution note: in each run the vstest console stream was written to its coverage\logs\<task-id>.vstest.log file by Tee-Object and then discarded with Out-Null to limit tool output; every line below is printed by the payload after the run and reads the TRX document.

## UtilitiesCS.Test, FILTER-SORTEMAIL (p8-t8)

```
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=56 executed=56 passed=56 failed=0
RESULT_COUNT: 56
RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT WriteCSV_WhenFileExists_DoesNotWrite = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed
RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes] = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll] = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
RESULT SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false] = Passed
```

Row-set comparison with the P7-T10 rows (pwsh Compare-Object, case-sensitive, over the sorted names): OLD-COUNT: 56, CUR-COUNT: 56, SET-DIFFERENCES: 0.

## QuickFiler.Test (P8-T8)

- CLEANUP-VSTEST_EXIT_CODE: 0
- ARCHIVE-VSTEST_EXIT_CODE: 0

Cleanup run (p8-t8-cleanup):

```
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=3 executed=3 passed=3 failed=0
RESULT_COUNT: 3
RESULT MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce = Passed
RESULT MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState = Passed
RESULT MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates = Passed
```

Archive run (p8-t8-archive):

```
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=11 executed=11 passed=11 failed=0
RESULT_COUNT: 11
RESULT OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress = Passed
RESULT MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce = Passed
RESULT OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates = Passed
```

## Acceptance (P8-T8, all five required)

1. EXIT_CODE: 0 with COUNTERS total=56 executed=56 passed=56 failed=0 and the fifty-six rows exactly the P7-T10 union (SET-DIFFERENCES: 0), each = Passed; P8-T3 MAIN-TOUCHED-WRITE-SET-CODE empty and MAIN-TOUCHED-TEST-DIRS names no SortEmail_ file: met.
2. CLEANUP-VSTEST_EXIT_CODE: 0 with COUNTERS total=3 executed=3 passed=3 failed=0 and the rows exactly NAMES-TEF: met.
3. ARCHIVE-VSTEST_EXIT_CODE: 0 with failed=0, passed equal to total, total=11 and the rows exactly NAMES-EFC-ARCHIVE (MAIN-TOUCHED-TEST-DIRS does not name EfcDataModelArchiveRootTests.cs): met.
4. Every RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH of P0-T4 (98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57): met.
5. Every SANDBOX- value is False and every SEQUENCE_FILES: 0: met.
