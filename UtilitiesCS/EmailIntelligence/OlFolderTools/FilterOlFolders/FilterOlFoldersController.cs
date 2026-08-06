#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public partial class FilterOlFoldersController : IDisposable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!
        );

        #region Event Handlers

        internal void Discard() => _viewer.Close();

        internal void Save()
        {
            _viewer.Close();

            var selected = _folderTreeView!
                .Roots.SelectMany(x => x.FlattenIf(info => info.Selected))
                .Select(info => info.RelativePath!);

            // remove any keys that are no longer selected
            _globals
                .TD.FilteredFolderScraping.Keys.Where(x => !selected.Contains(x))
                .ForEach(x => _globals.TD.FilteredFolderScraping.TryRemove(x, out _));

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

        internal CheckStatePutterDelegate PutCheckedState = null!;

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
    }
}
