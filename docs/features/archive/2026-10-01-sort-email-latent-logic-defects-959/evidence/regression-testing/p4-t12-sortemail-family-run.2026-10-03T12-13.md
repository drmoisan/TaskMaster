# P4-T12 SortEmail Family Run

Timestamp: 2026-10-03T12-13
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\959\p4-t12" "/Logger:trx;LogFileName=p4-t12.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0
Output Summary: All fifty-five rows of the five SortEmail test classes passed (COUNTERS total=55 executed=55 passed=55 failed=0). The RESULT rows are exactly the union of NAMES-TST1-FINAL (18), NAMES-T12 (12), NAMES-TSC-FINAL (12), NAMES-TAS-FINAL (11) and NAMES-TUL (2). No sandbox root existed before or after the run, and no sequence file was written.

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
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll] = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes] = Passed
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT WriteCSV_WhenFileExists_DoesNotWrite = Passed
RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
```

## Acceptance (P4-T12, all three required)

1. `EXIT_CODE: 0`: met.
2. `COUNTERS total=55 executed=55 passed=55 failed=0` with the fifty-five `RESULT` rows exactly the union of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL` and `NAMES-TUL`, each `= Passed`: met.
3. Every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`: met.
