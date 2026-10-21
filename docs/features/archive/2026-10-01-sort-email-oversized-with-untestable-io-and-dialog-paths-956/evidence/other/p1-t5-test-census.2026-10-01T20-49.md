# P1-T5 Post-format census of the two new test files

Timestamp: 2026-10-01T20-49
Command: CMD-CENSUS with PATHS-TEST-T and TOKENS-TEST-T; then CMD-CENSUS with PATHS-TEST-S and TOKENS-TEST-S (each path set holds one file, so the per-file print statement was omitted and the TOTAL lines carry the per-file counts)
EXIT_CODE: 0
Output Summary:
## TOKENS-TEST-T (UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs)
TOKEN [TestMethod] @ TOTAL = 11
TOKEN [TestClass] @ TOTAL = 1
TOKEN namespaceUtilitiesCS.Test.EmailIntelligence{ @ TOTAL = 1
TOKEN publicclassSortEmail_TrySaveAttachment_Tests @ TOTAL = 1
TOKEN C:\Sortemail956Sandbox\attachments @ TOTAL = 3
TOKEN TrySaveAttachmentAsync( @ TOTAL = 1
TOKEN SaveAsync(attachment,seams) @ TOTAL = 13
TOKEN newSeams( @ TOTAL = 11
TOKEN newSeams(YesNoToAllResponse.YesToAll) @ TOTAL = 3
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN .Throws(newUnauthorizedAccessException( @ TOTAL = 12
TOKEN .Throws(newIOException( @ TOTAL = 1
TOKEN ThrowAsync<UnauthorizedAccessException>() @ TOTAL = 1
TOKEN ThrowAsync<IOException>() @ TOTAL = 1
TOKEN seams.Session.Response.Should().Be( @ TOTAL = 11
TOKEN newMock<Attachment>(MockBehavior.Loose) @ TOTAL = 11
TOKEN System.Exception @ TOTAL = 1
TOKEN SortEmail. @ TOTAL = 0
TOKEN typeof( @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt @ TOTAL = 0
TOKEN Cleanup_Files @ TOTAL = 0
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN [DataRow @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN Directory. @ TOTAL = 0
TOKEN Path. @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 0
TOKEN GetTemp @ TOTAL = 0
TOKEN Xunit @ TOTAL = 0
TOKEN NUnit @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 375
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645
## TOKENS-TEST-S (UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs)
TOKEN [TestMethod] @ TOTAL = 7
TOKEN namespaceUtilitiesCS.Test.Dialogs{ @ TOTAL = 1
TOKEN publicclassYesNoToAllPromptSession_Tests @ TOTAL = 1
TOKEN newYesNoToAllPromptSession( @ TOTAL = 9
TOKEN WithParameterName( @ TOTAL = 1
TOKEN .ReleaseSingleAnswer(); @ TOTAL = 4
TOKEN .Reset(); @ TOTAL = 1
TOKEN [DataRow @ TOTAL = 0
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN SortEmail @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Xunit @ TOTAL = 0
TOKEN NUnit @ TOTAL = 0
LINES UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 180
SHA256 UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 0A0FBBD8F51CFEE972E60718EA619F9A0FFB1670FA5A76B7635FC74AC4F40C8B
Acceptance: every TOTAL equals its expectation (TOKENS-TEST-T 11, 1, 1, 1, 3, 1, 13, 11, 3, 1, 12, 1, 1, 1, 11, 11, 1 then fifteen zeros; TOKENS-TEST-S 7, 1, 1, 9, 1, 4, 1 then nine zeros); LINES of the TrySave test file is 375 (at most 499; predicted about 375); LINES of the session test file is 180 (at most 499; predicted about 181) (all hold).
