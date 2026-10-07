# P4-T10 Second Re-Rooting Statement (D6 Step Two), Census, Scoped Format and Test Build

Timestamp: 2026-10-03T12-11
Command: Edit E-A-RR-SECOND applied to UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs (adds `attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;` after `attachmentHelper.FolderPathSave = destinationPath;` in `RedirectSaveFolder`); then CMD-CENSUS (PATHS-A with TOKENS-A); then CMD-SCOPED-FORMAT (PATHS-A; TASKID p4-t10); then CMD-BUILD-TEST (PROJECT UtilitiesCS.Test, TASKID p4-t10): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation, the printed MSBUILD_EXIT_CODE)
Output Summary: The Edit anchor occurred once and the second re-rooting statement was added. Every TOKENS-A total equals the FINAL column (`FilePathHelperSaveAlt.FolderPath=destinationPath;` 1, every other value as in SEAMED); A has 329 lines. The scoped format passed with no rewrite (BEFORE and AFTER hashes equal; FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0). The test build is green (MSBUILD_EXIT_CODE 0, ERROR_LINES 0, ERROR_CODES empty, DLL_ADVANCED True). The AFTER hash of A recorded here is 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2 (the P4-T13 `FIX-HASH-A:` reference).

## CMD-CENSUS PATHS-A with TOKENS-A (TOTAL lines; the single per-file line of each token carries the same value)

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
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 329
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
```

## CMD-SCOPED-FORMAT output

```
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
Formatted 1 files in 1126ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
Checked 1 files in 485ms.
CHECK_EXIT_CODE: 0
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

## Acceptance (P4-T10, all four required)

1. Every TOKENS-A total equals the FINAL column (`FilePathHelperSaveAlt.FolderPath=destinationPath;` 1, every other value as in SEAMED): met.
2. FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0: met.
3. MSBUILD_EXIT_CODE 0 and ERROR_LINES 0: met.
4. DLL_ADVANCED True: met.
