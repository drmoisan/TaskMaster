using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
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
    public class QfcItemController_InitializationTests
    {
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
