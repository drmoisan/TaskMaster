#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public sealed partial class FilterOlFoldersControllerRefreshDisposalTests
    {
        [TestMethod]
        public async Task RefreshArchiveRootClose_BeforeCompatibilityView_DoesNotCommit()
        {
            var refreshSnapshot = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot()),
                refreshSnapshot.Task
            );
            var viewer = new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
            var archiveRootReads = 0;
            var controller = new RefreshTrackingFilterOlFoldersController(
                FilterOlFoldersControllerInitializationTests
                    .CreateGlobals(
                        service,
                        () =>
                        {
                            if (Interlocked.Increment(ref archiveRootReads) == 4)
                            {
                                viewer.Close();
                            }
                        }
                    )
                    .Object,
                viewer
            );

            await controller.Readiness;
            service.RaiseSnapshotChanged();
            await service.RefreshRequested;
            var refreshOperation = controller.LastAsyncOperation;
            refreshSnapshot.SetResult(
                FilterOlFoldersControllerInitializationTests.CreateSnapshot()
            );
            await refreshOperation;

            controller.FolderTreeView.Should().BeNull();
            controller.RefreshViewAppliedCount.Should().Be(0);
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [TestMethod]
        public async Task DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var service = new DelayedFolderTreeService(snapshot.Task);
            var viewer = new BlockingRecordingViewer();
            var controller = await Task.Run(() =>
                    new RefreshTrackingFilterOlFoldersController(
                        FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                        viewer
                    )
                )
                .ConfigureAwait(false);

            try
            {
                snapshot.SetResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot());
                await viewer.SetControllerEntered.ConfigureAwait(false);
                controller.Dispose();
                viewer.ReleaseSetController();
                await controller.Readiness.ConfigureAwait(false);
                service.RaiseSnapshotChanged();
                var notificationOperation = controller.LastAsyncOperation;
                await notificationOperation.ConfigureAwait(false);

                controller.FolderTreeView.Should().BeNull();
                controller.RefreshViewAppliedCount.Should().Be(0);
                service.SnapshotChangedHandlerCount.Should().Be(0);
            }
            finally
            {
                viewer.ReleaseSetController();
                await controller.Readiness.ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var viewer = new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
            var service = new SubscriptionBarrierFolderTreeService(snapshot.Task);
            var controller = await Task.Run(() =>
                    new RefreshTrackingFilterOlFoldersController(
                        FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                        viewer
                    )
                )
                .ConfigureAwait(false);

            try
            {
                snapshot.SetResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot());
                await service.SnapshotChangedStored.ConfigureAwait(false);
                controller.Dispose();
                service.RaiseSnapshotChanged();
                service.ReleaseSubscription();
                await controller.Readiness.ConfigureAwait(false);

                controller.FolderTreeView.Should().BeNull();
                controller.RefreshViewAppliedCount.Should().Be(0);
                service.SnapshotChangedHandlerCount.Should().Be(0);
            }
            finally
            {
                service.ReleaseSubscription();
                await controller.Readiness.ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task CommittedCandidate_DisposeBeforeInitializationContinuation_DoesNotMutateViewer()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var controller = CreateDisposalRaceController(snapshot, disposeAfterCommit: true);
            snapshot.SetResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot());
            await controller.Readiness;

            (controller.CandidateCreatedCount, controller.CommittedCount).Should().Be((1, 1));
            controller.FolderTreeView.Should().BeNull();
        }

        [TestMethod]
        public async Task CommittedCandidate_DisposeBeforeRefreshNotification_DoesNotMutateViewer()
        {
            var service = new DelayedFolderTreeService(
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot()),
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot())
            );
            var dispatcher = new RecordingInlineUiDispatcher();
            var controller = new DisposalRaceController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer(),
                dispatcher,
                false,
                false,
                false
            );
            await controller.Readiness;
            service.SnapshotChangedHandlerCount.Should().Be(1);
            controller.DisposeAfterCommit = true;
            service.RaiseSnapshotChanged();

            controller.CommittedCount.Should().Be(2);
            controller.FolderTreeView.Should().BeNull();
        }

        [TestMethod]
        public async Task FinalCheckStatePutterAssignment_DisposeStopsInitialization()
        {
            var service = new DelayedFolderTreeService(
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot())
            );
            var viewer = new BlockingRecordingViewer();
            viewer.ReleaseSetController();
            viewer.OnSecondFilteredTreeRequest = viewer.DisposeController;
            var controller = new DisposalRaceController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                viewer,
                new RecordingInlineUiDispatcher()
            );

            await controller.Readiness.ConfigureAwait(false);

            controller.FolderTreeView.Should().BeNull();
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        private sealed class BlockingRecordingViewer : IFilterOlFoldersViewer
        {
            private readonly FilterOlFoldersControllerInitializationTests.RecordingFilterViewer _inner =
                new();
            private readonly TaskCompletionSource<bool> _setControllerEntered = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly TaskCompletionSource<bool> _releaseSetController = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private int _filteredTreeRequestCount;
            internal Task SetControllerEntered => _setControllerEntered.Task;
            internal Action? OnSecondFilteredTreeRequest { get; set; }
            public event FormClosedEventHandler FormClosed
            {
                add => _inner.FormClosed += value;
                remove => _inner.FormClosed -= value;
            }
            public BrightIdeasSoftware.TreeListView TlvNotFiltered => _inner.TlvNotFiltered;
            public BrightIdeasSoftware.TreeListView TlvFiltered =>
                ++_filteredTreeRequestCount == 2
                    ? InvokeSecondFilteredTreeRequest()
                    : _inner.TlvFiltered;

            private BrightIdeasSoftware.TreeListView InvokeSecondFilteredTreeRequest()
            {
                OnSecondFilteredTreeRequest?.Invoke();
                return _inner.TlvFiltered;
            }

            public bool InvokeRequired => _inner.InvokeRequired;

            public void SetController(FilterOlFoldersController controller)
            {
                _setControllerEntered.TrySetResult(true);
                _releaseSetController.Task.GetAwaiter().GetResult();
                _inner.SetController(controller);
            }

            public void Show() => _inner.Show();

            public void Close() => _inner.Close();

            public object Invoke(Delegate method) => _inner.Invoke(method);

            public void Dispose() => _inner.Dispose();

            internal void ReleaseSetController() => _releaseSetController.TrySetResult(true);

            internal void DisposeController() => _inner.Controller.Dispose();
        }

        private sealed class SubscriptionBarrierFolderTreeService : IOutlookFolderTreeService
        {
            private readonly Task<FolderTreeSnapshot> _snapshot;
            private readonly TaskCompletionSource<bool> _snapshotChangedStored = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly TaskCompletionSource<bool> _releaseSubscription = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private EventHandler<FolderTreeSnapshotChangedEventArgs>? _snapshotChanged;

            internal SubscriptionBarrierFolderTreeService(Task<FolderTreeSnapshot> snapshot) =>
                _snapshot = snapshot;

            internal Task SnapshotChangedStored => _snapshotChangedStored.Task;

            internal int SnapshotChangedHandlerCount { get; private set; }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs>? SnapshotChanged
            {
                add
                {
                    _snapshotChanged += value;
                    SnapshotChangedHandlerCount++;
                    _snapshotChangedStored.TrySetResult(true);
                    _releaseSubscription.Task.GetAwaiter().GetResult();
                }
                remove
                {
                    _snapshotChanged -= value;
                    SnapshotChangedHandlerCount--;
                }
            }

            public Task<FolderTreeSnapshot> GetSnapshotAsync(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            ) => _snapshot;

            public void MarkStale(string storeId, FolderTreeRefreshReason reason) { }

            public void Dispose() { }

            internal void ReleaseSubscription() => _releaseSubscription.TrySetResult(true);

            internal void RaiseSnapshotChanged() =>
                _snapshotChanged?.Invoke(
                    this,
                    new FolderTreeSnapshotChangedEventArgs(
                        FilterOlFoldersControllerInitializationTests.CreateSnapshot(),
                        FolderTreeRefreshReason.ManualRefresh,
                        null
                    )
                );
        }
    }
}
