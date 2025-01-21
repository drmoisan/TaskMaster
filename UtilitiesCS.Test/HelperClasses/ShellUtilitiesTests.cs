using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS;

namespace ObjectListViewDemo.Tests
{
    [TestClass]
    public class ShellUtilitiesTests
    {
        private Mock<IFileInfo> _mockFileInfo;
        private Mock<IDirectoryInfo> _mockDirectoryInfo;

        [TestInitialize]
        public void Setup()
        {
            _mockFileInfo = new Mock<IFileInfo>();
            _mockDirectoryInfo = new Mock<IDirectoryInfo>();

            _mockFileInfo.Setup(f => f.FullName).Returns("TestFile.txt");
            _mockFileInfo.Setup(f => f.Exists).Returns(true);
            _mockFileInfo.Setup(f => f.Length).Returns(1024);
            _mockFileInfo.Setup(f => f.Directory).Returns(_mockDirectoryInfo.Object);

            _mockDirectoryInfo.Setup(d => d.FullName).Returns("TestDirectory");
            _mockDirectoryInfo.Setup(d => d.Exists).Returns(true);
        }

        [TestMethod]
        public void Execute_ExecutesFile()
        {
            // Arrange & Act
            var result = ShellUtilities.Execute(_mockFileInfo.Object.FullName);

            // Assert
            Assert.IsTrue(result >= 31);
        }

        [TestMethod]
        public void Execute_ExecutesOperationOnFile()
        {
            // Arrange & Act
            var result = ShellUtilities.Execute(_mockFileInfo.Object.FullName, "open");

            // Assert
            Assert.IsTrue(result >= 31);
        }

        [TestMethod]
        public void GetFileType_ReturnsFileType()
        {
            // Arrange & Act
            var result = ShellUtilities.GetFileType(_mockFileInfo.Object.FullName);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetFileIcon_ReturnsFileIcon()
        {
            // Arrange & Act
            var result = ShellUtilities.GetFileIcon(_mockFileInfo.Object.FullName, true, true);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetSysImageIndex_ReturnsSysImageIndex()
        {
            // Arrange & Act
            var result = ShellUtilities.GetSysImageIndex(_mockFileInfo.Object.FullName);

            // Assert
            Assert.IsTrue(result >= 0);
        }
    }

    [TestClass]
    public class SysImageListHelperTests
    {
        private ObjectListView _listView;
        private TreeView _treeView;
        private SysImageListHelper _listViewHelper;
        private SysImageListHelper _treeViewHelper;
        private Mock<IFileInfo> _mockFileInfo;
        private Mock<IDirectoryInfo> _mockDirectoryInfo;

        [TestInitialize]
        public void Setup()
        {
            _listView = new ObjectListView();
            _treeView = new TreeView();
            _listViewHelper = new SysImageListHelper(_listView);
            _treeViewHelper = new SysImageListHelper(_treeView);

            _mockFileInfo = new Mock<IFileInfo>();
            _mockDirectoryInfo = new Mock<IDirectoryInfo>();

            _mockFileInfo.Setup(f => f.FullName).Returns("TestFile.txt");
            _mockFileInfo.Setup(f => f.Exists).Returns(true);
            _mockFileInfo.Setup(f => f.Length).Returns(1024);
            _mockFileInfo.Setup(f => f.Directory).Returns(_mockDirectoryInfo.Object);

            _mockDirectoryInfo.Setup(d => d.FullName).Returns("TestDirectory");
            _mockDirectoryInfo.Setup(d => d.Exists).Returns(true);
        }

        [TestMethod]
        public void GetImageIndex_ReturnsImageIndexForFile()
        {
            // Arrange & Act
            var result = _listViewHelper.GetImageIndex(_mockFileInfo.Object.FullName);

            // Assert
            Assert.IsTrue(result >= 0);
        }

        [TestMethod]
        public void GetImageIndex_ReturnsImageIndexForDirectory()
        {
            // Arrange & Act
            var result = _listViewHelper.GetImageIndex(_mockDirectoryInfo.Object.FullName);

            // Assert
            Assert.IsTrue(result >= 0);
        }

        [TestMethod]
        public void SmallImageCollection_ReturnsSmallImageCollection()
        {
            // Arrange & Act
            var result = GetProtectedProperty<ImageList.ImageCollection>(_listViewHelper, "SmallImageCollection");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LargeImageCollection_ReturnsLargeImageCollection()
        {
            // Arrange & Act
            var result = GetProtectedProperty<ImageList.ImageCollection>(_listViewHelper, "LargeImageCollection");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SmallImageList_ReturnsSmallImageList()
        {
            // Arrange & Act
            var result = GetProtectedProperty<ImageList>(_listViewHelper, "SmallImageList");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LargeImageList_ReturnsLargeImageList()
        {
            // Arrange & Act
            var result = GetProtectedProperty<ImageList>(_listViewHelper, "LargeImageList");

            // Assert
            Assert.IsNotNull(result);
        }

        private T GetProtectedProperty<T>(object instance, string propertyName)
        {
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)property.GetValue(instance);
        }
    }
}
