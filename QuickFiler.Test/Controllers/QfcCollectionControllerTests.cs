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
        //private Mock<QfcFormViewer> mockQfcFormViewer;
        private QfcFormViewer mockQfcFormViewer;
        private Mock<IQfcFormController> mockQfcFormController;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);

            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            //this.mockQfcFormViewer = this.mockRepository.Create<QfcFormViewer>();
            //this.mockQfcFormViewer.SetupAllProperties();
            this.mockQfcFormViewer = new QfcFormViewer();
            this.mockQfcFormController = this.mockRepository.Create<IQfcFormController>();
        }

        private QfcCollectionController CreateQfcCollectionController()
        {
            return new QfcCollectionController(
                this.mockApplicationGlobals.Object,
                this.mockQfcFormViewer, //.Object,
                false,
                Enums.InitTypeEnum.InitSort,
                this.mockQfcFormController.Object);
        }

        [TestMethod]
        public void LoadControlsAndHandlers_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            IList<MailItem> listMailItems = null;
            RowStyle template = null;

            // Act
            qfcCollectionController.LoadControlsAndHandlers(
                listMailItems,
                template);

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
        public void AddEmailControlGroup_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            MailItem mailItem = null;
            int posInsert = 0;
            bool blGroupConversation = false;
            int ConvCt = 0;
            object varList = null;
            bool blChild = false;

            // Act
            qfcCollectionController.AddEmailControlGroup(
                mailItem,
                posInsert,
                blGroupConversation,
                ConvCt,
                varList,
                blChild);

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
            var result = qfcCollectionController.ActivateByIndex(
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

        [TestMethod]
        public void MoveDownControlGroups_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intPosition = 0;
            int intMoves = 0;

            // Act
            qfcCollectionController.MoveDownControlGroups(
                intPosition,
                intMoves);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void MoveDownPix_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intPosition = 0;
            int intPix = 0;

            // Act
            qfcCollectionController.MoveDownPix(
                intPosition,
                intPix);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ResizeChildren_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intDiffx = 0;

            // Act
            qfcCollectionController.ResizeChildren(
                intDiffx);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ConvToggle_Group_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            IList<MailItem> selItems = null;
            int indexOriginal = 0;

            // Act
            qfcCollectionController.ConvToggle_Group(
                selItems,
                indexOriginal);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ConvToggle_UnGroup_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            IList<MailItem> mailItems = null;
            int baseEmailIndex = 0;
            int conversationCount = 0;
            object folderList = null;

            // Act
            qfcCollectionController.ConvToggle_UnGroup(
                mailItems,
                baseEmailIndex,
                conversationCount,
                folderList);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void MakeSpaceToEnumerateConversation_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.MakeSpaceToEnumerateConversation();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void IsSelectionBelowMax_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();
            int intNewSelection = 0;

            // Act
            var result = qfcCollectionController.IsSelectionBelowMax(
                intNewSelection);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SetDarkMode_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var qfcCollectionController = this.CreateQfcCollectionController();

            // Act
            qfcCollectionController.SetDarkMode();

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
            qfcCollectionController.SetLightMode();

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
