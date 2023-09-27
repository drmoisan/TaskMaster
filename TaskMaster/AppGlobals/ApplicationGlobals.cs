using Microsoft.Office.Interop.Outlook;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{

    public class ApplicationGlobals : IApplicationGlobals
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AppFileSystemFolderPaths _fs;
        private AppOlObjects _olObjects;
        private AppToDoObjects _toDoObjects;
        private AppAutoFileObjects _autoFileObjects;
        private AppEvents _events;

        public ApplicationGlobals(Application OlApp)
        {
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(OlApp);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
            _events = new AppEvents(this);
        }

        async public Task LoadAsync()
        {
            logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is beginning.");
            await Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync());
            logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is complete.");
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

        public IAppEvents Events => _events;
        

        #region Legacy Definitions and Constants


        #endregion

    }
}