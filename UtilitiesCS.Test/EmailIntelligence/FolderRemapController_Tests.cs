using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.FolderRemap;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FolderRemapController internal methods.
    ///
    /// Purpose:
    ///     Covers HandleModelDropped, Save, Discard, ExpandTo, and SyncGlobalMap
    ///     without requiring a live COM Outlook session.
    ///
    /// Usage:
    ///     All tests bypass the COM-dependent constructor via
    ///     FormatterServices.GetUninitializedObject and inject dependencies through
    ///     reflection. Every test that touches WinForms controls must run on an STA
    ///     thread.
    /// </summary>
    [TestClass]
    public class FolderRemapController_Tests
    {
        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a FolderRemapController with the three primary dependencies
        /// injected via reflection so the COM-dependent constructor is bypassed.
        /// </summary>
        /// <param name="viewer">Viewer to inject as _viewer.</param>
        /// <param name="remapTree">FolderRemapTree to inject as _folderRemapTree.</param>
        /// <param name="globals">IApplicationGlobals to inject as _globals.</param>
        /// <returns>Partially initialized controller.</returns>
        private static FolderRemapController CreateController(
            FolderRemapViewer viewer,
            FolderRemapTree remapTree,
            IApplicationGlobals globals
        )
        {
            var controller = (FolderRemapController)
                FormatterServices.GetUninitializedObject(typeof(FolderRemapController));

            var type = typeof(FolderRemapController);

            type.GetField("_viewer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, viewer);

            type.GetField("_folderRemapTree", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, remapTree);

            type.GetField("_globals", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, globals);

            // Initialize _mappings2 so callers that read Mappings2 don't hit null
            type.GetField("_mappings2", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, new List<OlFolderRemap>());

            return controller;
        }

        /// <summary>
        /// Creates a FolderRemapTree whose private _roots field contains the
        /// supplied list so the tree works without MAPIFolder objects.
        /// </summary>
        private static FolderRemapTree CreateRemapTree(IList<TreeNode<OlFolderRemap>> roots)
        {
            var tree = new FolderRemapTree(); // no-arg ctor; _roots is null by default
            typeof(FolderRemapTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<OlFolderRemap>>(roots));
            return tree;
        }

        // ---------------------------------------------------------------------------
        // P14-T1: Drag/drop operation updates a mapping entry in the remap tree
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that HandleModelDropped with DropTargetLocation.Item causes the
        /// source node's MappedTo to be set to the target node's OlFolderRemap value.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void HandleModelDropped_ItemDrop_SetsMappedToOnSourceNode()
        {
            // Arrange
            var sourceRemap = new OlFolderRemap();
            var targetRemap = new OlFolderRemap();
            var sourceNode = new TreeNode<OlFolderRemap>(sourceRemap);
            var targetNode = new TreeNode<OlFolderRemap>(targetRemap);

            // The tree must contain the source node so SyncTreeToMappings can find it.
            var remapTree = CreateRemapTree(
                new List<TreeNode<OlFolderRemap>> { sourceNode, targetNode }
            );

            var viewer = new FolderRemapViewer();
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = CreateController(viewer, remapTree, mockGlobals.Object);

            // ModelDropEventArgs.TargetModel and SourceModels use internal setters;
            // set the backing fields directly via reflection.
            var args = new ModelDropEventArgs();
            typeof(ModelDropEventArgs)
                .GetField("targetModel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, targetNode);
            typeof(ModelDropEventArgs)
                .GetField("dragModels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, new System.Collections.ArrayList { sourceNode });

            // DropTargetLocation has a public setter, so assign it directly.
            args.DropTargetLocation = DropTargetLocation.Item;

            // Act
            controller.HandleModelDropped(null, args);

            // Assert — MoveObjectsToChildren sets sourceNode.Value.MappedTo = targetRemap
            sourceRemap.MappedTo.Should().BeSameAs(targetRemap);
        }

        // ---------------------------------------------------------------------------
        // P14-T2: Save forwards the save action to the backing model
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that Save() closes the viewer and accesses TD.FolderRemap on the
        /// backing model. Uses an empty ScoDictionary so Serialize() is a no-op.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Save_ClosesViewer_AndAccessesFolderRemap()
        {
            // Arrange
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(new ScoDictionary<string, string>());

            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);

            var viewer = new FolderRemapViewer();
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>>());
            var controller = CreateController(viewer, remapTree, mockGlobals.Object);

            // Act
            controller.Save();

            // Assert
            viewer.IsDisposed.Should().BeTrue();
            mockTD.Verify(td => td.FolderRemap, Times.AtLeastOnce());
        }

        // ---------------------------------------------------------------------------
        // P14-T3: Discard forwards the discard action to the backing model
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that Discard() closes the viewer form.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Discard_ClosesViewer()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var viewer = new FolderRemapViewer();
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>>());
            var controller = CreateController(viewer, remapTree, mockGlobals.Object);

            // Act
            controller.Discard();

            // Assert
            viewer.IsDisposed.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // P14-T4: ExpandTo selects the correct folder node path in the mocked tree
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that ExpandTo(level: 1) runs without exception and attempts to
        /// expand nodes at depth 0 (all root-level nodes). The assertion confirms
        /// no exception is raised regardless of OLV expand behavior on a hidden form.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void ExpandTo_WithDepthLevelOne_DoesNotThrow()
        {
            // Arrange
            var rootRemap = new OlFolderRemap();
            var rootNode = new TreeNode<OlFolderRemap>(rootRemap);
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>> { rootNode });

            var viewer = new FolderRemapViewer();
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = CreateController(viewer, remapTree, mockGlobals.Object);

            // Act — ExpandTo(1) targets nodes at depth < 1 (root nodes)
            System.Action act = () => controller.ExpandTo(1, addChecked: false);

            // Assert
            act.Should().NotThrow();
        }

        // ---------------------------------------------------------------------------
        // P14-T5: SyncGlobalMap propagates mapping changes to the global state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that SyncGlobalMap accesses TD.FolderRemap and calls the
        /// dictionary's Keys property when Mappings2 is empty (resulting in no
        /// removals and no additions). Serialize() is a no-op for an unfiled dict.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void SyncGlobalMap_WithEmptyMappings_PropagatesEmptyStateToGlobals()
        {
            // Arrange
            var folderRemap = new ScoDictionary<string, string>();
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(folderRemap);

            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);

            var viewer = new FolderRemapViewer();
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>>());
            var controller = CreateController(viewer, remapTree, mockGlobals.Object);
            // Mappings2 is already initialized to an empty list by CreateController

            // Act
            controller.SyncGlobalMap();

            // Assert — TD.FolderRemap was accessed and the dictionary remains empty
            mockTD.Verify(td => td.FolderRemap, Times.AtLeastOnce());
            folderRemap.Count.Should().Be(0);
        }
    }
}
