using System.IO;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.EmailParsing;

namespace UtilitiesCS.Test.OutlookObjects.Attachment
{
    [TestClass]
    public class AttachmentSerializableTests
    {
        [TestMethod]
        public void Constructor_WithoutAttachment_LeavesSerializedDefaultsUnset()
        {
            // Arrange / Act
            var attachment = new AttachmentSerializable();

            // Assert
            attachment.DisplayName.Should().BeNull();
            attachment.FileName.Should().BeNull();
            attachment.PathName.Should().BeNull();
            attachment.AttachmentData.Should().BeNull();
            attachment.FileExtension.Should().BeNull();
            attachment.FilenameSeed.Should().BeNull();
            attachment.IsImage.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithAttachment_ProjectsAttachmentMetadataAndReferences()
        {
            // Arrange
            var outlookAttachment = new Mock<Microsoft.Office.Interop.Outlook.Attachment>();
            var application = new Mock<Application>();
            var accessor = new Mock<PropertyAccessor>();
            var session = new Mock<NameSpace>();
            var parent = new object();

            outlookAttachment.SetupGet(x => x.Type).Returns(OlAttachmentType.olByValue);
            outlookAttachment
                .SetupGet(x => x.BlockLevel)
                .Returns(OlAttachmentBlockLevel.olAttachmentBlockLevelNone);
            outlookAttachment.SetupGet(x => x.Class).Returns(OlObjectClass.olAttachment);
            outlookAttachment.SetupGet(x => x.DisplayName).Returns("chart.png");
            outlookAttachment.SetupGet(x => x.FileName).Returns("chart.png");
            outlookAttachment.SetupGet(x => x.Index).Returns(2);
            outlookAttachment.SetupGet(x => x.PathName).Returns(@"C:\mail\chart.png");
            outlookAttachment.SetupGet(x => x.Position).Returns(4);
            outlookAttachment.SetupGet(x => x.Size).Returns(128);
            outlookAttachment.SetupGet(x => x.Application).Returns(application.Object);
            outlookAttachment.SetupGet(x => x.Parent).Returns(parent);
            outlookAttachment.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);
            outlookAttachment.SetupGet(x => x.Session).Returns(session.Object);

            // Act
            var attachment = new AttachmentSerializable(outlookAttachment.Object);

            // Assert
            attachment.Type.Should().Be(OlAttachmentType.olByValue);
            attachment.BlockLevel.Should().Be(OlAttachmentBlockLevel.olAttachmentBlockLevelNone);
            attachment.Class.Should().Be(OlObjectClass.olAttachment);
            attachment.DisplayName.Should().Be("chart.png");
            attachment.FileName.Should().Be("chart.png");
            attachment.Index.Should().Be(2);
            attachment.PathName.Should().Be(@"C:\mail\chart.png");
            attachment.Position.Should().Be(4);
            attachment.Size.Should().Be(128);
            attachment.Application.Should().BeSameAs(application.Object);
            attachment.Parent.Should().BeSameAs(parent);
            attachment.PropertyAccessor.Should().BeSameAs(accessor.Object);
            attachment.Session.Should().BeSameAs(session.Object);
            attachment.ImageBytesOnly.Should().BeTrue();
        }

        [TestMethod]
        public void IsImage_UsesFileExtension_WhenNoExplicitValueWasAssigned()
        {
            // Arrange
            var attachment = new AttachmentSerializable { FileExtension = ".png" };

            // Act
            bool isImage = attachment.IsImage;

            // Assert
            isImage.Should().BeTrue();
        }

        [TestMethod]
        public void AttachmentDataAndGetStream_WorkWithInMemoryBytesOnly()
        {
            // Arrange
            var attachment = new AttachmentSerializable();
            byte[] expected = [1, 2, 3, 5, 8];

            // Act
            attachment.AttachmentData = expected;
            using MemoryStream stream = attachment.GetStream(attachment.AttachmentData);

            // Assert
            attachment.AttachmentData.Should().Equal(expected);
            stream.ToArray().Should().Equal(expected);
        }

