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

        private static FolderTreeNodeKey Key(string storeId, string entryId, string folderPath)
        {
            return new FolderTreeNodeKey(storeId, entryId, folderPath);
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
