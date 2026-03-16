using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class CtfIncidence_Tests
    {
#pragma warning disable CS0618 // CtfIncidence is intentionally covered while deprecated.
        [TestMethod]
        public void Constructor_WithNoArguments_InitializesDefaults()
        {
            // Arrange

            // Act
            var incidence = new CtfIncidence();

            // Assert
            incidence.MaxFoldersPerConv.Should().Be(3);
            incidence.EmailConversationID.Should().BeNull();
            incidence.FolderCount.Should().Be(0);
            incidence.EmailFolders.Should().NotBeNull().And.BeEmpty();
            incidence.EmailCounts.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithMaxFoldersPerConversation_SetsMaxAndInitializesCollections()
        {
            // Arrange
            const int maxFoldersPerConversation = 7;

            // Act
            var incidence = new CtfIncidence(maxFoldersPerConversation);

            // Assert
            incidence.MaxFoldersPerConv.Should().Be(maxFoldersPerConversation);
            incidence.EmailFolders.Should().NotBeNull().And.BeEmpty();
            incidence.EmailCounts.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithValues_PreservesProvidedState()
        {
            // Arrange
            var expectedFolders = new List<string> { "Inbox", "Archive" };
            var expectedCounts = new List<int> { 5, 2 };

            // Act
            var incidence = new CtfIncidence(
                emailConversationID: "conversation-1",
                folderCount: 2,
                emailFolder: expectedFolders,
                emailConversationCount: expectedCounts);

            // Assert
            incidence.EmailConversationID.Should().Be("conversation-1");
            incidence.FolderCount.Should().Be(2);
            incidence.EmailFolders.Should().BeSameAs(expectedFolders);
            incidence.EmailCounts.Should().BeSameAs(expectedCounts);
        }

        [TestMethod]
        public void Constructor_WithNullAndNegativeValues_AllowsValuesWithoutValidation()
        {
            // Arrange

            // Act
            var incidence = new CtfIncidence(
                emailConversationID: null,
                folderCount: -4,
                emailFolder: null,
                emailConversationCount: null);

            // Assert
            incidence.EmailConversationID.Should().BeNull();
            incidence.FolderCount.Should().Be(-4);
            incidence.EmailFolders.Should().BeNull();
            incidence.EmailCounts.Should().BeNull();
        }

        [TestMethod]
        public void PropertySetters_UpdateAllMutableState()
        {
            // Arrange
            var incidence = new CtfIncidence();
            var folders = new List<string> { "Projects" };
            var counts = new List<int> { 0 };

            // Act
            incidence.MaxFoldersPerConv = int.MaxValue;
            incidence.EmailConversationID = "conversation-2";
            incidence.FolderCount = int.MinValue;
            incidence.EmailFolders = folders;
            incidence.EmailCounts = counts;

            // Assert
            incidence.MaxFoldersPerConv.Should().Be(int.MaxValue);
            incidence.EmailConversationID.Should().Be("conversation-2");
            incidence.FolderCount.Should().Be(int.MinValue);
            incidence.EmailFolders.Should().BeSameAs(folders);
            incidence.EmailCounts.Should().BeSameAs(counts);
        }

        [TestMethod]
        public void Equals_ReturnsReferenceEqualitySemantics()
        {
            // Arrange
            var incidence = new CtfIncidence();

            // Act
            var equalsSelf = incidence.Equals(incidence);
            var equalsDifferentInstance = incidence.Equals(new CtfIncidence());

            // Assert
            equalsSelf.Should().BeTrue();
            equalsDifferentInstance.Should().BeFalse();
        }

        [TestMethod]
        public void CompareTo_ThrowsNotImplementedException()
        {
            // Arrange
            var incidence = new CtfIncidence();

            // Act
            Action act = () => incidence.CompareTo(other: null);

            // Assert
            act.Should().Throw<NotImplementedException>();
        }
#pragma warning restore CS0618
    }
}