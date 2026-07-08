using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerLifecycleTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            EfcHomeController.ResetDefaultDependenciesFactory();
        }

        [TestMethod]
        public async Task CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies()
        {
            var probe = new LifecycleProbe();
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var factoryCalls = 0;
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
                    mailItems.Should().Equal(mail);
                    return Task.FromResult(probe.DataModelWithMail);
                }
            );
            EfcHomeController.SetDefaultDependenciesFactory(() =>
            {
                factoryCalls++;
                return dependencies;
            });

            var controller = await EfcHomeController.CreateAsync(
                probe.Globals,
                probe.ParentCleanup,
                mail
            );

            factoryCalls.Should().BeGreaterThan(0);
            controller.DataModel.Should().BeSameAs(probe.DataModelWithMail);
            probe.Calls.Should().ContainInOrder("selection", "async-data");
        }

        [TestMethod]
        public async Task LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies()
        {
            var probe = new LifecycleProbe();
            var dependencies = probe.CreateDependencies(
                selectionLoader: (globals, selectedMail) =>
                {
                    probe.Calls.Add("selection");
                    return new List<MailItem>();
                },
                dataModelFactory: (globals, mail, tokenSource, token) =>
                {
                    probe.Calls.Add("dummy-data");
                    mail.Should().BeNull();
                    return probe.DataModelWithoutMail;
                }
            );
            EfcHomeController.SetDefaultDependenciesFactory(() => dependencies);

            var controller = await EfcHomeController.LoadFinderAsync(
                probe.Globals,
                probe.ParentCleanup
            );

            controller.InitType.Should().Be(QfEnums.InitTypeEnum.Find);
            controller.DataModel.Should().BeSameAs(probe.DataModelWithoutMail);
            probe.Calls.Should().ContainInOrder("selection", "dummy-data");
        }

        [TestMethod]
        public void Run_WithoutMail_ShowsMessageThroughInjectedSeam()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(null);
            MessageBoxCall message = null;
            controller.MessageBoxShowAction = (text, caption, buttons, icon) =>
                message = new MessageBoxCall(text, caption, buttons, icon);

            controller.Run();

            message.Should().NotBeNull();
            message.Text.Should().Be("Error");
            message.Caption.Should().Be("No MailItem Selected");
            message.Buttons.Should().Be(MessageBoxButtons.OK);
            message.Icon.Should().Be(MessageBoxIcon.Error);
        }

        [TestMethod]
        public async Task RunAsync_WithoutMail_ShowsMessageThroughInjectedSeam()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(null);
            MessageBoxCall message = null;
            controller.MessageBoxShowAction = (text, caption, buttons, icon) =>
                message = new MessageBoxCall(text, caption, buttons, icon);

            await controller.RunAsync();

            message.Should().NotBeNull();
            message.Caption.Should().Be("No MailItem Selected");
        }

        [TestMethod]
        public void Run_WithMail_ShowsViewerThroughInjectedSeam()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(probe.Mail);
            EfcViewer shownViewer = null;
            controller.ViewerShowAction = viewer => shownViewer = viewer;

            controller.Run();

            shownViewer.Should().BeSameAs(probe.Viewer);
        }

        [TestMethod]
        public async Task RunAsync_WithMail_ShowsViewerThroughInjectedSeam()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(probe.Mail);
            EfcViewer shownViewer = null;
            controller.ViewerShowAsyncAction = viewer =>
            {
                shownViewer = viewer;
                return Task.CompletedTask;
            };

            await controller.RunAsync();

            shownViewer.Should().BeSameAs(probe.Viewer);
        }

        [TestMethod]
        public void Cleanup_ClearsControllerFieldsAndInvokesParentCleanup()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(probe.Mail);

            controller.Cleanup();

            probe.ParentCleanupCalled.Should().BeTrue();
            controller.Globals.Should().BeNull();
            controller.FormViewer.Should().BeNull();
            controller.ExplorerController.Should().BeNull();
            controller.FormController.Should().BeNull();
            controller.KeyboardHandler.Should().BeNull();
        }

        [TestMethod]
        public void ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(null);
            var explorer = new Mock<IQfcExplorerController>(MockBehavior.Loose).Object;
            var keyboard = new Mock<IQfcKeyboardHandler>(MockBehavior.Loose).Object;

            controller.ExplorerController = explorer;
            controller.KeyboardHandler = keyboard;

            controller.ExplorerController.Should().BeSameAs(explorer);
            controller.KeyboardHandler.Should().BeSameAs(keyboard);
        }

        [TestMethod]
        public void LoadedAndFilerQueue_PreserveNotImplementedContracts()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(null);

            System.Action loaded = () =>
            {
                _ = controller.Loaded;
            };
            System.Action filerQueue = () =>
            {
                _ = controller.FilerQueue;
            };

            loaded.Should().Throw<NotImplementedException>();
            filerQueue.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public async Task OpenFolderMethods_DelegateToDataModelWithoutExternalServices()
        {
            var probe = new LifecycleProbe();
            var controller = probe.CreateControllerWithMail(null);
            controller.DataModel = LifecycleProbe.CreateDataModelWithGlobals(probe.Globals);

            await controller.OpenOlFolderAsync("Archive/Target");
            await controller.OpenFsFolderAsync("Archive/Target");

            probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2);
        }

        private sealed class LifecycleProbe
        {
            internal FakeApplicationGlobals Globals { get; }

            internal FakeFileSystemFolderPaths FileSystem { get; }

            internal List<string> Calls { get; } = new List<string>();

            internal MailItem Mail { get; } = new Mock<MailItem>(MockBehavior.Loose).Object;

            internal EfcViewer Viewer { get; } = CreateUninitialized<EfcViewer>();

            internal EfcDataModel DataModelWithMail { get; private set; }

            internal EfcDataModel DataModelWithoutMail { get; private set; }

            internal bool ParentCleanupCalled { get; private set; }

            internal System.Action ParentCleanup => () => ParentCleanupCalled = true;

            private EfcFormController FormController { get; } =
                CreateUninitialized<EfcFormController>();

            internal LifecycleProbe()
            {
                FileSystem = new FakeFileSystemFolderPaths(
                    new ConcurrentDictionary<string, string>()
                );
                Globals = new FakeApplicationGlobals(FileSystem);
                DataModelWithMail = CreateDataModel(Mail);
                DataModelWithoutMail = CreateDataModel(null);
            }

            internal EfcHomeController CreateControllerWithMail(MailItem mail)
            {
                return new EfcHomeController(
                    Globals,
                    ParentCleanup,
                    CreateDependencies(
                        dataModelFactory: (globals, selectedMail, tokenSource, token) =>
                            CreateDataModel(mail)
                    ),
                    mail
                );
            }

            internal EfcHomeControllerDependencies CreateDependencies(
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
                Func<IApplicationGlobals, MailItem, List<MailItem>> selectionLoader = null
            )
            {
                return new EfcHomeControllerDependencies(
                    dataModelFactory: dataModelFactory
                        ?? ((globals, mail, tokenSource, token) => CreateDataModel(mail)),
                    asyncDataModelFactory: asyncDataModelFactory
                        ?? (
                            (globals, mailItems, tokenSource, token, loadAll) =>
                                Task.FromResult(DataModelWithMail)
                        ),
                    viewerFactory: () =>
                    {
                        Calls.Add("viewer");
                        return Viewer;
                    },
                    keyboardHandlerFactory: (viewer, controller) =>
                    {
                        Calls.Add("keyboard");
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
                        return FormController;
                    },
                    initializeDataFields: (formController, dataModel) =>
                    {
                        Calls.Add("initialize-data");
                        return FormController;
                    },
                    selectionLoader: selectionLoader ?? CreateDefaultSelection
                );
            }

            private static List<MailItem> CreateDefaultSelection(
                IApplicationGlobals globals,
                MailItem mail
            )
            {
                return mail is null ? new List<MailItem>() : new List<MailItem> { mail };
            }

            internal static EfcDataModel CreateDataModelWithGlobals(IApplicationGlobals globals)
            {
                return new EfcDataModel(
                    globals,
                    null,
                    new CancellationTokenSource(),
                    CancellationToken.None
                );
            }

            private static EfcDataModel CreateDataModel(MailItem mail)
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

        private sealed class FakeApplicationGlobals : IApplicationGlobals
        {
            internal FakeApplicationGlobals(IFileSystemFolderPaths fileSystem)
            {
                FS = fileSystem;
            }

            public Task LoadAsync(bool parallel)
            {
                return Task.CompletedTask;
            }

            public IFileSystemFolderPaths FS { get; }

            public IOlObjects Ol => null;

            public IToDoObjects TD => null;

            public IAppAutoFileObjects AF => null;

            public IAppEvents Events => null;

            public IAppQuickFilerSettings QfSettings => null;

            public IAppItemEngines Engines => null;

            public IntelligenceConfig IntelRes => null;

            public IStoreDisableService StoreDisable => null;
        }

        private sealed class FakeFileSystemFolderPaths : IFileSystemFolderPaths
        {
            private readonly ConcurrentDictionary<string, string> specialFolders;

            internal FakeFileSystemFolderPaths(ConcurrentDictionary<string, string> specialFolders)
            {
                this.specialFolders = specialFolders;
            }

            public int SpecialFoldersAccessCount { get; private set; }

            public ConcurrentDictionary<string, string> SpecialFolders
            {
                get
                {
                    SpecialFoldersAccessCount++;
                    return specialFolders;
                }
            }

            public IAppStagingFilenames Filenames => null;

            public void Reload() { }

            public string MatchBestSpecialFolder(string path)
            {
                return null;
            }
        }

        private sealed class MessageBoxCall
        {
            internal MessageBoxCall(
                string text,
                string caption,
                MessageBoxButtons buttons,
                MessageBoxIcon icon
            )
            {
                Text = text;
                Caption = caption;
                Buttons = buttons;
                Icon = icon;
            }

            internal string Text { get; }

            internal string Caption { get; }

            internal MessageBoxButtons Buttons { get; }

            internal MessageBoxIcon Icon { get; }
        }
    }
}
