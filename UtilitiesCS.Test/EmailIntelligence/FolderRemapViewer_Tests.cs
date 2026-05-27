using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.FolderRemap;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FolderRemapViewer public surface and controller-forwarding paths.
    ///
    /// Purpose:
    ///     Covers drag/drop event forwarding, initial renderer/tree state after
    ///     SetController, and the FormatFileSize helper.
    ///
    /// Usage:
    ///     This class runs under MSTest's STA class execution mode because every
    ///     test instantiates FolderRemapViewer.
    ///     SetController tests inject an uninitialized controller whose _folderRemapTree
    ///     and _mappings2 are set via reflection to avoid COM access.
    /// </summary>
    [STATestClass]
    public class FolderRemapViewer_Tests
    {
        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a FolderRemapController with the minimum fields needed by
        /// SetupTree() injected via reflection so the COM constructor is avoided.
        /// </summary>
        private static FolderRemapController CreateControllerForViewer(FolderRemapTree remapTree)
        {
            var controller = (FolderRemapController)
                FormatterServices.GetUninitializedObject(typeof(FolderRemapController));

            var type = typeof(FolderRemapController);

            type.GetField("_folderRemapTree", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, remapTree);

            // Initialize _mappings2 so SetupTree -> OlvMap.SetObjects(Mappings2) doesn't crash
            type.GetField("_mappings2", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, new List<OlFolderRemap>());

            return controller;
        }

        /// <summary>
        /// Creates a FolderRemapTree whose private _roots list contains the given
        /// roots so it works without MAPIFolder objects.
        /// </summary>
        private static FolderRemapTree CreateRemapTree(IList<TreeNode<OlFolderRemap>> roots)
        {
            var tree = new FolderRemapTree();
            typeof(FolderRemapTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<OlFolderRemap>>(roots));
            return tree;
        }

        // ---------------------------------------------------------------------------
        // P15-T1: Viewer forwards a drag/drop event to the controller
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that TlvOriginal_ModelDropped (the private event handler bound in
        /// the Designer) forwards the event to the controller by invoking it via
        /// reflection. Uses DropTargetLocation.Background so HandleModelDropped is
        /// a no-op and no additional dependencies are required.
        /// </summary>
        [TestMethod]
        public void TlvOriginal_ModelDropped_ForwardsEventToController_DoesNotThrow()
        {
            // Arrange
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>>());
            var controller = CreateControllerForViewer(remapTree);
            var viewer = new FolderRemapViewer();

            // Inject the controller directly into the viewer's backing field so the
            // event handler can call _controller.HandleModelDropped without needing
            // SetController (which would call SetupTree -> accesses _viewer).
            typeof(FolderRemapViewer)
                .GetField("_controller", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(viewer, controller);

            // Build a ModelDropEventArgs with Background location — HandleModelDropped
            // executes an empty 'break' branch, verifying the forwarding without side effects.
            var args = new ModelDropEventArgs();

            // Act — invoke private forwarding method via reflection
            System.Action act = () =>
                typeof(FolderRemapViewer)
                    .GetMethod(
                        "TlvOriginal_ModelDropped",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    )
                    .Invoke(viewer, new object[] { viewer, args });

            // Assert
            act.Should().NotThrow();
        }

        // ---------------------------------------------------------------------------
        // P15-T2: Setup methods establish the expected initial renderer and tree state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that SetController runs to completion and configures the
        /// CanExpandGetter on TlvOriginal (confirming SetupTree executed).
        /// </summary>
        [TestMethod]
        public void SetController_WithSyntheticController_ConfiguresTreeDelegates()
        {
            // Arrange
            var remapTree = CreateRemapTree(new List<TreeNode<OlFolderRemap>>());
            var controller = CreateControllerForViewer(remapTree);
            var viewer = new FolderRemapViewer();

            // Act
            System.Action act = () => viewer.SetController(controller);

            // Assert
            act.Should().NotThrow();
            viewer.TlvOriginal.CanExpandGetter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P15-T3: File-size formatting helper returns the expected string
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "KB" string for a 1 KB input and a
        /// "bytes" string for a sub-KB input.
        /// </summary>
        [TestMethod]
        public void FormatFileSize_ReturnsExpectedStringForSampleInputs()
        {
            // Arrange
            var viewer = new FolderRemapViewer();

            // Act
            var bytesResult = viewer.FormatFileSize(512);
            var kbResult = viewer.FormatFileSize(1024);

            // Assert
            bytesResult.Should().EndWith("bytes");
            kbResult.Should().Contain("KB");
        }
    }
}
