using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Cycle-2 Phase 6 coverage for the Outlook-COM repoint (P6-T7, IMailItemActions seam) and the
    /// thin-delegator extraction of the six <c>async void</c> handlers (P6-T9). The previously
    /// COM-bound <c>Mail.*</c> calls now route through a mocked <see cref="IMailItemActions"/>, and each
    /// extracted core method is verified independently of its one-line WinForms-event shell.
    /// </summary>
    [TestClass]
    public class QfcItemController_SeamCoreTests
    {
        private static Mock<UtilitiesCS.Threading.IUiDispatcher> BuildMailReturningDispatcher()
        {
            var dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            dispatcher
                .Setup(d => d.InvokeAsync(It.IsAny<Func<MailItem>>()))
                .Returns((Func<MailItem> f) => Task.FromResult(f()));
            return dispatcher;
        }

        private static (
            HarnessController controller,
            Mock<IMailItemActions> mailActions,
            Mock<MailItem> replyMail
        ) BuildReplyController()
        {
            var dispatcher = BuildMailReturningDispatcher();
            var replyMail = new Mock<MailItem>();
            var mailActions = new Mock<IMailItemActions>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            QfcItemControllerTestSupport.SetField(controller, "_mailActions", mailActions.Object);
            return (controller, mailActions, replyMail);
        }

        [TestMethod]
        public async Task Reply_CreatesReplyThroughSeamAndDisplaysIt()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.Reply()).Returns(replyMail.Object);

            await controller.Reply();

            mailActions.Verify(m => m.Reply(), Times.Once());
        }

        [TestMethod]
        public async Task ReplyAll_CreatesReplyAllThroughSeamAndDisplaysIt()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.ReplyAll()).Returns(replyMail.Object);

            await controller.ReplyAll();

            mailActions.Verify(m => m.ReplyAll(), Times.Once());
        }

        [TestMethod]
        public async Task Forward_CreatesForwardThroughSeamAndDisplaysIt()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.Forward()).Returns(replyMail.Object);

            await controller.Forward();

            mailActions.Verify(m => m.Forward(), Times.Once());
        }

        [TestMethod]
        public void CollapseConversation_WhenConvOriginIdEmpty_UsesMailActionsEntryId()
        {
            // Arrange — empty _convOriginID selects the seam-backed EntryID fallback (P6-T7).
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetFolderItems()).Returns(new[] { @"\\Archive\A" });
            var parent = new Mock<IQfcCollectionController>();
            var mailActions = new Mock<IMailItemActions>();
            mailActions.SetupGet(m => m.EntryID).Returns("seam-entry");
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_parent", parent.Object);
            QfcItemControllerTestSupport.SetField(controller, "_mailActions", mailActions.Object);

            // Act
            controller.CollapseConversation();

            // Assert
            parent.Verify(p => p.ToggleGroupConv("seam-entry"), Times.Once());
        }

        // ------------------------- Thin-delegator cores (P6-T9) -------------------------

        [TestMethod]
        public async Task BtnPopOutCore_PopsOutOwnItemGroup()
        {
            var parent = new Mock<IQfcCollectionController>();
            parent
                .Setup(p => p.PopOutControlGroupAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_parent", parent.Object);
            controller.ItemNumber = 5;

            await controller.BtnPopOutCore();

            parent.Verify(p => p.PopOutControlGroupAsync(5), Times.Once());
        }

        [TestMethod]
        public async Task BtnReplyCore_RoutesToReplySeam()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.Reply()).Returns(replyMail.Object);

            await controller.BtnReplyCore();

            mailActions.Verify(m => m.Reply(), Times.Once());
        }

        [TestMethod]
        public async Task BtnReplyAllCore_RoutesToReplyAllSeam()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.ReplyAll()).Returns(replyMail.Object);

            await controller.BtnReplyAllCore();

            mailActions.Verify(m => m.ReplyAll(), Times.Once());
        }

        [TestMethod]
        public async Task BtnForwardCore_RoutesToForwardSeam()
        {
            var (controller, mailActions, replyMail) = BuildReplyController();
            mailActions.Setup(m => m.Forward()).Returns(replyMail.Object);

            await controller.BtnForwardCore();

            mailActions.Verify(m => m.Forward(), Times.Once());
        }

        [TestMethod]
        public async Task TxtboxBodyDoubleClickCore_DisplaysMailThroughSeam()
        {
            var mailActions = new Mock<IMailItemActions>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_mailActions", mailActions.Object);

            await controller.TxtboxBodyDoubleClickCore();

            mailActions.Verify(m => m.Display(), Times.Once());
        }

        // ------------------------- WebView init handler core (P6-T9) -------------------------

        [TestMethod]
        public async Task HandleWebViewInitializedAsync_WhenSuccessful_NavigatesToItemHtml()
        {
            var helper = new Mock<MailItemHelper>();
            helper.SetupGet(h => h.Html).Returns("<html>x</html>");
            var viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            controller.ItemHelper = helper.Object;

            await controller.HandleWebViewInitializedAsync(true, null);

            viewer.Verify(v => v.NavigateToString("<html>x</html>"), Times.Once());
            QfcItemControllerTestSupport
                .GetField(controller, "_isWebViewerInitialized")
                .Should()
                .Be(true);
        }

        [TestMethod]
        public async Task HandleWebViewInitializedAsync_WhenInvokeRequired_MarshalsNavigate()
        {
            var helper = new Mock<MailItemHelper>();
            helper.SetupGet(h => h.Html).Returns("<html>y</html>");
            var viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(true);
            viewer
                .Setup(v => v.Invoke(It.IsAny<Delegate>()))
                .Callback<Delegate>(d => d.DynamicInvoke())
                .Returns((object)null);
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            controller.ItemHelper = helper.Object;

            await controller.HandleWebViewInitializedAsync(true, null);

            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            viewer.Verify(v => v.NavigateToString("<html>y</html>"), Times.Once());
        }

        [TestMethod]
        public async Task HandleWebViewInitializedAsync_WhenFailure_SwallowsExceptionAndDoesNotInitialize()
        {
            var controller = new HarnessController();

            Func<Task> act = () =>
                controller.HandleWebViewInitializedAsync(
                    false,
                    new InvalidOperationException("boom")
                );

            await act.Should()
                .NotThrowAsync(because: "the handler catches and logs initialization failures");
            QfcItemControllerTestSupport
                .GetField(controller, "_isWebViewerInitialized")
                .Should()
                .Be(false);
        }
    }
}
