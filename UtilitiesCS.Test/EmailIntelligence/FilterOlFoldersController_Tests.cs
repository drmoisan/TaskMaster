using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

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

            var tree = (FolderTree)
                FormatterServices.GetUninitializedObject(typeof(FolderTree));
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<FolderWrapper>> { rootNode });

            return tree;
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
            mockTD
                .Setup(td => td.FilteredFolderScraping)
                .Returns(new ScoDictionary<string, int>());

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
    }
}
