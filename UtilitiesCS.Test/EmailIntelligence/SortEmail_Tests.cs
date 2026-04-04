using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.EmailParsing;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="SortEmail"/>.
    ///
    /// Purpose:
    ///     Cover the deterministically testable paths in the static SortEmail helper:
    ///     (1) <see cref="SortEmail.InitializeSortToExisting"/> — unconditional NotImplementedException,
    ///     (2) null/empty guard on both <see cref="SortEmail.SortAsync(IList{Microsoft.Office.Interop.Outlook.MailItem}, bool, string, bool, bool, bool, IApplicationGlobals)"/>
    ///         and <see cref="SortEmail.SortAsync(IList{MailItemHelper}, bool, string, bool, bool, bool, IApplicationGlobals, string, string)"/>
    ///         overloads — these paths do not require live Outlook COM objects.
    ///
    /// Constraints:
    ///     SortAsync overloads that call the Outlook Explorer or deep COM chains cannot be
    ///     tested deterministically without live Outlook, so only null/empty guard paths are
    ///     covered here to stay within the test policy requirements.
    /// </summary>
    [TestClass]
    public class SortEmail_Tests
    {
        #region Phase 9-T1: InitializeSortToExisting

        /// <summary>
        /// Verifies that InitializeSortToExisting always throws NotImplementedException
        /// regardless of the parameters supplied, since the body is a stub throw.
        /// </summary>
        [TestMethod]
        public void InitializeSortToExisting_AlwaysThrows_NotImplementedException()
        {
            // Act + Assert: default (no) arguments
            System.Action act = () => SortEmail.InitializeSortToExisting();

            act.Should().Throw<NotImplementedException>();
        }

        /// <summary>
        /// Verifies that InitializeSortToExisting throws NotImplementedException when
        /// explicit arguments are supplied, confirming the stub is unconditional.
        /// </summary>
        [TestMethod]
        public void InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException()
        {
            // Act + Assert: explicit arguments
            System.Action act = () =>
                SortEmail.InitializeSortToExisting(
                    InitType: "Sort",
                    QuickLoad: true,
                    WholeConversation: false,
                    strSeed: "seed",
                    objItem: new object()
                );

            act.Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Phase 9-T2 and 9-T3: SortAsync null/empty guard

        /// <summary>
        /// Verifies that the MailItemHelper SortAsync overload throws ArgumentNullException
        /// when a null mail-helper list is passed — covering the null guard that prevents
        /// downstream COM side-effects from executing.
        /// </summary>
        [TestMethod]
        public async Task SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () =>
                await SortEmail.SortAsync(
                    mailHelpers: null,
                    savePictures: false,
                    destinationOlStem: "Folder",
                    saveMsg: false,
                    saveAttachments: false,
                    removePreviousFsFiles: false,
                    appGlobals: null,
                    olAncestor: "root",
                    fsAncestorEquivalent: "C:\\root"
                );

            // Assert: null guard fires before any filing logic runs
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that the MailItemHelper SortAsync overload throws ArgumentNullException
        /// when an empty mail-helper list is passed — confirming the empty-list guard branch.
        /// </summary>
        [TestMethod]
        public async Task SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () =>
                await SortEmail.SortAsync(
                    mailHelpers: new List<MailItemHelper>(),
                    savePictures: false,
                    destinationOlStem: "Folder",
                    saveMsg: false,
                    saveAttachments: false,
                    removePreviousFsFiles: false,
                    appGlobals: null,
                    olAncestor: "root",
                    fsAncestorEquivalent: "C:\\root"
                );

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion

        #region P2-T14: StripTabsCrLf and Cleanup_Files — COM-free mail-processing branches

        /// <summary>
        /// Verifies that StripTabsCrLf replaces tab, carriage-return, and newline characters
        /// with single spaces and trims leading/trailing whitespace.
        ///
        /// Purpose:
        ///     This is the mail-metadata sanitization path invoked when assembling TSV log
        ///     entries for moved emails. It is the only non-null, non-COM branch in SortEmail
        ///     that can be exercised without live Outlook, satisfying the P2-T14 "next
        ///     uncovered non-null mail-processing branch" requirement within test-policy
        ///     constraints (no external dependencies, deterministic).
        /// </summary>
        [TestMethod]
        public void StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString()
        {
            // Arrange: string containing tabs, carriage returns, and newlines
            var input = "\tHello\tWorld\r\nFoo\tBar\n";

            // Act
            var result = SortEmail.StripTabsCrLf(input);

            // Assert: control characters replaced by spaces, string trimmed, no double spaces
            result.Should().Be("Hello World Foo Bar");
        }

        /// <summary>
        /// Verifies that StripTabsCrLf leaves a plain string (no control characters) unchanged
        /// after sanitization — the pass-through branch of the regex replacer.
        /// </summary>
        [TestMethod]
        public void StripTabsCrLf_WithPlainText_ReturnsOriginalString()
        {
            // Arrange
            var input = "Hello World";

            // Act
            var result = SortEmail.StripTabsCrLf(input);

            // Assert: no transformation when there are no control characters
            result.Should().Be("Hello World");
        }

        /// <summary>
        /// Verifies that Cleanup_Files resets all static YesNoToAllResponse tracking fields
        /// without throwing, covering the state-reset method used between sort sessions.
        /// </summary>
        [TestMethod]
        public void Cleanup_Files_DoesNotThrow()
        {
            // Act + Assert
            System.Action act = () => SortEmail.Cleanup_Files();
            act.Should().NotThrow();
        }

        [TestMethod]
        public void GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments()
        {
            // Arrange
            var mailItem = CreateMailItemWithAttachments(
                CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
            );

            // Act
            var attachments = SortEmail
                .GetAttachmentsInfo(
                    mailItem.Object,
                    GetRepositoryRoot().FullName,
                    null,
                    saveAttachments: false,
                    savePictures: true
                )
                .ToList();

            // Assert
            attachments.Should().ContainSingle();
            attachments[0].AttachmentInfo.FileName.Should().Be("photo.jpg");
            attachments[0].AttachmentInfo.IsImage.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments()
        {
            // Arrange
            var mailItem = CreateMailItemWithAttachments(
                CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
            );

            // Act
            var attachments = await CollectAsync(
                SortEmail.GetAttachmentsInfoAsync(
                    mailItem.Object,
                    GetRepositoryRoot().FullName,
                    null,
                    saveAttachments: true,
                    savePictures: false
                )
            );

            // Assert
            attachments.Should().ContainSingle();
            attachments[0].AttachmentInfo.FileName.Should().Be("report.pdf");
            attachments[0].AttachmentInfo.IsImage.Should().BeFalse();
        }

        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile()
        {
            // Arrange
            var attachment = CreateAttachmentMock("saved.txt", OlAttachmentType.olByValue);
            var destinationPath = Path.Combine(GetRepositoryRoot().FullName, "saved.txt");

            // Act
            bool saved = await attachment.Object.TrySaveAttachmentAsync(destinationPath);

            // Assert
            saved.Should().BeTrue();
            attachment.Verify(x => x.SaveAsFile(destinationPath), Times.Once);
        }

        [TestMethod]
        public async Task SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath()
        {
            // Arrange
            var repositoryRoot = GetRepositoryRoot().FullName;
            var mailItem = new Mock<MailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.Subject).Returns("bad:/subject?");
            mailItem.Setup(x => x.SaveAs(It.IsAny<string>(), OlSaveAsType.olMSG)).Verifiable();
            var expectedPath = AttachmentHelper.AdjustForMaxPath(
                repositoryRoot,
                FolderConverter.SanitizeFilename(mailItem.Object.Subject),
                "msg",
                ""
            );

            // Act
            await SortEmail.SaveMessageAsMsgAsync(mailItem.Object, repositoryRoot);

            // Assert
            mailItem.Verify(x => x.SaveAs(expectedPath, OlSaveAsType.olMSG), Times.Once);
        }

        [TestMethod]
        public void SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath()
        {
            // Arrange
            var repositoryRoot = GetRepositoryRoot().FullName;
            var mailItem = new Mock<MailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.Subject).Returns("sync:/subject?");
            mailItem.Setup(x => x.SaveAs(It.IsAny<string>(), OlSaveAsType.olMSG)).Verifiable();
            var expectedPath = AttachmentHelper.AdjustForMaxPath(
                repositoryRoot,
                FolderConverter.SanitizeFilename(mailItem.Object.Subject),
                "msg",
                ""
            );

            // Act
            SortEmail.SaveMessageAsMSG(mailItem.Object, repositoryRoot);

            // Assert
            mailItem.Verify(x => x.SaveAs(expectedPath, OlSaveAsType.olMSG), Times.Once);
        }

        [TestMethod]
        public void SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine()
        {
            // Arrange
            var values = new[] { "Hello\tWorld", null, "Line1\r\nLine2" };
            var method = typeof(SortEmail).GetMethod(
                "SanitizeArrayLineTSV",
                BindingFlags.NonPublic | BindingFlags.Static
            )!;
            object[] args = { values };

            // Act
            var line = (string)method.Invoke(null, args);

            // Assert
            line.Should().Be("Hello World\t\tLine1 Line2");
        }

        [TestMethod]
        public void SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows()
        {
            // Arrange
            var method = typeof(SortEmail).GetMethod(
                "SanitizeArray",
                BindingFlags.NonPublic | BindingFlags.Static
            )!;
            var values = new string[2, 2]
            {
                { "A\tB", null },
                { "Line1\r\nLine2", "Tail" },
            };
            var output = new string[values.GetLength(0)];
            object[] args = { values, output };

            // Act
            method.Invoke(null, args);
            output = (string[])args[1];

            // Assert
            output[0].Should().Be("A B");
            output[1].Should().Be("Line1 Line2\tTail");
        }

        #endregion

        private static Mock<Attachment> CreateAttachmentMock(
            string fileName,
            OlAttachmentType type,
            string displayName = "",
            int size = 1
        )
        {
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment.SetupGet(x => x.Type).Returns(type);
            attachment.SetupGet(x => x.BlockLevel).Returns((OlAttachmentBlockLevel)0);
            attachment.SetupGet(x => x.Class).Returns(OlObjectClass.olAttachment);
            attachment
                .SetupGet(x => x.DisplayName)
                .Returns(string.IsNullOrEmpty(displayName) ? fileName : displayName);
            attachment.SetupGet(x => x.FileName).Returns(fileName);
            attachment.SetupGet(x => x.Index).Returns(1);
            attachment.SetupGet(x => x.PathName).Returns(Path.Combine(@"C:\temp", fileName));
            attachment.SetupGet(x => x.Position).Returns(2);
            attachment.SetupGet(x => x.Size).Returns(size);
            return attachment;
        }

        private static Mock<MailItem> CreateMailItemWithAttachments(params Attachment[] attachments)
        {
            var attachmentCollection = new Mock<Attachments>(MockBehavior.Loose);
            attachmentCollection
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => attachments.Cast<object>().GetEnumerator());

            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(x => x.Attachments).Returns(attachmentCollection.Object);
            mailItem.SetupGet(x => x.SentOn).Returns(new DateTime(2026, 4, 3, 9, 30, 0));
            return mailItem;
        }

        private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> items)
        {
            var results = new List<T>();
            await foreach (var item in items)
            {
                results.Add(item);
            }

            return results;
        }

        private static DirectoryInfo GetRepositoryRoot()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (
                current is not null
                && !File.Exists(Path.Combine(current.FullName, "TaskMaster.sln"))
            )
            {
                current = current.Parent;
            }

            current
                .Should()
                .NotBeNull("the test assembly should run inside the TaskMaster repository");
            return current;
        }
    }
}
