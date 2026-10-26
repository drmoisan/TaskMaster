#nullable enable
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
    /// Issue #1007 AC14: a mapped segment's <c>HasChildren</c> is derived from the children that
    /// resolve in the snapshot (the set <see cref="FolderTreeSnapshot.GetChildren"/> returns), not
    /// from the raw child-key count. The snapshot is built through its public constructor with a
    /// reader-reported child key that has no node, and served through a mocked tree service.
    /// </summary>
    [TestClass]
    public sealed class OutlookFolderHierarchyProviderIssue1007Tests
    {
        private static readonly FolderTreeNodeKey RootKey = Key("root", "\\Root");
        private static readonly FolderTreeNodeKey ClientsKey = Key("clients", "\\Root\\Clients");
        private static readonly FolderTreeNodeKey AcmeKey = Key("acme", "\\Root\\Clients\\Acme");

        // Reported by the reader as a child of Acme, but absent from the snapshot.
        private static readonly FolderTreeNodeKey GhostKey = Key(
            "ghost",
            "\\Root\\Clients\\Acme\\Ghost"
        );

        /// <summary>
        /// AC14: the leaf Acme lists only an unresolvable child key, so the ancestor chain reports
        /// it without children and no renderer draws an expand affordance on it.
        /// </summary>
        [TestMethod]
        public async Task MapNode_UnresolvableChildKey_HasChildrenFalse()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(Service().Object);

            // Act
            var chain = await provider.GetAncestorChainAsync(AcmeKey, CancellationToken.None);

            // Assert
            chain.Last().Key.Should().Be(AcmeKey);
            chain
                .Last()
                .HasChildren.Should()
                .BeFalse("Acme's only child key does not resolve in the snapshot");
        }

        /// <summary>
        /// AC14: the immediate-subfolder fetch maps its children through the same derivation, so
        /// a child whose child keys all fail to resolve is reported without children.
        /// </summary>
        [TestMethod]
        public async Task GetImmediateSubfolders_ChildWithOnlyUnresolvableKeys_HasChildrenFalse()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(Service().Object);

            // Act
            var children = await provider.GetImmediateSubfoldersAsync(
                ClientsKey,
                CancellationToken.None
            );

            // Assert
            children.Should().ContainSingle().Which.Key.Should().Be(AcmeKey);
            children[0]
                .HasChildren.Should()
                .BeFalse("the subfolder fetch for Acme would return zero children");
        }

        /// <summary>
        /// Guard (AC14): a segment whose child key resolves keeps <c>HasChildren</c> true.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChain_ResolvableChildKey_HasChildrenTrue()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(Service().Object);

            // Act
            var chain = await provider.GetAncestorChainAsync(AcmeKey, CancellationToken.None);

            // Assert
            chain.Should().HaveCount(3);
            chain[0].HasChildren.Should().BeTrue("Root's child Clients resolves");
            chain[1].HasChildren.Should().BeTrue("Clients' child Acme resolves");
        }

        private static Mock<IOutlookFolderTreeService> Service()
        {
            var snapshot = new FolderTreeSnapshot(
                new[] { RootKey },
                new[]
                {
                    Node(RootKey, "Root", null, ClientsKey),
                    Node(ClientsKey, "Clients", RootKey, AcmeKey),
                    Node(AcmeKey, "Acme", ClientsKey, GhostKey),
                }
            );
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service
                .Setup(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(snapshot);
            return service;
        }

        private static FolderTreeNodeKey Key(string entryId, string folderPath) =>
            new FolderTreeNodeKey("store-a", entryId, folderPath);

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            FolderTreeNodeKey? parentKey,
            params FolderTreeNodeKey[] childKeys
        ) =>
            new FolderTreeSnapshotNode(
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
