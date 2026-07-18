using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Contract-shape pinning tests for the merged 9101 provider surface the QuickFiler breadcrumb
    /// (#351) consumes. The P2-T1 reconciliation decided DIRECT-CONSUME (no adapter), so these tests
    /// pin the exact <see cref="IFolderHierarchyProvider"/> / <see cref="FolderBreadcrumbSegment"/>
    /// shape and the consumer composition paths (path resolution + ancestor chain; key-based expand;
    /// substrate failure propagation) with a Moq-mocked <see cref="IOutlookFolderTreeService"/>
    /// substrate and completed tasks only. No live Outlook, COM, WebView2, or temp files.
    /// </summary>
    [TestClass]
    public sealed class FolderHierarchyProviderAdapterTests
    {
        private static readonly FolderTreeNodeKey RootKey = new FolderTreeNodeKey(
            "store-a",
            "root",
            "\\Inbox"
        );
        private static readonly FolderTreeNodeKey ProjectsKey = new FolderTreeNodeKey(
            "store-a",
            "projects",
            "\\Inbox\\Projects"
        );
        private static readonly FolderTreeNodeKey LeafKey = new FolderTreeNodeKey(
            "store-a",
            "leaf",
            "\\Inbox\\Projects\\Apollo"
        );

        [TestMethod]
        public void ContractShape_ProviderInterface_ExposesTheThreeMembersTheBreadcrumbConsumes()
        {
            // Arrange: the breadcrumb router binds to exactly these three async members.
            var methods = typeof(IFolderHierarchyProvider)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => m.Name)
                .OrderBy(n => n)
                .ToArray();

            // Act, Assert: pin the merged surface so an upstream rename breaks here first.
            methods
                .Should()
                .Equal(
                    "GetAncestorChainAsync",
                    "GetImmediateSubfoldersAsync",
                    "ResolveLeafKeyAsync"
                );
            typeof(IFolderHierarchyProvider)
                .GetMethod("GetAncestorChainAsync")
                .GetParameters()[0]
                .ParameterType.Should()
                .Be<FolderTreeNodeKey>("chain queries are keyed, not path-keyed");
            typeof(IFolderHierarchyProvider)
                .GetMethod("ResolveLeafKeyAsync")
                .GetParameters()[0]
                .ParameterType.Should()
                .Be<string>("path entry resolves to a key via ResolveLeafKeyAsync");
        }

        [TestMethod]
        public void ContractShape_Segment_IsImmutableWithBridgeSerializableMembers()
        {
            // Arrange: the bridge serializes DisplayName/FolderPath/HasChildren; Key is identity.
            var properties = typeof(FolderBreadcrumbSegment)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.Name)
                .ToArray();

            // Act, Assert
            properties
                .Select(p => p.Name)
                .Should()
                .Equal("DisplayName", "FolderPath", "HasChildren", "Key");
            properties
                .Should()
                .OnlyContain(p => p.GetSetMethod() == null, "segments are immutable");
            typeof(FolderBreadcrumbSegment)
                .GetProperty("HasChildren")
                .PropertyType.Should()
                .Be<bool>("the merged member is HasChildren, not the assumed HasSubfolders");
        }

        [TestMethod]
        public async Task PathAComposition_ResolveThenChain_YieldsRootFirstChainForSelectedPath()
        {
            // Arrange: the breadcrumb's Path A entry composes ResolveLeafKeyAsync + GetAncestorChainAsync.
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var key = await provider.ResolveLeafKeyAsync(
                "\\Inbox\\Projects\\Apollo",
                CancellationToken.None
            );
            var chain = await provider.GetAncestorChainAsync(key, CancellationToken.None);

            // Assert: root-first/leaf-last ordering with the leaf equal to the resolved key.
            chain
                .Select(s => s.FolderPath)
                .Should()
                .Equal("\\Inbox", "\\Inbox\\Projects", "\\Inbox\\Projects\\Apollo");
            chain.Last().Key.Should().Be(LeafKey);
            chain.Select(s => s.DisplayName).Should().Equal("Inbox", "Projects", "Apollo");
        }

        [TestMethod]
        public async Task ExpandComposition_SegmentKey_ListsRealImmediateSubfolders()
        {
            // Arrange: expand routes on the segment Key carried in the render payload.
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var children = await provider.GetImmediateSubfoldersAsync(
                ProjectsKey,
                CancellationToken.None
            );

            // Assert
            children.Should().ContainSingle();
            children[0].FolderPath.Should().Be("\\Inbox\\Projects\\Apollo");
            children[0].HasChildren.Should().BeFalse("Apollo is a leaf with no subfolders");
        }

        [TestMethod]
        public async Task ResolveLeafKeyAsync_UnknownPath_ReturnsNullSoConsumerFailsExplicitly()
        {
            // Arrange: Path B plain rows may not exist in the snapshot; null is the explicit signal.
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var key = await provider.ResolveLeafKeyAsync(
                "\\Inbox\\DoesNotExist",
                CancellationToken.None
            );

            // Assert
            key.Should().BeNull();
        }

        [TestMethod]
        public async Task SubstrateException_PropagatesToTheBreadcrumbCaller()
        {
            // Arrange: a failing snapshot acquisition must surface, never be swallowed.
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .Returns(
                    Task.FromException<FolderTreeSnapshot>(
                        new InvalidOperationException("snapshot unavailable")
                    )
                );
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(service.Object);

            // Act
            Func<Task> act = () => provider.GetAncestorChainAsync(LeafKey, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("snapshot unavailable");
        }

        [TestMethod]
        public async Task GetAncestorChainAsync_RootOnlyLeaf_ReturnsSingleSegmentChain()
        {
            // Arrange: a store-root selection renders a one-segment breadcrumb.
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var chain = await provider.GetAncestorChainAsync(RootKey, CancellationToken.None);

            // Assert
            chain.Should().ContainSingle();
            chain[0].Key.Should().Be(RootKey);
            chain[0].HasChildren.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAncestorChainAsync_NullKey_ReturnsEmptyChainNeverNull()
        {
            // Arrange: an unresolved (null) key yields an empty chain, the documented contract.
            IFolderHierarchyProvider provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(BuildSnapshot()).Object
            );

            // Act
            var chain = await provider.GetAncestorChainAsync(null, CancellationToken.None);

            // Assert
            chain.Should().NotBeNull().And.BeEmpty();
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
                    Node(RootKey, "Inbox", null, ProjectsKey),
                    Node(ProjectsKey, "Projects", RootKey, LeafKey),
                    Node(LeafKey, "Apollo", ProjectsKey),
                }
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
