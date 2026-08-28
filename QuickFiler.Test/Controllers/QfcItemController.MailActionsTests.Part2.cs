using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Partial continuation of the mail-actions cluster tests, carrying the issue #490 regressions.
    /// The parent file measures 498 lines against the repository's 500-line ceiling, so these tests
    /// land here instead. The parent imports the Outlook interop namespace and does not import
    /// <c>System</c>, so every framework type below is written fully qualified, exactly as the
    /// parent does; a bare <c>Action</c>, <c>Exception</c> or <c>Delegate</c> would silently bind to
    /// an Outlook interop type.
    /// </summary>
    public partial class QfcItemController_MailActionsTests
    {
        /// <summary>
        /// Produces a <c>FlagTasks</c> without running its Outlook-bound constructor, which reads
        /// <c>globals.Ol.App.ActiveExplorer()</c> and can show a MessageBox. The private controller
        /// field is therefore left null, and <c>Run(modal: true)</c> returns immediately with
        /// <c>DialogResult.None</c> instead of showing a live modal dialog.
        /// </summary>
        private static TaskVisualization.FlagTasks BuildInertFlagTasks() =>
            (TaskVisualization.FlagTasks)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(TaskVisualization.FlagTasks)
                );

        /// <summary>
        /// Builds a controller wired with the injected flag-task factory, mocked globals and a
        /// mocked home controller, so <c>FlagAsTask</c> reaches its dialog-result assignment without
        /// touching Outlook.
        /// </summary>
        private static MailController BuildFlagTaskController(Mock<IItemViewer> viewer)
        {
            var globals = new Mock<IApplicationGlobals>();
            var home = new Mock<IFilerHomeController>();
            var formController = new Mock<IFilerFormController>();
            formController.SetupGet(f => f.FormHandle).Returns(new System.IntPtr(42));
            home.SetupGet(h => h.FormController).Returns(formController.Object);
            System.Func<
                IApplicationGlobals,
                List<MailItem>,
                bool,
                System.IntPtr,
                TaskVisualization.FlagTasks
            > factory = (g, itemList, blFile, hWndCaller) => BuildInertFlagTasks();
            var controller = new MailController();
            SetField(controller, "_globals", globals.Object);
            SetField(controller, "_homeController", home.Object);
            SetField(controller, "_flagTasksFactory", factory);
            SetField(controller, "_itemViewer", viewer.Object);
            controller.Mail = new Mock<MailItem>().Object;
            return controller;
        }

        /// <summary>
        /// Issue #490 D4: <c>FlagAsTask</c> must branch on the local dialog result it already holds,
        /// not read the value back off the viewer it just wrote it to.
        /// </summary>
        [TestMethod]
        public void FlagAsTask_DoesNotReadBackFlagTaskDialogResult()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            MailController controller = BuildFlagTaskController(viewer);

            // Act
            controller.FlagAsTask();

            // Assert
            viewer.VerifyGet(v => v.FlagTaskDialogResult, Times.Never());
        }

        /// <summary>
        /// Issue #490 D4: the asynchronous overload carries the same redundant read-back inside its
        /// dispatcher callback and must branch on the local instead.
        /// </summary>
        [TestMethod]
        public async Task FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            MailController controller = BuildFlagTaskController(viewer);
            SetField(
                controller,
                "_uiDispatcher",
                QfcItemControllerTestSupport.BuildSyncDispatcher().Object
            );

            // Act
            await controller.FlagAsTaskAsync();

            // Assert
            viewer.VerifyGet(v => v.FlagTaskDialogResult, Times.Never());
        }

        /// <summary>
        /// Issue #490 D3: once <c>FocusSubject</c> reports failure, the <c>&amp;Expand</c> menu
        /// action must still enumerate the conversation. The subject label is not selectable, so
        /// the focus attempt always fails; swallowing that failure must not also swallow the
        /// enumeration the gesture exists to perform.
        /// </summary>
        [TestMethod]
        public void Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation()
        {
            // Arrange
            string[] folderItems = new[] { "Archive" };
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.FocusSubject()).Returns(false);
            viewer.Setup(v => v.GetFolderItems()).Returns(folderItems);
            var parent = new Mock<IQfcCollectionController>();
            var mailActions = new Mock<IMailItemActions>();
            mailActions.SetupGet(m => m.EntryID).Returns("entry-xyz");
            ConversationResolver resolver = BuildResolverWithCount(4);
            var controller = new MailController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_parent", parent.Object);
            SetField(controller, "_conversationResolver", resolver);
            SetField(controller, "_mailActions", mailActions.Object);

            // Act
            controller.RightKeyActions["&Expand"]();

            // Assert
            parent.Verify(
                p => p.ToggleUnGroupConv(resolver, "entry-xyz", 4, folderItems),
                Times.Once()
            );
        }
    }
}
