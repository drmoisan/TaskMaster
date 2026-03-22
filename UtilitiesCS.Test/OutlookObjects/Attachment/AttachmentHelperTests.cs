using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.EmailParsing;
using OutlookAttachment = Microsoft.Office.Interop.Outlook.Attachment;

namespace UtilitiesCS.Test.OutlookObjects.Attachment
{
    [TestClass]
    public class AttachmentHelperTests
    {
        [TestMethod]
        public void Constructor_WithAttachment_SetsAttachmentAndSerializableInfo()
        {
            // Arrange
            var attachment = CreateAttachmentMock(
                fileName: "report.pdf",
                displayName: "report.pdf",
                type: OlAttachmentType.olByValue,
                size: 42
            );

            // Act
            var helper = new AttachmentHelper(attachment.Object);

            // Assert
            helper.Attachment.Should().BeSameAs(attachment.Object);
            helper.AttachmentInfo.Should().NotBeNull();
            helper.AttachmentInfo.Type.Should().Be(OlAttachmentType.olByValue);
            helper.AttachmentInfo.Size.Should().Be(42);
        }

        [TestMethod]
        public void Constructor_WithDeleteFolder_InitializesSaveAndDeletePaths()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Local);
            var attachment = CreateAttachmentMock(
                fileName: "budget.xlsx",
                displayName: "budget.xlsx",
                type: OlAttachmentType.olByValue,
                size: 88
            );

            // Act
            var helper = new AttachmentHelper(
                attachment.Object,
                sentOn,
                @"C:\save-root",
                @"C:\delete-root"
            );

