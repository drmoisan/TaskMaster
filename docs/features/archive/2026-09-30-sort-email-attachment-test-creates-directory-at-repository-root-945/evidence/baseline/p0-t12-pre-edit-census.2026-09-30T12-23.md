# P0-T12 pre-edit token census

Timestamp: 2026-09-30T12-23
Command: CMD-CENSUS with PATH UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs and TOKENS-SRC; CMD-CENSUS with PATH UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs and TOKENS-TST
EXIT_CODE: 0

Output Summary:
All `pre` values of the Token Census Expectations table match for both files. SRC and TST SHA256 equal PRE-EDIT-HASH-SORTEMAIL and PRE-EDIT-HASH-TST of P0-T4. TST `[TestMethod]` 14 equals the test-run-baseline.md COUNTERS total 14.

FILE SRC (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs)
TOKEN [ExcludeFromCodeCoverage] = 27
TOKEN TrySaveAttachmentAsync( = 5
TOKEN System.IO.Directory.CreateDirectory( = 1
TOKEN System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave)) = 1
TOKEN createDirectory = 0
TOKEN Action<string>createDirectory = 0
TOKEN staticAction< = 0
TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); = 0
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave); = 1
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory); = 0
TOKEN returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path)); = 0
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 1
TOKEN internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 0
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory) = 0
TOKEN [ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync( = 0
TOKEN [ExcludeFromCodeCoverage]internalstaticasyncTask<bool>TrySaveAttachmentAsync( = 1
TOKEN awaitattachmentHelper.Attachment.TrySaveAttachmentAsync( = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSaveAlt); = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSave); = 1
TOKEN YesNoToAll.ShowDialog( = 9
TOKEN catch(System.UnauthorizedAccessException = 1
LINES = 1429
SHA256 = FBB07E251FAC8C3BA488FADBD9A8DD65B7C100ECE26F3231B24A44014D1B1448

FILE TST (UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs)
TOKEN [TestMethod] = 14
TOKEN GetRepositoryRoot() = 6
TOKEN destinationPath=Path.Combine(GetRepositoryRoot() = 1
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = 0
TOKEN C:\Sortemail945Sandbox\attachments = 0
TOKEN attachment.Object.TrySaveAttachmentAsync( = 1
TOKEN TrySaveAttachmentAsync(destinationPath); = 1
TOKEN newIOException( = 0
TOKEN ThrowAsync<IOException>() = 0
TOKEN Times.Never = 0
TOKEN events.Should().Equal( = 0
TOKEN mkdir: = 0
TOKEN save: = 0
TOKEN Directory.CreateDirectory = 0
TOKEN Path.GetTemp = 0
TOKEN File.Create = 0
TOKEN File.WriteAll = 0
TOKEN UnauthorizedAccessException = 0
TOKEN Thread.Sleep = 0
TOKEN Task.Delay = 0
TOKEN DoNotParallelize = 0
TOKEN File.Exists( = 1
TOKEN TaskMaster.sln = 1
LINES = 417
SHA256 = 440D5A99678FE7B8093BE56388364448A0A9B813C47E2DF79E7391C32E37087C
