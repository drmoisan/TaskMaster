# P4-T7 Seam Type Rewrite, Censuses, Scoped Format and Test Build (iteration 2)

Timestamp: 2026-10-03T11-25
Command: Write tool rewrote UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs from Listing L-A-FINAL (revision 1.6 form, PD-14; four leading spaces stripped) with exactly one listing line omitted, `attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;`; Write tool rewrote UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs from Listing L-TSC-FINAL and UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs from Listing L-TAS-FINAL (each after reading it); then CMD-CENSUS three times (PATHS-A with TOKENS-A, PATHS-TSC with TOKENS-TSC, PATHS-TAS with TOKENS-TAS); then CMD-SCOPED-FORMAT (PATHS-A, PATHS-TSC, PATHS-TAS, PATHS-TST1; TASKID p4-t7); then CMD-BUILD-TEST (PROJECT UtilitiesCS.Test, TASKID p4-t7): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation, the printed MSBUILD_EXIT_CODE)
ITERATION: 2
Output Summary: The three files were rewritten from the revision-1.6 listings with the nested non-generic TrySaveAttachmentDelegate seam (PD-14). Every TOKENS-A total equals the SEAMED column (A has 328 lines). Every TOKENS-TSC total and every TOKENS-TAS total equals its FINAL column. The scoped format passed (FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0; A, TSC and TAS rewritten by the formatter, TST1 unchanged). The test build is green (MSBUILD_EXIT_CODE 0, ERROR_LINES 0, ERROR_CODES empty, DLL_ADVANCED True); CS1769 is no longer reported. This artifact supersedes the stop record p4-t7-format-and-build.2026-10-03T10-21.md, which stays on disk unchanged. The compile-red span P4-T1 to P4-T7 is closed.

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
TOKEN FilePathHelperSaveAlt.FolderPath=destinationPath; @ TOTAL = 0
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
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = C562D3ECF9AA3FE31EDBBACCB383EF2E0B90D8D0208DBAFD65FACCC21EBEE072
```

## CMD-CENSUS PATHS-TSC with TOKENS-TSC

```
TOKEN [TestMethod] @ TOTAL = 6
TOKEN [DataTestMethod] @ TOTAL = 3
TOKEN [DataRow( @ TOTAL = 6
TOKEN DisplayName= @ TOTAL = 6
TOKEN SortEmail.SaveCase( @ TOTAL = 3
TOKEN SortEmail.SaveCaseAsync( @ TOTAL = 9
TOKEN newScriptedPrompt( @ TOTAL = 6
TOKEN RecordingSave(saves) @ TOTAL = 9
TOKEN SortEmail.TrySaveAttachmentDelegate @ TOTAL = 1
TOKEN Func<Attachment,string,Task<bool>> @ TOTAL = 0
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN Times.Once @ TOTAL = 2
TOKEN Times.Never @ TOTAL = 3
TOKEN C:\Sortemail959Sandbox @ TOTAL = 2
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 341
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 4EDF0B560F83E7025884BC9B6BC9A2108A97DFE1F9899D906B8753EE59D5E0FA
```

## CMD-CENSUS PATHS-TAS with TOKENS-TAS

```
TOKEN [TestMethod] @ TOTAL = 11
TOKEN [DataTestMethod] @ TOTAL = 0
TOKEN [DataRow( @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptAnswerField @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptSession @ TOTAL = 1
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 0
TOKEN field.SetValue(null,YesNoToAllResponse.YesToAll); @ TOTAL = 0
TOKEN SetValue( @ TOTAL = 0
TOKEN SortEmail.SaveAttachmentAsync( @ TOTAL = 7
TOKEN SortEmail.SaveAttachment( @ TOTAL = 3
TOKEN SortEmail.RedirectSaveFolder( @ TOTAL = 1
TOKEN SortEmail.TrySaveAttachmentDelegate @ TOTAL = 1
TOKEN Func<Attachment,string,Task<bool>> @ TOTAL = 0
TOKEN "AllPromptSessions" @ TOTAL = 1
TOKEN typeof(YesNoToAllPromptSession) @ TOTAL = 1
TOKEN HaveCount(4) @ TOTAL = 2
TOKEN OnlyHaveUniqueItems() @ TOTAL = 1
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN C:\Sortemail959Sandbox @ TOTAL = 3
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 437
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = ADB3E379E0DE8ADE399D750806FB9F59E41F3CAC74A6B75245C307E56ECD923E
```

## CMD-SCOPED-FORMAT output

```
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = C562D3ECF9AA3FE31EDBBACCB383EF2E0B90D8D0208DBAFD65FACCC21EBEE072
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 4EDF0B560F83E7025884BC9B6BC9A2108A97DFE1F9899D906B8753EE59D5E0FA
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = ADB3E379E0DE8ADE399D750806FB9F59E41F3CAC74A6B75245C307E56ECD923E
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 900B58CF0CB9C457EBAF62375D6087452844D880EBF395A1EC259544FA158A7F
Formatted 4 files in 4328ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = C3253F5443354EC5DAA6A65B42805293A5D3B4E7CDD179C339BF46558EC6BC8C
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 915560E406A9CD4C14D6FE06360902D7C0030BE079E0C300EFADCB511E9DB247
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = D1D485E11108E56879CE6E031CBC2E5007AAF5E2E28F59688716F4A602A9A046
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 900B58CF0CB9C457EBAF62375D6087452844D880EBF395A1EC259544FA158A7F
Checked 4 files in 1441ms.
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

## Acceptance (P4-T7, all five required)

1. Every TOKENS-A total equals the SEAMED column (`Func<Attachment,string,Task<bool>>trySave` 0, `TrySaveAttachmentDelegatetrySave` 2, `delegateTask<bool>TrySaveAttachmentDelegate(` 1, `TrySaveAttachmentAsync` 1, `SaveCaseAsync(` 2, `FilePathHelperSaveAlt.FolderPath=destinationPath;` 0, `[ExcludeFromCodeCoverage]` 3, every other row as tabled), and LINES for A is 328 (at most 499): met.
2. Every TOKENS-TSC total and every TOKENS-TAS total equals its FINAL column (`SortEmail.TrySaveAttachmentDelegate` 1 and `Func<Attachment,string,Task<bool>>` 0 in each file; `RecordingSave(saves)` 9 and `SortEmail.SaveCaseAsync(` 9 in TSC; `SortEmail.SaveAttachmentAsync(` 7 and `Cleanup_Files_ResetsEveryPromptSession` 1 in TAS): met.
3. FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0: met.
4. MSBUILD_EXIT_CODE 0 and ERROR_LINES 0 (no CS1769, no CS0121): met.
5. DLL_ADVANCED True: met.

The compile-red span P4-T1 to P4-T7 is closed by this green build.
