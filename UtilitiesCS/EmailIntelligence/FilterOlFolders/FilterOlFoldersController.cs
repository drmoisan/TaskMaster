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
            _olFolderTree = new OlFolderTree(_globals.Ol.EmailRoot,_globals.TD.FilteredFolderScraping);
        }

        private IApplicationGlobals _globals;
                
        private OlFolderTree _olFolderTree;
        private OlFolderTree _olFilterTree;
    }
}
