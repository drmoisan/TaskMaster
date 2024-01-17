using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesCS;

namespace Z.Unfinished.ToDoModel.Test.Email_Utilities
{
    [TestClass]
    public class Disabled_SortItemsToExistingFolderTests_Unfinished
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
        public void Disabled_InitializeSortToExisting_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //string InitType = null;
            //bool QuickLoad = false;
            //bool WholeConversation = false;
            //string strSeed = null;
            //object objItem = null;

            //// Act
            //SortEmail.InitializeSortToExisting(
            //        InitType,
            //        QuickLoad,
            //        WholeConversation,
            //        strSeed,
            //        objItem);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task Disabled_Run_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //bool savePictures = false;
            //string destinationFolderpath = null;
            //bool saveMsg = false;
            //bool saveAttachments = false;
            //bool removeFlowFile = false;
            //IApplicationGlobals appGlobals = null;

            //// Act
            //await SortEmail.SortAsync(
            //    savePictures,
            //    destinationFolderpath,
            //    saveMsg,
            //    saveAttachments,
            //    removeFlowFile,
            //    appGlobals);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task Disabled_Run_StateUnderTest_ExpectedBehavior1()
        {
            //// Arrange
            //IList<MailItem> mailItems = null;
            //bool savePictures = false;
            //string destinationFolderpath = null;
            //bool saveMsg = false;
            //bool saveAttachments = false;
            //bool removeFlowFile = false;
            //IApplicationGlobals appGlobals = null;

            //// Act
            //await SortEmail.SortAsync(
            //    mailItems,
            //    savePictures,
            //    destinationFolderpath,
            //    saveMsg,
            //    saveAttachments,
            //    removeFlowFile,
            //    appGlobals);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Disabled_Run_StateUnderTest_ExpectedBehavior2()
        {
            //// Arrange
            //IList<MailItem> mailItems = null;
            //bool savePictures = false;
            //string destinationOlPath = null;
            //bool saveMsg = false;
            //bool saveAttachments = false;
            //bool removePreviousFsFiles = false;
            //IApplicationGlobals appGlobals = null;
            //string olAncestor = null;
            //string fsAncestorEquivalent = null;

            //// Act
            //SortEmail.Sort(
            //    mailItems,
            //    savePictures,
            //    destinationOlPath,
            //    saveMsg,
            //    saveAttachments,
            //    removePreviousFsFiles,
            //    appGlobals,
            //    olAncestor,
            //    fsAncestorEquivalent);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Disabled_Cleanup_Files_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange

            //// Act
            //SortEmail.Cleanup_Files();

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }
    }
}
