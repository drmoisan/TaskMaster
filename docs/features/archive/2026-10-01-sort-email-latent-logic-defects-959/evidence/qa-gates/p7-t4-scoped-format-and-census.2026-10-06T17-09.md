# P7-T4 Scoped Format and Census of the Two Edited Test Files

Timestamp: 2026-10-06T17-09
Command: (1) CMD-SCOPED-FORMAT (PATHS PATHS-TAS, PATHS-TSC; TASKID p7-t4): dotnet tool run csharpier format then dotnet tool run csharpier check over the two files; (2) CMD-CENSUS (PATHS-TAS; the TOKENS-TAS list plus `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`); (3) CMD-CENSUS (PATHS-TSC; the TOKENS-TSC list plus `usingSystem;`); (4) CMD-LINES (PATHS-CSHARP-FINAL); each run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (scoped to the CMD-LINES payload, the last invocation, its process exit code)
ITERATION: 1
Output Summary: the formatter changed neither file (BEFORE and AFTER hashes equal) and the read-only check passed; every TAS and TSC census total equals the SS4 column; the twelve C# Write Set files are at most 488 lines (TST1), TAS 469 and TSC 342.

## CMD-SCOPED-FORMAT

- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = FBF492B2744725BBB9D71C57175E7F01B252449B68F22CA50EA3F4983B64F1C0
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 90D1A2CC48FE1AD85F49833E3C4E1137165AD3087B160194FBD248751B23F9DC
- Formatter summary (observation): Formatted 2 files in 2088ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = FBF492B2744725BBB9D71C57175E7F01B252449B68F22CA50EA3F4983B64F1C0 (equal to BEFORE)
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 90D1A2CC48FE1AD85F49833E3C4E1137165AD3087B160194FBD248751B23F9DC (equal to BEFORE)
- Check summary (observation): Checked 2 files in 829ms.
- CHECK_EXIT_CODE: 0

## CMD-CENSUS (TAS)

```
TOKEN [TestMethod] @ TOTAL = 12
TOKEN [DataTestMethod] @ TOTAL = 0
TOKEN [DataRow( @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptAnswerField @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptSession @ TOTAL = 1
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 0
TOKEN field.SetValue(null,YesNoToAllResponse.YesToAll); @ TOTAL = 0
TOKEN SetValue( @ TOTAL = 0
TOKEN SortEmail.SaveAttachmentAsync( @ TOTAL = 7
TOKEN SortEmail.SaveAttachment( @ TOTAL = 4
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
TOKEN SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly @ TOTAL = 1
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 469
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = FBF492B2744725BBB9D71C57175E7F01B252449B68F22CA50EA3F4983B64F1C0
```

(One file in the path set, so each per-file `TOKEN ... @ <path> = n` line equals its TOTAL line and is not repeated here.)

## CMD-CENSUS (TSC)

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
TOKEN usingSystem; @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 342
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 90D1A2CC48FE1AD85F49833E3C4E1137165AD3087B160194FBD248751B23F9DC
```

## CMD-LINES (PATHS-CSHARP-FINAL)

```
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 329
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 204
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 190
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 268
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 379
LINES QuickFiler\Controllers\EfcDataModel.cs = 485
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 412
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 342
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 469
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 82
LINES QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 192
MAX-LINES: 488
```

AC23-CLOSEST:
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 469 (predicted 469)
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 342 (predicted 342)
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488 (as P6-T10 recorded)
- LINES QuickFiler\Controllers\EfcDataModel.cs = 485 (as P6-T10 recorded)

## Acceptance (P7-T4, all four required)

1. FORMAT_EXIT_CODE: 0 with both files' BEFORE and AFTER hashes recorded (equal), and CHECK_EXIT_CODE: 0: met.
2. Every TOKENS-TAS total equals the SS4 column ([TestMethod] 12, SortEmail.SaveAttachment( 4, the SS4 name 1, the rest unchanged from FINAL): met.
3. Every TOKENS-TSC total equals the SS4 column (usingSystem; 0 under the P7-T3 edit branch, the rest unchanged from FINAL): met.
4. MAX-LINES: 488 (at most 499) with the twelve LINES rows present and the four AC23-CLOSEST rows quoted: met.
