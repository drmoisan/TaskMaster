using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Initialization-cluster tests (cycle-2 Phase 5, AC8). Covers the de-exempted constructors and
    /// <see cref="QfcItemController.SaveParameters"/> field-delegation: the public constructors route
    /// their arguments to SaveParameters, which assigns the private fields, resolves collaborators
    /// from the home controller, and sets the viewer's controller back-reference
    /// (<c>_itemViewer.Controller = this</c>). No live WinForms/Outlook object is required because
    /// SaveParameters only stores references (it does not dereference the mail item or tlp states).
    /// </summary>
    [TestClass]
    public partial class QfcItemController_InitializationTests
    {
        /// <summary>
        /// Harness bound for the #230 pump-hosted tests (MSTest <c>[Timeout]</c> precedent
        /// <c>TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs</c>). Every wait in those tests is
        /// on a deterministic completion signal; the attribute only converts a genuine deadlock in
        /// production code into a test failure instead of a CI hang.
        /// </summary>
        internal const int PumpTimeoutMs = 60000;

        private static Mock<IFilerHomeController> BuildHomeController(
            out Mock<IQfcKeyboardHandler> kbd,
            out Mock<IQfcExplorerController> explorer,
            out CancellationTokenSource cts
        )
        {
            kbd = new Mock<IQfcKeyboardHandler>();
            explorer = new Mock<IQfcExplorerController>();
            cts = new CancellationTokenSource();
            Mock<IFilerHomeController> home = new Mock<IFilerHomeController>();
            home.SetupGet(h => h.KeyboardHandler).Returns(kbd.Object);
            home.SetupGet(h => h.ExplorerController).Returns(explorer.Object);
            home.SetupGet(h => h.TokenSource).Returns(cts);
            home.SetupGet(h => h.Token).Returns(cts.Token);
            return home;
        }

        /// <summary>
        /// #670 shared arrange helper: wires a harness controller to the faulting
        /// <c>IWebViewCoreInitializer</c> mock and to an <c>IItemViewer</c> whose
        /// <c>UiSyncContext</c> returns <paramref name="context"/>. The caller must install that
        /// same context as <c>SynchronizationContext.Current</c> before awaiting the guard, so the
        /// await at <c>ViewerSetup.cs:64</c> continues inline and execution reaches the mocked seam.
        /// </summary>
        private static HarnessController BuildGuardedWebViewTarget(SynchronizationContext context)
        {
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_webViewInitializer",
                BuildWebViewInitializerMock().Object
            );
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.UiSyncContext).Returns(context);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            return controller;
        }

        /// <summary>
        /// #670: cooperative cancellation during teardown is not a fault. <c>InitializeWebViewAsync</c>
        /// opens with <c>Token.ThrowIfCancellationRequested()</c> before any seam call, so a
        /// pre-cancelled token reaches the guard's <c>OperationCanceledException</c> arm
        /// deterministically, and the sink must not be invoked.
        /// </summary>
        [TestMethod]
        public async Task InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink()
        {
            // Arrange
            HarnessController controller = BuildGuardedWebViewTarget(new SynchronizationContext());
            bool sinkInvoked = false;
            controller.WebViewInitializationErrorSink = (message, exception) => sinkInvoked = true;
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                source.Cancel();
                controller.Token = source.Token;

                // Act
                Func<Task> act = () => controller.InitializeWebViewGuardedAsync();

                // Assert
                await act.Should()
                    .NotThrowAsync(because: "cancellation is swallowed, not surfaced")
                    .ConfigureAwait(false);
                sinkInvoked
                    .Should()
                    .BeFalse(because: "cooperative cancellation is not a fault to report");
            }
        }

        [TestMethod]
        public void PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference()
        {
            // Arrange
            Mock<IQfcKeyboardHandler> kbd;
            Mock<IQfcExplorerController> explorer;
            CancellationTokenSource cts;
            Mock<IFilerHomeController> home = BuildHomeController(out kbd, out explorer, out cts);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            Mock<IQfcCollectionController> parent = new Mock<IQfcCollectionController>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();

            // Act
            QfcItemController controller = new QfcItemController(
                globals.Object,
                home.Object,
                parent.Object,
                viewer.Object,
                viewerPosition: 7,
                itemNumberDigits: 2,
                mailItem: null,
                tlpStates: null
            );

            // Assert — back-reference set on the viewer, and public getters reflect the saved params.
            viewer.VerifySet(v => v.Controller = controller, Times.Once());
            controller.Parent.Should().BeSameAs(parent.Object);
            controller.ItemNumber.Should().Be(7);
            controller.ItemNumberDigits.Should().Be(2);

            cts.Dispose();
        }

        [TestMethod]
        public void PredeterminedFolderConstructor_StoresPredeterminedFolder()
        {
            // Arrange
            Mock<IQfcKeyboardHandler> kbd;
            Mock<IQfcExplorerController> explorer;
            CancellationTokenSource cts;
            Mock<IFilerHomeController> home = BuildHomeController(out kbd, out explorer, out cts);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            Mock<IQfcCollectionController> parent = new Mock<IQfcCollectionController>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();

            // Act
            QfcItemController controller = new QfcItemController(
                globals.Object,
                home.Object,
                parent.Object,
                viewer.Object,
                viewerPosition: 1,
                itemNumberDigits: 1,
                mailItem: null,
                tlpStates: null,
                predeterminedFolder: @"\\Archive\Predetermined"
            );

            // Assert — the high-confidence folder path is stored in the readonly private field.
            object stored = typeof(QfcItemController)
                .GetField("_predeterminedFolder", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(controller);
            stored.Should().Be(@"\\Archive\Predetermined");
            viewer.VerifySet(v => v.Controller = controller, Times.Once());

            cts.Dispose();
        }

        [TestMethod]
        public void AsyncFlagConstructor_AssignsFieldsViaSaveParameters()
        {
            // Arrange
            Mock<IQfcKeyboardHandler> kbd;
            Mock<IQfcExplorerController> explorer;
            CancellationTokenSource cts;
            Mock<IFilerHomeController> home = BuildHomeController(out kbd, out explorer, out cts);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            Mock<IQfcCollectionController> parent = new Mock<IQfcCollectionController>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();

            // Act
            QfcItemController controller = new QfcItemController(
                globals.Object,
                home.Object,
                parent.Object,
                viewer.Object,
                viewerPosition: 3,
                itemNumberDigits: 1,
                mailItem: null,
                tlpStates: null,
                async: true
            );

            // Assert
            controller.ItemNumber.Should().Be(3);
            viewer.VerifySet(v => v.Controller = controller, Times.Once());

            cts.Dispose();
        }

        [TestMethod]
        public void SaveParameters_AssignsAllFieldsAndResolvesCollaborators()
        {
            // Arrange — call SaveParameters directly on a harness instance built from the protected
            // parameterless constructor, then verify each field/collaborator assignment.
            Mock<IQfcKeyboardHandler> kbd;
            Mock<IQfcExplorerController> explorer;
            CancellationTokenSource cts;
            Mock<IFilerHomeController> home = BuildHomeController(out kbd, out explorer, out cts);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            Mock<IQfcCollectionController> parent = new Mock<IQfcCollectionController>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            HarnessController controller = new HarnessController();

            // Act
            controller.SaveParameters(
                globals.Object,
                home.Object,
                parent.Object,
                viewer.Object,
                viewerPosition: 5,
                itemNumberDigits: 2,
                mailItem: null,
                tlpStates: null
            );

            // Assert
            viewer.VerifySet(v => v.Controller = controller, Times.Once());
            controller.Parent.Should().BeSameAs(parent.Object);
            controller.ItemNumber.Should().Be(5);
            controller.ItemNumberDigits.Should().Be(2);
            controller.Token.Should().Be(cts.Token);
            QfcItemControllerTestSupport
                .GetField(controller, "_kbdHandler")
                .Should()
                .BeSameAs(kbd.Object);
            QfcItemControllerTestSupport
                .GetField(controller, "_explorerController")
                .Should()
                .BeSameAs(explorer.Object);
            QfcItemControllerTestSupport
                .GetField(controller, "_globals")
                .Should()
                .BeSameAs(globals.Object);
            QfcItemControllerTestSupport
                .GetField(controller, "_tokenSource")
                .Should()
                .BeSameAs(cts);

            cts.Dispose();
        }
    }
}
