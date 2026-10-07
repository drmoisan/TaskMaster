# P1-T2 AttachmentSaving Test File Census (Phase One)

Timestamp: 2026-10-03T08-37
Command: Write tool created UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs from Listing L-TAS-P1 (four leading spaces stripped); then CMD-CENSUS with PATHS-TAS and TOKENS-TAS
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: every TOTAL equals the P1 column of TOKENS-TAS; the file has 56 lines. Each per-file TOKEN line equals its TOTAL line (single path).

- TOKEN [TestMethod] @ TOTAL = 0
- TOKEN [DataTestMethod] @ TOTAL = 1
- TOKEN [DataRow( @ TOTAL = 4
- TOKEN Cleanup_Files_ResetsEveryPromptAnswerField @ TOTAL = 5
- TOKEN Cleanup_Files_ResetsEveryPromptSession @ TOTAL = 0
- TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 1
- TOKEN field.SetValue(null,YesNoToAllResponse.YesToAll); @ TOTAL = 1
- TOKEN SetValue( @ TOTAL = 1
- TOKEN SortEmail.SaveAttachmentAsync( @ TOTAL = 0
- TOKEN SortEmail.SaveAttachment( @ TOTAL = 0
- TOKEN SortEmail.RedirectSaveFolder( @ TOTAL = 0
- TOKEN "AllPromptSessions" @ TOTAL = 0
- TOKEN typeof(YesNoToAllPromptSession) @ TOTAL = 0
- TOKEN HaveCount(4) @ TOTAL = 0
- TOKEN OnlyHaveUniqueItems() @ TOTAL = 0
- TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 0
- TOKEN C:\Sortemail959Sandbox @ TOTAL = 0
- TOKEN DoNotParallelize @ TOTAL = 0
- TOKEN Thread.Sleep @ TOTAL = 0
- TOKEN Task.Delay @ TOTAL = 0
- TOKEN Timeout @ TOTAL = 0
- TOKEN Directory.CreateDirectory @ TOTAL = 0
- TOKEN File. @ TOTAL = 0
- TOKEN MemoryAppender @ TOTAL = 0
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 56
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = A2B3B7E1E2DE3A182EDD06FABB600B5B6364FED65E3F407AB2C9C2A76F8740DA

Acceptance check: [DataTestMethod] 1, [DataRow( 4, Cleanup_Files_ResetsEveryPromptAnswerField 5, SortEmail.Cleanup_Files(); 1, field.SetValue(null,YesNoToAllResponse.YesToAll); 1, [TestMethod] 0, banned tokens 0 (DoNotParallelize 0). Holds.
