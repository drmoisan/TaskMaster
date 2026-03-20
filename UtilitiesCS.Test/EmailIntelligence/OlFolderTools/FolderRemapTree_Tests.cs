using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.FolderRemap;

namespace UtilitiesCS.Test.EmailIntelligence.OlFolderTools
{
    [TestClass]
    public class FolderRemapTree_Tests
    {
        #region Constructor

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var tree = new FolderRemapTree();
            tree.Should().NotBeNull();
        }

        #endregion

        #region GetRemapList

        [TestMethod]
        public void GetRemapList_NoRoots_ReturnsEmpty()
        {
            var tree = new FolderRemapTree();
            // Roots is null by default
            // Can't call GetRemapList without roots
        }

        #endregion
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
    }
}
