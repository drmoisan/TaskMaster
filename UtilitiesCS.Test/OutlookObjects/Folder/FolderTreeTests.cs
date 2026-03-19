using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderTreeTests
    {
        [TestMethod]
        public void CompareMembers_WithFolderWrapperLists_ReturnsIntersectionAndDifferences()
        {
            var tree = new FolderTree();
            var current = new List<FolderWrapper>
            {
                CreateFolderWrapper("Inbox", itemCount: 2, folderSize: 10L),
                CreateFolderWrapper("Archive", itemCount: 1, folderSize: 5L),
            };
            var other = new List<FolderWrapper>
            {
                CreateFolderWrapper("INBOX", itemCount: 2, folderSize: 10L),
                CreateFolderWrapper("Sent", itemCount: 1, folderSize: 5L),
            };

            var (same, onlyCurrent, onlyOther) = tree.CompareMembers(
                current,
                other,
                new FolderWrapperNameCountSizeComparer()
            );

            same.Should().ContainSingle(x => x.Name == "Inbox");
            onlyCurrent.Should().ContainSingle(x => x.Name == "Archive");
            onlyOther.Should().ContainSingle(x => x.Name == "Sent");
        }

        [TestMethod]
        public void CompareMembers_WithTreeNodeLists_ReturnsIntersectionAndDifferences()
        {
            var tree = new FolderTree();
            var currentRoot = new TreeNode<FolderWrapper>(CreateFolderWrapper("Mailbox"));
            var otherRoot = new TreeNode<FolderWrapper>(CreateFolderWrapper("Mailbox"));
            var current = new List<TreeNode<FolderWrapper>>
            {
                currentRoot.AddChild(CreateFolderWrapper("Inbox", itemCount: 2, folderSize: 10L)),
                currentRoot.AddChild(CreateFolderWrapper("Projects", itemCount: 1, folderSize: 5L)),
            };
            var other = new List<TreeNode<FolderWrapper>>
            {
                otherRoot.AddChild(CreateFolderWrapper("INBOX", itemCount: 2, folderSize: 10L)),
                otherRoot.AddChild(CreateFolderWrapper("Sent", itemCount: 1, folderSize: 5L)),
            };

            var (same, onlyCurrent, onlyOther) = tree.CompareMembers(
                current,
                other,
                new FolderWrapperNodeComparer()
            );

            same.Should().ContainSingle(x => x.Value.Name == "Inbox");
            onlyCurrent.Should().ContainSingle(x => x.Value.Name == "Projects");
            onlyOther.Should().ContainSingle(x => x.Value.Name == "Sent");
        }

        [TestMethod]
        public void FilterSelected_WhenSomeNodesSelected_ReturnsOnlySelectedSubtree()
        {
            var fy26 = CreateFolder(@"\\Mailbox\Projects\FY26");
            var projects = CreateFolder(@"\\Mailbox\Projects", fy26.Object);
            var reference = CreateFolder(@"\\Mailbox\Reference");
            var root = CreateFolder(@"\\Mailbox", projects.Object, reference.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);
            var nodes = tree.FlattenNodes();
            nodes.Single(x => x.Value.Name == "Projects").Value.Selected = true;
            nodes.Single(x => x.Value.Name == "FY26").Value.Selected = true;

            var result = tree.FilterSelected(include: true);

            result.Should().ContainSingle();
            result[0].Value.Name.Should().Be("Projects");
            result[0].Children.Should().ContainSingle();
            result[0].Children[0].Value.Name.Should().Be("FY26");
            result
                .SelectMany(node => node.Flatten())
                .Should()
                .OnlyContain(folder => folder.Selected);
        }

        [TestMethod]
        public void Flatten_WithPopulatedTree_ReturnsAllFolderWrappers()
        {
            var child = CreateFolder(@"\\Mailbox\Projects");
            var root = CreateFolder(@"\\Mailbox", child.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);

            var result = tree.Flatten();

            result.Select(folder => folder.Name).Should().Equal("Mailbox", "Projects");
        }

        [TestMethod]
        public void SelectionConstructor_WhenSelectionMatchesRelativePath_MarksNodeAsSelected()
        {
            var grandchild = CreateFolder(@"\\Mailbox\Projects\FY26");
            var child = CreateFolder(@"\\Mailbox\Projects", grandchild.Object);
            var root = CreateFolder(@"\\Mailbox", child.Object);

            var tree = new FolderTree(
                (Outlook.MAPIFolder)root.Object,
                new List<string> { @"Projects\FY26" }
            );

            tree.Flatten().Single(x => x.Name == "FY26").Selected.Should().BeTrue();
            tree.Flatten().Single(x => x.Name == "Projects").Selected.Should().BeFalse();
        }

        [TestMethod]
        public void ProgressConstructor_WhenSelectionsProvided_ReportsCompletionAndAppliesSelection()
        {
            var grandchild = CreateFolder(@"\\Mailbox\Projects\FY26");
            var child = CreateFolder(@"\\Mailbox\Projects", grandchild.Object);
            var root = CreateFolder(@"\\Mailbox", child.Object);
            var progress = new RecordingProgressTracker();

            var tree = new FolderTree(
                (Outlook.MAPIFolder)root.Object,
                new List<string> { "Projects" },
                progress
            );

            tree.Flatten().Single(x => x.Name == "Projects").Selected.Should().BeTrue();
            progress.ReportedValues.Should().Contain(100d);
        }

        [TestMethod]
        public void Constructor_WithMultipleRoots_RemovesNestedSubsetRoots()
        {
            var child = CreateFolder(@"\\Mailbox\Projects");
            var root = CreateFolder(@"\\Mailbox", child.Object);

            var tree = new FolderTree(
                new[] { (Outlook.MAPIFolder)root.Object, (Outlook.MAPIFolder)child.Object }
            );

            tree.Roots.Should().HaveCount(2);
            tree.Flatten().Select(x => x.Name).Should().Equal("Mailbox", "Projects", "Projects");
        }

        [TestMethod]
        public void LoadItemCounts_WhenNestedFoldersHaveItems_ComputesRecursiveTotals()
        {
            var grandchild = CreateFolder(@"\\Mailbox\Projects\FY26", 3);
            var child = CreateFolder(@"\\Mailbox\Projects", 2, grandchild.Object);
            var root = CreateFolder(@"\\Mailbox", 1, child.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);

            tree.LoadItemCounts();

            tree.Flatten().Single(x => x.Name == "FY26").ItemCountSubFolders.Should().Be(3);
            tree.Flatten().Single(x => x.Name == "Projects").ItemCountSubFolders.Should().Be(5);
            tree.Flatten().Single(x => x.Name == "Mailbox").ItemCountSubFolders.Should().Be(6);
        }

        [TestMethod]
        public void Compare_WhenTreesShareAndDiffer_ReturnsGroupedResults()
        {
            var currentFy26 = CreateFolder(@"\\Mailbox\Projects\FY26");
            var currentProjects = CreateFolder(@"\\Mailbox\Projects", currentFy26.Object);
            var currentRoot = CreateFolder(@"\\Mailbox", currentProjects.Object);
            var otherProjects = CreateFolder(@"\\Mailbox\Projects");
            var otherReference = CreateFolder(@"\\Mailbox\Reference");
            var otherRoot = CreateFolder(@"\\Mailbox", otherProjects.Object, otherReference.Object);
            var currentTree = new FolderTree((Outlook.MAPIFolder)currentRoot.Object);
            var otherTree = new FolderTree((Outlook.MAPIFolder)otherRoot.Object);

            var (sameNodes, sameContents, sameName, currentOnly, otherOnly) = currentTree.Compare(
                otherTree
            );

            sameNodes.Should().BeEmpty();
            sameContents.Should().NotBeEmpty();
            currentOnly.Should().Contain(node => node.Value.Name == "FY26");
            otherOnly.Should().Contain(node => node.Value.Name == "Reference");
            sameName.Should().BeEmpty();
        }

        [TestMethod]
        public void SetSelected_WhenIncludeDescendentsIsTrue_SelectsEntireSubtree()
        {
            var grandchild = CreateFolder(@"\\Mailbox\Projects\FY26");
            var child = CreateFolder(@"\\Mailbox\Projects", grandchild.Object);
            var root = CreateFolder(@"\\Mailbox", child.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);
            var projectsNode = tree.FlattenNodes().Single(x => x.Value.Name == "Projects");

            tree.SetSelected(projectsNode, includeDescendents: true);

            projectsNode.Value.Selected.Should().BeTrue();
            projectsNode.Children.Should().OnlyContain(childNode => childNode.Value.Selected);
        }

        private static FolderWrapper CreateFolderWrapper(
            string name,
            int itemCount = 0,
            long folderSize = 0L
        )
        {
            return new FolderWrapper(
                selected: false,
                itemCount: itemCount,
                folderSize: folderSize,
                name: name,
                relativePath: name
            );
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            params OutlookFolder[] children
        )
        {
            return CreateFolder(folderPath, itemCount: 0, children);
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            int itemCount,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            folder.SetupGet(x => x.Items).Returns(CreateItems(itemCount).Object);
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

        private static Mock<OutlookItems> CreateItems(int count = 0)
        {
            var items = new Mock<OutlookItems>(MockBehavior.Strict);
            var collection = new ArrayList();
            items.SetupGet(x => x.Count).Returns(count);
            items.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return items;
        }

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));
        }

        private sealed class RecordingProgressTracker : ProgressTracker
        {
            public RecordingProgressTracker()
                : base(new CancellationTokenSource()) { }

            public List<double> ReportedValues { get; } = new();

            public override void Report(double value)
            {
                ReportedValues.Add(value);
            }

            public override void Report(double value, string jobName)
            {
                ReportedValues.Add(value);
            }

            public override ProgressTracker SpawnChild(int allocation)
            {
                return this;
            }

            public override ProgressTracker SpawnChild(double allocation)
            {
                return this;
            }

            public override ProgressTracker SpawnChild()
            {
                return this;
            }
        }
    }
}
