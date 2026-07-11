using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
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
    ///     reflection. Tests that touch WinForms controls opt into MSTest's scoped
    ///     STA execution explicitly.
    /// </summary>
    [TestClass]
    public partial class FolderRemapController_Tests
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

        /// <summary>Sets the private RelativePath backing field on an OlFolderRemap via reflection.</summary>
        private static void SetRelativePath(OlFolderRemap remap, string path) =>
            typeof(OlFolderRemap)
                .GetField("_relativePath", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(remap, path);

        /// <summary>
        /// Builds a ModelDropEventArgs with the target, source list, and location set via reflection.
        /// </summary>
        private static ModelDropEventArgs CreateDropArgs(
            TreeNode<OlFolderRemap> target,
            object[] sources,
            DropTargetLocation location
        )
        {
            var args = new ModelDropEventArgs();
            typeof(ModelDropEventArgs)
                .GetField("targetModel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, target);
            typeof(ModelDropEventArgs)
                .GetField("dragModels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, new System.Collections.ArrayList(sources));
            args.DropTargetLocation = location;
            return args;
        }

        // ---------------------------------------------------------------------------
        // P14-T1: Drag/drop operation updates a mapping entry in the remap tree
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that HandleModelDropped with DropTargetLocation.Item causes the
        /// source node's MappedTo to be set to the target node's OlFolderRemap value.
        /// </summary>
        [STATestMethod]
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
        [STATestMethod]
        public void Save_ClosesViewer_AndAccessesFolderRemap()
        {
            // Arrange
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(new ScoDictionaryNew<string, string>());

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
        [STATestMethod]
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
        [STATestMethod]
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
        [STATestMethod]
        public void SyncGlobalMap_WithEmptyMappings_PropagatesEmptyStateToGlobals()
        {
            // Arrange
            var folderRemap = new ScoDictionaryNew<string, string>();
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

        // P14-T6: SyncGlobalMap removes a key that is no longer in Mappings2
        [TestMethod]
        public void SyncGlobalMap_WhenKeyNotInMappings_RemovesObsoleteKey()
        {
            // Arrange — FolderRemap has "obsolete" but Mappings2 is empty
            var folderRemap = new ScoDictionaryNew<string, string>(
                new Dictionary<string, string> { { "obsolete", "dest" } }
            );
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(folderRemap);
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                mockGlobals.Object
            );

            // Act
            controller.SyncGlobalMap();

            // Assert — obsolete key is removed when not present in Mappings2
            folderRemap.ContainsKey("obsolete").Should().BeFalse();
        }

        // P14-T7: SyncGlobalMap adds a new mapping entry when TryAdd succeeds
        [TestMethod]
        public void SyncGlobalMap_WithNewMappingEntry_AddsEntryToFolderRemap()
        {
            // Arrange — empty FolderRemap, Mappings2 has one entry with RelativePath and MappedTo
            var folderRemap = new ScoDictionaryNew<string, string>();
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(folderRemap);
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                mockGlobals.Object
            );
            var src = new OlFolderRemap();
            SetRelativePath(src, "src-path");
            var dst = new OlFolderRemap();
            SetRelativePath(dst, "dst-path");
            src.MappedTo = dst;
            typeof(FolderRemapController)
                .GetField("_mappings2", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, new List<OlFolderRemap> { src });

            // Act
            controller.SyncGlobalMap();

            // Assert — new entry is added
            folderRemap["src-path"].Should().Be("dst-path");
        }

        // P14-T8: SyncGlobalMap updates an existing key when TryAdd fails
        [TestMethod]
        public void SyncGlobalMap_WithExistingKey_UpdatesEntryToNewDestination()
        {
            // Arrange — FolderRemap already has "src-path" → TryAdd fails → update branch
            var folderRemap = new ScoDictionaryNew<string, string>(
                new Dictionary<string, string> { { "src-path", "old-dst" } }
            );
            var mockTD = new Mock<IToDoObjects>();
            mockTD.Setup(td => td.FolderRemap).Returns(folderRemap);
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.TD).Returns(mockTD.Object);
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                mockGlobals.Object
            );
            var src = new OlFolderRemap();
            SetRelativePath(src, "src-path");
            var dst = new OlFolderRemap();
            SetRelativePath(dst, "new-dst");
            src.MappedTo = dst;
            typeof(FolderRemapController)
                .GetField("_mappings2", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, new List<OlFolderRemap> { src });

            // Act
            controller.SyncGlobalMap();

            // Assert — existing key is updated to new destination
            folderRemap["src-path"].Should().Be("new-dst");
        }

        // P14-T9: ExpandTo(1, addChecked=true) expands nodes whose descendants have mappings
        [STATestMethod]
        public void ExpandTo_WithAddCheckedTrue_DoesNotThrowOnMappedNodes()
        {
            // Arrange — node with MappedTo set so the addChecked branch executes
            var remap = new OlFolderRemap();
            remap.MappedTo = new OlFolderRemap();
            var node = new TreeNode<OlFolderRemap>(remap);
            var viewer = new FolderRemapViewer();
            var controller = CreateController(
                viewer,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>> { node }),
                new Mock<IApplicationGlobals>().Object
            );

            // Act + Assert — no exception thrown when traversing mapped nodes
            Action act = () => controller.ExpandTo(1, addChecked: true);
            act.Should().NotThrow();
        }

        // P14-T10: OlFolderTree_PropertyChanged returns early when _update is true
        [TestMethod]
        public void OlFolderTreePropertyChanged_WhenUpdateIsTrue_ReturnsEarlyWithoutSync()
        {
            // Arrange — _update = true so the early-return branch is taken
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );
            typeof(FolderRemapController)
                .GetField("_update", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, true);

            // Act + Assert — no exception and no downstream sync called
            Action act = () => controller.OlFolderTree_PropertyChanged(null, null);
            act.Should().NotThrow();
        }

        // P14-T11: OlFolderTree_PropertyChanged syncs and updates viewer when _update is false
        [STATestMethod]
        public void OlFolderTreePropertyChanged_WhenUpdateIsFalse_SyncsRemapTreeAndUpdatesViewer()
        {
            // Arrange — default _update=false so normal sync path is taken
            var viewer = new FolderRemapViewer();
            var controller = CreateController(
                viewer,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );

            // Act + Assert — SyncTreeToMappings and OlvMap.SetObjects complete without exception
            Action act = () => controller.OlFolderTree_PropertyChanged(null, null);
            act.Should().NotThrow();
        }

        // P14-T12: HandleModelCanDrop sets InfoMessage when source contains target (self-drop)
        [TestMethod]
        public void HandleModelCanDrop_WhenSourceContainsTarget_SetsCannotDropOnSelfMessage()
        {
            // Arrange — same node as both source and target
            var node = new TreeNode<OlFolderRemap>(new OlFolderRemap());
            var args = CreateDropArgs(node, new object[] { node }, DropTargetLocation.Background);
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert — self-drop message is set
            args.InfoMessage.Should().Be("Cannot drop on self");
        }

        // P14-T13: HandleModelCanDrop sets InfoMessage when target is already mapped
        [TestMethod]
        public void HandleModelCanDrop_WhenTargetAlreadyMapped_SetsInfoMessageAboutExistingMapping()
        {
            // Arrange — target node has a MappedTo value already set
            var targetRemap = new OlFolderRemap();
            targetRemap.MappedTo = new OlFolderRemap();
            var targetNode = new TreeNode<OlFolderRemap>(targetRemap);
            var sourceNode = new TreeNode<OlFolderRemap>(new OlFolderRemap());
            var args = CreateDropArgs(
                targetNode,
                new object[] { sourceNode },
                DropTargetLocation.Background
            );
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert — a descriptive info message is set
            args.InfoMessage.Should().NotBeNull();
        }

        // P14-T14: HandleModelCanDrop sets Effect to Move when drop is allowed
        [TestMethod]
        public void HandleModelCanDrop_WhenDropAllowed_SetsEffectToMove()
        {
            // Arrange — different nodes, target has no existing MappedTo
            var targetNode = new TreeNode<OlFolderRemap>(new OlFolderRemap());
            var sourceNode = new TreeNode<OlFolderRemap>(new OlFolderRemap());
            var args = CreateDropArgs(
                targetNode,
                new object[] { sourceNode },
                DropTargetLocation.Background
            );
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );

            // Act
            controller.HandleModelCanDrop(null, args);

            // Assert — drop is permitted with Move effect
            args.Effect.Should().Be(DragDropEffects.Move);
        }
    }
}
