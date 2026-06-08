using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for FolderInfoViewer.SetFolderTree.
    ///
    /// Purpose:
    ///     Covers the SetFolderTree assignment and re-assignment paths to ensure the
    ///     FolderTree property mirrors the most-recently supplied reference.
    ///
    /// Usage:
    ///     This class runs under MSTest's STA class execution mode because every
    ///     test instantiates FolderInfoViewer and FolderTree.
    ///     A synthetic FolderTree with an empty _roots list is used to avoid
    ///     accessing COM MAPIFolder objects; TreeListView.Roots accepts null/empty.
    /// </summary>
    [STATestClass]
    public class FolderInfoViewer_Tests
    {
        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a FolderTree whose _roots field is set to an empty list so that
        /// SetFolderTree can set Tlv.Roots without requiring a MAPIFolder.
        /// </summary>
        private static FolderTree CreateEmptyFolderTree()
        {
            var tree = (FolderTree)FormatterServices.GetUninitializedObject(typeof(FolderTree));
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tree, new List<TreeNode<FolderWrapper>>());
            return tree;
        }

        // ---------------------------------------------------------------------------
        // P12-T1: SetFolderTree updates the FolderTree property to the assigned reference
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that after calling SetFolderTree the internal FolderTree property
        /// returns the same instance that was passed in.
        /// </summary>
        [TestMethod]
        public void SetFolderTree_AssignedOnce_PropertyReturnsSuppliedReference()
        {
            // Arrange
            var viewer = new FolderInfoViewer();
            var tree = CreateEmptyFolderTree();

            // Act
            viewer.SetFolderTree(tree);

            // Assert
            viewer.FolderTree.Should().BeSameAs(tree);
        }

        // ---------------------------------------------------------------------------
        // P12-T2: Assigning a new tree reference via SetFolderTree replaces the prior reference
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling SetFolderTree a second time replaces the previously
        /// stored reference with the most-recently supplied instance.
        /// </summary>
        [TestMethod]
        public void SetFolderTree_ReassignedWithNewInstance_PropertyReturnsMostRecentReference()
        {
            // Arrange
            var viewer = new FolderInfoViewer();
            var firstTree = CreateEmptyFolderTree();
            var secondTree = CreateEmptyFolderTree();

            // Act
            viewer.SetFolderTree(firstTree);
            viewer.SetFolderTree(secondTree);

            // Assert
            viewer.FolderTree.Should().BeSameAs(secondTree);
            viewer.FolderTree.Should().NotBeSameAs(firstTree);
        }
    }
}
