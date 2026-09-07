using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="ArchiveChainProjection"/> and its single member
    /// <c>TryTrimBelowArchiveRoot</c>, the ancestor-chain trim introduced by issue #799 (AC1, AC2).
    /// Every chain is built from <see cref="FolderBreadcrumbSegment"/> literals through the
    /// four-argument constructor, so there is no snapshot, no provider, no COM, and no mock: the
    /// unit under test is the trim itself and nothing else.
    /// </summary>
    [TestClass]
    public sealed class ArchiveChainProjectionTests
    {
        private const string StorePath = "\\\\Mailbox - User";
        private const string ArchiveRoot = "\\\\Mailbox - User\\Archive";
        private const string ClientsPath = "\\\\Mailbox - User\\Archive\\Clients";
        private const string AcmePath = "\\\\Mailbox - User\\Archive\\Clients\\Acme";

        /// <summary>
        /// The ordinary case: the chain passes through the archive root, so the trim yields exactly
        /// the segments after that node. Segment identity is preserved by reference, which proves
        /// the trim is a projection over the existing chain rather than a rebuild that could drop a
        /// key or a display name.
        /// </summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_ChainPassesThroughRoot_ReturnsSegmentsAfterTheRoot()
        {
            // Arrange
            var chain = FullChain();

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeTrue();
            trimmed.Should().HaveCount(2);
            trimmed[0].Should().BeSameAs(chain[2], "segment identity is preserved by reference");
            trimmed[1].Should().BeSameAs(chain[3]);
        }

        /// <summary>
        /// A chain that never reaches the archive root is the AC2 diagnostic case: the trim reports
        /// failure and yields nothing, so the caller can log once and fall back.
        /// </summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_ChainMissesTheRoot_ReturnsFalseAndEmptyOutput()
        {
            // Arrange
            var chain = new List<FolderBreadcrumbSegment>
            {
                Segment("inbox", StorePath + "\\Inbox", "Inbox"),
                Segment("inbox-clients", StorePath + "\\Inbox\\Clients", "Clients"),
            };

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeFalse();
            trimmed.Should().BeEmpty();
        }

        /// <summary>
        /// When the LEAF is the archive root there is nothing below it to render, so the trim
        /// reports failure rather than returning an empty lineage that would render as a blank row.
        /// </summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_LeafIsTheRoot_ReturnsFalseAndEmptyOutput()
        {
            // Arrange
            var chain = new List<FolderBreadcrumbSegment>
            {
                Segment("store", StorePath, "Mailbox - User"),
                Segment("archive", ArchiveRoot, "Archive"),
            };

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeFalse();
            trimmed.Should().BeEmpty();
        }

        /// <summary>An empty chain has no archive-root node and therefore reports failure.</summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_EmptyChain_ReturnsFalseAndEmptyOutput()
        {
            // Arrange
            var chain = new List<FolderBreadcrumbSegment>();

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeFalse();
            trimmed.Should().BeEmpty();
        }

        /// <summary>
        /// A single-element chain that IS the archive root is the degenerate form of the
        /// leaf-is-the-root case and must behave identically.
        /// </summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_SingleElementChainIsTheRoot_ReturnsFalse()
        {
            // Arrange
            var chain = new List<FolderBreadcrumbSegment>
            {
                Segment("archive", ArchiveRoot, "Archive"),
            };

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeFalse();
            trimmed.Should().BeEmpty();
        }

        /// <summary>A root supplied with a trailing separator trims identically.</summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_RootWithTrailingSeparator_ReturnsSegmentsAfterTheRoot()
        {
            // Arrange
            var chain = FullChain();

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot + "\\",
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeTrue();
            trimmed.Should().HaveCount(2);
            trimmed[0].FolderPath.Should().Be(ClientsPath);
        }

        /// <summary>
        /// The #614 false-prefix case at chain level: a lineage under a sibling folder named
        /// Archive2 must not be treated as passing through the root named Archive.
        /// </summary>
        [TestMethod]
        public void TryTrimBelowArchiveRoot_FalsePrefixSiblingArchive2_ReturnsFalse()
        {
            // Arrange
            var chain = new List<FolderBreadcrumbSegment>
            {
                Segment("archive2", StorePath + "\\Archive2", "Archive2"),
                Segment("archive2-clients", StorePath + "\\Archive2\\Clients", "Clients"),
            };

            // Act
            bool trimmedOk = ArchiveChainProjection.TryTrimBelowArchiveRoot(
                chain,
                ArchiveRoot,
                out var trimmed
            );

            // Assert
            trimmedOk.Should().BeFalse("the separator-boundary test rejects a false prefix");
            trimmed.Should().BeEmpty();
        }

        /// <summary>Store node, archive root, one intermediate folder, and the leaf.</summary>
        private static List<FolderBreadcrumbSegment> FullChain()
        {
            return new List<FolderBreadcrumbSegment>
            {
                Segment("store", StorePath, "Mailbox - User"),
                Segment("archive", ArchiveRoot, "Archive"),
                Segment("clients", ClientsPath, "Clients"),
                Segment("acme", AcmePath, "Acme"),
            };
        }

        private static FolderBreadcrumbSegment Segment(
            string entryId,
            string folderPath,
            string displayName
        )
        {
            return new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-a", entryId, folderPath),
                displayName,
                folderPath,
                false
            );
        }
    }
}