            // Assert
            helper.FilePathSave.Should().Be(@"C:\save-root\20260314_budget.xlsx");
            helper.FilePathDelete.Should().Be(@"C:\delete-root\20260314_budget.xlsx");
            helper.AttachmentInfo.Size.Should().Be(88);
        }

        [TestMethod]
        public void AdjustForMaxPath_WhenPathIsWithinLimit_ReturnsCombinedPath()
        {
            // Arrange
            const string folderPath = @"C:\archive";

            // Act
            string result = AttachmentHelper.AdjustForMaxPath(folderPath, "report", ".pdf");

            // Assert
            result.Should().Be(Path.Combine(folderPath, "report.pdf"));
        }

        [TestMethod]
        public void AdjustForMaxPath_WhenPathWouldExceedLimit_TruncatesFilenameSeed()
        {
            // Arrange
            string folderPath = @"C:\" + new string('a', 220);
            string filenameSeed = new string('b', 80);

            // Act
            string result = AttachmentHelper.AdjustForMaxPath(
                folderPath,
                filenameSeed,
                ".pdf",
                "_copy"
            );

            // Assert
            result.Length.Should().BeLessThanOrEqualTo(AttachmentHelper.MAX_PATH);
            Path.GetFileName(result).Should().EndWith("_copy.pdf");
            Path.GetFileNameWithoutExtension(result)
                .Length.Should()
                .BeLessThan(filenameSeed.Length + "_copy".Length);
        }

        [TestMethod]
        public void CheckParameters_WithNullAttachmentAndOversizedPaths_ReturnsFalseWithAllMessages()
        {
            // Arrange
            var helper = new AttachmentHelper();
            string saveFolderPath = new string('s', AttachmentHelper.MAX_PATH - 10);
            string deleteFolderPath = new string('d', AttachmentHelper.MAX_PATH);

            // Act
            bool result = helper.CheckParameters(
                null,
                new DateTime(2026, 3, 14),
                saveFolderPath,
                deleteFolderPath
            );

            // Assert
            result.Should().BeFalse();
            helper
                .ErrorMessages.Should()
                .Contain(message =>
                    message.IndexOf("attachment is null", StringComparison.OrdinalIgnoreCase) >= 0
                );
            helper
                .ErrorMessages.Should()
                .Contain(message => message.IndexOf(saveFolderPath, StringComparison.Ordinal) >= 0);
            helper
                .ErrorMessages.Should()
                .Contain(message =>
                    message.IndexOf(deleteFolderPath, StringComparison.Ordinal) >= 0
                );
        }

        [TestMethod]
        public void CheckParameters_WithValidAttachmentAndShortSavePath_ReturnsTrueWithoutErrors()
        {
            // Arrange
            var helper = new AttachmentHelper();
            var attachment = CreateAttachmentMock(fileName: "valid.txt");

            // Act
            bool result = helper.CheckParameters(
                attachment.Object,
                new DateTime(2026, 3, 14),
                @"C:\save-root"
            );

            // Assert
            result.Should().BeTrue();
            helper.ErrorMessages.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAttachmentFilename_WhenFileNameContainsOnlyExtension_TreatsExtensionAsSeed()
        {
            // Arrange
            var helper = new AttachmentHelper();
            var attachment = CreateAttachmentMock(fileName: ".gitignore");

            // Act
            (string filename, string extension) = helper.GetAttachmentFilename(attachment.Object);

            // Assert
            filename.Should().Be(".gitignore");
            extension.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAttachmentFilename_WhenAttachmentThrows_ReturnsEmptyFilenameAndExtension()
        {
            // Arrange
            var helper = new AttachmentHelper();
            var attachment = new Mock<OutlookAttachment>();
            attachment.SetupGet(x => x.Type).Returns(OlAttachmentType.olByValue);
            attachment.SetupGet(x => x.FileName).Throws(new InvalidOperationException("boom"));

            // Act
            (string filename, string extension) = helper.GetAttachmentFilename(attachment.Object);

            // Assert
            filename.Should().BeEmpty();
            extension.Should().BeEmpty();
        }

        [TestMethod]
        public void Init_WhenByValueAttachment_UsesSanitizedFilenameAndBuildsSaveAndDeletePaths()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Local);
            var attachment = CreateAttachmentMock(
                fileName: "report?.pdf",
                displayName: "display?.pdf",
                type: OlAttachmentType.olByValue,
                size: 123
            );
            var helper = new AttachmentHelper();

            // Act
            helper.Init(attachment.Object, sentOn, @"C:\save-root", @"C:\delete-root");

            // Assert
            helper.AttachmentInfo.Type.Should().Be(OlAttachmentType.olByValue);
            helper.AttachmentInfo.FilenameSeed.Should().Be("20260314_report_");
            helper.AttachmentInfo.FileExtension.Should().Be(".pdf");
            helper.AttachmentInfo.Size.Should().Be(123);
            helper.FilePathSave.Should().Be(@"C:\save-root\20260314_report_.pdf");
            helper.FilePathDelete.Should().Be(@"C:\delete-root\20260314_report_.pdf");
            helper.FilePathSaveAlt.Should().StartWith(@"C:\save-root\20260314_report__");
            helper.FilePathSaveAlt.Should().EndWith(".pdf");
            Regex
                .IsMatch(
                    Path.GetFileNameWithoutExtension(helper.FilePathSaveAlt),
                    "^20260314_report__\\d{14}$"
                )
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void Init_WhenOleAttachmentHasNoUsableNames_FallsBackToUnknownSeed()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 14);
            var attachment = CreateAttachmentMock(
                fileName: string.Empty,
                displayName: string.Empty,
                type: OlAttachmentType.olOLE,
                size: 50
            );
            var helper = new AttachmentHelper();

            // Act
            helper.Init(attachment.Object, sentOn, @"C:\save-root", null);

            // Assert
            helper.AttachmentInfo.FilenameSeed.Should().Be("20260314_unknown");
            helper.AttachmentInfo.FileExtension.Should().BeNull();
            helper.FilePathSave.Should().Be(@"C:\save-root\20260314_unknown");
            helper.FilePathDelete.Should().BeNull();
        }

        [TestMethod]
        public void Init_WhenOleAttachmentHasDisplayName_UsesDisplayNameAsFallbackSeed()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 14);
            var attachment = CreateAttachmentMock(
                fileName: string.Empty,
                displayName: "display?.msg",
                type: OlAttachmentType.olOLE,
                size: 50
            );
            var helper = new AttachmentHelper();

            // Act
            helper.Init(attachment.Object, sentOn, @"C:\save-root", null);

            // Assert
            helper.AttachmentInfo.FilenameSeed.Should().Be("20260314_display_.msg");
            helper.AttachmentInfo.FileExtension.Should().BeNull();
            helper.FilePathSave.Should().Be(@"C:\save-root\20260314_display_.msg");
        }

        [TestMethod]
        public async Task CreateAsync_WhenCalled_ReturnsInitializedHelper()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 14);
            var attachment = CreateAttachmentMock(
                fileName: "async.txt",
                displayName: "async.txt",
                type: OlAttachmentType.olByValue,
                size: 7
            );

            // Act
            AttachmentHelper helper = await AttachmentHelper.CreateAsync(
                attachment.Object,
                sentOn,
                @"C:\save-root",
                @"C:\delete-root"
            );

            // Assert
            helper.Attachment.Should().BeSameAs(attachment.Object);
            helper.FilePathSave.Should().Be(@"C:\save-root\20260314_async.txt");
            helper.FilePathDelete.Should().Be(@"C:\delete-root\20260314_async.txt");
        }

        [TestMethod]
        public void PropertySetters_WhenAssigned_UpdateBackingState()
        {
            // Arrange
            var helper = new AttachmentHelper();
            var attachment = CreateAttachmentMock(fileName: "setter.txt");
            var attachmentInfo = new AttachmentSerializable();

            // Act
            helper.AttachmentInfo = attachmentInfo;
            helper.Attachment = attachment.Object;
            helper.DatePrefix = true;
            helper.FilePathDelete = @"C:\delete-root\setter.txt";
            helper.FolderPathSave = @"C:\save-root";
            helper.FolderPathDelete = @"C:\delete-root";

            // Assert
            helper.AttachmentInfo.Should().BeSameAs(attachmentInfo);
            helper.Attachment.Should().BeSameAs(attachment.Object);
            helper.DatePrefix.Should().BeTrue();
            helper.FilePathDelete.Should().Be(@"C:\delete-root\setter.txt");
            helper.FolderPathSave.Should().Be(@"C:\save-root");
            helper.FolderPathDelete.Should().Be(@"C:\delete-root");
        }

        [TestMethod]
        public void HelperMethods_ReturnDeterministicPrefixAndTimestampSuffixShape()
        {
            // Arrange
            var helper = new AttachmentHelper();

            // Act
            string prefixed = helper.PrependDatePrefix("seed", new DateTime(2026, 3, 14));
            string suffix = helper.GetNameSuffix();

            // Assert
            prefixed.Should().Be("20260314_seed");
            Regex.IsMatch(suffix, "^_\\d{14}$").Should().BeTrue();
        }

        private static Mock<OutlookAttachment> CreateAttachmentMock(
            string fileName,
            string displayName = null,
            OlAttachmentType type = OlAttachmentType.olByValue,
            int size = 1
        )
        {
            var attachment = new Mock<OutlookAttachment>();
            attachment.SetupGet(x => x.Type).Returns(type);
            attachment.SetupGet(x => x.BlockLevel).Returns((OlAttachmentBlockLevel)0);
            attachment.SetupGet(x => x.Class).Returns(OlObjectClass.olAttachment);
            attachment.SetupGet(x => x.DisplayName).Returns(displayName ?? fileName);
            attachment.SetupGet(x => x.FileName).Returns(fileName);
            attachment.SetupGet(x => x.Index).Returns(1);
            attachment
                .SetupGet(x => x.PathName)
                .Returns(Path.Combine(@"C:\temp", fileName ?? string.Empty));
            attachment.SetupGet(x => x.Position).Returns(2);
            attachment.SetupGet(x => x.Size).Returns(size);
            return attachment;
        }
    }
}
