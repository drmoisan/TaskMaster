using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS
{
    public class FilterOlFoldersController : IDisposable
    {
        public FilterOlFoldersController(IApplicationGlobals appGlobals)
            : this(appGlobals, CreateAndShowViewer()) { }

        /// <summary>
        /// Initializes a new instance of <see cref="FilterOlFoldersController"/> with an
        /// injected viewer. Intended for unit testing: pass a
        /// <see cref="IFilterOlFoldersViewer"/> mock so that no real window is opened.
        /// </summary>
        /// <param name="appGlobals">Application globals.</param>
        /// <param name="viewer">Viewer to use. Must not be null.</param>
        internal FilterOlFoldersController(
            IApplicationGlobals appGlobals,
            IFilterOlFoldersViewer viewer
        )
        {
            _globals = appGlobals;
            _viewer = viewer;
            _folderTreeService = _globals.Ol.FolderTreeService;
            _folderTreeService.SnapshotChanged += FolderTreeService_SnapshotChanged;
            _viewer.FormClosed += Viewer_FormClosed;
            SetFolderTreeView(
                CreateCompatibilityView(GetFolderTreeSnapshotAsync().GetAwaiter().GetResult())
            );
            _viewer.SetController(this);
            //PutCheckedState = PutCheckedStateMethod;
            _viewer.TlvNotFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvNotFiltered.CheckStatePutter = PutCheckedStateMethodNotFiltered;
            _viewer.TlvFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvFiltered.CheckStatePutter = PutCheckedStateMethodFiltered;
        }

        private static IFilterOlFoldersViewer CreateAndShowViewer()
        {
            var viewer = new FilterOlFoldersViewer();
            viewer.Show();
            return viewer;
        }

        private IApplicationGlobals _globals;
        private IFilterOlFoldersViewer _viewer;
        private IOutlookFolderTreeService _folderTreeService;
        private FolderTreeCompatibilityView _folderTreeView;
        private bool _disposed;

        internal FolderTreeCompatibilityView FolderTreeView => _folderTreeView;

        #region Event Handlers

        internal void Discard() => _viewer.Close();

        internal void Save()
        {
            _viewer.Close();

            var selected = _folderTreeView
                .Roots.SelectMany(x => x.FlattenIf(info => info.Selected))
                .Select(info => info.RelativePath);

            // remove any keys that are no longer selected
            _globals
                .TD.FilteredFolderScraping.Keys.Where(x => !selected.Contains(x))
                .ForEach(x => _globals.TD.FilteredFolderScraping.Remove(x));

            // add any new keys that are selected
            selected.ForEach(x => _globals.TD.FilteredFolderScraping.TryAdd(x, 1));

            // save the settings
            _globals.TD.FilteredFolderScraping.Serialize();
        }

        public void OlFolderTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_viewer.InvokeRequired)
            {
                _viewer.Invoke(new Action(() => OlFolderTree_PropertyChangedInternal(sender, e)));
            }
            else
            {
                OlFolderTree_PropertyChangedInternal(sender, e);
            }
        }

        private void Viewer_FormClosed(object sender, FormClosedEventArgs e) => Dispose();

        private void FolderTreeService_SnapshotChanged(
            object sender,
            FolderTreeSnapshotChangedEventArgs e
        )
        {
            _ = RefreshFolderTreeViewAsync();
        }

        private async Task RefreshFolderTreeViewAsync()
        {
            var snapshot = await GetFolderTreeSnapshotAsync().ConfigureAwait(false);
            SetFolderTreeView(CreateCompatibilityView(snapshot));
            OlFolderTree_PropertyChanged(
                this,
                new PropertyChangedEventArgs(nameof(FolderTreeView))
            );
        }

        internal void OlFolderTree_PropertyChangedInternal(
            object sender,
            PropertyChangedEventArgs e
        )
        {
            var expanded = (
                _viewer
                    .TlvNotFiltered.ExpandedObjects.Cast<TreeNode<FolderWrapper>>()
                    .Concat(_viewer.TlvFiltered.ExpandedObjects.Cast<TreeNode<FolderWrapper>>())
            )
                .Select(x => x.Value.RelativePath)
                .ToArray();

            var notFiltered = FilterSelected(false);
            _viewer.TlvNotFiltered.Roots = notFiltered;

            var nfExpanded = notFiltered
                .SelectMany(x => x.FindAll(x => expanded.Contains(x.Value.RelativePath)))
                .ToList();
            _viewer.TlvNotFiltered.ExpandedObjects = nfExpanded;
            _viewer.TlvNotFiltered.RebuildAll(true);
            _viewer.TlvNotFiltered.Refresh();

            var filtered = FilterSelected(true);
            _viewer.TlvFiltered.Roots = filtered;
            var filteredExpanded = filtered
                .SelectMany(x => x.FindAll(x => expanded.Contains(x.Value.RelativePath)))
                .ToList();
            _viewer.TlvFiltered.ExpandedObjects = filteredExpanded;
            _viewer.TlvFiltered.RebuildAll(true);
            _viewer.TlvFiltered.Refresh();
        }

        internal CheckStateGetterDelegate GetCheckedState = delegate(object rowObject)
        {
            var node = (TreeNode<FolderWrapper>)rowObject;
            if (node.Value.Selected)
                return CheckState.Checked;
            else if (node.Flatten().Any(x => x.Selected))
                return CheckState.Indeterminate;
            else
                return CheckState.Unchecked;
        };

        //internal CheckStatePutterDelegate PutCheckedState = delegate (object rowObject, CheckState newValue)
        //{
        //    var node = (TreeNode<OlFolderWrapper>)rowObject;
        //    if (newValue == CheckState.Checked)
        //    {
        //        node.Traverse(x => x.Value.Selected = true);
        //        //node.Value.Selected = true;
        //        return CheckState.Checked;
        //    }
        //    else
        //    {
        //        node.Traverse(x => x.Value.Selected = false);
        //        //node.Value.Selected = false;
        //        return CheckState.Unchecked;
        //    }
        //};

        internal CheckStatePutterDelegate PutCheckedState;

        internal CheckState PutCheckedStateMethodFiltered(object rowObject, CheckState newValue) =>
            PutCheckedStateMethod(rowObject, newValue, _viewer.TlvFiltered);

        internal CheckState PutCheckedStateMethodNotFiltered(
            object rowObject,
            CheckState newValue
        ) => PutCheckedStateMethod(rowObject, newValue, _viewer.TlvNotFiltered);

        internal CheckState PutCheckedStateMethod(
            object rowObject,
            CheckState newValue,
            TreeListView tree
        )
        {
            var node = (TreeNode<FolderWrapper>)rowObject;

            if (!tree.IsExpanded(node))
            {
                node.Traverse(x => x.Value.Selected = (newValue == CheckState.Checked));
                //node.Value.Selected = true;
                return newValue;
            }
            else
            {
                node.Value.Selected = (newValue == CheckState.Checked);
                return newValue;
            }
        }

        #endregion Event Handlers

        internal IReadOnlyList<TreeNode<FolderWrapper>> FilterSelected(bool include)
        {
            var selected = new List<TreeNode<FolderWrapper>>();
            if (_folderTreeView == null)
            {
                return selected;
            }

            foreach (var root in _folderTreeView.Roots)
            {
                FilterChildren(root, selected, include);
            }

            return selected;
        }

        internal virtual Task<FolderTreeSnapshot> GetFolderTreeSnapshotAsync()
        {
            return _folderTreeService.GetSnapshotAsync(
                CreateFolderTreeRequest(),
                CancellationToken.None
            );
        }

        private FolderTreeRequest CreateFolderTreeRequest()
        {
            var storeId = _globals.Ol.ArchiveRoot?.StoreID;
            return string.IsNullOrWhiteSpace(storeId)
                ? FolderTreeRequest.AllStores(allowStaleSnapshot: true)
                : FolderTreeRequest.ForStore(storeId, allowStaleSnapshot: true);
        }

        private FolderTreeCompatibilityView CreateCompatibilityView(FolderTreeSnapshot snapshot)
        {
            var selectedPaths = _globals.TD.FilteredFolderScraping.Keys.ToList();
            return new(CreateArchiveRootSnapshot(snapshot), new(selectedPaths));
        }

        private FolderTreeSnapshot CreateArchiveRootSnapshot(FolderTreeSnapshot snapshot)
        {
            var archiveRoot = _globals.Ol.ArchiveRoot;
            var archiveNode = archiveRoot is null
                ? null
                : snapshot.FindByPath(archiveRoot.StoreID, archiveRoot.FolderPath);
            if (archiveNode is null)
            {
                return snapshot;
            }

            return FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, archiveNode);
        }

        private void SetFolderTreeView(FolderTreeCompatibilityView view)
        {
            UnsubscribeFolderTreeView();
            _folderTreeView?.Dispose();
            _folderTreeView = view;
            foreach (var node in _folderTreeView.Roots.SelectMany(EnumerateNodes))
            {
                node.Value.PropertyChanged += OlFolderTree_PropertyChanged;
            }
        }

        private void UnsubscribeFolderTreeView()
        {
            if (_folderTreeView == null)
            {
                return;
            }

            foreach (var node in _folderTreeView.Roots.SelectMany(EnumerateNodes))
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

        private static void FilterChildren(
            TreeNode<FolderWrapper> source,
            ICollection<TreeNode<FolderWrapper>> destination,
            bool include
        )
        {
            if (source.Value.Selected == include)
            {
                var destinationChild = new TreeNode<FolderWrapper>(source.Value);
                destination.Add(destinationChild);
                foreach (var sourceChild in source.Children)
                {
                    FilterChildren(sourceChild, destinationChild.Children, include);
                }
                return;
            }

            foreach (var sourceChild in source.Children)
            {
                FilterChildren(sourceChild, destination, include);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_viewer != null)
            {
                _viewer.FormClosed -= Viewer_FormClosed;
            }

            if (_folderTreeService != null)
            {
                _folderTreeService.SnapshotChanged -= FolderTreeService_SnapshotChanged;
            }

            UnsubscribeFolderTreeView();
            _folderTreeView?.Dispose();
            _disposed = true;
        }
    }
}
