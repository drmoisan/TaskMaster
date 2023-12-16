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
    public class FolderHandlerTests_ToWrite
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
            List<string> emailSearchRoots = null;
            bool reCalcSuggestions = false;

            // Act
            var result = folderHandler.FindFolder(
                searchString,
                objItem,
                reloadCTFStagingFiles,
                emailSearchRoots,
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
            var folderHandler = new FolderHandler(olApp);
            var result = folderHandler.GetFolder(
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
            bool throwEx = false;

            // Act
            var result = folderHandler.GetFolder(
                selectedValue,
                throwEx);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void InputNewFoldername_StateUnderTest_ExpectedBehavior()
        {
            // ToDo: Write test for InputNewFoldername ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            Folder parentFolder = null;

            // Act
            var result = folderHandler.InputFoldername(
                parentFolder);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateFolder_StateUnderTest_ExpectedBehavior()
        {
            // ToDo: Write test for CreateFolder ExpectedBehavior
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
            // ToDo: Write test for AddRecents ExpectedBehavior
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
            // ToDo: Write test for AddMatches ExpectedBehavior
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
            // ToDo: Write test for AddSuggestions ExpectedBehavior
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
            // ToDo: Write test for GetMatchingFolders ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            string searchString = null;
            string strEmailFolderPath = null;
            bool includeChildren = false;
            IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions = null;

            // Act
            var result = folderHandler.GetMatchingFolders(
                searchString,
                strEmailFolderPath,
                includeChildren,
                exclusions);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void LoopFolders_StateUnderTest_ExpectedBehavior()
        {
            // ToDo: Write test for LoopFolders ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            Folders folders = null;
            List<string> matchingFolders = null;
            string strEmailFolderPath = null;
            bool includeChildren = false;
            IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions = null;

            // Act
            folderHandler.LoopFolders(
                folders,
                ref matchingFolders,
                strEmailFolderPath,
                includeChildren,
                exclusions);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RecalculateSuggestions_StateUnderTest_ExpectedBehavior()
        {
            // ToDo: Write test for RecalculateSuggestions ExpectedBehavior
            // Arrange
            var folderHandler = this.CreateFolderHandler();
            object ObjItem = null;

            // Act
            folderHandler.RefreshSuggestions(ObjItem);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
