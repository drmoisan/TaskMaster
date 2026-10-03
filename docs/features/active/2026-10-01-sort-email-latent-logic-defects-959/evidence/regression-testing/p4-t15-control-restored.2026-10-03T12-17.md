# P4-T15 Mutation Control Restored

Timestamp: 2026-10-03T12-17
Command: Edit E-A-CONTROL-RESTORE applied to UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs (the `AttachmentsAltNamePrompt` element reinstated before `RemoveReadOnlyPrompt` in `AllPromptSessions`); then CMD-HASH on PATHS-A; then CMD-BUILD-TEST (PROJECT UtilitiesCS.Test, TASKID p4-t15): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger; then CMD-VSTEST: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t15" "/Logger:trx;LogFileName=p4-t15.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0 (scoped to the CMD-VSTEST invocation, the printed VSTEST_EXIT_CODE)
Output Summary: The Edit restore reproduced the fixed A byte for byte (RESTORED-HASH-A equals FIX-HASH-A), so the CMD-RESTORE copy fallback was not run. The build is green and all eleven NAMES-TAS-FINAL rows pass again. A as staged by this task is identical to the fix committed by P4-T10, so it contributes no diff.

```
SHA256: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
RESTORED-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
RESTORE-ROUTE: EDIT
FIX-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
```

## CMD-BUILD-TEST labelled output

```
    0 Warning(s)
    0 Error(s)
MSBUILD_EXIT_CODE: 0
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ERROR_LINES: 0
ERROR_LINES_TEST_FILES: 0
ERROR_LINES_OTHER_FILES: 0
MISSING_SAVEATTACHMENTASYNC_6: 0
MISSING_SAVECASEASYNC_6: 0
MISSING_SAVEATTACHMENT_4: 0
MISSING_REDIRECTSAVEFOLDER: 0
ERROR_CODES: 
DLL_ADVANCED: True
```

## CMD-VSTEST output

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
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
```

## Acceptance (P4-T15, all four required)

1. The final `RESTORED-HASH-A:` equals `FIX-HASH-A:` (route EDIT): met.
2. `MSBUILD_EXIT_CODE: 0` and `DLL_ADVANCED: True`: met.
3. `EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-TAS-FINAL`, each `= Passed`: met.
4. Every `SANDBOX-` value is `False`: met.
