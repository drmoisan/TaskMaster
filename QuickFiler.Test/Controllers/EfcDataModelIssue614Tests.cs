using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #614 tests for <see cref="EfcDataModel.ToArchiveRelativeStem"/> (defect D8): the
    /// pure stem derivation that replaced an unanchored Replace plus a single Substring(1).
    /// Reachable through the assembly's existing InternalsVisibleTo("QuickFiler.Test").
    /// </summary>
    [TestClass]
    public class EfcDataModelIssue614Tests
    {
        private const string ArchiveRoot = @"\\mailbox@example.com\Archive";

        [TestMethod]
        public void ToArchiveRelativeStem_UnderRootFolder_ReturnsTheRelativeStem()
        {
            // Arrange / Act
            string stem = EfcDataModel.ToArchiveRelativeStem(
                ArchiveRoot + @"\Clients\North",
                ArchiveRoot
            );

            // Assert
            stem.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void ToArchiveRelativeStem_UnderRootFolderFromAMapiFolderSeam_ReturnsTheStem()
        {
            // Arrange: the non-static path reads FolderPath from the Outlook seam.
            var folder = new Mock<MAPIFolder>();
            folder.SetupGet(value => value.FolderPath).Returns(ArchiveRoot + @"\Clients");

            // Act
            string stem = EfcDataModel.ToArchiveRelativeStem(folder.Object.FolderPath, ArchiveRoot);

            // Assert
            stem.Should().Be("Clients");
        }

        [TestMethod]
        public void ToArchiveRelativeStem_StoreRootFolder_ThrowsWithoutLeakingIdentifiers()
        {
            // Arrange: the mailbox store root sits ABOVE the archive root.
            System.Action act = () =>
                EfcDataModel.ToArchiveRelativeStem(@"\\mailbox@example.com", ArchiveRoot);

            // Act
            ArgumentException thrown = act.Should().Throw<ArgumentException>().Which;

            // Assert
            thrown.Message.Should().NotContain("mailbox@example.com");
        }

        [TestMethod]
        public void ToArchiveRelativeStem_ArchiveRootItself_Throws()
        {
            // Arrange: an empty stem would file to the archive root, which is not a destination.
            System.Action act = () => EfcDataModel.ToArchiveRelativeStem(ArchiveRoot, ArchiveRoot);

            // Act / Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ToArchiveRelativeStem_CrossStoreFolder_Throws()
        {
            // Arrange
            var folder = new Mock<Folder>();
            folder
                .SetupGet(value => value.FolderPath)
                .Returns(@"\\other@example.org\Archive\Clients");
            System.Action act = () =>
                EfcDataModel.ToArchiveRelativeStem(folder.Object.FolderPath, ArchiveRoot);

            // Act / Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ToArchiveRelativeStem_CaseDifferingAncestor_StillMatches()
        {
            // Arrange / Act
            string stem = EfcDataModel.ToArchiveRelativeStem(
                @"\\MAILBOX@EXAMPLE.COM\aRcHiVe\Clients\North",
                ArchiveRoot
            );

            // Assert
            stem.Should().Be(@"Clients\North", "the ancestor match is OrdinalIgnoreCase");
        }

        [TestMethod]
        public void ToArchiveRelativeStem_SeparatorBoundaryNearMiss_Throws()
        {
            // Arrange: "Archive2" is a sibling of "Archive", not a folder inside it.
            System.Action act = () =>
                EfcDataModel.ToArchiveRelativeStem(ArchiveRoot + @"2\Clients", ArchiveRoot);

            // Act / Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ToArchiveRelativeStem_RepeatedAncestorSubstring_StripsOnlyThePrefix()
        {
            // Arrange / Act
            string stem = EfcDataModel.ToArchiveRelativeStem(
                ArchiveRoot + @"\Clients\Archive\North",
                ArchiveRoot
            );

            // Assert
            stem.Should().Be(@"Clients\Archive\North");
        }
    }

    [TestClass]
    public class EfcDataModelIssue637Tests
    {
        private const string ArchiveRoot = @"\\mailbox@example.com\Archive";

        [TestMethod]
        public void ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem()
        {
            EfcDataModel
                .ToFilingStemOrVerbatim(ArchiveRoot + @"\Clients\North", ArchiveRoot)
                .Should()
                .Be(@"Clients\North");
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem()
        {
            EfcDataModel
                .ToFilingStemOrVerbatim(@"\\MAILBOX@EXAMPLE.COM\aRcHiVe\Clients", ArchiveRoot)
                .Should()
                .Be("Clients");
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim()
        {
            EfcDataModel
                .ToFilingStemOrVerbatim(@"Clients\North", ArchiveRoot)
                .Should()
                .Be(@"Clients\North");
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim()
        {
            EfcDataModel
                .ToFilingStemOrVerbatim("Trash to Delete", ArchiveRoot)
                .Should()
                .Be("Trash to Delete");
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_ArchiveRootExact_ReturnsTheInputVerbatimAndDoesNotThrow()
        {
            EfcDataModel.ToFilingStemOrVerbatim(ArchiveRoot, ArchiveRoot).Should().Be(ArchiveRoot);
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_OutOfRootRootedInput_ReturnsTheInputVerbatimAndDoesNotThrow()
        {
            const string candidate = @"\\other@example.org\Archive\Clients";
            EfcDataModel.ToFilingStemOrVerbatim(candidate, ArchiveRoot).Should().Be(candidate);
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_NullEmptyWhitespaceOrSeparatorOnlyAncestor_ReturnsTheInputVerbatim()
        {
            const string candidate = @"\\mailbox@example.com\Archive\Clients";

            EfcDataModel.ToFilingStemOrVerbatim(candidate, null).Should().Be(candidate);
            EfcDataModel.ToFilingStemOrVerbatim(candidate, string.Empty).Should().Be(candidate);
            EfcDataModel.ToFilingStemOrVerbatim(candidate, " ").Should().Be(candidate);
            EfcDataModel.ToFilingStemOrVerbatim(candidate, @"\").Should().Be(candidate);
        }

        [TestMethod]
        public void ToFilingStemOrVerbatim_NullOrEmptyCandidate_ReturnsTheInputVerbatim()
        {
            EfcDataModel.ToFilingStemOrVerbatim(null, ArchiveRoot).Should().BeNull();
            EfcDataModel.ToFilingStemOrVerbatim(string.Empty, ArchiveRoot).Should().BeEmpty();
        }
    }
}
