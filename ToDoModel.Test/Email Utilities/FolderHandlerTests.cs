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
    public class FolderHandlerTests
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
        public void FindFolder_StateUnderTest_ExpectedBehavior()
        {
            //ToDo: Write test for FindFolder
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string searchString = null;
            object objItem = null;
            bool reloadCTFStagingFiles = false;
            string emailSearchRoot = null;
            bool reCalcSuggestions = false;

            // Act
            var result = folderHandler.FindFolder(
                searchString,
                objItem,
                reloadCTFStagingFiles,
                emailSearchRoot,
                reCalcSuggestions);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetFolder_StateUnderTest_ExpectedBehavior()
        {
            //ToDo: Write test for GetFolder ExpectedBehavior
            // Arrange
            string folderpath = null;
            Application olApp = null;

            // Act
            var result = FolderHandler.GetFolder(
                folderpath,
                olApp);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetFolder_StateUnderTest_ExpectedBehavior1()
        {
            //ToDo: Write test for GetFolder ExpectedBehavior1
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string folderpath = null;

            // Act
            var result = folderHandler.GetFolder(
                folderpath);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetFolder_StateUnderTest_ExpectedBehavior2()
        {
            //ToDo: Write test for GetFolder ExpectedBehavior2
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string selectedValue = null;
            bool mustMatch = false;
            bool throwEx = false;

            // Act
            var result = folderHandler.GetFolder(
                selectedValue,
                mustMatch,
                throwEx);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void InputNewFoldername_StateUnderTest_ExpectedBehavior()
        {
            // Write test for InputNewFoldername ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            Folder parentFolder = null;

            // Act
            var result = folderHandler.InputNewFoldername(
                parentFolder);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateFolder_StateUnderTest_ExpectedBehavior()
        {
            // Write test for CreateFolder ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string parentBranchPath = null;
            string olRoot = null;
            string fsRoot = null;

            // Act
            var result = folderHandler.CreateFolder(
                parentBranchPath,
                olRoot,
                fsRoot);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddRecents_StateUnderTest_ExpectedBehavior()
        {
            // Write test for AddRecents ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            List<string> folderList = null;

            // Act
            folderHandler.AddRecents(
                ref folderList);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddMatches_StateUnderTest_ExpectedBehavior()
        {
            // Write test for AddMatches ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            List<string> matchingFolders = null;

            // Act
            folderHandler.AddMatches(
                matchingFolders);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddSuggestions_StateUnderTest_ExpectedBehavior()
        {
            // Write test for AddSuggestions ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            List<string> folderList = null;

            // Act
            folderHandler.AddSuggestions(
                ref folderList);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetMatchingFolders_StateUnderTest_ExpectedBehavior()
        {
            // Write test for GetMatchingFolders ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string searchString = null;
            string strEmailFolderPath = null;
            bool includeChildren = false;

            // Act
            var result = folderHandler.GetMatchingFolders(
                searchString,
                strEmailFolderPath,
                includeChildren);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void LoopFolders_StateUnderTest_ExpectedBehavior()
        {
            // Write test for LoopFolders ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            Folders folders = null;
            List<string> matchingFolders = null;
            string strEmailFolderPath = null;
            bool includeChildren = false;

            // Act
            folderHandler.LoopFolders(
                folders,
                ref matchingFolders,
                strEmailFolderPath,
                includeChildren);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
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
            var actual = folderHandler.GetRelevantOlPathPortion(
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
            var actual = folderHandler.GetRelevantOlPathPortion(
                path,
                root,
                includeChildren);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RecalculateSuggestions_StateUnderTest_ExpectedBehavior()
        {
            // Write test for RecalculateSuggestions ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            object ObjItem = null;
            bool ReloadCTFStagingFiles = false;

            // Act
            folderHandler.RecalculateSuggestions(
                ObjItem,
                ReloadCTFStagingFiles);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
