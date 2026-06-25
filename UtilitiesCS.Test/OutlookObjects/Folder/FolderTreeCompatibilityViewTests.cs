using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeCompatibilityViewTests
    {
        [TestMethod]
        public void Constructor_ProjectsSnapshotIntoLegacyTreeNodes()
        {
            var root = Key("root", "\\Root");
            var child = Key("child", "\\Root\\Child");
            var snapshot = new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, "Root", "Root", child), Node(child, "Child", "Root\\Child") }
            );

            using (
                var view = new FolderTreeCompatibilityView(
                    snapshot,
                    new FolderTreeSelectionOverlay(new[] { "Root\\Child" })
                )
            )
            {
                view.Roots.Should().ContainSingle();
                view.Roots[0].Value.Name.Should().Be("Root");
                view.Roots[0].Children.Should().ContainSingle();
                view.Roots[0].Children[0].Value.Selected.Should().BeTrue();
            }
        }

        [TestMethod]
        public void Constructor_NullSelectionOverlay_UsesEmptySelection()
        {
            var root = Key("root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, "Root", "Root") }
            );

            using (var view = new FolderTreeCompatibilityView(snapshot, null))
            {
                view.Roots.Should().ContainSingle();
                view.Roots[0].Value.Selected.Should().BeFalse();
            }
        }

        [TestMethod]
        public void Constructor_NullSnapshot_Throws()
        {
            Action act = () => new FolderTreeCompatibilityView(null, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("snapshot");
        }

        [TestMethod]
        public void Constructor_MissingRootKey_SkipsMissingNode()
        {
            var root = Key("root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { root },
                Array.Empty<FolderTreeSnapshotNode>()
            );

            using (var view = new FolderTreeCompatibilityView(snapshot, null))
            {
                view.Roots.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void Dispose_WhenCalledTwice_UnsubscribesOnlyOnce()
        {
            var root = Key("root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, "Root", "Root") }
            );
            var view = new FolderTreeCompatibilityView(snapshot, null);

            view.Dispose();
            view.Dispose();

            view.SubscriptionCount.Should().Be(0);
        }

        private static FolderTreeNodeKey Key(string entryId, string path)
        {
            return new FolderTreeNodeKey("store-a", entryId, path);
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            string relativePath,
            params FolderTreeNodeKey[] children
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                "store-a",
                key.EntryId,
                null,
                key.FolderPath,
                relativePath,
                children,
                false,
                string.Empty
            );
        }
    }
}
