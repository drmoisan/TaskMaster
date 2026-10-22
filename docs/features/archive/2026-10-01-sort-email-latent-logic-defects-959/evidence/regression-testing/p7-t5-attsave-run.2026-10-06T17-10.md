# P7-T5 Test Build and Attachment-Saving Class Run

Timestamp: 2026-10-06T17-10
Command: (1) CMD-BUILD-TEST (PROJECT UtilitiesCS.Test; TASKID p7-t5): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p7-t5.msbuild.log); (2) CMD-VSTEST (ASSEMBLY-UCT, FILTER-ATTSAVE, TASKID p7-t5): vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p7-t5" "/Logger:trx;LogFileName=p7-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0 (the printed VSTEST_EXIT_CODE, the last invocation)
ITERATION: 1
Output Summary: the test project built with 0 Warning(s) and 0 Error(s) and the output assembly advanced; all twelve attachment-saving rows passed, including the new SS4 test SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly; the runsettings hash equals P0-T4; no sandbox root existed before or after and no sequence file was written.

## CMD-BUILD-TEST

- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- ERROR_LINES: 0
- ERROR_LINES_TEST_FILES: 0
- ERROR_LINES_OTHER_FILES: 0
- MISSING_SAVEATTACHMENTASYNC_6: 0
- MISSING_SAVECASEASYNC_6: 0
- MISSING_SAVEATTACHMENT_4: 0
- MISSING_REDIRECTSAVEFOLDER: 0
- ERROR_CODES: (empty)
- DLL_ADVANCED: True

## CMD-VSTEST

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
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
```

## Acceptance (P7-T5, all five required)

1. MSBUILD_EXIT_CODE: 0, ERROR_LINES: 0 and DLL_ADVANCED: True: met.
2. VSTEST_EXIT_CODE: 0 with COUNTERS total=12 executed=12 passed=12 failed=0: met.
3. The twelve RESULT rows are exactly NAMES-TAS-FINAL2 (the eleven NAMES-TAS-FINAL names plus SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly), each = Passed: met.
4. RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH of P0-T4 (98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57): met.
5. Every SANDBOX- value is False and SEQUENCE_FILES: 0: met.
