# P2-T11 SortEmail_Tests Pins After the SanitizeArray Deletion

Timestamp: 2026-10-03T08-50
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests." "/ResultsDirectory:coverage\test-results\959\p2-t11" "/Logger:trx;LogFileName=p2-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, TASKID p2-t11)
EXIT_CODE: 0 (the printed VSTEST_EXIT_CODE)
Output Summary: the fourteen remaining SortEmail_Tests methods all passed, including the SanitizeArrayLineTSV test, both StripTabsCrLf tests and the two try-save pins.

- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=14 executed=14 passed=14 failed=0
- RESULT_COUNT: 14
- RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
- RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
- RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
- RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
- RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
- RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
- RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments = Passed
- RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
- RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments = Passed
- RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
- RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
- RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
- RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
- RESULT Cleanup_Files_DoesNotThrow = Passed

Acceptance check: EXIT_CODE 0; COUNTERS total=14 executed=14 passed=14 failed=0 with the fourteen RESULT rows exactly NAMES-TST1-MID, each Passed; every SANDBOX value False and SEQUENCE_FILES 0. All three hold.
