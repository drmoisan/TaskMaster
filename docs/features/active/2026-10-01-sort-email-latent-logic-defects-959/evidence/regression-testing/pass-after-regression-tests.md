# Pass-After Regression Tests (P7-T10)

Timestamp: 2026-10-06T17-15
ITERATION: 2
SUPERSEDES: 54d066ee1a07eefbb2211a3060cb07ee092ee323
WRITTEN-BY: P7-T10
Command: (1) CMD-VSTEST (ASSEMBLY-UCT, FILTER-SORTEMAIL, TASKID p7-t10): vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\959\p7-t10" "/Logger:trx;LogFileName=p7-t10.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; (2) CMD-VSTEST (ASSEMBLY-QFT, FILTER-EFC-CLEANUP, TASKID p7-t10-cleanup): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll with "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelFilerCleanupTests" and the same settings, isolation, results directory coverage\test-results\959\p7-t10-cleanup and trx logger p7-t10-cleanup.trx; (3) CMD-VSTEST (ASSEMBLY-QFT, FILTER-EFC-ARCHIVE, TASKID p7-t10-archive): the same with "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelArchiveRootTests", coverage\test-results\959\p7-t10-archive and p7-t10-archive.trx; each resolved through vswhere and run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (the SortEmail run's VSTEST_EXIT_CODE)
Output Summary: on the Phase 7 tree all fifty-six rows of the five SortEmail test classes passed (COUNTERS total=56 executed=56 passed=56 failed=0), the fifty-five Phase 6 rows plus the new SS4 test SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly; the EfcDataModel cleanup class passed 3/3 and the archive-root class 11/11. Every runsettings hash equals P0-T4, no sandbox root existed before or after any run and no sequence file was written. This rewrite supersedes the ITERATION 1 content written by P6-T5 and P6-T6, which remains in git history.

## UtilitiesCS.Test, FILTER-SORTEMAIL (p7-t10)

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
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls = Passed
RESULT SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll] = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes] = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT WriteCSV_WhenFileExists_DoesNotWrite = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
```

## QuickFiler.Test (P7-T10)

- CLEANUP-VSTEST_EXIT_CODE: 0
- ARCHIVE-VSTEST_EXIT_CODE: 0

Cleanup run (p7-t10-cleanup):

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

Archive run (p7-t10-archive):

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
RESULT MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress = Passed
RESULT MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates = Passed
RESULT OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
```

## Acceptance (P7-T10, all five required)

1. EXIT_CODE: 0 with COUNTERS total=56 executed=56 passed=56 failed=0: met.
2. The fifty-six RESULT rows are exactly the union of NAMES-TST1-FINAL (18), NAMES-T12 (12), NAMES-TSC-FINAL (12), NAMES-TAS-FINAL2 (12) and NAMES-TUL (2), each = Passed (the fifty-five ITERATION 1 names plus SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly): met.
3. CLEANUP-VSTEST_EXIT_CODE: 0 with COUNTERS total=3 executed=3 passed=3 failed=0 and the rows exactly NAMES-TEF, and ARCHIVE-VSTEST_EXIT_CODE: 0 with COUNTERS total=11 executed=11 passed=11 failed=0 and the rows exactly NAMES-EFC-ARCHIVE, each = Passed: met.
4. Every RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH of P0-T4 (98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57): met.
5. Every SANDBOX- value is False and every SEQUENCE_FILES: 0: met.
