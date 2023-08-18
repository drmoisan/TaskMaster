using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using ToDoModel;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;

namespace ToDoModel.Test
{
	[TestClass]
    public class FolderHandlerTests_Written
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockApplicationGlobals;
        private Mock<IOlObjects> mockOlObjects;
        private Mock<Application> mockApplication;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockApplication = this.mockRepository.Create<Application>();
            this.mockOlObjects = this.mockRepository.Create<IOlObjects>();
            this.mockOlObjects.SetupGet(x => x.App).Returns(this.mockApplication.Object);
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            this.mockApplicationGlobals.SetupGet(x => x.Ol).Returns(this.mockOlObjects.Object);
        }

        private FolderHandler CreateFolderHandler()
        {
            return new FolderHandler(
                this.mockApplicationGlobals.Object);
        }
                
        [TestMethod]
        public void GetRelevantOlPathPortion_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string path = "\\\\email@company.com\\Folder 1\\Folder 2\\Folder 3";
            string root = "\\\\email@company.com";
            bool includeChildren = false;
            string expected = "Folder 3";

            // Act
            var actual = folderHandler.GetOlSubpath(
                path,
                root,
                includeChildren);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetRelevantOlPathPortion_StateUnderTest_ExpectedBehavior2()
        {
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string path = "\\\\email@company.com\\Folder 1\\Folder 2\\Folder 3";
            string root = "\\\\email@company.com";
            bool includeChildren = true;
            string expected = "\\Folder 1\\Folder 2\\Folder 3";

            // Act
            var actual = folderHandler.GetOlSubpath(
                path,
                root,
                includeChildren);

            // Assert
            Assert.AreEqual(expected, actual);
        }

    }
}
