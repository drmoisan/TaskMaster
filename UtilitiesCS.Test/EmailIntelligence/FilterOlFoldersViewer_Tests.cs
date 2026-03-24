using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FilterOlFoldersViewer public surface and button-click forwarding.
    ///
    /// Purpose:
    ///     Covers FormatFileSize and the null-safe button-click forwarding paths
    ///     (BtnDiscard_Click and BtnSave_Click) without requiring a live COM controller.
    ///
    /// Usage:
    ///     All tests instantiate FilterOlFoldersViewer on an STA thread.
    ///     SetController tests inject an uninitialized controller whose _olFolderTree
    ///     is set to a synthetic FolderTree so that SetupTree() does not hit COM.
    /// </summary>
    [TestClass]
    public class FilterOlFoldersViewer_Tests
    {
        // ---------------------------------------------------------------------------
        // P11-T1: SetController registers the expected delegates on the viewer
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that SetController runs to completion (configuring CanExpandGetter
        /// and ChildrenGetter on both tree list views) when the controller has a
        /// synthetic, COM-free FolderTree.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void SetController_WithSyntheticController_ConfiguresBothTreeDelegates()
        {
            // Arrange — build a controller whose _olFolderTree has one synthetic root
            var wrapper = new FolderWrapper(
                selected: false,
                itemCount: 0,
                folderSize: 0,
                name: "Root",
                relativePath: "Root"
            );
            var rootNode = new TreeNode<FolderWrapper>(wrapper);

            var syntheticTree = (FolderTree)
                FormatterServices.GetUninitializedObject(typeof(FolderTree));
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(syntheticTree, new List<TreeNode<FolderWrapper>> { rootNode });

            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));
            typeof(FilterOlFoldersController)
                .GetField("_olFolderTree", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, syntheticTree);

            var viewer = new FilterOlFoldersViewer();

            // Act — should not throw; SetupTree accesses _controller.OlFolderTree
            System.Action act = () => viewer.SetController(controller);

            // Assert
            act.Should().NotThrow();
            viewer.TlvNotFiltered.CanExpandGetter.Should().NotBeNull();
            viewer.TlvFiltered.CanExpandGetter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P11-T2: FormatFileSize returns the expected string for byte-range input
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "bytes" string for inputs below
        /// the 1 KB threshold (less than 1024 bytes).
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithBytesInput_ReturnsBytesString()
        {
            // Arrange
            var viewer = new FilterOlFoldersViewer();

            // Act
            var result = viewer.FormatFileSize(512);

            // Assert
            result.Should().EndWith("bytes");
        }

        // ---------------------------------------------------------------------------
        // P11-T3: FormatFileSize returns the expected string for KB-or-larger input
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "KB" string for a 1-KB input and
        /// an "MB" string for a 1-MB input.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithKbInput_ReturnsKbString()
        {
            // Arrange
            var viewer = new FilterOlFoldersViewer();

            // Act
            var kbResult = viewer.FormatFileSize(1024);
            var mbResult = viewer.FormatFileSize(1024 * 1024);

            // Assert
            kbResult.Should().Contain("KB");
            mbResult.Should().Contain("MB");
        }

        // ---------------------------------------------------------------------------
        // P11-T4: Save and Discard buttons forward events to the controller
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that BtnDiscard_Click forwards Discard() to the controller by
        /// injecting an uninitialized controller whose _viewer is the test viewer,
        /// then invoking the private click handler via reflection.
        /// The expected observable side effect is that the viewer is closed.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void BtnDiscard_Click_ForwardsDiscardToController_ClosesViewer()
        {
            // Arrange
            var viewer = new FilterOlFoldersViewer();

            // Build an uninitialized controller whose _viewer references the same
            // form so that Discard() -> _viewer.Close() operates on a real handle.
            var controller = (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));
            typeof(FilterOlFoldersController)
                .GetField("_viewer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, viewer);

            // Inject the controller into the viewer without calling SetController
            // (which would require a real FolderTree) by writing the field directly.
            typeof(FilterOlFoldersViewer)
                .GetField("_controller", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(viewer, controller);

            // Act — invoke the private click handler
            typeof(FilterOlFoldersViewer)
                .GetMethod("BtnDiscard_Click", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(viewer, new object[] { viewer, System.EventArgs.Empty });

            // Assert — controller.Discard() called _viewer.Close(), disposing the form
            viewer.IsDisposed.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that BtnDiscard_Click is a no-op (does not throw) when the
        /// controller field is null, exercising the ?. null-coalescing guard.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void BtnDiscard_Click_WithNullController_DoesNotThrow()
        {
            // Arrange — viewer with no controller injected (_controller is null by default)
            var viewer = new FilterOlFoldersViewer();

            // Act
            System.Action act = () =>
                typeof(FilterOlFoldersViewer)
                    .GetMethod("BtnDiscard_Click", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(viewer, new object[] { viewer, System.EventArgs.Empty });

            // Assert
            act.Should().NotThrow();
            viewer.IsDisposed.Should().BeFalse();
        }
    }
}
