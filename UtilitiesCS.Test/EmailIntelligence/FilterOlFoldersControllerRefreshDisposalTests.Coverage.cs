#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public sealed partial class FilterOlFoldersControllerRefreshDisposalTests
    {
        [TestMethod]
        public async Task ViewerFactory_ShowFailure_DisposesCandidateAndPreservesOriginalException()
        {
            var showFailure = new InvalidOperationException("controlled viewer show failure");
            var viewer = new CoverageViewer
            {
                ShowFailure = showFailure,
                DisposeFailure = new InvalidOperationException("controlled dispose failure"),
            };

            Action create = () => _ = new FilterOlFoldersController(null!, () => viewer);
            create.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(showFailure);
            viewer.DisposeCount.Should().Be(1);
            var successfulDisposeViewer = new CoverageViewer { ShowFailure = showFailure };
            Action createWithSuccessfulDispose = () =>
                _ = new FilterOlFoldersController(null!, () => successfulDisposeViewer);
            createWithSuccessfulDispose.Should().Throw<InvalidOperationException>();
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var service = new DelayedFolderTreeService(snapshot.Task);
            using var initialized = new PublicConstructorController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object
            );
            initialized.GetBaseViewerFactory().Should().NotBeNull();
            initialized.Dispose();
            snapshot.SetResult(Snapshot());
            await initialized.Readiness;
            initialized.FolderTreeView.Should().BeNull();
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [TestMethod]
        public void ViewerFactory_InvokeRequiredClose_ClosesOnceAndPreservesConstructionFailure()
        {
            var failure = new InvalidOperationException("controlled subscription failure");
            var viewer = new CoverageViewer
            {
                InvokeRequired = true,
                FormClosedAddFailure = failure,
            };
            Action create = () =>
                _ = new FilterOlFoldersController(
                    FilterOlFoldersControllerInitializationTests
                        .CreateGlobals(new DelayedFolderTreeService(Task.FromResult(Snapshot())))
                        .Object,
                    () => viewer
                );

            create.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(failure);
            (viewer.ShowCount, viewer.InvokeCount, viewer.CloseCount, viewer.DisposeCount)
                .Should()
                .Be((1, 1, 1, 0));
        }

        [TestMethod]
        public void ViewerFactory_InvokeFault_IsContainedWithoutReplacingConstructionFailure()
        {
            var failure = new InvalidOperationException("controlled subscription failure");
            var invokeFailure = new InvalidOperationException("controlled invoke failure");
            var viewer = new CoverageViewer
            {
                InvokeRequired = true,
                FormClosedAddFailure = failure,
                InvokeFailure = invokeFailure,
            };

            Action create = () =>
                _ = new FilterOlFoldersController(
                    FilterOlFoldersControllerInitializationTests
                        .CreateGlobals(new DelayedFolderTreeService(Task.FromResult(Snapshot())))
                        .Object,
                    () => viewer
                );

            create.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(failure);
            (viewer.ShowCount, viewer.InvokeCount, viewer.CloseCount, viewer.DisposeCount)
                .Should()
                .Be((1, 1, 0, 0));
        }

        [TestMethod]
        public async Task CompatibilityFactory_DisposeAfterConstruction_DiscardsCandidate()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var controller = CreateDisposalRaceController(snapshot, disposeFromFactory: true);

            snapshot.SetResult(Snapshot());
            await controller.Readiness;
            (controller.CompatibilityFactoryCount, controller.CandidateCreatedCount)
                .Should()
                .Be((1, 0));
            controller.FolderTreeView.Should().BeNull();
        }

        [TestMethod]
        public async Task CandidateCreated_DisposeBeforeCommit_DiscardsCandidate()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var controller = CreateDisposalRaceController(snapshot, disposeBeforeCommit: true);

            snapshot.SetResult(Snapshot());
            await controller.Readiness;
            (controller.CompatibilityFactoryCount, controller.CandidateCreatedCount)
                .Should()
                .Be((1, 1));
            controller.FolderTreeView.Should().BeNull();
        }

        [TestMethod]
        public async Task PendingSnapshot_CoversNullViewInvokeRequiredAndDuplicateDispose()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var viewer = new CoverageViewer { InvokeRequired = true };
            Action? captured = null;
            viewer.InvokeHandler = action => captured = action;
            var controller = new FilterOlFoldersController(
                FilterOlFoldersControllerInitializationTests
                    .CreateGlobals(new DelayedFolderTreeService(snapshot.Task))
                    .Object,
                viewer,
                new RecordingInlineUiDispatcher()
            );
            controller.FilterSelected(false).Should().BeEmpty();
            var selectedNode = new TreeNode<FolderWrapper>(
                new FolderWrapper(true, 0, 0, "archive", "\\Archive")
            );
            controller.GetCheckedState(selectedNode).Should().Be(CheckState.Checked);
            selectedNode.Value.Selected = false;
            selectedNode.Children.Add(
                new TreeNode<FolderWrapper>(
                    new FolderWrapper(true, 0, 0, "child", "\\Archive\\Child")
                )
            );
            controller.GetCheckedState(selectedNode).Should().Be(CheckState.Indeterminate);
            selectedNode.Children.Clear();
            controller.GetCheckedState(selectedNode).Should().Be(CheckState.Unchecked);
            controller.OlFolderTree_PropertyChanged(
                controller,
                new System.ComponentModel.PropertyChangedEventArgs("value")
            );
            captured.Should().NotBeNull();
            viewer.InvokeCount.Should().Be(1);
            controller.Dispose();
            controller.Dispose();
            controller.TryAttachSnapshotSubscription().Should().BeFalse();
            snapshot.SetResult(Snapshot());
            await controller.Readiness;
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task QueuedDispatcher_DisposeBeforeInitializationOrRefreshEntry_DoesNotMutateView(
            bool refresh
        )
        {
            var pendingDispatch = new TaskCompletionSource<bool>();
            Func<Task<bool>>? queued = null;
            var calls = 0;
            var dispatcher = new Mock<UtilitiesCS.Threading.IUiDispatcher>(MockBehavior.Strict);
            dispatcher
                .Setup(value => value.InvokeAsync(It.IsAny<Func<Task<bool>>>()))
                .Returns(
                    (Func<Task<bool>> operation) =>
                    {
                        if (refresh && ++calls == 1)
                            return operation();
                        queued = operation;
                        return pendingDispatch.Task;
                    }
                );
            var service = new DelayedFolderTreeService(Task.FromResult(Snapshot()));
            IFilterOlFoldersViewer viewer = refresh
                ? new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer()
                : new CoverageViewer();
            var controller = new FilterOlFoldersController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                viewer,
                dispatcher.Object
            );
            if (refresh)
                await controller.Readiness;
            if (refresh)
                service.RaiseSnapshotChanged();
            queued.Should().NotBeNull();
            controller.Dispose();
            await queued!();
            pendingDispatch.SetResult(true);
            await controller.Readiness;
            controller.FolderTreeView.Should().BeNull();
        }

        [TestMethod]
        public async Task RefreshFault_UsesBaseObserverAndSignalsInstanceObserver()
        {
            var failure = new InvalidOperationException("controlled refresh failure");
            var service = new DelayedFolderTreeService(
                Task.FromResult(Snapshot()),
                Task.FromException<FolderTreeSnapshot>(failure)
            );
            using var controller = new BaseRefreshFaultController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object
            );
            await controller.Readiness;
            service.RaiseSnapshotChanged();
            (await controller.RefreshFault).Should().BeSameAs(failure);
        }

        [DataTestMethod]
        [DataRow("request-archive-root")]
        [DataRow("request-store-id")]
        [DataRow("selected-paths")]
        [DataRow("archive-root-store-id")]
        [DataRow("archive-root-folder-path")]
        public async Task GetterDisposalCheckpoint_StopsInitialization(string checkpoint)
        {
            var controller = CreateGetterDisposalController(checkpoint);
            await controller.Readiness;
            controller.FolderTreeView.Should().BeNull();
        }

        private static DisposalRaceController CreateDisposalRaceController(
            TaskCompletionSource<FolderTreeSnapshot> snapshot,
            bool disposeFromFactory = false,
            bool disposeBeforeCommit = false,
            bool disposeAfterCommit = false
        ) =>
            new(
                FilterOlFoldersControllerInitializationTests
                    .CreateGlobals(new DelayedFolderTreeService(snapshot.Task))
                    .Object,
                new CoverageViewer(),
                new RecordingInlineUiDispatcher(),
                disposeFromFactory,
                disposeBeforeCommit,
                disposeAfterCommit
            );

        private static FolderTreeSnapshot Snapshot() =>
            FilterOlFoldersControllerInitializationTests.CreateSnapshot();

        private static FilterOlFoldersController CreateGetterDisposalController(string checkpoint)
        {
            var viewer = new CoverageViewer();
            var archiveRoot = new Mock<Microsoft.Office.Interop.Outlook.Folder>(
                MockBehavior.Strict
            );
            var storeIdReads = 0;
            archiveRoot
                .SetupGet(value => value.StoreID)
                .Returns(() =>
                {
                    storeIdReads++;
                    if (
                        checkpoint == "request-store-id"
                        || (checkpoint == "archive-root-store-id" && storeIdReads == 2)
                    )
                    {
                        viewer.Close();
                    }
                    return "store";
                });
            archiveRoot
                .SetupGet(value => value.FolderPath)
                .Returns(() =>
                {
                    if (checkpoint == "archive-root-folder-path")
                    {
                        viewer.Close();
                    }
                    return "\\Archive";
                });
            var service = new DelayedFolderTreeService(Task.FromResult(Snapshot()));
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(value => value.ArchiveRoot)
                .Returns(() =>
                {
                    if (checkpoint == "request-archive-root")
                    {
                        viewer.Close();
                    }
                    return archiveRoot.Object;
                });
            ol.SetupGet(value => value.FolderTreeService).Returns(service);
            var toDo = new Mock<IToDoObjects>(MockBehavior.Strict);
            toDo.SetupGet(value => value.FilteredFolderScraping)
                .Returns(() =>
                {
                    if (checkpoint == "selected-paths")
                    {
                        viewer.Close();
                    }
                    return new ScoDictionaryNew<string, int>();
                });
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(value => value.Ol).Returns(ol.Object);
            globals.SetupGet(value => value.TD).Returns(toDo.Object);
            return new FilterOlFoldersController(
                globals.Object,
                viewer,
                new RecordingInlineUiDispatcher()
            );
        }

        private sealed class DisposalRaceController : FilterOlFoldersController
        {
            private readonly bool _disposeAfterCommit,
                _disposeBeforeCommit,
                _disposeFromFactory;

            internal DisposalRaceController(
                IApplicationGlobals globals,
                IFilterOlFoldersViewer viewer,
                RecordingInlineUiDispatcher dispatcher,
                bool disposeFromFactory = false,
                bool disposeBeforeCommit = false,
                bool disposeAfterCommit = false
            )
                : base(globals, viewer, dispatcher)
            {
                _disposeFromFactory = disposeFromFactory;
                _disposeBeforeCommit = disposeBeforeCommit;
                _disposeAfterCommit = disposeAfterCommit;
            }

            internal int CompatibilityFactoryCount { get; private set; }

            internal int CandidateCreatedCount { get; private set; }

            internal int CommittedCount { get; private set; }

            internal bool DisposeAfterCommit { get; set; }

            protected internal override FolderTreeCompatibilityView CreateFolderTreeCompatibilityView(
                FolderTreeSnapshot archiveRootSnapshot,
                System.Collections.Generic.IReadOnlyCollection<string> selectedPaths
            )
            {
                CompatibilityFactoryCount++;
                var candidate = base.CreateFolderTreeCompatibilityView(
                    archiveRootSnapshot,
                    selectedPaths
                );
                if (_disposeFromFactory)
                {
                    Dispose();
                }

                return candidate;
            }

            protected internal override FolderTreeCompatibilityView OnFolderTreeViewCandidateCreated(
                FolderTreeCompatibilityView candidateView
            )
            {
                CandidateCreatedCount++;
                if (_disposeBeforeCommit)
                {
                    Dispose();
                }

                return candidateView;
            }

            protected internal override void OnFolderTreeViewCommitted()
            {
                CommittedCount++;
                if (_disposeAfterCommit || DisposeAfterCommit)
                {
                    Dispose();
                }
            }
        }

        private sealed class BaseRefreshFaultController : FilterOlFoldersController
        {
            private readonly TaskCompletionSource<Exception> _refreshFault = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            internal BaseRefreshFaultController(IApplicationGlobals globals)
                : base(
                    globals,
                    new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer(),
                    new RecordingInlineUiDispatcher()
                ) { }

            internal Task<Exception> RefreshFault => _refreshFault.Task;

            protected internal override void ObserveFolderTreeRefreshFault(Exception exception)
            {
                base.ObserveFolderTreeRefreshFault(exception);
                _refreshFault.TrySetResult(exception);
            }
        }

        private sealed class PublicConstructorController : FilterOlFoldersController
        {
            internal PublicConstructorController(IApplicationGlobals globals)
                : base(globals) { }

            protected internal override Func<IFilterOlFoldersViewer> CreateViewerFactory() =>
                () => new CoverageViewer();

            protected internal override UtilitiesCS.Threading.IUiDispatcher CreateFolderTreeUiDispatcher() =>
                new RecordingInlineUiDispatcher();

            internal Func<IFilterOlFoldersViewer> GetBaseViewerFactory() =>
                base.CreateViewerFactory();
        }

        private sealed class CoverageViewer : IFilterOlFoldersViewer
        {
            private FormClosedEventHandler? _formClosed;
            public event FormClosedEventHandler? FormClosed
            {
                add
                {
                    if (FormClosedAddFailure is not null)
                        throw FormClosedAddFailure;
                    _formClosed += value;
                }
                remove => _formClosed -= value;
            }
            public BrightIdeasSoftware.TreeListView TlvNotFiltered => null!;
            public BrightIdeasSoftware.TreeListView TlvFiltered => null!;
            public bool InvokeRequired { get; set; }
            public Exception? ShowFailure { get; set; }
            public Exception? FormClosedAddFailure { get; set; }
            public Exception? InvokeFailure { get; set; }
            public Exception? DisposeFailure { get; set; }
            public Action<Action>? InvokeHandler { get; set; }
            public int ShowCount { get; private set; }
            public int InvokeCount { get; private set; }
            public int CloseCount { get; private set; }
            public int DisposeCount { get; private set; }

            public void SetController(FilterOlFoldersController controller) { }

            public void Show()
            {
                ShowCount++;
                if (ShowFailure is not null)
                    throw ShowFailure;
            }

            public void Close()
            {
                CloseCount++;
                _formClosed?.Invoke(this, new FormClosedEventArgs(CloseReason.None));
            }

            public object Invoke(Delegate method)
            {
                InvokeCount++;
                if (InvokeFailure is not null)
                    throw InvokeFailure;
                var action = (Action)method;
                if (InvokeHandler is not null)
                {
                    InvokeHandler(action);
                    return null!;
                }
                action();
                return null!;
            }

            public void Dispose()
            {
                DisposeCount++;
                if (DisposeFailure is not null)
                    throw DisposeFailure;
            }
        }
    }
}
