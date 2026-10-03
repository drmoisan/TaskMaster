# P4-T8 SaveCase and TST1 Suites on the Seamed Tree

Timestamp: 2026-10-03T11-27
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_SaveCase_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t8" "/Logger:trx;LogFileName=p4-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere); then the same command with "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests." "/ResultsDirectory:coverage\test-results\959\p4-t8-tst1" "/Logger:trx;LogFileName=p4-t8-tst1.trx"
EXIT_CODE: 0 (scoped to the first run, FILTER-SAVECASE, the printed VSTEST_EXIT_CODE)
Output Summary: SaveCase suite 12 of 12 passed with rows exactly NAMES-TSC-FINAL; TST1 suite 18 of 18 passed with rows exactly NAMES-TST1-FINAL (TST1-VSTEST_EXIT_CODE 0); every SANDBOX value False; SEQUENCE_FILES 0 in both runs.

## Run 1: FILTER-SAVECASE (TASKID p4-t8)

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
COUNTERS total=12 executed=12 passed=12 failed=0
RESULT_COUNT: 12
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll] = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer = Passed
RESULT SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable = Passed
RESULT SaveCase_WhenAnswerIsEmpty_DoesNotSave = Passed
RESULT SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing = Passed
RESULT SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes] = Passed
RESULT SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll] = Passed
RESULT SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls = Passed
```

## Run 2: FILTER-TST1 (TASKID p4-t8-tst1)

```
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
TST1-VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=18 executed=18 passed=18 failed=0
RESULT_COUNT: 18
RESULT SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath = Passed
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
RESULT InitializeSortToExisting_AlwaysThrows_NotImplementedException = Passed
RESULT SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException = Passed
RESULT Cleanup_Files_DoesNotThrow = Passed
RESULT SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine = Passed
RESULT StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false] = Passed
RESULT GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true] = Passed
RESULT GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true] = Passed
RESULT StripTabsCrLf_WithPlainText_ReturnsOriginalString = Passed
```

(The second run's payload printed `VSTEST_EXIT_CODE: 0`; it is recorded above as `TST1-VSTEST_EXIT_CODE: 0` per the task.)

## Acceptance (P4-T8, all four required)

1. EXIT_CODE 0 with COUNTERS total=12 executed=12 passed=12 failed=0 and the rows exactly NAMES-TSC-FINAL, each Passed: met.
2. TST1-VSTEST_EXIT_CODE 0 with COUNTERS total=18 executed=18 passed=18 failed=0 and the rows exactly NAMES-TST1-FINAL, each Passed: met.
3. Every SANDBOX value False: met.
4. Both SEQUENCE_FILES 0: met.
