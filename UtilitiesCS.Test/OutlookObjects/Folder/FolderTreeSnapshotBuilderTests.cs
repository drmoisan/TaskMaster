using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotBuilderTests
    {
        [TestMethod]
        public void Constructor_NullReader_Throws()
        {
            Action act = () => new FolderTreeSnapshotBuilder(null, null, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("reader");
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_DeepHierarchy_BuildsWithoutRecursiveTraversal()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 1500
            );
            var builder = new FolderTreeSnapshotBuilder(reader);

            var snapshot = await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(allowStaleSnapshot: false),
                CancellationToken.None
            );

            snapshot.NodesByKey.Count.Should().Be(1501);
            snapshot.RootKeys.Should().ContainSingle();
            snapshot.NodesByKey.Values.Last().RelativePath.Should().EndWith("Child1500");
            reader.EnumerationCount.Should().Be(1);
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_MissingChildKey_SkipsUnknownChild()
        {
            var root = new FolderTreeNodeKey("store-a", "root", "\\Root");
            var missing = new FolderTreeNodeKey("store-a", "missing", "\\Root\\Missing");
            var reader = new StaticFolderHierarchyReader(
                new[]
                {
                    new FolderTreeSnapshotNode(
                        root,
                        "Root",
                        "store-a",
                        "root",
                        null,
                        "\\Root",
                        "Root",
                        new[] { missing },
                        false,
                        string.Empty
                    ),
                }
            );
            var builder = new FolderTreeSnapshotBuilder(reader);

            var snapshot = await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            snapshot.RootKeys.Should().ContainSingle().Which.Should().Be(root);
            snapshot.NodesByKey.Should().ContainSingle();
        }

        private sealed class StaticFolderHierarchyReader : IOutlookFolderHierarchyReader
        {
            private readonly FolderTreeSnapshotNode[] _nodes;

            public StaticFolderHierarchyReader(FolderTreeSnapshotNode[] nodes)
            {
                _nodes = nodes;
            }

            public IReadOnlyList<FolderTreeSnapshotNode> ReadFolders(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            )
            {
                return _nodes;
            }
        }
    }
}
