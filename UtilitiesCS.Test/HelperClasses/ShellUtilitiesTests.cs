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
using ObjectListViewDemo;

namespace UtilitiesCS.Test.HelperClasses.ObjListViewDemo
{
    //[TestClass]
    //public class ShellUtilitiesTests
    //{
    //    private Mock<IFileInfo> _mockFileInfo;
    //    private Mock<IDirectoryInfo> _mockDirectoryInfo;

    //    [TestInitialize]
    //    public void Setup()
    //    {
    //        _mockFileInfo = new Mock<IFileInfo>();
    //        _mockDirectoryInfo = new Mock<IDirectoryInfo>();

    //        _mockFileInfo.Setup(f => f.FullName).Returns("TestFile.txt");
    //        _mockFileInfo.Setup(f => f.Exists).Returns(true);
    //        _mockFileInfo.Setup(f => f.Length).Returns(1024);
    //        _mockFileInfo.Setup(f => f.Directory).Returns(_mockDirectoryInfo.Object);

    //        _mockDirectoryInfo.Setup(d => d.FullName).Returns("TestDirectory");
    //        _mockDirectoryInfo.Setup(d => d.Exists).Returns(true);
    //    }

    //    [TestMethod]
    //    public void Execute_ExecutesFile()
    //    {
    //        // Arrange & Act
    //        var shellUtilities = new ShellUtilities();
    //        var result = shellUtilities.Execute(_mockFileInfo.Object.FullName);

    //        // Assert
    //        Assert.IsTrue(result >= 31);
    //    }

    //    [TestMethod]
    //    public void Execute_ExecutesOperationOnFile()
    //    {
    //        // Arrange & Act
    //        var shellUtilities = new ShellUtilities();
    //        var result = shellUtilities.Execute(_mockFileInfo.Object.FullName, "open");

    //        // Assert
    //        Assert.IsTrue(result >= 31);
    //    }

    //    [TestMethod]
    //    public void GetFileType_ReturnsFileType()
    //    {
    //        // Arrange & Act
    //        var shellUtilities = new ShellUtilities();
    //        var result = shellUtilities.GetFileType(_mockFileInfo.Object.FullName);

    //        // Assert
    //        Assert.IsFalse(string.IsNullOrEmpty(result));
    //    }

    //    [TestMethod]
    //    public void GetFileIcon_ReturnsFileIcon()
    //    {
    //        // Arrange & Act
    //        var shellUtilities = new ShellUtilities();
    //        var result = shellUtilities.GetFileIcon(_mockFileInfo.Object.FullName, true, true);

    //        // Assert
    //        Assert.IsNotNull(result);
    //    }

    //    [TestMethod]
    //    public void GetSysImageIndex_ReturnsSysImageIndex()
    //    {
    //        // Arrange
    //        var shellUtilities = new ShellUtilities();

    //        // Act
    //        var result = shellUtilities.GetSysImageIndex(_mockFileInfo.Object.FullName);

    //        // Assert
    //        Assert.IsTrue(result >= 0);
    //    }
    //}

    
}
