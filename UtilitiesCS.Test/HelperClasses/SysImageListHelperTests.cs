using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ObjectListViewDemo;
using BrightIdeasSoftware;
using Moq;
using System.Reflection;
using System.Windows.Forms;

namespace UtilitiesCS.Test.HelperClasses.ObjListViewDemo
{
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
