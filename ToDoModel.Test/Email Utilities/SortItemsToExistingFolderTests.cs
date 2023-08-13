using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using ToDoModel;
using UtilitiesCS;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

namespace ToDoModel.Test
{
	[TestClass]
    public class SortItemsToExistingFolderTests
    {
        private MockRepository mockRepository;
        private Mock<Attachment> mockAttachment;


        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockAttachment = this.mockRepository.Create<Attachment>();
        }

        [TestMethod]
        public void InitializeSortToExisting_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            string InitType = null;
            bool QuickLoad = false;
            bool WholeConversation = false;
            string strSeed = null;
            object objItem = null;

                // Act
                SortEmail.InitializeSortToExisting(
                        InitType,
                        QuickLoad,
                        WholeConversation,
                        strSeed,
                        objItem);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Run_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            bool savePictures = false;
            string destinationFolderpath = null;
            bool saveMsg = false;
            bool saveAttachments = false;
            bool removeFlowFile = false;
            IApplicationGlobals appGlobals = null;

            // Act
            SortEmail.Run(
                savePictures,
                destinationFolderpath,
                saveMsg,
                saveAttachments,
                removeFlowFile,
                appGlobals);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Run_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            IList<MailItem> mailItems = null;
            bool savePictures = false;
            string destinationFolderpath = null;
            bool saveMsg = false;
            bool saveAttachments = false;
            bool removeFlowFile = false;
            IApplicationGlobals appGlobals = null;

            // Act
            SortEmail.Run(
                mailItems,
                savePictures,
                destinationFolderpath,
                saveMsg,
                saveAttachments,
                removeFlowFile,
                appGlobals);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Run_StateUnderTest_ExpectedBehavior2()
        {
            // Arrange
            IList<MailItem> mailItems = null;
            bool savePictures = false;
            string destinationOlPath = null;
            bool saveMsg = false;
            bool saveAttachments = false;
            bool removePreviousFsFiles = false;
            IApplicationGlobals appGlobals = null;
            string olAncestor = null;
            string fsAncestorEquivalent = null;

            // Act
            SortEmail.Run(
                mailItems,
                savePictures,
                destinationOlPath,
                saveMsg,
                saveAttachments,
                removePreviousFsFiles,
                appGlobals,
                olAncestor,
                fsAncestorEquivalent);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetAttachmentFilename_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            this.mockAttachment.SetupAllProperties();
            this.mockAttachment.Setup(x => x.FileName).Returns("TestAttachment.atm");
            Attachment attachment = this.mockAttachment.Object;
            string expectedFilename = "TestAttachment";
            string expectedExtension = ".atm";
            string actualFilename = "";
            string actualExtension = "";
            

            // Act
            //(filenameActual, extensionActual) = SortItemsToExistingFolder.GetAttachmentFilename(attachment);

            // Assert
            AssertAll.Check
            (
                () => Assert.AreEqual(expectedFilename, actualFilename),
                () => Assert.AreEqual(expectedExtension, actualExtension)
            );

        }

        [TestMethod]
        public void GetAttachmentFilename_StateUnderTest_NoExtension()
        {
            // Arrange
            string filenameExpected = "TestAttachment";
            string extensionExpected = "";
            this.mockAttachment.SetupAllProperties();
            this.mockAttachment.Setup(x => x.FileName).Returns(filenameExpected);
            Attachment attachment = this.mockAttachment.Object;
            string actualFilename = "";
            string actualExtension = "";


            // Act
            //(filenameActual, extensionActual) = SortItemsToExistingFolder.GetAttachmentFilename(attachment);

            // Assert
            AssertAll.Check
            (
                () => Assert.AreEqual(filenameExpected, actualFilename),
                () => Assert.AreEqual(extensionExpected, actualExtension)
            );

        }

        [TestMethod]
        public void Run2_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            IList<MailItem> mailItems = null;
            bool savePictures = false;
            string destinationFolderpath = null;
            bool saveMsg = false;
            bool saveAttachments = false;
            bool removeFlowFile = false;
            IApplicationGlobals appGlobals = null;
            string olRoot = null;
            string fsRoot = null;

            // Act
            SortEmail.Run2(
                mailItems,
                savePictures,
                destinationFolderpath,
                saveMsg,
                saveAttachments,
                removeFlowFile,
                appGlobals,
                olRoot,
                fsRoot);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Cleanup_Files_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            SortEmail.Cleanup_Files();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
