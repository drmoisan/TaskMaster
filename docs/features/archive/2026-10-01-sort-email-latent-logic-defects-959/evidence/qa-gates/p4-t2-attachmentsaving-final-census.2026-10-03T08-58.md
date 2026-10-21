# P4-T2 AttachmentSaving Test File Final State Census

Timestamp: 2026-10-03T08-58
Command: Write tool rewrote UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs from Listing L-TAS-FINAL (after reading it; four leading spaces stripped), replacing the phase-one reflection test with the structural test; then CMD-CENSUS with PATHS-TAS and TOKENS-TAS (single path; TOTAL lines)
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: eleven test methods (six asynchronous-core, three synchronous-core, the re-rooting test and the structural cleanup test); no static write remains; every TOTAL equals the FINAL column of TOKENS-TAS; the file has 437 lines.

- TOKEN [TestMethod] @ TOTAL = 11
- TOKEN [DataTestMethod] @ TOTAL = 0
- TOKEN [DataRow( @ TOTAL = 0
- TOKEN Cleanup_Files_ResetsEveryPromptAnswerField @ TOTAL = 0
- TOKEN Cleanup_Files_ResetsEveryPromptSession @ TOTAL = 1
- TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 0
- TOKEN field.SetValue(null,YesNoToAllResponse.YesToAll); @ TOTAL = 0
- TOKEN SetValue( @ TOTAL = 0
- TOKEN SortEmail.SaveAttachmentAsync( @ TOTAL = 7
- TOKEN SortEmail.SaveAttachment( @ TOTAL = 3
- TOKEN SortEmail.RedirectSaveFolder( @ TOTAL = 1
- TOKEN "AllPromptSessions" @ TOTAL = 1
- TOKEN typeof(YesNoToAllPromptSession) @ TOTAL = 1
- TOKEN HaveCount(4) @ TOTAL = 2
- TOKEN OnlyHaveUniqueItems() @ TOTAL = 1
- TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
- TOKEN C:\Sortemail959Sandbox @ TOTAL = 3
- TOKEN DoNotParallelize @ TOTAL = 0
- TOKEN Thread.Sleep @ TOTAL = 0
- TOKEN Task.Delay @ TOTAL = 0
- TOKEN Timeout @ TOTAL = 0
- TOKEN Directory.CreateDirectory @ TOTAL = 0
- TOKEN File. @ TOTAL = 0
- TOKEN MemoryAppender @ TOTAL = 0
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 437
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = FE92FDCB45DEA937269759387F66106A59F8AD15B488FBFF90F6E02C07877100

Acceptance check: every TOTAL equals the FINAL column of TOKENS-TAS ([TestMethod] 11, Cleanup_Files_ResetsEveryPromptAnswerField 0, Cleanup_Files_ResetsEveryPromptSession 1, SetValue( 0, SortEmail.SaveAttachmentAsync( 7, SortEmail.SaveAttachment( 3, SortEmail.RedirectSaveFolder( 1, "AllPromptSessions" 1, HaveCount(4) 2, banned tokens 0). Holds.
