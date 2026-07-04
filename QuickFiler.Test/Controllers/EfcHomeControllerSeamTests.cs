using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerSeamTests
    {
        [TestMethod]
        public async Task CreateAsync_WithExplicitMail_UsesSelectionAndInitializationFactories()
        {
            var probe = new ControllerSeamProbe();
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;
            List<MailItem> asyncMailItems = null;
            var dependencies = probe.CreateDependencies(
                selectionLoader: (globals, selectedMail) =>
                {
                    probe.Calls.Add("selection");
                    selectedMail.Should().BeSameAs(mail);
                    return new List<MailItem> { selectedMail };
                },
                asyncDataModelFactory: (globals, mailItems, tokenSource, token, loadAll) =>
                {
                    probe.Calls.Add("async-data");
                    asyncMailItems = mailItems;
                    tokenSource.Should().NotBeNull();
                    loadAll.Should().BeFalse();
                    return Task.FromResult(probe.DataModel);
                }
            );

            var controller = await EfcHomeController.CreateAsync(
                probe.Globals.Object,
                probe.ParentCleanup,
                dependencies,
                mail
            );

            asyncMailItems.Should().Equal(mail);
            controller.FormViewer.Should().BeSameAs(probe.Viewer);
            controller.DataModel.Should().BeSameAs(probe.DataModel);
            controller
                .InitType.Should()
                .Be(QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv);
            probe
                .Calls.Should()
                .ContainInOrder(
                    "selection",
                    "async-data",
                    "viewer",
                    "keyboard",
                    "explorer",
                    "form-without-data",
                    "initialize-data"
                );
        }

        [TestMethod]
        public async Task CreateAsync_WithEmptySelection_DoesNotInitializeViewerOrDataModel()
        {
            var probe = new ControllerSeamProbe();
            var dependencies = probe.CreateDependencies(
                selectionLoader: (globals, mail) =>
                {
                    probe.Calls.Add("selection");
                    return new List<MailItem>();
                },
                asyncDataModelFactory: (globals, mailItems, tokenSource, token, loadAll) =>
                    throw new InvalidOperationException("Async data should not load."),
                viewerFactory: () => throw new InvalidOperationException("Viewer should not load.")
            );

            var controller = await EfcHomeController.CreateAsync(
                probe.Globals.Object,
                probe.ParentCleanup,
                dependencies
            );

            controller.FormViewer.Should().BeNull();
            controller.DataModel.Should().BeNull();
            probe.Calls.Should().Equal("selection");
        }

        [TestMethod]
        public async Task LoadFinderAsync_WithEmptySelection_InitializesFindShellAndDummyDataModel()
        {
            var probe = new ControllerSeamProbe();
            MailItem dummyMail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var dummyModel = ControllerSeamProbe.CreateDataModel(dummyMail);
            var dependencies = probe.CreateDependencies(
                selectionLoader: (globals, mail) => new List<MailItem>(),
                dataModelFactory: (globals, mail, tokenSource, token) =>
                {
                    probe.Calls.Add("dummy-data");
                    mail.Should().BeNull();
                    tokenSource.Should().NotBeNull();
                    return dummyModel;
                }
            );

            var controller = await EfcHomeController.LoadFinderAsync(
                probe.Globals.Object,
                probe.ParentCleanup,
                dependencies
            );

            controller.InitType.Should().Be(QfEnums.InitTypeEnum.Find);
            controller.FormViewer.Should().BeSameAs(probe.Viewer);
            controller.DataModel.Should().BeSameAs(dummyModel);
            probe
                .Calls.Should()
                .ContainInOrder(
                    "viewer",
                    "keyboard",
                    "explorer",
                    "form-without-data",
                    "dummy-data",
                    "initialize-data"
                );
        }

        [TestMethod]
        public async Task HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad()
        {
            var probe = new ControllerSeamProbe();
            var firstMail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var secondMail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var liveSelection = new List<MailItem> { firstMail, secondMail };
            List<MailItem> capturedSelection = null;
            var modelCompletion = new TaskCompletionSource<EfcDataModel>();
            var dependencies = probe.CreateDependencies(
                asyncDataModelFactory: (globals, mailItems, tokenSource, token, loadAll) =>
                {
                    capturedSelection = mailItems;
                    return modelCompletion.Task;
                }
            );
            var controller = new EfcHomeController(
                probe.Globals.Object,
                probe.ParentCleanup,
                dependencies
            );

            var initializeTask = controller.HandleSelectionChangedAsync(
                probe.Globals.Object,
                liveSelection,
                QfEnums.InitTypeEnum.Sort
            );
            liveSelection.Clear();
            modelCompletion.SetResult(probe.DataModel);
            await initializeTask;

            capturedSelection.Should().Equal(firstMail, secondMail);
            capturedSelection.Should().NotBeSameAs(liveSelection);
            controller.DataModel.Should().BeSameAs(probe.DataModel);
        }

        private sealed class ControllerSeamProbe
        {
            internal Mock<IApplicationGlobals> Globals { get; } =
                new Mock<IApplicationGlobals>(MockBehavior.Loose);

            internal System.Action ParentCleanup { get; } = () => { };

            internal List<string> Calls { get; } = new List<string>();

            internal MailItem Mail { get; } = new Mock<MailItem>(MockBehavior.Loose).Object;

            internal EfcDataModel DataModel { get; }

            internal EfcViewer Viewer { get; } = CreateUninitialized<EfcViewer>();

            private EfcFormController FormController { get; } =
                CreateUninitialized<EfcFormController>();

            internal ControllerSeamProbe()
            {
                DataModel = CreateDataModel(Mail);
            }

            internal EfcHomeControllerDependencies CreateDependencies(
                Func<IApplicationGlobals, MailItem, List<MailItem>> selectionLoader = null,
                Func<
                    IApplicationGlobals,
                    MailItem,
                    CancellationTokenSource,
                    CancellationToken,
                    EfcDataModel
                > dataModelFactory = null,
                Func<
                    IApplicationGlobals,
                    List<MailItem>,
                    CancellationTokenSource,
                    CancellationToken,
                    bool,
                    Task<EfcDataModel>
                > asyncDataModelFactory = null,
                Func<EfcViewer> viewerFactory = null
            )
            {
                return new EfcHomeControllerDependencies(
                    dataModelFactory: dataModelFactory
                        ?? ((globals, mail, tokenSource, token) => CreateDataModel(Mail)),
                    asyncDataModelFactory: asyncDataModelFactory
                        ?? (
                            (globals, mailItems, tokenSource, token, loadAll) =>
                                Task.FromResult(DataModel)
                        ),
                    viewerFactory: viewerFactory
                        ?? (
                            () =>
                            {
                                Calls.Add("viewer");
                                return Viewer;
                            }
                        ),
                    keyboardHandlerFactory: (viewer, controller) =>
                    {
                        Calls.Add("keyboard");
                        viewer.Should().BeSameAs(Viewer);
                        return new Mock<IQfcKeyboardHandler>(MockBehavior.Loose).Object;
                    },
                    explorerControllerFactory: (initType, globals, controller) =>
                    {
                        Calls.Add("explorer");
                        return new Mock<IQfcExplorerController>(MockBehavior.Loose).Object;
                    },
                    formControllerWithDataFactory: (
                        globals,
                        dataModel,
                        viewer,
                        controller,
                        cleanup,
                        initType,
                        token
                    ) =>
                    {
                        Calls.Add("form-with-data");
                        dataModel.Should().NotBeNull();
                        viewer.Should().BeSameAs(Viewer);
                        return FormController;
                    },
                    formControllerWithoutDataFactory: (
                        globals,
                        viewer,
                        controller,
                        cleanup,
                        initType,
                        token
                    ) =>
                    {
                        Calls.Add("form-without-data");
                        viewer.Should().BeSameAs(Viewer);
                        return FormController;
                    },
                    initializeDataFields: (formController, dataModel) =>
                    {
                        Calls.Add("initialize-data");
                        formController.Should().BeSameAs(FormController);
                        dataModel.Should().NotBeNull();
                        return FormController;
                    },
                    selectionLoader: selectionLoader
                        ?? ((globals, mail) => new List<MailItem> { mail ?? Mail })
                );
            }

            internal static EfcDataModel CreateDataModel(MailItem mail)
            {
                var dataModel = CreateUninitialized<EfcDataModel>();
                dataModel.Mail = mail;
                return dataModel;
            }

            private static T CreateUninitialized<T>()
                where T : class
            {
                return (T)FormatterServices.GetUninitializedObject(typeof(T));
            }
        }
    }
}
