using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #614 unit tests for <see cref="ArchiveStemContract"/>, the single authority for the
    /// archive-relative stem contract. The class under test is pure, so no collaborator requires
    /// mocking and Moq is deliberately not used here.
    /// </summary>
    [TestClass]
    public class ArchiveStemContractTests
    {
        private const string MailboxRoot = @"\\mailbox@example.com";
        private const string ArchiveRoot = @"\\mailbox@example.com\Archive";

        [TestMethod]
        public void IsFullOutlookPath_StoreRootedValue_IsTrue()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.IsFullOutlookPath(MailboxRoot);

            // Assert
            outputValue.Should().BeTrue("a store-rooted Outlook path is never a valid stem");
        }

        [TestMethod]
        public void IsFullOutlookPath_SingleBackslashLeadingValue_IsTrue()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.IsFullOutlookPath(@"\Archive\Clients");

            // Assert
            outputValue.Should().BeTrue("a single-separator-leading value is rooted");
        }

        [TestMethod]
        public void IsFullOutlookPath_SingleForwardSlashLeadingValue_IsTrue()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.IsFullOutlookPath("/Archive/Clients");

            // Assert
            outputValue.Should().BeTrue("a forward-separator-leading value is rooted");
        }

        [TestMethod]
        public void IsFullOutlookPath_DriveRootedValue_IsTrue()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.IsFullOutlookPath(
                @"C:\Users\testuser\OneDrive - Contoso"
            );

            // Assert: recorded #614 decision, a volume separator in position 1 is never a stem.
            outputValue.Should().BeTrue("a drive-rooted filesystem path is never a valid stem");
        }

        [TestMethod]
        public void IsFullOutlookPath_RelativeStem_IsFalse()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.IsFullOutlookPath(@"Clients\North");

            // Assert
            outputValue.Should().BeFalse("an archive-relative stem is not a rooted path");
        }

        [TestMethod]
        public void IsFullOutlookPath_NullOrEmpty_IsFalse()
        {
            // Arrange / Act / Assert: emptiness is enforced separately by the Require method.
            ArchiveStemContract.IsFullOutlookPath(null).Should().BeFalse();
            ArchiveStemContract.IsFullOutlookPath(string.Empty).Should().BeFalse();
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_NullValue_ThrowsNamingTheParameter()
        {
            // Arrange
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(null, "DestinationOlStem");

            // Act / Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("DestinationOlStem")
                .And.Message.Should()
                .Contain("DestinationOlStem");
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_EmptyValue_Throws()
        {
            // Arrange
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(string.Empty, "DestinationOlStem");

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("DestinationOlStem");
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_WhitespaceValue_Throws()
        {
            // Arrange
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem("   ", "DestinationOlStem");

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("DestinationOlStem");
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_StoreRootedValue_ThrowsWithoutEmbeddingTheValue()
        {
            // Arrange
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(MailboxRoot, "DestinationOlStem");

            // Act
            ArgumentException thrown = act.Should().Throw<ArgumentException>().Which;

            // Assert: the rule and parameter are named; the value is withheld (#602, AC21).
            thrown.Message.Should().Contain("DestinationOlStem");
            thrown.Message.Should().Contain("relative to the Outlook archive root");
            thrown.Message.Should().NotContain("mailbox@example.com");
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_DriveRootedValue_ThrowsWithoutEmbeddingTheValue()
        {
            // Arrange
            const string driveRooted = @"C:\Users\testuser\OneDrive - Contoso";
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(driveRooted, "fsPath");

            // Act
            ArgumentException thrown = act.Should().Throw<ArgumentException>().Which;

            // Assert
            thrown.Message.Should().Contain("fsPath");
            thrown.Message.Should().NotContain("testuser");
            thrown.Message.Should().NotContain("OneDrive");
        }

        [TestMethod]
        public void RequireArchiveRelativeStem_ValidRelativeStem_DoesNotThrow()
        {
            // Arrange
            Action act = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(
                    @"Clients\North",
                    "DestinationOlStem"
                );

            // Act / Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void TryMakeArchiveRelative_UnderRoot_ReturnsRelativeStemWithoutLeadingSeparator()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot + @"\Clients\North",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue();
            stem.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_ExactRoot_ReturnsTrueWithEmptyStem()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot,
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue();
            stem.Should().BeEmpty();
        }

        [TestMethod]
        public void TryMakeArchiveRelative_OutOfRootAncestor_ReturnsFalseAndDoesNotPassThrough()
        {
            // Arrange / Act: the mailbox store root sits ABOVE the archive root.
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                MailboxRoot,
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeFalse();
            stem.Should().BeEmpty("a failed conversion must never pass the input through");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_CrossStorePath_ReturnsFalse()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                @"\\other@example.org\Archive\Clients",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeFalse();
            stem.Should().BeEmpty();
        }

        [TestMethod]
        public void TryMakeArchiveRelative_CaseDifferingRoot_StillMatches()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                @"\\MAILBOX@EXAMPLE.COM\aRcHiVe\Clients\North",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue("the comparison is OrdinalIgnoreCase");
            stem.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_SeparatorBoundaryNearMiss_ReturnsFalse()
        {
            // Arrange / Act: Archive2 is a sibling of Archive, not a child of it.
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot + @"2\Clients",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeFalse("the prefix match is separator-terminated");
            stem.Should().BeEmpty();
        }

        [TestMethod]
        public void TryMakeArchiveRelative_RepeatedAncestorSubstring_StripsOnlyThePrefix()
        {
            // Arrange / Act: the root name recurs deeper in the path and must survive there.
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot + @"\Clients\Archive\North",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue();
            stem.Should().Be(@"Clients\Archive\North");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_ForwardSeparatorBoundary_IsAccepted()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot + "/Clients",
                ArchiveRoot,
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue();
            stem.Should().Be("Clients");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_TrailingSeparatorOnRoot_IsIgnored()
        {
            // Arrange / Act
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot + @"\Clients",
                ArchiveRoot + @"\",
                out string stem
            );

            // Assert
            outputValue.Should().BeTrue();
            stem.Should().Be("Clients");
        }

        [TestMethod]
        public void TryMakeArchiveRelative_SeparatorOnlyRoot_ReturnsFalse()
        {
            // Arrange / Act: a root consisting only of separators is not whitespace, so it passes
            // the emptiness guard, but it trims to length zero and names no folder.
            bool outputValue = ArchiveStemContract.TryMakeArchiveRelative(
                ArchiveRoot,
                @"\\",
                out string stem
            );

            // Assert
            outputValue.Should().BeFalse("a separator-only root anchors nothing");
            stem.Should().BeEmpty();
        }

        [TestMethod]
        public void TryMakeArchiveRelative_EmptyOrNullInputs_ReturnFalse()
        {
            // Arrange / Act / Assert
            ArchiveStemContract
                .TryMakeArchiveRelative(null, ArchiveRoot, out string nullStem)
                .Should()
                .BeFalse();
            nullStem.Should().BeEmpty();
            ArchiveStemContract
                .TryMakeArchiveRelative(ArchiveRoot, null, out string nullRootStem)
                .Should()
                .BeFalse();
            nullRootStem.Should().BeEmpty();
            ArchiveStemContract
                .TryMakeArchiveRelative(ArchiveRoot, string.Empty, out string emptyRootStem)
                .Should()
                .BeFalse();
            emptyRootStem.Should().BeEmpty();
        }
    }
}
