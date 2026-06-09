using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.FolderRemap;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Test.TestHelpers;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

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

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(params OutlookFolder[] children)
        {
            var folders = new Mock<OutlookFolders>(MockBehavior.Strict);
            var enumerableChildren = children ?? [];
            var collection = new ArrayList(enumerableChildren);
            folders.SetupGet(x => x.Count).Returns(enumerableChildren.Length);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return folders;
        }

        private static string GetLeafName(string folderPath) =>
            folderPath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)[^1];

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

            // Inject a deterministic timer into the batch notifier (seam S6) so the notification
            // chain (OlFolderRemap -> TimedBatchAction -> FolderRemapTree.PropertyChanged) can be
            // driven synchronously instead of waiting on the real 50 ms TimedBatchAction timer.
            using var timerStub = new ManualFireTimerWrapper();
            typeof(FolderRemapTree)
                .GetField("_batchNotifier", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(
                    tree,
                    new TimedBatchAction(TimeSpan.FromMilliseconds(50), null, _ => timerStub)
                );
            tree.WireNotifications();

            using var eventFired = new ManualResetEventSlim(false);
            tree.PropertyChanged += (_, _) => eventFired.Set();

            // Act: modify MappedTo to trigger the notification chain, then fire the batch timer.
            nodeRemap.MappedTo = targetRemap;
            timerStub.FireElapsed();

            // Assert: the PropertyChanged event fired deterministically once the batch timer ticked.
            eventFired
                .IsSet.Should()
                .BeTrue("the PropertyChanged event must fire after MappedTo is set");
        }

        [TestMethod]
        public void ConstructorWithMappings_BuildsRemapTreeAndInvertsMappedTargets()
        {
            var fy26 = CreateFolder(@"\\Mailbox\Projects\FY26");
            var projects = CreateFolder(@"\\Mailbox\Projects", fy26.Object);
            var inbox = CreateFolder(@"\\Mailbox\Inbox");
            var archive = CreateFolder(@"\\Mailbox\Archive");
            var root = CreateFolder(@"\\Mailbox", inbox.Object, archive.Object, projects.Object);

            var tree = new FolderRemapTree(
                root.Object,
                new Dictionary<string, string>
                {
                    ["Inbox"] = "Archive",
                    [@"Projects\FY26"] = "Archive",
                }
            );

            tree.Roots.Should().ContainSingle();
            tree.Roots[0].Children.Select(x => x.Value.Name).Should().Contain("Inbox");
            tree.Roots[0].Children.Select(x => x.Value.Name).Should().Contain("Archive");
            tree.Roots[0].Children.Select(x => x.Value.Name).Should().Contain("Projects");

            var remaps = tree.GetRemapList();
            remaps.Select(x => x.RelativePath).Should().BeEquivalentTo("Inbox", @"Projects\FY26");
            remaps.Should().OnlyContain(x => x.MappedTo.RelativePath == "Archive");

            var inverted = tree.GetInvertedMapTree();
            inverted.Should().ContainSingle();
            inverted[0].Value.RelativePath.Should().Be("Archive");
            inverted[0].Children.Should().HaveCount(2);
        }

        [TestMethod]
        public void FilterMapped_IncludeFalse_ReturnsOnlyUnmappedNodes()
        {
            var fy26 = CreateFolder(@"\\Mailbox\Projects\FY26");
            var projects = CreateFolder(@"\\Mailbox\Projects", fy26.Object);
            var inbox = CreateFolder(@"\\Mailbox\Inbox");
            var archive = CreateFolder(@"\\Mailbox\Archive");
            var root = CreateFolder(@"\\Mailbox", inbox.Object, archive.Object, projects.Object);

            var tree = new FolderRemapTree(
                root.Object,
                new Dictionary<string, string> { ["Inbox"] = "Archive" }
            );

            var filtered = tree.FilterMapped(include: false);

            filtered
                .SelectMany(node => node.Flatten())
                .Select(x => x.Name)
                .Should()
                .Contain("Archive");
            filtered
                .SelectMany(node => node.Flatten())
                .Select(x => x.Name)
                .Should()
                .Contain("Projects");
            filtered
                .SelectMany(node => node.Flatten())
                .Select(x => x.Name)
                .Should()
                .Contain("FY26");
            filtered
                .SelectMany(node => node.Flatten())
                .Select(x => x.Name)
                .Should()
                .NotContain("Inbox");
        }

        [TestMethod]
        public void NotifyPropertyChanged_WhenCalled_RaisesRequestedPropertyName()
        {
            var tree = CreateTree(new[] { new TreeNode<OlFolderRemap>(new OlFolderRemap()) });
            string propertyName = null;
            tree.PropertyChanged += (_, args) => propertyName = args.PropertyName;

            tree.NotifyPropertyChanged(nameof(FolderRemapTree.Roots));

            propertyName.Should().Be(nameof(FolderRemapTree.Roots));
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

        [TestMethod]
        public void OlFolder_Setter_RefreshesNameAndRelativePath()
        {
            var mockRoot = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            mockRoot.Setup(f => f.FolderPath).Returns(@"\\Root");

            var initialFolder = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            initialFolder.Setup(f => f.FolderPath).Returns(@"\\Root\\Original");
            initialFolder.Setup(f => f.Name).Returns("Original");

            var updatedFolder = new Mock<Microsoft.Office.Interop.Outlook.MAPIFolder>();
            updatedFolder.Setup(f => f.FolderPath).Returns(@"\\Root\\Updated");
            updatedFolder.Setup(f => f.Name).Returns("Updated");

            var remap = new OlFolderRemap(initialFolder.Object, mockRoot.Object);
            remap.OlRoot = mockRoot.Object;

            remap.OlFolder = updatedFolder.Object;

            remap.OlRoot.Should().BeSameAs(mockRoot.Object);
            remap.Name.Should().Be("Updated");
            remap.RelativePath.Should().Be(@"\\Updated");
        }
    }
}
