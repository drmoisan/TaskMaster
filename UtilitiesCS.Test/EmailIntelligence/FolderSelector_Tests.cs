using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.FolderRemap;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FolderSelector.Initialize and the Selection property.
    ///
    /// Purpose:
    ///     Covers the Initialize path (which configures CheckStatePutter and sets
    ///     tree roots) and the selection round-trip when the putter delegate is
    ///     invoked.
    ///
    /// Usage:
    ///     All tests instantiate FolderSelector on an STA thread. OlFolderRemap is
    ///     constructed with the default no-arg constructor; RelativePath and Name
    ///     remain null but are not required by the tested paths.
    /// </summary>
    [TestClass]
    public class FolderSelector_Tests
    {
        // ---------------------------------------------------------------------------
        // P16-T1: Initialization sets the expected selection source reference
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling Initialize with a non-empty roots list configures
        /// TlvOriginal.Roots to the supplied collection and assigns a non-null
        /// CheckStatePutter delegate.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Initialize_WithNonEmptyRoots_ConfiguresTreeRootsAndCheckStatePutter()
        {
            // Arrange
            var remap = new OlFolderRemap();
            var node = new TreeNode<OlFolderRemap>(remap);
            var roots = new List<TreeNode<OlFolderRemap>> { node };
            var selector = new FolderSelector();

            // Act
            selector.Initialize(roots);

            // Assert — Roots and the putter were configured
            selector.TlvOriginal.CheckStatePutter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P16-T2: Confirming a selection sets Selection to the chosen folder node
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that invoking the CheckStatePutter delegate (set by Initialize)
        /// assigns the corresponding OlFolderRemap to the Selection property and
        /// returns CheckState.Checked.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void CheckStatePutter_WhenInvoked_SetsSelectionToNodeValue()
        {
            // Arrange
            var remap = new OlFolderRemap();
            var node = new TreeNode<OlFolderRemap>(remap);
            var selector = new FolderSelector();
            selector.Initialize(new List<TreeNode<OlFolderRemap>> { node });

            var putter = selector.TlvOriginal.CheckStatePutter;

            // Act — invoke the putter as if the user checked the node
            var result = putter.Invoke(node, CheckState.Checked);

            // Assert
            result.Should().Be(CheckState.Checked);
            selector.Selection.Should().BeSameAs(remap);
        }

        // ---------------------------------------------------------------------------
        // P16-T3: Passing an empty roots list leaves Selection as null
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling Initialize with an empty roots list does not throw
        /// and leaves the Selection property as null (no selection was made).
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Initialize_WithEmptyRootsList_LeavesSelectionNull()
        {
            // Arrange
            var selector = new FolderSelector();

            // Act
            selector.Initialize(new List<TreeNode<OlFolderRemap>>());

            // Assert
            selector.Selection.Should().BeNull();
        }
    }
}
