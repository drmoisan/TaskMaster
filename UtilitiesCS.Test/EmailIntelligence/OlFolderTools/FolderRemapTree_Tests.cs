using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.FolderRemap;

namespace UtilitiesCS.Test.EmailIntelligence.OlFolderTools
{
    [TestClass]
    public class FolderRemapTree_Tests
    {
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="FolderRemapTree"/> with <paramref name="roots"/>
        /// injected via reflection, bypassing the COM-dependent constructor.
        /// </summary>
        private static FolderRemapTree CreateTree(IList<TreeNode<OlFolderRemap>> roots)
        {
            var tree = new FolderRemapTree();
            typeof(FolderRemapTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(tree, new List<TreeNode<OlFolderRemap>>(roots));
            return tree;
        }

        /// <summary>
        /// Creates a mock-backed <see cref="OlFolderRemap"/> without a live COM session.
        /// </summary>
        private static OlFolderRemap MakeRemap(string folderPath, string rootPath, string name)
        {
            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            var mockRoot = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            mockFolder.Setup(f => f.FolderPath).Returns(folderPath);
            mockFolder.Setup(f => f.Name).Returns(name);
            mockRoot.Setup(f => f.FolderPath).Returns(rootPath);
            return new OlFolderRemap(mockFolder.Object, mockRoot.Object);
        }

        // -----------------------------------------------------------------------
        // P42-T1 — Building a tree from a mapping source yields expected nodes
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that manually constructing a <see cref="FolderRemapTree"/> from
        /// synthetic <see cref="OlFolderRemap"/> nodes exposes expected paths and labels.
        ///
        /// Purpose:
        ///     Confirm that once _roots is populated, Roots reflects the injected node
        ///     hierarchy with the correct node count and label values.
        ///
        /// Returns:
        ///     Passes when Roots has one root with the expected name and one child with
        ///     the expected child name.
        /// </summary>
        [TestMethod]
        public void BuildTreeFromMappingSource_YieldsExpectedNodesAndLabels()
        {
            // Arrange: create root and child OlFolderRemap objects via mocked COM folders
            var rootRemap = MakeRemap("\\\\Root", "\\\\Root", "Root");
            var childRemap = MakeRemap("\\\\Root\\Inbox", "\\\\Root", "Inbox");

            var rootNode = new TreeNode<OlFolderRemap>(rootRemap);
            rootNode.AddChild(childRemap);

            var tree = CreateTree(new[] { rootNode });

            // Assert: root is present with correct label; child is present under root
            tree.Roots.Should().HaveCount(1);
            tree.Roots[0].Value.Name.Should().Be("Root");
            tree.Roots[0].Children.Should().HaveCount(1);
            tree.Roots[0].Children[0].Value.Name.Should().Be("Inbox");
        }

        // -----------------------------------------------------------------------
        // P42-T2 — Filter path removes excluded nodes
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="FolderRemapTree.FilterMapped"/> with
        /// <c>include = true</c> omits nodes whose <see cref="OlFolderRemap.MappedTo"/>
        /// is null.
        ///
        /// Purpose:
        ///     Confirm that only nodes with an active mapping survive the filter pass
        ///     and that unmapped siblings are excluded from the result.
        ///
        /// Returns:
        ///     Passes when the filtered list contains exactly the mapped node and the
        ///     unmapped node is absent.
        /// </summary>
        [TestMethod]
        public void FilterMapped_IncludeTrue_ExcludesUnmappedNodes()
        {
            // Arrange: root with one mapped child and one unmapped child
            var rootRemap = new OlFolderRemap();
            var mappedRemap = new OlFolderRemap();
            var unmappedRemap = new OlFolderRemap();
            var targetRemap = new OlFolderRemap();

            mappedRemap.MappedTo = targetRemap;

            var rootNode = new TreeNode<OlFolderRemap>(rootRemap);
            rootNode.AddChild(mappedRemap);
            rootNode.AddChild(unmappedRemap);

            var tree = CreateTree(new[] { rootNode });

            // Act
            var filtered = tree.FilterMapped(include: true);

            // Assert: only the mapped child is present
            filtered.Should().HaveCount(1);
            filtered[0].Value.MappedTo.Should().NotBeNull();
            filtered[0].Value.Should().BeSameAs(mappedRemap);
        }

        // -----------------------------------------------------------------------
        // P42-T3 — Notification fires on a map update
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="FolderRemapTree.PropertyChanged"/> is raised exactly
        /// once when a child node's <see cref="OlFolderRemap.MappedTo"/> is modified after
        /// <see cref="FolderRemapTree.WireNotifications"/> has been called.
        ///
        /// Purpose:
        ///     Confirm that the property-change subscription chain (OlFolderRemap →
        ///     TimedBatchAction → FolderRemapTree.PropertyChanged) is active after
        ///     WireNotifications and delivers the event within a generous timeout.
        ///
        /// Returns:
        ///     Passes when the event fires at least once within 500 ms of the mutation.
        /// </summary>
        [TestMethod]
        public void WireNotifications_OnMappedToChange_RaisesPropertyChanged()
        {
            // Arrange: build a one-node tree and wire notifications
            var nodeRemap = new OlFolderRemap();
            var targetRemap = new OlFolderRemap();
            var rootNode = new TreeNode<OlFolderRemap>(nodeRemap);

            var tree = CreateTree(new[] { rootNode });
            tree.WireNotifications();

            var eventFired = new ManualResetEventSlim(false);
            tree.PropertyChanged += (_, _) => eventFired.Set();

            // Act: modify MappedTo to trigger the notification chain
            nodeRemap.MappedTo = targetRemap;

            // Assert: notification arrives within 500 ms (TimedBatchAction delay is 50 ms)
            eventFired
                .Wait(TimeSpan.FromMilliseconds(500))
                .Should()
                .BeTrue("the PropertyChanged event must fire after MappedTo is set");
        }
    }

    [TestClass]
    public class OlFolderRemap_Tests
    {
        [TestMethod]
        public void MappedTo_SetAndGet()
        {
            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            var mockRoot = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            mockFolder.Setup(f => f.FolderPath).Returns("\\\\Root\\Folder1");
            mockRoot.Setup(f => f.FolderPath).Returns("\\\\Root");

            var remap = new OlFolderRemap(mockFolder.Object, mockRoot.Object);
            remap.MappedTo.Should().BeNull();

            var mockTarget = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            mockTarget.Setup(f => f.FolderPath).Returns("\\\\Root\\Folder2");
            var target = new OlFolderRemap(mockTarget.Object, mockRoot.Object);
            remap.MappedTo = target;

            remap.MappedTo.Should().BeSameAs(target);
        }
    }
}
