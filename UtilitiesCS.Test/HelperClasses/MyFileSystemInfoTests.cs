using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using UtilitiesCS;

namespace ObjectListViewDemo.Tests
{
    [TestClass]
    public class MyFileSystemInfoTests
    {
        private Mock<IFileInfo> _mockFileInfo;
        private Mock<IDirectoryInfo> _mockDirectoryInfo;

        [TestInitialize]
        public void Setup()
        {
            _mockFileInfo = new Mock<IFileInfo>();
            _mockDirectoryInfo = new Mock<IDirectoryInfo>();

            _mockDirectoryInfo.Setup(d => d.FullName).Returns("TestDirectory");
            _mockDirectoryInfo.Setup(d => d.Name).Returns("TestDirectory");
            _mockDirectoryInfo.Setup(d => d.Attributes).Returns(System.IO.FileAttributes.Directory);
            
            _mockFileInfo.Setup(f => f.FullName).Returns("TestFile.txt");
            _mockFileInfo.Setup(f => f.Name).Returns("TestFile.txt");
            _mockFileInfo.Setup(f => f.Extension).Returns(".txt");
            _mockFileInfo.Setup(f => f.CreationTime).Returns(DateTime.Now);
            _mockFileInfo.Setup(f => f.LastWriteTime).Returns(DateTime.Now);
            _mockFileInfo.Setup(f => f.Attributes).Returns(System.IO.FileAttributes.Normal);
            _mockFileInfo.Setup(f => f.Length).Returns(1024);
            _mockFileInfo.Setup(f => f.Directory).Returns(_mockDirectoryInfo.Object);
        }

        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.FullName, myFileSystemInfo.FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ThrowsExceptionForNullArgument()
        {
            // Arrange & Act
            var myFileSystemInfo = new MyFileSystemInfo(default(FileSystemInfo));
        }

        [TestMethod]
        public void IsDirectory_ReturnsTrueForDirectory()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockDirectoryInfo.Object);

            // Act
            var result = myFileSystemInfo.IsDirectory;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDirectory_ReturnsFalseForFile()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.IsDirectory;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AsDirectory_ReturnsDirectoryInfo()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockDirectoryInfo.Object);

            // Act
            var result = myFileSystemInfo.AsDirectory;

            // Assert
            Assert.AreEqual(_mockDirectoryInfo.Object, result);
        }

        [TestMethod]
        public void AsFile_ReturnsFileInfo()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.AsFile;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object, result);
        }

        [TestMethod]
        public void Info_ReturnsFileSystemInfo()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.Info;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object, result);
        }

        [TestMethod]
        public void Name_ReturnsCorrectName()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.Name;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.Name, result);
        }

        [TestMethod]
        public void Extension_ReturnsCorrectExtension()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.Extension;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.Extension, result);
        }

        [TestMethod]
        public void CreationTime_ReturnsCorrectCreationTime()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.CreationTime;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.CreationTime, result);
        }

        [TestMethod]
        public void LastWriteTime_ReturnsCorrectLastWriteTime()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.LastWriteTime;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.LastWriteTime, result);
        }

        [TestMethod]
        public void FullName_ReturnsCorrectFullName()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.FullName;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.FullName, result);
        }

        [TestMethod]
        public void Attributes_ReturnsCorrectAttributes()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.Attributes;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.Attributes, result);
        }

        [TestMethod]
        public void Length_ReturnsCorrectLength()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo.Length;

            // Assert
            Assert.AreEqual(_mockFileInfo.Object.Length, result);
        }

        [TestMethod]
        public void GetFileSystemInfos_ReturnsChildInfosForDirectory()
        {
            // Arrange
            var myFileSystemInfo = new MyFileSystemInfo(_mockDirectoryInfo.Object);
            var mockChildFile = new Mock<IFileInfo>();
            mockChildFile.Setup(f => f.FullName).Returns("ChildFile.txt");

            _mockDirectoryInfo.Setup(d => d.GetFileSystemInfos()).Returns(new IFileSystemInfo[] { mockChildFile.Object });

            // Act
            var result = myFileSystemInfo.GetFileSystemInfos();

            // Assert
            foreach (MyFileSystemInfo info in result)
            {
                Assert.AreEqual(mockChildFile.Object.FullName, info.FullName);
            }
        }

        [TestMethod]
        public void Equals_ReturnsTrueForSamePath()
        {
            // Arrange
            var myFileSystemInfo1 = new MyFileSystemInfo(_mockFileInfo.Object);
            var myFileSystemInfo2 = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo1.Equals(myFileSystemInfo2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_ReturnsFalseForDifferentPaths()
        {
            // Arrange
            var myFileSystemInfo1 = new MyFileSystemInfo(_mockFileInfo.Object);
            var myFileSystemInfo2 = new MyFileSystemInfo(_mockDirectoryInfo.Object);

            // Act
            var result = myFileSystemInfo1.Equals(myFileSystemInfo2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetHashCode_ReturnsSameHashCodeForSamePath()
        {
            // Arrange
            var myFileSystemInfo1 = new MyFileSystemInfo(_mockFileInfo.Object);
            var myFileSystemInfo2 = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var hashCode1 = myFileSystemInfo1.GetHashCode();
            var hashCode2 = myFileSystemInfo2.GetHashCode();

            // Assert
            Assert.AreEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void OperatorEquals_ReturnsTrueForSamePath()
        {
            // Arrange
            var myFileSystemInfo1 = new MyFileSystemInfo(_mockFileInfo.Object);
            var myFileSystemInfo2 = new MyFileSystemInfo(_mockFileInfo.Object);

            // Act
            var result = myFileSystemInfo1 == myFileSystemInfo2;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void OperatorNotEquals_ReturnsTrueForDifferentPaths()
        {
            // Arrange
            var myFileSystemInfo1 = new MyFileSystemInfo(_mockFileInfo.Object);
            var myFileSystemInfo2 = new MyFileSystemInfo(_mockDirectoryInfo.Object);

            // Act
            var result = myFileSystemInfo1 != myFileSystemInfo2;

            // Assert
            Assert.IsTrue(result);
        }
    }
}
