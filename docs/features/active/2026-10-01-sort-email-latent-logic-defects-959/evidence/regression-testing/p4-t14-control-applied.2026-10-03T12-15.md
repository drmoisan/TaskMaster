# P4-T14 Mutation Control Applied (Structural Test Cleanup_Files_ResetsEveryPromptSession)

Timestamp: 2026-10-03T12-15
Command: Edit E-A-CONTROL-MUTATE applied to UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs (the `AttachmentsAltNamePrompt` element removed from the `AllPromptSessions` array); then CMD-CENSUS (PATHS-A with TOKENS-A); then CMD-BUILD-TEST (PROJECT UtilitiesCS.Test, TASKID p4-t14): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger; then CMD-VSTEST: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t14" "/Logger:trx;LogFileName=p4-t14.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1 (scoped to the vstest invocation, the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: With one element removed from `AllPromptSessions` (the array now holds three elements, observed by the Edit and by reading lines 31 to 37 of A), the structural test `Cleanup_Files_ResetsEveryPromptSession` fails with `Expected resetTargets to contain 4 item(s), but found 3`, and the other ten NAMES-TAS-FINAL rows pass. The build was green. The structural test therefore discriminates against the removal of a session. This commit stages FEATURE/ only, so the mutated A is never committed. P4-T15 restores A.

```
MUTATED-HASH-A: 0870C1B3D12C2F36F4098E0585680D09092C8A5F3E7B4B9DB51FE1E483496122
MSBUILD_EXIT_CODE: 0
ERROR_LINES: 0
VSTEST_EXIT_CODE: 1
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=11 executed=11 passed=10 failed=1
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Failed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
MESSAGE Cleanup_Files_ResetsEveryPromptSession :: Expected resetTargets to contain 4 item(s), but found 3: {UtilitiesCS.YesNoToAllPromptSession{ }, UtilitiesCS.YesNoToAllPromptSession{ }, UtilitiesCS.YesNoToAllPromptSession{ }}.
```

## CMD-CENSUS PATHS-A with TOKENS-A (mutated state; TOTAL lines)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 3
TOKEN caseYesNoToAllResponse.NoToAll: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.No: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.Yes: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.YesToAll: @ TOTAL = 1
TOKEN |YesNoToAllResponse. @ TOTAL = 0
TOKEN HasFlag @ TOTAL = 0
TOKEN _attachmentsAltName=YesNoToAllResponse.Empty; @ TOTAL = 0
TOKEN YesNoToAllResponse_ @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog( @ TOTAL = 0
TOKEN new(YesNoToAll.ShowDialog) @ TOTAL = 3
TOKEN AllPromptSessions @ TOTAL = 2
TOKEN RedirectSaveFolder( @ TOTAL = 2
TOKEN FolderPathSave=destinationPath; @ TOTAL = 1
TOKEN FilePathHelperSaveAlt.FolderPath=destinationPath; @ TOTAL = 1
TOKEN IsPicture @ TOTAL = 0
TOKEN _responseSaveFile @ TOTAL = 0
TOKEN Func<Attachment,string,Task<bool>>trySave @ TOTAL = 0
TOKEN TrySaveAttachmentDelegatetrySave @ TOTAL = 2
TOKEN delegateTask<bool>TrySaveAttachmentDelegate( @ TOTAL = 1
TOKEN Func<string,bool>fileExists @ TOTAL = 2
TOKEN File.Exists @ TOTAL = 2
TOKEN TrySaveAttachmentAsync @ TOTAL = 1
TOKEN SaveCaseAsync( @ TOTAL = 2
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 328
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 0870C1B3D12C2F36F4098E0585680D09092C8A5F3E7B4B9DB51FE1E483496122
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

## Acceptance (P4-T14, all five required)

1. `MUTATED-HASH-A:` (0870C1B3...) differs from `FIX-HASH-A:` (9F1A8D46...), and every TOKENS-A total equals the FINAL column, with `AllPromptSessions` 2 unchanged and the array holding three elements (observed by the Edit and a read of lines 31 to 37): met.
2. `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`: met.
3. `COUNTERS total=11 executed=11 passed=10 failed=1` with the `Failed` row exactly `Cleanup_Files_ResetsEveryPromptSession` and its `MESSAGE` containing `but found 3`: met.
4. `EXIT_CODE:` non-zero and equal to `ExpectedExitCode:` (1 = 1): met.
5. Every `SANDBOX-` value is `False`: met.
