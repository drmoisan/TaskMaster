using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Test.TestSupport;
using TaskVisualization;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

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
            EmailFiler producedFiler = null;
            Func<EmailFilerConfig, EmailFiler> factory = c =>
            {
                captured = c;
                producedFiler = new EmailFiler(c);
                return producedFiler;
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

            // A real FilerQueue whose ItemProcessor seam is assigned to a gated delegate, so the worker
            // hands the item to the test and parks there (deterministic, no external I/O).
            var filerQueue = new FilerQueue();
            var receivedItems = new List<FilerQueueItem>();
            var receivedItemsLock = new object();
            var receivedFirst = new TaskCompletionSource<FilerQueueItem>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var gate = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            filerQueue.ItemProcessor = item =>
            {
                lock (receivedItemsLock)
                {
                    receivedItems.Add(item);
                }
                receivedFirst.TrySetResult(item);
                return gate.Task;
            };
            var home = new Mock<IFilerHomeController>();
            home.SetupGet(h => h.FilerQueue).Returns(filerQueue);

            var controller = new HarnessController();
            controller.ItemHelper = new MailItemHelper();
            QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);
            QfcItemControllerTestSupport.SetField(controller, "_homeController", home.Object);
            QfcItemControllerTestSupport.SetField(controller, "_emailFilerFactory", factory);

            try
            {
                await controller.MoveMailAsync();

                // Completed by the queue worker itself, so awaiting it needs no timing assumption.
                FilerQueueItem received = await receivedFirst.Task;

                captured.Should().NotBeNull();
                captured.Globals.Should().BeSameAs(globals.Object);
                captured.OlAncestor.Should().Be("archive-root");
                captured.FsAncestorEquivalent.Should().Be(oneDrive);

                lock (receivedItemsLock)
                {
                    receivedItems
                        .Should()
                        .ContainSingle("MoveMailAsync enqueues exactly one item for this mail");
                }

                received
                    .Filer.Should()
                    .BeSameAs(
                        producedFiler,
                        "the queued item must carry the EmailFiler the factory produced"
                    );
            }
            finally
            {
                gate.TrySetResult(true);
            }
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

        // ------------------------- #230 static-factory de-exemption -------------------------

        /// <summary>
        /// Harness bound for the #230 pump-hosted factory tests (MSTest <c>[Timeout]</c> precedent
        /// <c>TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs</c>). Every wait is on a
        /// deterministic completion signal; the attribute only converts a genuine deadlock into a
        /// test failure instead of a CI hang.
        /// </summary>
        private const int PumpTimeoutMs = 60000;

        /// <summary>
        /// #230 (de-exempted): <c>CreateSequentialAsync</c> constructs the controller, applies the
        /// injected seams, saves its parameters, and awaits <c>InitializeSequentialAsync</c>, whose
        /// web-view tail is fire-and-forget — so the factory runs to normal completion and returns
        /// the initialized controller (D13). The new optional seam parameters supply a mocked
        /// <c>IWebViewCoreInitializer</c>, an inline-executing <c>IUiDispatcher</c>, and a mocked
        /// conversation-resolver factory, so the real WebView2 runtime is never reached.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            QfcItemController_InitializationTests.PumpHarness harness = null;
            try
            {
                harness = await QfcItemController_InitializationTests
                    .BuildPumpHarnessAsync(host, darkMode: false)
                    .ConfigureAwait(false);
                QfcItemController_InitializationTests.FactoryArguments arguments =
                    QfcItemController_InitializationTests.BuildFactoryArguments(harness);

                // Act — awaited to normal completion from the MSTest thread.
                QfcItemController created = await QfcItemController
                    .CreateSequentialAsync(
                        arguments.Globals,
                        arguments.HomeController,
                        arguments.Parent,
                        harness.Viewer,
                        viewerPosition: 4,
                        itemNumberDigits: 2,
                        mailItem: arguments.MailItem,
                        tlpStates: null,
                        token: arguments.Token,
                        uiDispatcher: arguments.UiDispatcher,
                        webViewInitializer: harness.WebViewInitializer.Object,
                        conversationResolverFactory: arguments.ConversationResolverFactory
                    )
                    .ConfigureAwait(false);

                // Assert — the factory returned a controller whose initialization actually ran.
                created.Should().NotBeNull();
                created.Parent.Should().BeSameAs(arguments.Parent);
                created.ItemNumber.Should().Be(4);
                created.TableLayoutPanels.Should().NotBeNullOrEmpty();
                created.Buttons.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(created, "_themes")
                    .Should()
                    .NotBeNull(because: "InitializeSequentialAsync ran through the pump");
                QfcItemControllerTestSupport
                    .GetField(created, "_webViewInitializer")
                    .Should()
                    .BeSameAs(
                        harness.WebViewInitializer.Object,
                        because: "the injected seam must survive SaveParameters' ??= defaults"
                    );
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #230 (de-exempted): <c>CreateAsync</c> awaits <c>InitializeAsync</c>, whose final
        /// statement is <c>await InitializeWebViewAsync()</c>. Under the mocked web-view seam that
        /// await always faults, so <c>CreateAsync</c> can never reach its <c>return controller;</c>
        /// statement in a unit test (D13) — its per-member coverage is partial by construction and
        /// the D5 gate (c) bar for it is "&gt; 0%", not "no uncovered lines". The test asserts the
        /// injected exception's identity on the awaited factory task, and asserts the observable
        /// state the preceding lines set on the injected viewer and controller.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            QfcItemController_InitializationTests.PumpHarness harness = null;
            try
            {
                harness = await QfcItemController_InitializationTests
                    .BuildPumpHarnessAsync(host, darkMode: false)
                    .ConfigureAwait(false);
                QfcItemController_InitializationTests.FactoryArguments arguments =
                    QfcItemController_InitializationTests.BuildFactoryArguments(harness);

                // Act
                Func<Task<QfcItemController>> act = () =>
                    QfcItemController.CreateAsync(
                        arguments.Globals,
                        arguments.HomeController,
                        arguments.Parent,
                        harness.Viewer,
                        viewerPosition: 6,
                        itemNumberDigits: 2,
                        mailItem: arguments.MailItem,
                        tlpStates: null,
                        token: arguments.Token,
                        uiDispatcher: arguments.UiDispatcher,
                        webViewInitializer: harness.WebViewInitializer.Object,
                        conversationResolverFactory: arguments.ConversationResolverFactory
                    );

                // Assert — the controlled fault from the mocked seam, not a timeout or a hang.
                await act.Should()
                    .ThrowAsync<QfcItemController_InitializationTests.WebViewSentinelException>(
                        because: "execution must stop at the mocked web-view seam"
                    )
                    .ConfigureAwait(false);

                // Assert — the lines preceding the faulting tail did run against the real viewer.
                // SaveParameters sets the viewer's back-reference to the factory-built controller,
                // which is the only handle a test has on it once the factory faults.
                harness.Viewer.Controller.Should().NotBeNull();
                QfcItemController factoryBuilt = (QfcItemController)harness.Viewer.Controller;
                factoryBuilt.ItemNumber.Should().Be(6);
                factoryBuilt.TableLayoutPanels.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(factoryBuilt, "_themes")
                    .Should()
                    .NotBeNull(because: "SetupThemes precedes the faulting await");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
