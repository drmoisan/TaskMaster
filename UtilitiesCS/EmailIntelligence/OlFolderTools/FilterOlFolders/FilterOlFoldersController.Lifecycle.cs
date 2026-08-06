#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public partial class FilterOlFoldersController
    {
        public FilterOlFoldersController(IApplicationGlobals appGlobals)
        {
            InitializeConstruction(appGlobals, CreateAndShowViewer());
        }

        internal FilterOlFoldersController(
            IApplicationGlobals appGlobals,
            IFilterOlFoldersViewer viewer
        )
        {
            InitializeConstruction(appGlobals, viewer);
        }

        internal FilterOlFoldersController(
            IApplicationGlobals appGlobals,
            IFilterOlFoldersViewer viewer,
            IUiDispatcher uiDispatcher
        )
        {
            InitializeConstruction(appGlobals, viewer, uiDispatcher);
        }

        private void InitializeConstruction(
            IApplicationGlobals appGlobals,
            IFilterOlFoldersViewer viewer,
            IUiDispatcher? uiDispatcher = null
        )
        {
            _viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));
            try
            {
                _globals = appGlobals ?? throw new ArgumentNullException(nameof(appGlobals));
                _uiDispatcher = uiDispatcher ?? CreateFolderTreeUiDispatcher();
                _folderTreeService = _globals.Ol.FolderTreeService;
                _viewer.FormClosed += Viewer_FormClosed;
                Readiness = InitializeAsync();
            }
            catch
            {
                Dispose();
                CloseViewerAfterInitializationFailure();
                throw;
            }
        }

        public Task Readiness { get; private set; } = Task.CompletedTask;

        public static async Task<FilterOlFoldersController> CreateAsync(
            IApplicationGlobals appGlobals,
            Func<IFilterOlFoldersViewer>? viewerFactory = null
        ) => await CreateAsync(appGlobals, viewerFactory, null);

        internal static async Task<FilterOlFoldersController> CreateAsync(
            IApplicationGlobals appGlobals,
            Func<IFilterOlFoldersViewer>? viewerFactory,
            IUiDispatcher? uiDispatcher
        )
        {
            var factory = viewerFactory ?? CreateAndShowViewer;
            var viewer = factory();
            var controller = uiDispatcher is null
                ? new FilterOlFoldersController(appGlobals, viewer)
                : new FilterOlFoldersController(appGlobals, viewer, uiDispatcher);
            await controller.Readiness;
            return controller;
        }

        private async Task InitializeAsync()
        {
            try
            {
                await _uiDispatcher.InvokeAsync(async () =>
                {
                    await InitializeOnCapturedDispatcherAsync();
                    return true;
                });
            }
            catch
            {
                Dispose();
                CloseViewerAfterInitializationFailure();
                throw;
            }
        }

        private async Task InitializeOnCapturedDispatcherAsync()
        {
            if (IsDisposed)
            {
                return;
            }

            var snapshot = await GetFolderTreeSnapshotAsync();
            if (snapshot is null || IsDisposed)
            {
                return;
            }

            var candidateView = CreateCompatibilityView(snapshot);
            if (candidateView is null || !TryCommitFolderTreeView(candidateView))
            {
                return;
            }

            if (IsDisposed)
            {
                return;
            }

            _viewer.SetController(this);
            if (IsDisposed)
            {
                return;
            }

            _viewer.TlvNotFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvNotFiltered.CheckStatePutter = PutCheckedStateMethodNotFiltered;
            _viewer.TlvFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvFiltered.CheckStatePutter = PutCheckedStateMethodFiltered;
            if (IsDisposed)
            {
                return;
            }

            TryAttachSnapshotSubscription();
        }

        private void CloseViewerAfterInitializationFailure()
        {
            try
            {
                if (_viewer.InvokeRequired)
                {
                    _viewer.Invoke(new Action(_viewer.Close));
                    return;
                }

                _viewer.Close();
            }
            catch (Exception exception)
            {
                logger.Warn(
                    "Unable to close the folder-filter viewer after initialization failed.",
                    exception
                );
            }
        }

        private static IFilterOlFoldersViewer CreateAndShowViewer()
        {
            var viewer = new FilterOlFoldersViewer();
            try
            {
                viewer.Show();
                return viewer;
            }
            catch
            {
                try
                {
                    viewer.Dispose();
                }
                catch (Exception exception)
                {
                    logger.Warn(
                        "Unable to dispose the folder-filter viewer after showing it failed.",
                        exception
                    );
                }

                throw;
            }
        }

        private readonly object _lifecycleGate = new();
        private IApplicationGlobals _globals = null!;
        private IFilterOlFoldersViewer _viewer = null!;
        private IOutlookFolderTreeService _folderTreeService = null!;
        private IUiDispatcher _uiDispatcher = null!;
        private FolderTreeCompatibilityView? _folderTreeView;
        private int _disposeState;
        private bool _snapshotSubscriptionAttached;

        internal FolderTreeCompatibilityView FolderTreeView => _folderTreeView!;

        private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;

        private void Viewer_FormClosed(object sender, FormClosedEventArgs e) => Dispose();

        private async void FolderTreeService_SnapshotChanged(
            object sender,
            FolderTreeSnapshotChangedEventArgs e
        )
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                await _uiDispatcher.InvokeAsync(async () =>
                {
                    await RefreshFolderTreeViewAsync();
                    return true;
                });
            }
            catch (Exception exception)
            {
                if (!IsDisposed)
                {
                    ObserveFolderTreeRefreshFault(exception);
                }
            }
        }

        private async Task RefreshFolderTreeViewAsync()
        {
            if (IsDisposed)
            {
                return;
            }

            var snapshot = await GetFolderTreeSnapshotAsync();
            if (snapshot is null || IsDisposed)
            {
                return;
            }

            var candidateView = CreateCompatibilityView(snapshot);
            if (candidateView is null || !TryCommitFolderTreeView(candidateView))
            {
                return;
            }

            lock (_lifecycleGate)
            {
                if (IsDisposed)
                {
                    return;
                }

                OlFolderTree_PropertyChanged(
                    this,
                    new PropertyChangedEventArgs(nameof(FolderTreeView))
                );
                if (!IsDisposed)
                {
                    OnFolderTreeRefreshViewApplied();
                }
            }
        }

        internal virtual async Task<FolderTreeSnapshot?> GetFolderTreeSnapshotAsync()
        {
            var request = CreateFolderTreeRequest();
            if (request is null)
            {
                return null;
            }

            return await _folderTreeService.GetSnapshotAsync(request, CancellationToken.None);
        }

        protected internal virtual IUiDispatcher CreateFolderTreeUiDispatcher() =>
            new WpfUiDispatcher();

        protected internal virtual void ObserveFolderTreeRefreshFault(Exception exception)
        {
            logger.Warn(
                "Unable to refresh the folder-filter tree after a snapshot change.",
                exception
            );
        }

        protected internal virtual void OnFolderTreeRefreshViewApplied() { }

        private FolderTreeRequest? CreateFolderTreeRequest()
        {
            var archiveRoot = _globals.Ol.ArchiveRoot;
            if (IsDisposed)
            {
                return null;
            }

            var storeId = archiveRoot?.StoreID;
            if (IsDisposed)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(storeId)
                ? FolderTreeRequest.AllStores(allowStaleSnapshot: true)
                : FolderTreeRequest.ForStore(storeId!, allowStaleSnapshot: true);
        }

        private FolderTreeCompatibilityView? CreateCompatibilityView(FolderTreeSnapshot snapshot)
        {
            var archiveRootSnapshot = CreateArchiveRootSnapshot(snapshot);
            if (archiveRootSnapshot is null || IsDisposed)
            {
                return null;
            }

            var selectedPaths = _globals.TD.FilteredFolderScraping.Keys.ToList();
            if (IsDisposed)
            {
                return null;
            }

            var candidateView = new FolderTreeCompatibilityView(
                archiveRootSnapshot,
                new(selectedPaths)
            );
            if (!IsDisposed)
            {
                return candidateView;
            }

            candidateView.Dispose();
            return null;
        }

        private FolderTreeSnapshot? CreateArchiveRootSnapshot(FolderTreeSnapshot snapshot)
        {
            var archiveRoot = _globals.Ol.ArchiveRoot;
            if (IsDisposed)
            {
                return null;
            }

            var storeId = archiveRoot?.StoreID;
            if (IsDisposed)
            {
                return null;
            }

            var folderPath = archiveRoot?.FolderPath;
            if (IsDisposed)
            {
                return null;
            }

            var archiveNode = archiveRoot is null
                ? null
                : snapshot.FindByPath(storeId!, folderPath!);
            if (archiveNode is null)
            {
                return snapshot;
            }

            return FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, archiveNode);
        }

        private bool TryCommitFolderTreeView(FolderTreeCompatibilityView candidateView)
        {
            lock (_lifecycleGate)
            {
                if (IsDisposed)
                {
                    candidateView.Dispose();
                    return false;
                }

                SetFolderTreeView(candidateView);
                return true;
            }
        }

        private bool TryAttachSnapshotSubscription()
        {
            if (IsDisposed)
            {
                return false;
            }

            _folderTreeService.SnapshotChanged += FolderTreeService_SnapshotChanged;
            var detachSubscription = false;
            lock (_lifecycleGate)
            {
                if (IsDisposed)
                {
                    detachSubscription = true;
                }
                else
                {
                    _snapshotSubscriptionAttached = true;
                }
            }

            if (detachSubscription)
            {
                _folderTreeService.SnapshotChanged -= FolderTreeService_SnapshotChanged;
            }

            return !detachSubscription;
        }

        private void SetFolderTreeView(FolderTreeCompatibilityView view)
        {
            var previousView = _folderTreeView;
            UnsubscribeFolderTreeView(previousView);
            previousView?.Dispose();
            _folderTreeView = view;
            foreach (var node in _folderTreeView.Roots.SelectMany(EnumerateNodes))
            {
                node.Value.PropertyChanged += OlFolderTree_PropertyChanged;
            }
        }

        private void UnsubscribeFolderTreeView(FolderTreeCompatibilityView? view)
        {
            if (view == null)
            {
                return;
            }

            foreach (var node in view.Roots.SelectMany(EnumerateNodes))
            {
                node.Value.PropertyChanged -= OlFolderTree_PropertyChanged;
            }
        }

        private static IEnumerable<TreeNode<FolderWrapper>> EnumerateNodes(
            TreeNode<FolderWrapper> root
        )
        {
            yield return root;
            foreach (var child in root.Children.SelectMany(EnumerateNodes))
            {
                yield return child;
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeState, 1) != 0)
            {
                return;
            }

            if (_viewer != null)
            {
                _viewer.FormClosed -= Viewer_FormClosed;
            }

            var detachSubscription = false;
            FolderTreeCompatibilityView? viewToDispose;
            lock (_lifecycleGate)
            {
                detachSubscription = _snapshotSubscriptionAttached;
                _snapshotSubscriptionAttached = false;
                viewToDispose = _folderTreeView;
                _folderTreeView = null;
            }

            if (detachSubscription)
            {
                _folderTreeService.SnapshotChanged -= FolderTreeService_SnapshotChanged;
            }

            UnsubscribeFolderTreeView(viewToDispose);
            viewToDispose?.Dispose();
        }
    }
}
