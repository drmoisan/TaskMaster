# Pass-After Regression Tests (P6-T5, P6-T6)

Timestamp: 2026-10-03T12-38
ITERATION: 1
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\959\p6-t5" "/Logger:trx;LogFileName=p6-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-UCT, FILTER-SORTEMAIL, TASKID p6-t5; resolved through vswhere)
EXIT_CODE: 0
Output Summary: All fifty-five rows of the five SortEmail test classes passed (COUNTERS total=55 executed=55 passed=55 failed=0). The RESULT rows are exactly the union of NAMES-TST1-FINAL (18), NAMES-T12 (12), NAMES-TSC-FINAL (12), NAMES-TAS-FINAL (11) and NAMES-TUL (2), the same set as the P4-T12 run. The runsettings hash equals RUNSETTINGS-HASH of P0-T4. No sandbox root existed before or after the run and no sequence file was written.

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
COUNTERS total=55 executed=55 passed=55 failed=0
RESULT_COUNT: 55
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll] = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
RESULT WriteCSV_WhenFileExists_DoesNotWrite = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes] = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer = Passed
```

## Acceptance (P6-T5, all five required)

1. EXIT_CODE: 0: met.
2. COUNTERS total=55 executed=55 passed=55 failed=0: met.
3. The fifty-five RESULT rows are exactly the union of NAMES-TST1-FINAL, NAMES-T12, NAMES-TSC-FINAL, NAMES-TAS-FINAL and NAMES-TUL, each = Passed (the same 55 names as the P4-T12 run, which was verified against the union): met.
4. RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH (98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57, P0-T4): met.
5. Every SANDBOX- value is False and SEQUENCE_FILES: 0: met.

## QuickFiler.Test (P6-T6)

Timestamp: 2026-10-03T12-40
ITERATION: 1
Command: CMD-VSTEST (ASSEMBLY-QFT QuickFiler.Test\bin\Debug\QuickFiler.Test.dll, FILTER-EFC-CLEANUP, TASKID p6-t6) and CMD-VSTEST (ASSEMBLY-QFT, FILTER-EFC-ARCHIVE, TASKID p6-t6-archive), each vstest.console.exe with "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation and the trx logger, resolved through vswhere

- CLEANUP-VSTEST_EXIT_CODE: 0
- ARCHIVE-VSTEST_EXIT_CODE: 0

Cleanup run (p6-t6):

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
RESULT MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates = Passed
RESULT MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState = Passed
RESULT MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce = Passed
```

Archive run (p6-t6-archive):

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
RESULT MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress = Passed
RESULT OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce = Passed
RESULT OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
```

### Acceptance (P6-T6, all three required)

1. CLEANUP-VSTEST_EXIT_CODE: 0 with COUNTERS total=3 executed=3 passed=3 failed=0 and the rows exactly NAMES-TEF, each = Passed: met.
2. ARCHIVE-VSTEST_EXIT_CODE: 0 with COUNTERS total=11 executed=11 passed=11 failed=0 and the rows exactly NAMES-EFC-ARCHIVE, each = Passed: met.
3. Every SANDBOX- value is False: met.
