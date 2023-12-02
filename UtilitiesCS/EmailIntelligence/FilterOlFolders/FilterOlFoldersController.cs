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
            _olFolderTree = new OlFolderTree(_globals.Ol.ArchiveRoot,_globals.TD.FilteredFolderScraping);
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
            throw new NotImplementedException();
        }

        public void OlFolderTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UIThreadExtensions.UiDispatcher.Invoke(() =>
            {
                _viewer.TlvNotFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == false);
                _viewer.TlvFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == true);
            });
        }

        #endregion Event Handlers

    }
}
