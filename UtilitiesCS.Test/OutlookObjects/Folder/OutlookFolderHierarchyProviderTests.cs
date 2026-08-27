using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="OutlookFolderHierarchyProvider"/> using a Moq
    /// <see cref="IOutlookFolderTreeService"/> that returns a prebuilt <see cref="FolderTreeSnapshot"/>.
    /// Covers the ancestor-chain happy path, immediate subfolders (populated, empty, unknown key),
    /// path resolution (found, not found, duplicate first-match), null-service construction, and
    /// deterministic cancellation propagation. No live Outlook process, COM, or temporary file is used.
    /// </summary>
    [TestClass]
    public sealed class OutlookFolderHierarchyProviderTests
    {
        private static readonly FolderTreeNodeKey RootKey = Key("store-a", "root", "\\Root");
        private static readonly FolderTreeNodeKey MidKey = Key("store-a", "mid", "\\Root\\Clients");
        private static readonly FolderTreeNodeKey AcmeKey = Key(
            "store-a",
            "acme",
            "\\Root\\Clients\\Acme"
        );
        private static readonly FolderTreeNodeKey BetaKey = Key(
            "store-a",
            "beta",
            "\\Root\\Clients\\Beta"
        );

        // Realistic store-qualified Outlook paths under an Archive root, used by the decision-D5
        // suffix-match tests. The Qfc surface presents ArchiveStem, never these full paths.
        private const string ArchiveStem = "Projects\\Alpha";
        private static readonly FolderTreeNodeKey ArchiveRootKey = Key(
            "store-b",
            "archive-root",
            "\\\\Mailbox - User\\Archive"
        );
        private static readonly FolderTreeNodeKey ArchiveProjectsKey = Key(
            "store-b",
            "archive-projects",
            "\\\\Mailbox - User\\Archive\\Projects"
        );
        private static readonly FolderTreeNodeKey ArchiveAlphaKey = Key(
            "store-b",
            "archive-alpha",
            "\\\\Mailbox - User\\Archive\\Projects\\Alpha"
        );

        // A decoy sharing the last two segments with the Archive leaf, under a different root.
        private static readonly FolderTreeNodeKey InboxRootKey = Key(
            "store-b",
            "inbox-root",
            "\\\\Mailbox - User\\Inbox"
        );
        private static readonly FolderTreeNodeKey InboxProjectsKey = Key(
            "store-b",
            "inbox-projects",
            "\\\\Mailbox - User\\Inbox\\Projects"
        );
        private static readonly FolderTreeNodeKey InboxAlphaKey = Key(
            "store-b",
            "inbox-alpha",
            "\\\\Mailbox - User\\Inbox\\Projects\\Alpha"
        );

        [TestMethod]
        public async Task GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var chain = await provider.GetAncestorChainAsync(AcmeKey, CancellationToken.None);

            // Assert
            chain
                .Select(s => s.FolderPath)
                .Should()
                .Equal("\\Root", "\\Root\\Clients", "\\Root\\Clients\\Acme");
            chain.Last().Key.Should().Be(AcmeKey);
            chain.Last().HasChildren.Should().BeFalse();
            chain.First().HasChildren.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAncestorChainAsync_RequestsAllStoresAllowingStaleSnapshot()
        {
            // Arrange
            var service = ServiceReturning(BuildSnapshot());
            var provider = new OutlookFolderHierarchyProvider(service.Object);

            // Act
            await provider.GetAncestorChainAsync(AcmeKey, CancellationToken.None);

            // Assert
            service.Verify(
                s =>
                    s.GetSnapshotAsync(
                        It.Is<FolderTreeRequest>(r => r.IsAllStores && r.AllowStaleSnapshot),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task GetImmediateSubfoldersAsync_PopulatedSegment_ReturnsRealChildren()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var children = await provider.GetImmediateSubfoldersAsync(
                MidKey,
                CancellationToken.None
            );

            // Assert
            children
                .Select(s => s.FolderPath)
                .Should()
                .Equal("\\Root\\Clients\\Acme", "\\Root\\Clients\\Beta");
        }

        [TestMethod]
        public async Task ResolveLeafKeyAsync_FoundPath_ReturnsNodeKey()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(
                "\\Root\\Clients\\Acme",
                CancellationToken.None
            );

            // Assert
            resolved.Should().Be(AcmeKey);
        }

        [TestMethod]
        public async Task ResolveLeafKeyAsync_UnknownPath_ReturnsNull()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(
                "\\Root\\Missing",
                CancellationToken.None
            );

            // Assert
            resolved.Should().BeNull();
        }

        [TestMethod]
        public async Task GetImmediateSubfoldersAsync_LeafWithNoChildren_ReturnsEmptyListNeverNull()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var children = await provider.GetImmediateSubfoldersAsync(
                AcmeKey,
                CancellationToken.None
            );

            // Assert
            children.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public async Task GetImmediateSubfoldersAsync_UnknownSegmentKey_ReturnsEmptyList()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );
            var unknownKey = Key("store-a", "ghost", "\\Root\\Ghost");

            // Act
            var children = await provider.GetImmediateSubfoldersAsync(
                unknownKey,
                CancellationToken.None
            );

            // Assert
            children.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public async Task ResolveLeafKeyAsync_DuplicatePaths_ReturnsFirstMatch()
        {
            // Arrange: two nodes share a FolderPath but differ by EntryId (distinct keys).
            var firstKey = Key("store-a", "first", "\\Root\\Dup");
            var secondKey = Key("store-a", "second", "\\Root\\Dup");
            var snapshot = new FolderTreeSnapshot(
                new[] { RootKey },
                new[] { Node(firstKey, "Dup", RootKey), Node(secondKey, "Dup", RootKey) }
            );
            var provider = new OutlookFolderHierarchyProvider(ServiceReturning(snapshot).Object);

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(
                "\\Root\\Dup",
                CancellationToken.None
            );

            // Assert
            resolved.Should().Be(firstKey);
        }

        /// <summary>
        /// The QuickFiler surface presents an archive-relative stem rather than a store-qualified
        /// full path. Exactly one snapshot node's path ends with that stem, so resolution must
        /// return that node's key; without it the row renders leaf-only and has no parent to
        /// navigate to.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildArchiveSnapshot()).Object
            );

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(ArchiveStem, CancellationToken.None);

            // Assert
            resolved
                .Should()
                .NotBeNull("exactly one snapshot node path ends with the presented stem");
            resolved.FolderPath.Should().Be(ArchiveAlphaKey.FolderPath);
        }

        /// <summary>
        /// The Efc surface supplies the store-qualified path produced by
        /// <c>BreadcrumbBridgeRouter.ToHierarchyPath</c>. That value must resolve through the exact
        /// first pass, so the decision-D5 suffix fallback is a strict no-op for the Efc surface.
        /// The snapshot is arranged so the suffix fallback, if it were reached, would find no
        /// candidate at all and return null; a non-null exact key therefore proves the first pass
        /// produced the answer.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_EfcFullHierarchyPath_ResolvesByExactFirstPassWithoutSuffixFallback()
        {
            // Arrange: "\\Mailbox - User\Archive" joined to the presented stem "Projects\Alpha",
            // which is exactly what ToHierarchyPath returns for an archive-relative Efc row.
            string hierarchyPath = ArchiveRootKey.FolderPath + "\\" + ArchiveStem;
            var snapshot = BuildArchiveSnapshot();
            var provider = new OutlookFolderHierarchyProvider(ServiceReturning(snapshot).Object);
            snapshot
                .NodesByKey.Values.Where(node =>
                    node.FolderPath.EndsWith(
                        "\\" + hierarchyPath,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeEmpty("the suffix fallback must have no candidate for a full hierarchy path");

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(
                hierarchyPath,
                CancellationToken.None
            );

            // Assert: a key came back, so the exact pass answered and the fallback was not reached.
            resolved.Should().Be(ArchiveAlphaKey);
        }

        /// <summary>
        /// The suffix fallback is accepted only when exactly one node qualifies. With a decoy under
        /// a different root sharing the last two segments, the method returns null and logs at
        /// Error, so the row keeps today's single-segment fallback rendering.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_AmbiguousStemWithDecoyNode_ReturnsNullAndLogsError()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildArchiveSnapshotWithDecoy()).Object
            );

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(ArchiveStem, CancellationToken.None);

            // Assert
            resolved
                .Should()
                .BeNull("an ambiguous stem must not be resolved to either candidate folder");
        }

        [TestMethod]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => new OutlookFolderHierarchyProvider(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("treeService");
        }

        [TestMethod]
        public async Task GetAncestorChainAsync_WithCanceledToken_PropagatesOperationCanceled()
        {
            // Arrange: the mocked snapshot acquisition observes the token deterministically.
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .Returns<FolderTreeRequest, CancellationToken>(
                    (_, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        return Task.FromResult(BuildSnapshot());
                    }
                );
            var provider = new OutlookFolderHierarchyProvider(service.Object);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = () => provider.GetAncestorChainAsync(AcmeKey, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        private static Mock<IOutlookFolderTreeService> ServiceReturning(FolderTreeSnapshot snapshot)
        {
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(snapshot);
            return service;
        }

        private static FolderTreeSnapshot BuildSnapshot()
        {
            return new FolderTreeSnapshot(
                new[] { RootKey },
                new[]
                {
                    Node(RootKey, "Root", null, MidKey),
                    Node(MidKey, "Clients", RootKey, AcmeKey, BetaKey),
                    Node(AcmeKey, "Acme", MidKey),
                    Node(BetaKey, "Beta", MidKey),
                }
            );
        }

        /// <summary>
        /// A three-level Archive tree carrying realistic store-qualified paths and realistic
        /// relative paths, so that a suffix match on <see cref="ArchiveStem"/> is unique.
        /// </summary>
        private static FolderTreeSnapshot BuildArchiveSnapshot()
        {
            return new FolderTreeSnapshot(new[] { ArchiveRootKey }, ArchiveNodes());
        }

        /// <summary>
        /// The Archive tree plus an Inbox tree whose leaf shares the last two segments, so a suffix
        /// match on <see cref="ArchiveStem"/> is ambiguous.
        /// </summary>
        private static FolderTreeSnapshot BuildArchiveSnapshotWithDecoy()
        {
            return new FolderTreeSnapshot(
                new[] { ArchiveRootKey, InboxRootKey },
                ArchiveNodes()
                    .Concat(BranchNodes(InboxRootKey, InboxProjectsKey, InboxAlphaKey, "Inbox"))
            );
        }

        private static IEnumerable<FolderTreeSnapshotNode> ArchiveNodes()
        {
            return BranchNodes(ArchiveRootKey, ArchiveProjectsKey, ArchiveAlphaKey, "Archive");
        }

        /// <summary>
        /// A root/Projects/Alpha branch whose nodes carry explicit, realistic relative paths.
        /// </summary>
        private static IEnumerable<FolderTreeSnapshotNode> BranchNodes(
            FolderTreeNodeKey rootKey,
            FolderTreeNodeKey projectsKey,
            FolderTreeNodeKey alphaKey,
            string rootName
        )
        {
            yield return NodeWithRelativePath(rootKey, rootName, rootName, null, projectsKey);
            yield return NodeWithRelativePath(
                projectsKey,
                "Projects",
                rootName + "\\Projects",
                rootKey,
                alphaKey
            );
            yield return NodeWithRelativePath(
                alphaKey,
                "Alpha",
                rootName + "\\Projects\\Alpha",
                projectsKey
            );
        }

        private static FolderTreeNodeKey Key(string storeId, string entryId, string folderPath)
        {
            return new FolderTreeNodeKey(storeId, entryId, folderPath);
        }

        /// <summary>
        /// Builds a node whose relative path is supplied explicitly, rather than the display-name
        /// shorthand used by <see cref="Node"/>.
        /// </summary>
        private static FolderTreeSnapshotNode NodeWithRelativePath(
            FolderTreeNodeKey key,
            string displayName,
            string relativePath,
            FolderTreeNodeKey parentKey,
            params FolderTreeNodeKey[] childKeys
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                key.StoreId,
                key.EntryId,
                parentKey,
                key.FolderPath,
                relativePath,
                childKeys,
                false,
                string.Empty
            );
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            FolderTreeNodeKey parentKey,
            params FolderTreeNodeKey[] childKeys
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                key.StoreId,
                key.EntryId,
                parentKey,
                key.FolderPath,
                displayName,
                childKeys,
                false,
                string.Empty
            );
        }
    }
}
