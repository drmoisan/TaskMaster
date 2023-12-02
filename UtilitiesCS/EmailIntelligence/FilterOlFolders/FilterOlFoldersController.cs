using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FilterOlFoldersController
    {
        public FilterOlFoldersController(IApplicationGlobals appGlobals) 
        { 
            _globals = appGlobals;
            _olFolderTree = new OlFolderTree(_globals.Ol.ArchiveRoot,_globals.TD.FilteredFolderScraping.Keys.ToList());
            _olFolderTree.PropertyChanged += OlFolderTree_PropertyChanged;
            _viewer = new FilterOlFoldersViewer();
            _viewer.SetController(this);
            _viewer.Show();
        }

        private IApplicationGlobals _globals;       
        private FilterOlFoldersViewer _viewer;

        private OlFolderTree _olFolderTree;
        public OlFolderTree OlFolderTree { get => _olFolderTree; }

        #region Event Handlers

        internal void Discard() => _viewer.Close();

        internal void Save()
        {
            var selected = OlFolderTree.Roots
                .SelectMany(x => x.FlattenIf(info => info.Selected))
                .Select(info => info.RelativePath);
                //.Select(x => _globals.TD.FilteredFolderScraping.TryAdd(x, 1));
            _globals.TD.FilteredFolderScraping.Keys.Where(x => !selected.Contains(x))
                .ForEach(x => _globals.TD.FilteredFolderScraping.Remove(x));

            selected.ForEach(x => _globals.TD.FilteredFolderScraping.TryAdd(x, 1));
            _globals.TD.FilteredFolderScraping.Serialize();
        }

        public void OlFolderTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UIThreadExtensions.UiDispatcher.Invoke(() =>
            {
                //_viewer.TlvNotFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == false);
                //_viewer.TlvFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == true);
                var expanded = (_viewer.TlvNotFiltered.ExpandedObjects.Cast<TreeNode<OlFolderInfo>>()
                    .Concat(_viewer.TlvFiltered.ExpandedObjects.Cast<TreeNode<OlFolderInfo>>()))
                    .Select(x=>x.Value.RelativePath).ToArray();

                var notFiltered = OlFolderTree.FilterSelected(false);
                _viewer.TlvNotFiltered.Roots = notFiltered;
                
                //var nfExpanded = notFiltered.SelectMany(x => x.Flatten()).Where(x => expanded.Contains(x.RelativePath)).ToList();
                var nfExpanded = notFiltered.SelectMany(x => x.FindAll(x=>expanded.Contains(x.Value.RelativePath))).ToList();
                _viewer.TlvNotFiltered.ExpandedObjects = nfExpanded;
                _viewer.TlvNotFiltered.RebuildAll(true);
                _viewer.TlvNotFiltered.Refresh();
                
                var filtered = OlFolderTree.FilterSelected(true);
                _viewer.TlvFiltered.Roots = filtered;
                var filteredExpanded = filtered.SelectMany(x => x.FindAll(x => expanded.Contains(x.Value.RelativePath))).ToList();
                _viewer.TlvFiltered.ExpandedObjects = filteredExpanded;
                _viewer.TlvFiltered.RebuildAll(true);
                _viewer.TlvFiltered.Refresh();
            });
        }

        #endregion Event Handlers

    }
}
