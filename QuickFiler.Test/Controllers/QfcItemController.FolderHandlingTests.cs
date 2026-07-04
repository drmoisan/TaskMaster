using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Folder-handling cluster tests (research §5.2). Covers the pure static folder-selection seam
    /// PopulateAndSelectFolder edge cases and the AssignFolderComboBox guard behavior.
    /// </summary>
    [TestClass]
    public class QfcItemController_FolderHandlingTests
    {
        private sealed class FolderController : QfcItemController
        {
            internal FolderController()
                : base() { }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_ExactMatchAtIndexZero_SelectsIndexZero()
        {
            // Arrange — predetermined folder equals items[0]; it must be selected at index 0,
            // not overridden by the index-1 fallback.
            var folders = new[] { @"\\A\predetermined", @"\\A\suggestion1", @"\\A\suggestion2" };
            using (var comboBox = new ComboBox())
            {
                // Act
                var selected = QfcItemController.PopulateAndSelectFolder(
                    comboBox,
                    folders,
                    predeterminedFolder: @"\\A\predetermined"
                );

                // Assert
                comboBox.SelectedIndex.Should().Be(0);
                selected.Should().Be(@"\\A\predetermined");
            }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne()
        {
            // Arrange — predetermined folder is not present in the array; the index-1 fallback applies.
            var folders = new[] { @"\\A\header", @"\\A\top", @"\\A\second" };
            using (var comboBox = new ComboBox())
            {
                // Act
                var selected = QfcItemController.PopulateAndSelectFolder(
                    comboBox,
                    folders,
                    predeterminedFolder: @"\\A\not-present"
                );

                // Assert
                comboBox.SelectedIndex.Should().Be(1);
                selected.Should().Be(@"\\A\top");
            }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection()
        {
            // Arrange — the production caller guards against an empty FolderArray; calling the seam
            // directly with no items documents that the index-1 selection is out of range.
            var folders = Array.Empty<string>();
            using (var comboBox = new ComboBox())
            {
                // Act
                Action act = () =>
                    QfcItemController.PopulateAndSelectFolder(
                        comboBox,
                        folders,
                        predeterminedFolder: null
                    );

                // Assert
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        /// <summary>
        /// Builds a <see cref="FolderPredictor"/> with a known <c>FolderArray</c> without touching
        /// Outlook COM. The single-arg <c>FolderPredictor(Outlook.Application)</c> constructor performs
        /// no COM work; seeding the private <c>_folderList</c> backing field makes the lazy
        /// <c>FolderArray</c> getter return that list directly.
        /// </summary>
        private static FolderPredictor BuildFolderHandlerWithArray(params string[] folders)
        {
            var ctor = typeof(FolderPredictor)
                .GetConstructors()
                .Single(c =>
                    c.GetParameters().Length == 1
                    && c.GetParameters()[0].ParameterType.Name == "Application"
                );
            var fp = (FolderPredictor)ctor.Invoke(new object[] { null });
            typeof(FolderPredictor)
                .GetField("_folderList", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fp, new List<string>(folders));
            return fp;
        }

        private static void SetPrivate(QfcItemController controller, string field, object value) =>
            typeof(QfcItemController)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, value);

        private static string ReadControllerSource(string fileName)
        {
            string path = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\QuickFiler\Controllers",
                    fileName
                )
            );
            return File.ReadAllText(path);
        }

        [TestMethod]
        public void LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore()
        {
            string source = ReadControllerSource("QfcItemController.FolderHandling.cs");

            source
                .Should()
                .Contain("Probability debug [QfcItemController.LoadFolderHandler (FromField)]");
            source
                .Should()
                .Contain(
                    "Probability debug [QfcItemController.LoadFolderHandlerAsync (FromArrayOrString)]"
                );
            source.Should().Contain("Subject='{ItemHelper?.Subject}'");
            source.Should().Contain("EntryID='{ItemHelper?.EntryId}'");
            source.Should().Contain("TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}");
        }

        // ------------------------- LoadFolderHandler (P10-T11: FolderPredictor factory seam) -------------------------

        [TestMethod]
        public void LoadFolderHandler_WhenVarListNull_InvokesFactoryWithItemHelperAndFromFieldOptions()
        {
            // Arrange
            var controller = new FolderController();
            var globals = new Mock<IApplicationGlobals>().Object;
            var helper = new MailItemHelper();
            controller.ItemHelper = helper;
            SetPrivate(controller, "_globals", globals);
            var returned = BuildFolderHandlerWithArray(@"\\A\one");
            IApplicationGlobals capturedGlobals = null;
            object capturedObjItem = null;
            FolderPredictor.InitOptions capturedOptions = default;
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > factory = (g, o, opt) =>
            {
                capturedGlobals = g;
                capturedObjItem = o;
                capturedOptions = opt;
                return returned;
            };
            SetPrivate(controller, "_folderPredictorFactory", factory);

            // Act
            controller.LoadFolderHandler();

            // Assert
            capturedGlobals.Should().BeSameAs(globals);
            capturedObjItem.Should().BeSameAs(helper);
            capturedOptions.Should().Be(FolderPredictor.InitOptions.FromField);
            QfcItemControllerTestSupport
                .GetField(controller, "_folderHandler")
                .Should()
                .BeSameAs(returned);
        }

        [TestMethod]
        public void LoadFolderHandler_WhenVarListProvided_InvokesFactoryWithArrayOrStringOptions()
        {
            // Arrange
            var controller = new FolderController();
            var globals = new Mock<IApplicationGlobals>().Object;
            SetPrivate(controller, "_globals", globals);
            object varList = new[] { "a", "b" };
            var returned = BuildFolderHandlerWithArray(@"\\A\two");
            IApplicationGlobals capturedGlobals = null;
            object capturedObjItem = null;
            FolderPredictor.InitOptions capturedOptions = default;
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > factory = (g, o, opt) =>
            {
                capturedGlobals = g;
                capturedObjItem = o;
                capturedOptions = opt;
                return returned;
            };
            SetPrivate(controller, "_folderPredictorFactory", factory);

            // Act
            controller.LoadFolderHandler(varList);

            // Assert
            capturedGlobals.Should().BeSameAs(globals);
            capturedObjItem.Should().BeSameAs(varList);
            capturedOptions.Should().Be(FolderPredictor.InitOptions.FromArrayOrString);
            QfcItemControllerTestSupport
                .GetField(controller, "_folderHandler")
                .Should()
                .BeSameAs(returned);
        }

        // ------------------------- LoadFolderHandlerAsync (P10-T13: FolderPredictor factory seam) -------------------------

        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenVarListNull_InvokesFactoryWithExpectedArgs()
        {
            // Arrange
            var controller = new FolderController();
            var globals = new Mock<IApplicationGlobals>().Object;
            var helper = new MailItemHelper();
            controller.ItemHelper = helper;
            SetPrivate(controller, "_globals", globals);
            IApplicationGlobals capturedGlobals = null;
            object capturedObjItem = null;
            FolderPredictor.InitOptions capturedOptions = default;
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > factory = (g, o, opt) =>
            {
                capturedGlobals = g;
                capturedObjItem = o;
                capturedOptions = opt;
                throw new InvalidOperationException("sentinel");
            };
            SetPrivate(controller, "_folderPredictorFactory", factory);

            // Act
            Func<Task> act = () => controller.LoadFolderHandlerAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            capturedGlobals.Should().BeSameAs(globals);
            capturedObjItem.Should().BeSameAs(helper);
            capturedOptions.Should().Be(FolderPredictor.InitOptions.FromField);
        }

        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenVarListProvided_InvokesFactoryWithArrayOrStringArgs()
        {
            // Arrange
            var controller = new FolderController();
            var globals = new Mock<IApplicationGlobals>().Object;
            SetPrivate(controller, "_globals", globals);
            object varList = new[] { "x" };
            IApplicationGlobals capturedGlobals = null;
            object capturedObjItem = null;
            FolderPredictor.InitOptions capturedOptions = default;
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > factory = (g, o, opt) =>
            {
                capturedGlobals = g;
                capturedObjItem = o;
                capturedOptions = opt;
                throw new InvalidOperationException("sentinel");
            };
            SetPrivate(controller, "_folderPredictorFactory", factory);

            // Act
            Func<Task> act = () =>
                controller.LoadFolderHandlerAsync(CancellationToken.None, varList);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            capturedGlobals.Should().BeSameAs(globals);
            capturedObjItem.Should().BeSameAs(varList);
            capturedOptions.Should().Be(FolderPredictor.InitOptions.FromArrayOrString);
        }

        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenPrimaryFactoryThrowsArgumentNull_InvokesEmptyFactoryFallback()
        {
            // Arrange
            var controller = new FolderController();
            var globals = new Mock<IApplicationGlobals>().Object;
            var helper = new MailItemHelper();
            controller.ItemHelper = helper;
            SetPrivate(controller, "_globals", globals);
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > primaryFactory = (g, o, opt) => throw new ArgumentNullException("objItem");
            var fallback = BuildFolderHandlerWithArray(@"\\A\empty");
            Func<IApplicationGlobals, FolderPredictor> emptyFactory = g => fallback;
            SetPrivate(controller, "_folderPredictorFactory", primaryFactory);
            SetPrivate(controller, "_folderPredictorEmptyFactory", emptyFactory);

            // Act
            Func<Task> act = () => controller.LoadFolderHandlerAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            QfcItemControllerTestSupport
                .GetField(controller, "_folderHandler")
                .Should()
                .BeSameAs(fallback);
        }

        // ------------------------- PopulateFolderComboBox / Async (P10-T14/P10-T15) -------------------------

        [TestMethod]
        public void PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", viewer.Object);
            SetPrivate(controller, "_globals", new Mock<IApplicationGlobals>().Object);
            controller.ItemHelper = new MailItemHelper();
            var returned = BuildFolderHandlerWithArray(@"\\A\one", @"\\A\two", @"\\A\three");
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > factory = (g, o, opt) => returned;
            SetPrivate(controller, "_folderPredictorFactory", factory);

            // Act
            controller.PopulateFolderComboBox();

            // Assert
            viewer.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
        }

        [TestMethod]
        public async Task PopulateFolderComboBoxAsync_WhenFactorySucceeds_DispatchesAssignFolderComboBoxThroughViewerDispatcher()
        {
            // Arrange — a dedicated running WPF Dispatcher exercises the real UiDispatcher.InvokeAsync
            // marshal, mirroring AssignControlsAsync_DispatchesAssignThroughViewerDispatcher. A non-null
            // varList routes LoadFolderHandlerAsync through FolderPredictor.InitAsync's
            // FromArrayOrString branch (FromArrayOrString(obj) only sets the in-memory _folderList — no
            // COM/Suggestions access), so the real double's InitAsync call completes deterministically.
            System.Windows.Threading.Dispatcher dispatcher =
                QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                var viewer = new Mock<IItemViewer>();
                viewer.SetupGet(v => v.InvokeRequired).Returns(false);
                viewer.SetupGet(v => v.UiDispatcher).Returns(dispatcher);
                var controller = new FolderController();
                SetPrivate(controller, "_itemViewer", viewer.Object);
                SetPrivate(controller, "_globals", new Mock<IApplicationGlobals>().Object);
                controller.ItemHelper = new MailItemHelper();
                object varList = new[] { @"\\A\one", @"\\A\two" };
                var returned = BuildFolderHandlerWithArray(@"\\A\one", @"\\A\two");
                Func<
                    IApplicationGlobals,
                    object,
                    FolderPredictor.InitOptions,
                    FolderPredictor
                > factory = (g, o, opt) => returned;
                SetPrivate(controller, "_folderPredictorFactory", factory);

                // Act
                await controller.PopulateFolderComboBoxAsync(CancellationToken.None, varList);

                // Assert
                viewer.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }

        [TestMethod]
        public void AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer()
        {
            // Arrange — a populated folder handler with no predetermined folder must route the
            // suggestions into the viewer and select the index-1 top suggestion.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.GetSelectedFolder()).Returns(@"\\A\top");
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", @"\\A\second")
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(1), Times.Once());
            mock.Verify(v => v.SetFolderSelectedItem(It.IsAny<string>()), Times.Never());
            controller.SelectedFolder.Should().Be(@"\\A\top");
        }

        [TestMethod]
        public void AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder()
        {
            // Arrange — a predetermined folder that the view reports as present must be preselected
            // by name rather than falling back to the index-1 suggestion.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.FolderContains(@"\\A\chosen")).Returns(true);
            mock.Setup(v => v.GetSelectedFolder()).Returns(@"\\A\chosen");
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_predeterminedFolder", @"\\A\chosen");
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", @"\\A\chosen")
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderSelectedItem(@"\\A\chosen"), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
            controller.SelectedFolder.Should().Be(@"\\A\chosen");
        }

        [TestMethod]
        public void AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer()
        {
            // Arrange — with no folder handler the method's guard must short-circuit and perform no
            // folder mutations on the view.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new FolderController();
            typeof(QfcItemController)
                .GetField(
                    "_itemViewer",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(controller, mock.Object);

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Never());
            mock.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }
    }
}
