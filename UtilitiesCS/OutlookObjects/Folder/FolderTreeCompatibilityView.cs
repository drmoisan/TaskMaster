using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Projects immutable folder snapshots into legacy TreeNode{FolderWrapper} views.
    /// </summary>
    public sealed class FolderTreeCompatibilityView : IDisposable
    {
        private readonly List<FolderWrapper> _trackedWrappers = new List<FolderWrapper>();
        private bool _disposed;

        public FolderTreeCompatibilityView(
            FolderTreeSnapshot snapshot,
            FolderTreeSelectionOverlay selectionOverlay
        )
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            SelectionOverlay =
                selectionOverlay ?? new FolderTreeSelectionOverlay(Array.Empty<string>());
            Roots = Snapshot.RootKeys.Select(CreateNode).Where(node => node != null).ToList();
        }

        public FolderTreeSnapshot Snapshot { get; }

        public FolderTreeSelectionOverlay SelectionOverlay { get; }

        public IReadOnlyList<TreeNode<FolderWrapper>> Roots { get; }

        internal int SubscriptionCount { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var wrapper in _trackedWrappers)
            {
                wrapper.PropertyChanged -= WrapperPropertyChanged;
                SubscriptionCount--;
            }

            _disposed = true;
        }

        private TreeNode<FolderWrapper> CreateNode(FolderTreeNodeKey key)
        {
            if (!Snapshot.TryGetNode(key, out var snapshotNode))
            {
                return null;
            }

            var wrapper = new FolderWrapper(
                SelectionOverlay.IsSelected(snapshotNode),
                0,
                0,
                snapshotNode.DisplayName,
                snapshotNode.RelativePath
            );
            wrapper.PropertyChanged += WrapperPropertyChanged;
            _trackedWrappers.Add(wrapper);
            SubscriptionCount++;
            var treeNode = new TreeNode<FolderWrapper>(wrapper);
            foreach (
                var child in snapshotNode.ChildKeys.Select(CreateNode).Where(node => node != null)
            )
            {
                treeNode.AddChild(child);
            }

            return treeNode;
        }

        private static void WrapperPropertyChanged(object sender, PropertyChangedEventArgs e) { }
    }
}
