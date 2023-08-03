using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler.Test.Controllers
{
    [TestClass]
    public class QfcCollectionControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IApplicationGlobals> mockApplicationGlobals;
        private QfcFormViewer formViewer;
        private Mock<IQfcFormController> mockQfcFormController;
        private Mock<IQfcKeyboardHandler> mockKeyboardHandler;
        private Mock<IQfcHomeController> mockHomeController;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);

            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            //this.mockQfcFormViewer = this.mockRepository.Create<QfcFormViewer>();
            //this.mockQfcFormViewer.SetupAllProperties();
            this.formViewer = new QfcFormViewer();
            this.mockQfcFormController = this.mockRepository.Create<IQfcFormController>();
            this.mockKeyboardHandler = this.mockRepository.Create<IQfcKeyboardHandler>();
            this.mockHomeController = this.mockRepository.Create<IQfcHomeController>();
        }

        private QfcCollectionController CreateQfcCollectionController()
        {
            return new QfcCollectionController(
                this.mockApplicationGlobals.Object,
                this.formViewer, //.Object,
                false,
                Enums.InitTypeEnum.InitSort,
                this.mockHomeController.Object,
                this.mockQfcFormController.Object);
        }

        [TestMethod]
        public void LoadControlsAndHandlers_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            IList<MailItem> listMailItems = null;
            RowStyle template = null;
            RowStyle templateExpanded = null;

            // Act
            qfcCollectionController.LoadControlsAndHandlers(
                listMailItems,
                template,
                templateExpanded);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void LoadItemViewer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int itemNumber = 0;
            RowStyle template = null;
            bool blGroupConversation = false;
            int columnNumber = 0;

            // Act
            var result = qfcCollectionController.LoadItemViewer(
                itemNumber,
                template,
                blGroupConversation,
                columnNumber);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        
        [TestMethod]
        public void RemoveControls_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.RemoveControls();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveSpaceToCollapseConversation_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.RemoveSpaceToCollapseConversation();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveSpecificControlGroup_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intPosition = 0;

            // Act
            qfcCollectionController.RemoveSpecificControlGroup(
                intPosition);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ActivateByIndex_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intNewSelection = 0;
            bool blExpanded = false;

            // Act
            var result = qfcCollectionController.ActivateBySelection(
                intNewSelection,
                blExpanded);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ToggleOffActiveItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            bool parentBlExpanded = false;

            // Act
            var result = qfcCollectionController.ToggleOffActiveItem(
                parentBlExpanded);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SelectNextItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.SelectNextItem();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SelectPreviousItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.SelectPreviousItem();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        

        

        

        //[TestMethod]
        //public void ConvToggle_Group_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var qfcCollectionController = this.CreateQfcCollectionController();
        //    IList<MailItem> selItems = null;
        //    int indexOriginal = 0;

        //    // Act
        //    qfcCollectionController.ConvToggle_Group(
        //        selItems,
        //        indexOriginal);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ConvToggle_UnGroup_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var qfcCollectionController = this.CreateQfcCollectionController();
        //    IList<MailItem> mailItems = null;
        //    int baseEmailIndex = 0;
        //    int conversationCount = 0;
        //    object folderList = null;

        //    // Act
        //    qfcCollectionController.ConvToggle_UnGroup(
        //        mailItems,
        //        baseEmailIndex,
        //        conversationCount,
        //        folderList);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void MakeSpaceToEnumerateConversation_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var qfcCollectionController = this.CreateQfcCollectionController();

        //    // Act
        //    qfcCollectionController.MakeSpaceToEnumerateConversation();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

       

        [TestMethod]
        public void SetDarkMode_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.SetDarkMode(async: false);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SetLightMode_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.SetLightMode(async: false);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Cleanup_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.Cleanup();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void MoveEmails_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            StackObjectCS<MailItem> stackMovedItems = null;

            // Act
            qfcCollectionController.MoveEmails(
                stackMovedItems);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetMoveDiagnostics_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            string durationText = null;
            string durationMinutesText = null;
            double Duration = 0;
            string dataLineBeg = null;
            DateTime OlEndTime = default(global::System.DateTime);
            AppointmentItem OlAppointment = null;

            // Act
            var result = qfcCollectionController.GetMoveDiagnostics(
                durationText,
                durationMinutesText,
                Duration,
                dataLineBeg,
                OlEndTime,
                ref OlAppointment);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
