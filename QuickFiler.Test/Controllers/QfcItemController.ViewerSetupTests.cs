using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// ViewerSetup-cluster tests (cycle-2 Phase 5, AC8). Covers the de-exempted
    /// PopulateControls(MailItemHelper,int), AssignControls, AssignControlsAsync, and Cleanup members,
    /// exercised through the narrowed IItemViewer intent members and a mocked settings object. No
    /// live WinForms control is required: the InvokeRequired guard is mocked and the async overload
    /// dispatches through a real (test-thread) WPF Dispatcher pumped deterministically.
    /// </summary>
    [TestClass]
    public class QfcItemController_ViewerSetupTests
    {
        private static Mock<IApplicationGlobals> BuildGlobals(
            bool moveConversation,
            bool saveEmailCopy,
            bool saveAttachments,
            bool savePictures
        )
        {
            Mock<IAppQuickFilerSettings> settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.MoveEntireConversation).Returns(moveConversation);
            settings.SetupGet(s => s.SaveEmailCopy).Returns(saveEmailCopy);
            settings.SetupGet(s => s.SaveAttachments).Returns(saveAttachments);
            settings.SetupGet(s => s.SavePictures).Returns(savePictures);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            return globals;
        }

        private static MailItemHelper BuildHelper(bool isTaskFlagSet, string body)
        {
            MailItemHelper helper = new MailItemHelper();
            helper.Body = body;
            helper.IsTaskFlagSet = isTaskFlagSet;
            return helper;
        }

        [TestMethod]
        public void PopulateControls_WithHelper_StoresHelperAndAssignsViewerFields()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );
            MailItemHelper helper = BuildHelper(false, "populated-body");

            // Act
            controller.PopulateControls(helper, 4);

            // Assert — the helper is stored and its values are pushed onto the viewer intent members.
            controller.ItemHelper.Should().BeSameAs(helper);
            viewer.VerifySet(v => v.BodyText = "populated-body", Times.Once());
            viewer.VerifySet(v => v.ItemNumberText = "4", Times.Once());
        }

        [TestMethod]
        public void AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings()
        {
            // Arrange — distinct settings values so each checkbox intent member is verified.
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(true, true, false, true).Object
            );
            MailItemHelper helper = BuildHelper(isTaskFlagSet: true, body: "b");

            // Act
            controller.AssignControls(helper, 9);

            // Assert
            viewer.VerifySet(v => v.BodyText = "b", Times.Once());
            viewer.VerifySet(v => v.ItemNumberText = "9", Times.Once());
            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.OK, Times.Once());
            viewer.VerifySet(v => v.ConversationModeChecked = true, Times.Once());
            viewer.VerifySet(v => v.EmailCopyChecked = true, Times.Once());
            viewer.VerifySet(v => v.AttachmentsChecked = false, Times.Once());
            viewer.VerifySet(v => v.PicturesChecked = true, Times.Once());
        }

        [TestMethod]
        public void AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );

            // Act
            controller.AssignControls(BuildHelper(isTaskFlagSet: false, body: "b"), 1);

            // Assert
            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.Cancel, Times.Once());
        }

        [TestMethod]
        public void AssignControls_WhenInvokeRequired_MarshalsViaInvoke()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );

            // Act
            controller.AssignControls(BuildHelper(false, "b"), 1);

            // Assert — the write is marshaled through Invoke rather than applied directly.
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            viewer.VerifySet(v => v.BodyText = It.IsAny<string>(), Times.Never());
        }

        [TestMethod]
        public void AssignControlsAsync_DispatchesAssignThroughViewerDispatcher()
        {
            // Arrange — supply a dedicated running WPF Dispatcher (on its own thread) for the async
            // overload; the inner AssignControls sees InvokeRequired == false and writes directly. A
            // dedicated dispatcher is used instead of the shared test-thread dispatcher so this test
            // only executes its own dispatched operation and is immune to fire-and-forget operations
            // posted to the thread dispatcher by unrelated tests.
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                Mock<IItemViewer> viewer = new Mock<IItemViewer>();
                viewer.SetupGet(v => v.InvokeRequired).Returns(false);
                viewer.SetupGet(v => v.UiDispatcher).Returns(dispatcher);
                HarnessController controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_globals",
                    BuildGlobals(false, false, false, false).Object
                );

                // Act — block deterministically on the dispatched task's completion (no polling).
                controller
                    .AssignControlsAsync(BuildHelper(false, "async-body"), 2)
                    .GetAwaiter()
                    .GetResult();

                // Assert
                viewer.VerifySet(v => v.BodyText = "async-body", Times.Once());
                viewer.VerifySet(v => v.ItemNumberText = "2", Times.Once());
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }

        [TestMethod]
        public void Cleanup_NullsTrackedPrivateFields()
        {
            // Arrange — populate the fields Cleanup is responsible for releasing.
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                new Mock<IApplicationGlobals>().Object
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_itemViewer",
                new Mock<IItemViewer>().Object
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_homeController",
                new Mock<QuickFiler.Interfaces.IFilerHomeController>().Object
            );
            controller.ItemHelper = new MailItemHelper();

            // Act
            controller.Cleanup();

            // Assert — the released references are null after cleanup.
            QfcItemControllerTestSupport.GetField(controller, "_globals").Should().BeNull();
            QfcItemControllerTestSupport.GetField(controller, "_itemViewer").Should().BeNull();
            QfcItemControllerTestSupport.GetField(controller, "_homeController").Should().BeNull();
            controller.ItemHelper.Should().BeNull();
        }
    }
}
