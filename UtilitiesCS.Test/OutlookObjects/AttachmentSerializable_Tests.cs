using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.EmailParsing;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class AttachmentSerializable_Tests
    {
        [TestMethod]
        public void Constructor_ShouldLeaveReferencePropertiesNull_ByDefault()
        {
            // Arrange
            var attachment = new AttachmentSerializable();

            // Act / Assert
            attachment.DisplayName.Should().BeNull();
            attachment.FileName.Should().BeNull();
            attachment.PathName.Should().BeNull();
            attachment.AttachmentData.Should().BeNull();
            attachment.FileExtension.Should().BeNull();
            attachment.FilenameSeed.Should().BeNull();
            attachment.IsImage.Should().BeFalse();
        }

        [TestMethod]
        public void IsImage_ShouldUseFileExtension_WhenNoExplicitValueWasAssigned()
        {
            // Arrange
            var attachment = new AttachmentSerializable { FileExtension = ".png" };

            // Act
            bool isImage = attachment.IsImage;

            // Assert
            isImage.Should().BeTrue();
        }

        [TestMethod]
        public void AttachmentData_ShouldRoundTripThroughPropertySetter()
        {
            // Arrange
            var attachment = new AttachmentSerializable();
            byte[] expected = [1, 2, 3, 5, 8];

            // Act
            attachment.AttachmentData = expected;

            // Assert
            attachment.AttachmentData.Should().Equal(expected);
        }

        [TestMethod]
        public void ParseFileName_ShouldSplitStandardFileNameIntoSeedAndExtension()
        {
            // Arrange
            var attachment = new AttachmentSerializable();

            // Act
            (string fileNameSeed, string fileExtension) = attachment.ParseFileName("report.pdf");

            // Assert
            fileNameSeed.Should().Be("report");
            fileExtension.Should().Be(".pdf");
        }

        [TestMethod]
        public void ParseFileName_ShouldTreatExtensionOnlyNameAsSeed()
        {
            // Arrange
            var attachment = new AttachmentSerializable();

            // Act
            (string fileNameSeed, string fileExtension) = attachment.ParseFileName(".gitignore");

            // Assert
            fileNameSeed.Should().Be(".gitignore");
            fileExtension.Should().BeEmpty();
        }

        [TestMethod]
        public void JsonSerialization_ShouldRoundTripSerializedProperties()
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
        }
    }
}
