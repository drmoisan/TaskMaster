using Microsoft.Office.Interop.Outlook;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{

    public class ApplicationGlobals : IApplicationGlobals
    {

        private AppFileSystemFolderPaths _fs;
        private AppOlObjects _olObjects;
        private AppToDoObjects _toDoObjects;
        private AppAutoFileObjects _autoFileObjects;

        public ApplicationGlobals(Application OlApp)
        {
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(OlApp);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
        }

        async public Task LoadAsync()
        {
            Debug.WriteLine($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is beginning.");
            await Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync());
            Debug.WriteLine($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is complete.");
        }

        public IFileSystemFolderPaths FS
        {
            get
            {
                return _fs;
            }
        }

        public IOlObjects Ol
        {
            get
            {
                return _olObjects;
            }
        }

        public IToDoObjects TD
        {
            get
            {
                return _toDoObjects;
            }
        }

        public IAppAutoFileObjects AF
        {
            get
            {
                return _autoFileObjects;
            }

        }

        #region Legacy Definitions and Constants


        #endregion

    }
}