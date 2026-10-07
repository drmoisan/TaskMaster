# P1-T7 post-edit token census and fixed-state anchors

Timestamp: 2026-09-30T12-26
Command: CMD-CENSUS on UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs with TOKENS-SRC; CMD-CENSUS on UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs with TOKENS-TST; CMD-DIFFHASH with MERGE-BASE 039cf779110df3313b3324299d019cabfccce980
EXIT_CODE: 0

Output Summary:
Every `post (fixed state)` value of the Token Census Expectations table holds for both files. Both SHA256 values equal the after-format hashes recorded by P1-T6.

FILE SRC (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs)
TOKEN [ExcludeFromCodeCoverage] = 28
TOKEN TrySaveAttachmentAsync( = 7
TOKEN System.IO.Directory.CreateDirectory( = 1
TOKEN System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave)) = 0
TOKEN createDirectory = 4
TOKEN Action<string>createDirectory = 1
TOKEN staticAction< = 0
TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); = 1
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave); = 0
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory); = 1
TOKEN returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path)); = 1
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 0
TOKEN internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 1
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory) = 1
TOKEN [ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync( = 1
TOKEN [ExcludeFromCodeCoverage]internalstaticasyncTask<bool>TrySaveAttachmentAsync( = 1
TOKEN awaitattachmentHelper.Attachment.TrySaveAttachmentAsync( = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSaveAlt); = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSave); = 1
TOKEN YesNoToAll.ShowDialog( = 9
TOKEN catch(System.UnauthorizedAccessException = 1
LINES = 1454
SHA256 = 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B

FILE TST (UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs)
TOKEN [TestMethod] = 15
TOKEN GetRepositoryRoot() = 5
TOKEN destinationPath=Path.Combine(GetRepositoryRoot() = 0
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = 1
TOKEN C:\Sortemail945Sandbox\attachments = 1
TOKEN attachment.Object.TrySaveAttachmentAsync( = 2
TOKEN TrySaveAttachmentAsync(destinationPath); = 0
TOKEN newIOException( = 1
TOKEN ThrowAsync<IOException>() = 1
TOKEN Times.Never = 1
TOKEN events.Should().Equal( = 1
TOKEN mkdir: = 2
TOKEN save: = 2
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
LINES = 457
SHA256 = 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E

FIX-HASH-SORTEMAIL: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
FIX-HASH-TST: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E
FIX-DIFF-HASH-SORTEMAIL: 9B58FAA848DADF26DEAAFEC963DC05F1243CFAA0FDD4217914F6D66DC5BA1FE0
FIX-DIFF-LINES: 56
SRC-LINES-DELTA: 25 (1454 minus the P0-T12 value 1429; within 1 to 60)
TST LINES: 457 (at most 500)
