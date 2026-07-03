using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using TaskVisualization;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Cycle-2 Phase 6 coverage for the factory-delegate seams (P6-T8: ConversationResolver, FlagTasks,
    /// EmailFiler) and the WireEvents intent-subscription split (P6-T10, WireIntentEvents). The
    /// factories are injected so no live ConversationResolver/FlagTasks/EmailFiler is constructed and no
    /// modal dialog is launched; each member's routing to the injected factory is verified directly.
    /// </summary>
    [TestClass]
    public class QfcItemController_SeamFactoryTests
    {
        private sealed class SentinelException : System.Exception { }

        // ------------------------- ConversationResolver factory (P6-T8) -------------------------

        [TestMethod]
        public void PopulateConversation_UsesResolverFactoryAndRendersCount()
        {
            var dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            var viewer = new Mock<IItemViewer>();
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockMail = new Mock<MailItem>();
            MailItem capturedMail = null;
            var resolver = new ConversationResolver(mockGlobals.Object, mockMail.Object)
            {
                Count = new Pair<int>(3, 3),
            };
            Func<MailItem, ConversationResolver> factory = m =>
            {
                capturedMail = m;
                return resolver;
            };
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_conversationResolverFactory",
                factory
            );
            controller.Mail = mockMail.Object;

            controller.PopulateConversation();

            capturedMail.Should().BeSameAs(mockMail.Object);
            controller.ConversationResolver.Should().BeSameAs(resolver);
            viewer.VerifySet(v => v.ConversationCountText = "3", Times.Once());
        }

        // ------------------------- FlagTasks factory (P6-T8) -------------------------

        private static (
            HarnessController controller,
            Mock<IApplicationGlobals> globals,
            Mock<MailItem> mail
        ) BuildFlagController(
            Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks> factory
        )
        {
            var globals = new Mock<IApplicationGlobals>();
            var home = new Mock<IFilerHomeController>();
            var formCtrl = new Mock<IFilerFormController>();
            formCtrl.SetupGet(f => f.FormHandle).Returns(new IntPtr(42));
            home.SetupGet(h => h.FormController).Returns(formCtrl.Object);
            var mail = new Mock<MailItem>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);
            QfcItemControllerTestSupport.SetField(controller, "_homeController", home.Object);
            QfcItemControllerTestSupport.SetField(controller, "_flagTasksFactory", factory);
            controller.Mail = mail.Object;
            return (controller, globals, mail);
        }

        [TestMethod]
        public void FlagAsTask_InvokesFactoryWithExpectedArguments()
        {
            IApplicationGlobals capturedGlobals = null;
            List<MailItem> capturedList = null;
            bool capturedBlFile = true;
            IntPtr capturedHwnd = default;
            Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks> factory = (
                g,
                list,
                bl,
                h
            ) =>
            {
                capturedGlobals = g;
                capturedList = list;
                capturedBlFile = bl;
                capturedHwnd = h;
                throw new SentinelException();
            };
            var (controller, globals, mail) = BuildFlagController(factory);

            System.Action act = () => controller.FlagAsTask();

            act.Should().Throw<SentinelException>();
            capturedGlobals.Should().BeSameAs(globals.Object);
            capturedList.Should().ContainSingle().Which.Should().BeSameAs(mail.Object);
            capturedBlFile.Should().BeFalse();
            capturedHwnd.Should().Be(new IntPtr(42));
        }

        [TestMethod]
        public async Task FlagAsTaskAsync_InvokesFactoryThroughDispatcher()
        {
            bool factoryCalled = false;
            Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks> factory = (
                g,
                list,
                bl,
                h
            ) =>
            {
                factoryCalled = true;
                throw new SentinelException();
            };
            var (controller, _, _) = BuildFlagController(factory);
            var dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);

            Func<Task> act = () => controller.FlagAsTaskAsync();

            await act.Should().ThrowAsync<SentinelException>();
            factoryCalled.Should().BeTrue();
        }

        // ------------------------- EmailFiler factory / MoveMailAsync (P6-T8) -------------------------

        [TestMethod]
        public async Task MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory()
        {
            bool factoryCalled = false;
            Func<EmailFilerConfig, EmailFiler> factory = c =>
            {
                factoryCalled = true;
                return new EmailFiler(c);
            };
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_emailFilerFactory", factory);

            await controller.MoveMailAsync();

            factoryCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory()
        {
            bool factoryCalled = false;
            Func<EmailFilerConfig, EmailFiler> factory = c =>
            {
                factoryCalled = true;
                return new EmailFiler(c);
            };
            var globals = new Mock<IApplicationGlobals>();
            var fs = new Mock<IFileSystemFolderPaths>();
            fs.SetupGet(f => f.SpecialFolders).Returns(new ConcurrentDictionary<string, string>());
            globals.SetupGet(g => g.FS).Returns(fs.Object);
            var controller = new HarnessController();
            controller.ItemHelper = new MailItemHelper();
            QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);
            QfcItemControllerTestSupport.SetField(controller, "_emailFilerFactory", factory);

            await controller.MoveMailAsync();

            factoryCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues()
        {
            EmailFilerConfig captured = null;
            Func<EmailFilerConfig, EmailFiler> factory = c =>
            {
                captured = c;
                return new EmailFiler(c);
            };
            var oneDrive = @"C:\OneDrive";
            var globals = new Mock<IApplicationGlobals>();
            var fs = new Mock<IFileSystemFolderPaths>();
            var special = new ConcurrentDictionary<string, string>();
            special["OneDrive"] = oneDrive;
            fs.SetupGet(f => f.SpecialFolders).Returns(special);
            globals.SetupGet(g => g.FS).Returns(fs.Object);
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(o => o.ArchiveRootPath).Returns("archive-root");
            globals.SetupGet(g => g.Ol).Returns(ol.Object);

            // A real FilerQueue whose single-shot guard is pre-tripped so Enqueue records the item
            // without spinning up the background consumer (deterministic, no external I/O).
            var filerQueue = new FilerQueue();
            var guard = typeof(FilerQueue)
                .GetField("guard", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(filerQueue);
            typeof(ThreadSafeSingleShotGuard)
                .GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(guard, 1);
            var home = new Mock<IFilerHomeController>();
            home.SetupGet(h => h.FilerQueue).Returns(filerQueue);

            var controller = new HarnessController();
            controller.ItemHelper = new MailItemHelper();
            QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);
            QfcItemControllerTestSupport.SetField(controller, "_homeController", home.Object);
            QfcItemControllerTestSupport.SetField(controller, "_emailFilerFactory", factory);

            await controller.MoveMailAsync();

            captured.Should().NotBeNull();
            captured.Globals.Should().BeSameAs(globals.Object);
            captured.OlAncestor.Should().Be("archive-root");
            captured.FsAncestorEquivalent.Should().Be(oneDrive);
            filerQueue.Queue.Count.Should().Be(1);
        }

        // ------------------------- WireIntentEvents split (P6-T10) -------------------------

        [TestMethod]
        public void WireIntentEvents_SubscribesEveryIntentEvent()
        {
            var viewer = new Mock<IItemViewer>();
            var kbd = new Mock<IQfcKeyboardHandler>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);

            controller.WireIntentEvents();

            viewer.VerifyAdd(
                v => v.ConversationModeChanged += It.IsAny<EventHandler>(),
                Times.Once()
            );
            viewer.VerifyAdd(v => v.FlagTaskClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.PopOutClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.DeleteItemClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.ReplyClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.ReplyAllClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.ForwardClicked += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.BodyDoubleClick += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.SearchTextChanged += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.FolderKeyDown += It.IsAny<KeyEventHandler>(), Times.Once());
            viewer.VerifyAdd(
                v => v.FolderSelectionChanged += It.IsAny<EventHandler>(),
                Times.Once()
            );
            viewer.VerifyAdd(
                v =>
                    v.WebViewInitializationCompleted += It.IsAny<
                        EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>
                    >(),
                Times.Once()
            );
            viewer.VerifyAdd(
                v =>
                    v.ConversationItemSelectionChanged +=
                        It.IsAny<ListViewItemSelectionChangedEventHandler>(),
                Times.Once()
            );
            viewer.VerifyAdd(v => v.SearchKeyDown += It.IsAny<KeyEventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.EmailCopyChanged += It.IsAny<EventHandler>(), Times.Once());
            viewer.VerifyAdd(v => v.AttachmentsChanged += It.IsAny<EventHandler>(), Times.Once());
        }
    }
}
