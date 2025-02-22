using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace UtilitiesCS
{
    public class FilterOlFoldersController
    {
        public FilterOlFoldersController(IApplicationGlobals appGlobals) 
        { 
            _globals = appGlobals;            
            _olFolderTree = new FolderTree(_globals.Ol.ArchiveRoot,_globals.TD.FilteredFolderScraping.Keys.ToList());
            _olFolderTree.PropertyChanged += OlFolderTree_PropertyChanged;
            _viewer = new FilterOlFoldersViewer();
            _viewer.SetController(this);
            //PutCheckedState = PutCheckedStateMethod;
            _viewer.TlvNotFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvNotFiltered.CheckStatePutter = PutCheckedStateMethodNotFiltered;
            _viewer.TlvFiltered.CheckStateGetter = GetCheckedState;
            _viewer.TlvFiltered.CheckStatePutter = PutCheckedStateMethodFiltered;

            _viewer.Show();
        }

        private IApplicationGlobals _globals;       
        private FilterOlFoldersViewer _viewer;

        private FolderTree _olFolderTree;
        public FolderTree OlFolderTree { get => _olFolderTree; }

        #region Event Handlers

        internal void Discard() => _viewer.Close();

        internal void Save()
        {
            _viewer.Close();

            var selected = OlFolderTree.Roots
                .SelectMany(x => x.FlattenIf(info => info.Selected))
                .Select(info => info.RelativePath);

            // remove any keys that are no longer selected
            _globals.TD.FilteredFolderScraping.Keys.Where(x => !selected.Contains(x))
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

        internal void OlFolderTree_PropertyChangedInternal(object sender, PropertyChangedEventArgs e)
        {
            var expanded = (_viewer.TlvNotFiltered.ExpandedObjects.Cast<TreeNode<FolderWrapper>>()
                .Concat(_viewer.TlvFiltered.ExpandedObjects.Cast<TreeNode<FolderWrapper>>()))
                .Select(x => x.Value.RelativePath).ToArray();

            var notFiltered = OlFolderTree.FilterSelected(false);
            _viewer.TlvNotFiltered.Roots = notFiltered;

            var nfExpanded = notFiltered.SelectMany(x => x.FindAll(x => expanded.Contains(x.Value.RelativePath))).ToList();
            _viewer.TlvNotFiltered.ExpandedObjects = nfExpanded;
            _viewer.TlvNotFiltered.RebuildAll(true);
            _viewer.TlvNotFiltered.Refresh();

            var filtered = OlFolderTree.FilterSelected(true);
            _viewer.TlvFiltered.Roots = filtered;
            var filteredExpanded = filtered.SelectMany(x => x.FindAll(x => expanded.Contains(x.Value.RelativePath))).ToList();
            _viewer.TlvFiltered.ExpandedObjects = filteredExpanded;
            _viewer.TlvFiltered.RebuildAll(true);
            _viewer.TlvFiltered.Refresh();
        }

        internal CheckStateGetterDelegate GetCheckedState = delegate (object rowObject)
        {
            var node = (TreeNode<FolderWrapper>)rowObject;
            if (node.Value.Selected)
                return CheckState.Checked;
            else
                if (node.Flatten().Any(x => x.Selected))
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

        internal CheckState PutCheckedStateMethodFiltered(object rowObject, CheckState newValue) => PutCheckedStateMethod(rowObject, newValue, _viewer.TlvFiltered);

        internal CheckState PutCheckedStateMethodNotFiltered(object rowObject, CheckState newValue) => PutCheckedStateMethod(rowObject, newValue, _viewer.TlvNotFiltered);

        internal CheckState PutCheckedStateMethod(object rowObject, CheckState newValue, TreeListView tree)
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

    }
}