        [TestMethod]
        [DataRow("report.pdf", "report", ".pdf")]
        [DataRow(".gitignore", ".gitignore", "")]
        public void ParseFileName_SplitsSeedAndExtensionAsExpected(
            string fileName,
            string expectedSeed,
            string expectedExtension
        )
        {
            // Arrange
            var attachment = new AttachmentSerializable();

            // Act
            (string fileNameSeed, string fileExtension) = attachment.ParseFileName(fileName);

            // Assert
            fileNameSeed.Should().Be(expectedSeed);
            fileExtension.Should().Be(expectedExtension);
        }

        [TestMethod]
        public void IsAnImage_WhenPngExtension_ShouldReturnTrue()
        {
            var attachment = new AttachmentSerializable { FileExtension = ".png" };
            attachment.IsAnImage().Should().BeTrue();
        }

        [TestMethod]
        [DataRow(".jpg")]
        [DataRow(".jpeg")]
        [DataRow(".gif")]
        [DataRow(".bmp")]
        public void IsAnImage_WhenImageExtension_ShouldReturnTrue(string ext)
        {
            var attachment = new AttachmentSerializable { FileExtension = ext };
            attachment.IsAnImage().Should().BeTrue();
        }

        [TestMethod]
        public void IsAnImage_WhenPdfExtension_ShouldReturnFalse()
        {
            var attachment = new AttachmentSerializable { FileExtension = ".pdf" };
            attachment.IsAnImage().Should().BeFalse();
        }

        [TestMethod]
        public void IsAnImage_WhenNullExtension_ShouldReturnFalse()
        {
            var attachment = new AttachmentSerializable { FileExtension = null };
            attachment.IsAnImage().Should().BeFalse();
        }

        [TestMethod]
        public void TryFromAccessor_WhenAccessorReturnsBytes_ShouldReturnTrueAndSetBytes()
        {
            var expected = new byte[] { 1, 2, 3 };
            var accessor = new Mock<PropertyAccessor>();
            accessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x37010102"))
                .Returns(expected);
            var outlookAttachment = new Mock<Microsoft.Office.Interop.Outlook.Attachment>();
            outlookAttachment.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);

            var attachment = new AttachmentSerializable();
            var result = attachment.TryFromAccessor(outlookAttachment.Object, out byte[] bytes);

            result.Should().BeTrue();
            bytes.Should().Equal(expected);
        }

        [TestMethod]
        public void TryFromAccessor_WhenAccessorThrows_ShouldReturnFalse()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor
                .Setup(x => x.GetProperty(It.IsAny<string>()))
                .Throws(new System.InvalidOperationException("COM error"));
            var outlookAttachment = new Mock<Microsoft.Office.Interop.Outlook.Attachment>();
            outlookAttachment.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);

            var attachment = new AttachmentSerializable();
            var result = attachment.TryFromAccessor(outlookAttachment.Object, out byte[] bytes);

            result.Should().BeFalse();
            bytes.Should().BeNull();
        }

        [TestMethod]
        public void JsonSerialization_RoundTripsSerializedProperties_AndOmitsRuntimeOnlyReferences()
        {
            // Arrange
            var original = new AttachmentSerializable
            {
                DisplayName = "Chart",
                FileName = "chart.png",
                FileExtension = ".png",
                FilenameSeed = "chart",
                PathName = null,
                Index = 2,
                Position = 4,
                Size = 128,
                AttachmentData = [10, 20, 30],
                IsImage = true,
                Application = new Mock<Application>().Object,
                Session = new Mock<NameSpace>().Object,
                ImageBytesOnly = true,
            };

            // Act
            string json = JsonConvert.SerializeObject(original);
            AttachmentSerializable clone = JsonConvert.DeserializeObject<AttachmentSerializable>(
                json
            );

            // Assert
            clone.Should().NotBeNull();
            clone.DisplayName.Should().Be("Chart");
            clone.FileName.Should().Be("chart.png");
            clone.FileExtension.Should().Be(".png");
            clone.FilenameSeed.Should().Be("chart");
            clone.PathName.Should().BeNull();
            clone.Index.Should().Be(2);
            clone.Position.Should().Be(4);
            clone.Size.Should().Be(128);
            clone.AttachmentData.Should().Equal((byte[])[10, 20, 30]);
            clone.IsImage.Should().BeTrue();
            clone.Application.Should().BeNull();
            clone.Session.Should().BeNull();
        }
    }
}
