# P4-T5 Final scoped SortEmail filter run

Timestamp: 2026-10-01T21-21
ITERATION: 1
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\956\p4-t5" "/Logger:trx;LogFileName=p4-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0
Output Summary:
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH of P0-T4)
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 0
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=26 executed=26 passed=26 failed=0
RESULT_COUNT: 26
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly = Passed
RESULT SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows = Passed
RESULT TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException = Passed
RESULT TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
NAME-SET-MATCH: True (added observation: one read-only statement appended to the payload compares the sorted Passed names with the sorted union of NAMES-TST and NAMES-T, ordinal and case-sensitive)
MESSAGE lines: none (no failed result)
Deviation (recorded, as at P3-T8): the vstest console stream is teed to coverage\logs\p4-t5.vstest.log (git-ignored) and not echoed to the tool output (Out-Null after Tee-Object); every field above is printed by the unchanged CMD-VSTEST statements.
Acceptance: EXIT_CODE 0; COUNTERS total=26 executed=26 passed=26 failed=0; the 26 RESULT lines are exactly NAMES-TST and NAMES-T, each Passed; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; every SANDBOX value False and SEQUENCE_FILES 0. All five hold.
