using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapEntry_Tests
    {
        [TestMethod]
        public void Constructor_WithFolderSubjectCountAndCommonWords_InitializesNormalizedState()
        {
            // Arrange
            IList<string> commonWords = new List<string> { "fwd" };

            // Act
            var entry = new SubjectMapEntry("Inbox\\Reports", "Fwd project plan", 3, commonWords);

            // Assert
            entry.Folderpath.Should().Be("Inbox\\Reports");
            entry.Foldername.Should().Be("Reports");
            entry.EmailSubject.Should().Be("project plan");
            entry.EmailSubjectCount.Should().Be(3);
            entry.FolderWordLengths.Should().Equal("reports".Length);
            entry.SubjectWordLengths.Should().Equal("project".Length, "plan".Length);
        }

        [TestMethod]
        public void Folderpath_Setter_UpdatesFolderNameAndWordLengths()
        {
            // Arrange
            var entry = new SubjectMapEntry("status update", 1, new List<string>());

            // Act
            entry.Folderpath = "Inbox\\Action Items";

            // Assert
            entry.Foldername.Should().Be("Action Items");
            entry.FolderWordLengths.Should().Equal("action".Length, "items".Length);
        }

        [TestMethod]
        public void EmailSubject_Setter_WithNull_ClearsSubjectState()
        {
            // Arrange
            var entry = new SubjectMapEntry("Inbox\\Reports", "Status update", 2, new List<string>());

            // Act
            entry.EmailSubject = null;

            // Assert
            entry.EmailSubject.Should().BeEmpty();
            entry.SubjectWordLengths.Should().BeEmpty();
            entry.SubjectEncoded.Should().BeNull();
        }

        [TestMethod]
        public void Equals_ReturnsTrueOnlyWhenFolderAndSubjectMatch()
        {
            // Arrange
            var left = new SubjectMapEntry("Inbox\\Reports", "Status update", 1, new List<string>());
            var same = new SubjectMapEntry("Inbox\\Reports", "Status update", 5, new List<string>());
            var different = new SubjectMapEntry("Inbox\\Reports", "Different subject", 1, new List<string>());

            // Act
            var sameResult = left.Equals(same);
            var differentResult = left.Equals(different);

            // Assert
            sameResult.Should().BeTrue();
            differentResult.Should().BeFalse();
        }

        [TestMethod]
        public void ReadyToEncode_WithEncoder_AugmentsTokenDictionary()
        {
            // Arrange
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(mock => mock.AugmentTokenDict(It.IsAny<string[]>()))
                .Verifiable();
            var entry = new SubjectMapEntry("Inbox\\Reports", "Status update", 2, new List<string>());

            // Act
            var ready = entry.ReadyToEncode(encoder.Object);

            // Assert
            ready.Should().BeTrue();
            encoder.Verify(mock => mock.AugmentTokenDict(It.Is<string[]>(tokens =>
                tokens.Length == 3 &&
                Array.Exists(tokens, token => token == "reports") &&
                Array.Exists(tokens, token => token == "status") &&
                Array.Exists(tokens, token => token == "update"))), Times.Once);
        }

        [TestMethod]
        public void Encode_WithEncoderAndTokens_ReturnsEncodedValues()
        {
            // Arrange
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(mock => mock.AugmentTokenDict(It.IsAny<string[]>())).Verifiable();
            encoder.Setup(mock => mock.Encode(It.IsAny<string[]>())).Returns(new[] { 10, 20 });
            var entry = new SubjectMapEntry();

            // Act
            var encoded = entry.Encode(encoder.Object, new[] { "alpha", "beta" });

            // Assert
            encoded.Should().Equal(10, 20);
            encoder.Verify(mock => mock.AugmentTokenDict(It.Is<string[]>(tokens => tokens.Length == 2)), Times.Once);
            encoder.Verify(mock => mock.Encode(It.Is<string[]>(tokens => tokens.Length == 2)), Times.Once);
        }

        [TestMethod]
        public void Constructor_WhenCommonWordsStripAllTokens_ThrowsInvalidOperationException()
        {
            // Arrange
            IList<string> commonWords = new List<string> { "fwd" };

            // Act
            Action act = () => new SubjectMapEntry("Fwd", 1, commonWords);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*has no valid tokens*");
        }
    }
}