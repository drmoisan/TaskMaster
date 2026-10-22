# Test Run Baseline (P0-T10)

Timestamp: 2026-10-03T08-30
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\959\p0-t10" "/Logger:trx;LogFileName=p0-t10.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p0-t10)
EXIT_CODE: 0 (the printed VSTEST_EXIT_CODE of the SortEmail run)
Output Summary: TRX-derived summary; SortEmail family 26 of 26 passed (fifteen SortEmail_Tests methods and eleven SortEmail_TrySaveAttachment_Tests methods); no sandbox root existed before or after.

- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- COUNTERS total=26 executed=26 passed=26 failed=0
- RESULT_COUNT: 26
- SEQUENCE_FILES: 0
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
- RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
- RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
- RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments = Passed
- RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
- RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
- RESULT Cleanup_Files_DoesNotThrow = Passed
- RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
- RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments = Passed
- RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
- RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
- RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
- RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
- RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
- RESULT SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows = Passed
- RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
- RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
- RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
- RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
- RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed

## QuickFiler.Test baseline (FILTER-EFC-ARCHIVE)

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelArchiveRootTests" "/ResultsDirectory:coverage\test-results\959\p0-t10-efc" "/Logger:trx;LogFileName=p0-t10-efc.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p0-t10-efc)

- EFC-VSTEST_EXIT_CODE: 0
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=11 executed=11 passed=11 failed=0
- RESULT MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates = Passed
- RESULT OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
- RESULT MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
- RESULT MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot = Passed
- RESULT MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot = Passed
- RESULT OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
- RESULT OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
- RESULT MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
- RESULT OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
- RESULT MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce = Passed
- RESULT ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress = Passed

Acceptance check: EXIT_CODE 0; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; COUNTERS total=26 executed=26 passed=26 failed=0; the 26 RESULT lines are exactly NAMES-TST1-BASE and NAMES-T, each Passed; EFC-VSTEST_EXIT_CODE 0 with 11 of 11 and the RESULT lines exactly NAMES-EFC-ARCHIVE; every SANDBOX value False and both SEQUENCE_FILES 0. All six hold.
