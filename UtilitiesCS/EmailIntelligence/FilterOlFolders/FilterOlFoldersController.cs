using System;
using System.Collections.Generic;
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


        #endregion Event Handlers

    }
}
