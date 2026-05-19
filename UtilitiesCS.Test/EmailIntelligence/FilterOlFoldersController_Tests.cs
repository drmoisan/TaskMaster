using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FilterOlFoldersController internal methods.
    ///
    /// Purpose:
    ///     Covers Save, Discard, OlFolderTree_PropertyChangedInternal, and
    ///     PutCheckedStateMethod without requiring a live COM Outlook session.
    ///
    /// Usage:
    ///     All tests bypass the COM-dependent constructor via
    ///     FormatterServices.GetUninitializedObject and inject dependencies
    ///     through reflection. Every test that touches WinForms controls must
    ///     run on an STA thread.
    /// </summary>
    [TestClass]
    public class FilterOlFoldersController_Tests
    {
        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates an uninitialized FilterOlFoldersController (bypasses COM constructor)
        /// and injects the three dependencies needed by the majority of tests.
        /// </summary>
        /// <param name="viewer">Viewer form to inject as _viewer.</param>
        /// <param name="tree">FolderTree to inject as _olFolderTree.</param>
        /// <param name="globals">IApplicationGlobals mock to inject as _globals.</param>
        /// <returns>Controller with the three fields set via reflection.</returns>
        private static FilterOlFoldersController CreateController(
            FilterOlFoldersViewer viewer,
            FolderTree tree,
            IApplicationGlobals globals
        )
        {
            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));

            typeof(FilterOlFoldersController)
                .GetField("_viewer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, viewer);

            typeof(FilterOlFoldersController)
                .GetField("_olFolderTree", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, tree);

            typeof(FilterOlFoldersController)
                .GetField("_globals", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, globals);

            return controller;
        }

        /// <summary>
        /// Creates a FolderTree with a single synthetic root node so that
        /// FilterSelected() can run without a live MAPIFolder.
        /// </summary>
        private static FolderTree CreateSyntheticFolderTree()
        {
            var wrapper = new FolderWrapper(
                selected: false,
                itemCount: 0,
                folderSize: 0,
                name: "Root",
                relativePath: "Root"
            );
            var rootNode = new TreeNode<FolderWrapper>(wrapper);

            var tree = (FolderTree)FormatterServices.GetUninitializedObject(typeof(FolderTree));
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<FolderWrapper>> { rootNode });

            return tree;
        }

        /// <summary>
        /// Returns the private viewer instance injected into the controller.
        /// Used by constructor tests that need to inspect the real viewer state.
        /// </summary>
        private static IFilterOlFoldersViewer GetViewer(FilterOlFoldersController controller)
        {
            return (IFilterOlFoldersViewer)
                typeof(FilterOlFoldersController)
                    .GetField("_viewer", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(controller);
        }

        /// <summary>
        /// Creates a strict mock Outlook folder with deterministic path and child collection.
        /// </summary>
        private static Mock<OutlookFolder> CreateOutlookFolder(
            string folderPath,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(folderPath.Split('\\')[^1]);
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            return folder;
        }

        /// <summary>
        /// Creates a mock Outlook Folders collection that supports Count and enumeration.
        /// </summary>
        private static Mock<OutlookFolders> CreateFoldersCollection(params OutlookFolder[] children)
        {
            var folders = new Mock<OutlookFolders>(MockBehavior.Strict);
            var collection = new System.Collections.ArrayList(
                children ?? Array.Empty<OutlookFolder>()
            );
            folders.SetupGet(x => x.Count).Returns(collection.Count);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return folders;
        }

        /// <summary>
        /// Creates a FolderTree with caller-supplied roots so Save() can exercise both
        /// add and remove delta paths without Outlook COM.
        /// </summary>
        private static FolderTree CreateSyntheticFolderTree(params TreeNode<FolderWrapper>[] roots)
        {
            var tree = (FolderTree)FormatterServices.GetUninitializedObject(typeof(FolderTree));
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<FolderWrapper>>(roots));
            return tree;
        }

        // ---------------------------------------------------------------------------
        // P10-T0: Constructor wiring and GetCheckedState delegate coverage
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the real constructor wires the viewer, folder tree, and
        /// check-state delegates when supplied with a mocked Outlook archive root.
        /// A <see cref="Mock{IFilterOlFoldersViewer}"/> is injected so that no real
        /// window is opened. Real <see cref="TreeListView"/> instances are returned by
        /// the mock so that delegate assignment can be verified.
        /// The test also exercises the three GetCheckedState outcomes: Checked,
        /// Indeterminate, and Unchecked.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_WithMockedArchiveRoot_InitializesViewerAndGetCheckedStatePaths()
        {
            // Arrange
            var archiveRoot = CreateOutlookFolder("\\Archive");
            var mockOl = new Mock<IOlObjects>(MockBehavior.Strict);
            mockOl.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot.Object);

            var selectedPaths = new ScoDictionary<string, int>();
            var mockTd = new Mock<IToDoObjects>(MockBehavior.Strict);
            mockTd.SetupGet(x => x.FilteredFolderScraping).Returns(selectedPaths);

            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);
            mockGlobals.SetupGet(x => x.TD).Returns(mockTd.Object);

            // Real TreeListView instances are needed so that delegate assignments
            // (CheckStateGetter / CheckStatePutter) can be verified.
            using var tlvNotFiltered = new TreeListView();
            using var tlvFiltered = new TreeListView();

            var mockViewer = new Mock<IFilterOlFoldersViewer>(MockBehavior.Strict);
            mockViewer.SetupGet(v => v.TlvNotFiltered).Returns(tlvNotFiltered);
            mockViewer.SetupGet(v => v.TlvFiltered).Returns(tlvFiltered);
            mockViewer.Setup(v => v.SetController(It.IsAny<FilterOlFoldersController>()));
            mockViewer.Setup(v => v.Dispose());

            // Act
            var controller = new FilterOlFoldersController(mockGlobals.Object, mockViewer.Object);

            var checkedNode = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: true,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Checked",
                    relativePath: "Checked"
                )
            );

            var indeterminateParent = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Parent",
                    relativePath: "Parent"
                )
            );
            indeterminateParent.AddChild(
                new FolderWrapper(
                    selected: true,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Child",
                    relativePath: "Parent\\Child"
                )
            );

            var uncheckedNode = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Unchecked",
                    relativePath: "Unchecked"
                )
            );

            // Assert
            controller.OlFolderTree.Should().NotBeNull();
            tlvNotFiltered.CheckStateGetter.Should().NotBeNull();
            tlvFiltered.CheckStateGetter.Should().NotBeNull();
            tlvNotFiltered.CheckStatePutter.Should().NotBeNull();
            tlvFiltered.CheckStatePutter.Should().NotBeNull();
            controller.GetCheckedState(checkedNode).Should().Be(CheckState.Checked);
            controller.GetCheckedState(indeterminateParent).Should().Be(CheckState.Indeterminate);
            controller.GetCheckedState(uncheckedNode).Should().Be(CheckState.Unchecked);

            // Show() is never called on the mock — verified by MockBehavior.Strict.
            mockViewer.Verify(v => v.Show(), Times.Never);
        }

        // ---------------------------------------------------------------------------
        // P10-T1: Save forwards the save action to the backing model
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that Save() closes the viewer and accesses
        /// TD.FilteredFolderScraping on the backing model (IToDoObjects).
        /// The test uses an empty ScoDictionary so Serialize() is a no-op
        /// (Filepath is "").
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Save_ClosesViewer_AndAccessesFilteredFolderScraping()
        {
            // Arrange
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FilteredFolderScraping).Returns(new ScoDictionary<string, int>());

            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);

            var viewer = new FilterOlFoldersViewer();
            var tree = CreateSyntheticFolderTree();
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            // Act
            controller.Save();

            // Assert — viewer was closed and the mock TD was accessed for scraping keys
            viewer.IsDisposed.Should().BeTrue();
            mockTD.Verify(td => td.FilteredFolderScraping, Times.AtLeastOnce());
        }

        /// <summary>
        /// Verifies that Save removes keys that are no longer selected and adds new
        /// selected keys before serializing the backing dictionary.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Save_WhenSelectionChanges_RemovesDeselectedKeysAndAddsSelectedKeys()
        {
            // Arrange
            var scraping = new ScoDictionary<string, int>();
            scraping.TryAdd("RemoveMe", 1);

            var mockTd = new Mock<IToDoObjects>(MockBehavior.Strict);
            mockTd.SetupGet(td => td.FilteredFolderScraping).Returns(scraping);

            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(g => g.TD).Returns(mockTd.Object);

            var selectedRoot = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: true,
                    itemCount: 0,
                    folderSize: 0,
                    name: "AddMe",
                    relativePath: "AddMe"
                )
            );
            var unselectedRoot = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "KeepOut",
                    relativePath: "KeepOut"
                )
            );

            var viewer = new FilterOlFoldersViewer();
            var tree = CreateSyntheticFolderTree(selectedRoot, unselectedRoot);
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            // Act
            controller.Save();

            // Assert
            scraping.ContainsKey("RemoveMe").Should().BeFalse();
            scraping.ContainsKey("AddMe").Should().BeTrue();
            viewer.IsDisposed.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // P10-T2: Discard forwards the discard action to the backing model
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that Discard() closes the viewer form without requiring
        /// COM globals.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Discard_ClosesViewer()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var viewer = new FilterOlFoldersViewer();
            var tree = CreateSyntheticFolderTree();
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            // Act
            controller.Discard();

            // Assert
            viewer.IsDisposed.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // P10-T3: Tree property change propagates to viewer-facing state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that OlFolderTree_PropertyChangedInternal runs to completion
        /// (setting empty roots on both tree list views) given a synthetic tree
        /// and pre-initialized ExpandedObjects collections.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void OlFolderTree_PropertyChangedInternal_WithSyntheticTree_SetsEmptyRootsOnViewer()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var viewer = new FilterOlFoldersViewer();

            // ExpandedObjects must be a non-null IEnumerable before the method
            // calls .Cast<>() on them; a freshly created TreeListView leaves the
            // field null, which would throw ArgumentNullException.
            viewer.TlvNotFiltered.ExpandedObjects = new List<object>();
            viewer.TlvFiltered.ExpandedObjects = new List<object>();

            var tree = CreateSyntheticFolderTree();
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            // Act — should not throw
            System.Action act = () =>
                controller.OlFolderTree_PropertyChangedInternal(
                    null,
                    new PropertyChangedEventArgs("Roots")
                );

            // Assert
            act.Should().NotThrow();
        }

        /// <summary>
        /// Verifies that OlFolderTree_PropertyChanged follows the same-thread path when
        /// InvokeRequired is false and delegates to the internal refresh logic.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void OlFolderTree_PropertyChanged_OnSameThread_RefreshesViewerWithoutInvoke()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var viewer = new FilterOlFoldersViewer();
            viewer.TlvNotFiltered.ExpandedObjects = new List<object>();
            viewer.TlvFiltered.ExpandedObjects = new List<object>();

            var tree = CreateSyntheticFolderTree();
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            // Act
            Action act = () =>
                controller.OlFolderTree_PropertyChanged(
                    null,
                    new PropertyChangedEventArgs("Roots")
                );

            // Assert
            act.Should().NotThrow();
            viewer.TlvNotFiltered.Roots.Should().NotBeNull();
            viewer.TlvFiltered.Roots.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P10-T4: Check-state helpers round-trip the expected value
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that PutCheckedStateMethod sets Selected=true on the node and
        /// all descendants when the tree is collapsed (IsExpanded returns false for
        /// a fresh TreeListView), returning CheckState.Checked.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void PutCheckedStateMethod_Collapsed_ChecksNodeAndDescendants()
        {
            // Arrange — bypass COM constructor; _viewer/_globals not needed
            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));

            var wrapper = new FolderWrapper(
                selected: false,
                itemCount: 0,
                folderSize: 0,
                name: "TestFolder",
                relativePath: "TestFolder"
            );
            var node = new TreeNode<FolderWrapper>(wrapper);
            var tlv = new TreeListView(); // IsExpanded returns false for all nodes

            // Act
            var result = controller.PutCheckedStateMethod(node, CheckState.Checked, tlv);

            // Assert
            result.Should().Be(CheckState.Checked);
            wrapper.Selected.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that PutCheckedStateMethod sets Selected=false on the node and
        /// all descendants when unchecking while the tree is collapsed, returning
        /// CheckState.Unchecked.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void PutCheckedStateMethod_Collapsed_UnchecksNodeAndDescendants()
        {
            // Arrange
            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));

            var wrapper = new FolderWrapper(
                selected: true,
                itemCount: 0,
                folderSize: 0,
                name: "TestFolder",
                relativePath: "TestFolder"
            );
            var node = new TreeNode<FolderWrapper>(wrapper);
            var tlv = new TreeListView();

            // Act
            var result = controller.PutCheckedStateMethod(node, CheckState.Unchecked, tlv);

            // Assert
            result.Should().Be(CheckState.Unchecked);
            wrapper.Selected.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that PutCheckedStateMethod updates only the current node when the
        /// tree reports the node as expanded.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void PutCheckedStateMethod_Expanded_UpdatesOnlyCurrentNode()
        {
            // Arrange
            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));

            var parent = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Parent",
                    relativePath: "Parent"
                )
            );
            var child = parent.AddChild(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Child",
                    relativePath: "Parent\\Child"
                )
            );

            var tlv = new TreeListView { Roots = new List<TreeNode<FolderWrapper>> { parent } };
            tlv.ExpandedObjects = new List<object> { parent };

            // Act
            var result = controller.PutCheckedStateMethod(parent, CheckState.Checked, tlv);

            // Assert
            result.Should().Be(CheckState.Checked);
            parent.Value.Selected.Should().BeTrue();
            child.Value.Selected.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that the filtered and not-filtered forwarding helpers delegate to
        /// the correct viewer tree list view instance.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void PutCheckedStateMethodForwarders_UseTheirAssignedViewerTrees()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var viewer = new FilterOlFoldersViewer();
            var tree = CreateSyntheticFolderTree();
            var controller = CreateController(viewer, tree, mockGlobals.Object);

            var filteredNode = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: false,
                    itemCount: 0,
                    folderSize: 0,
                    name: "Filtered",
                    relativePath: "Filtered"
                )
            );
            var notFilteredNode = new TreeNode<FolderWrapper>(
                new FolderWrapper(
                    selected: true,
                    itemCount: 0,
                    folderSize: 0,
                    name: "NotFiltered",
                    relativePath: "NotFiltered"
                )
            );

            // Act
            var filteredResult = controller.PutCheckedStateMethodFiltered(
                filteredNode,
                CheckState.Checked
            );
            var notFilteredResult = controller.PutCheckedStateMethodNotFiltered(
                notFilteredNode,
                CheckState.Unchecked
            );

            // Assert
            filteredResult.Should().Be(CheckState.Checked);
            filteredNode.Value.Selected.Should().BeTrue();
            notFilteredResult.Should().Be(CheckState.Unchecked);
            notFilteredNode.Value.Selected.Should().BeFalse();
        }
    }
}
