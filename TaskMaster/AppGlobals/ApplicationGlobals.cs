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

        public ApplicationGlobals(Application olApp)
        {
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(olApp, this);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
            _events = new AppEvents(this);
            _quickFilerSettings = new AppQuickFilerSettings();
        }

        async public Task LoadAsync()
        {
            logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is beginning.");
            await Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync());
            logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is complete.");
        }

        private AppFileSystemFolderPaths _fs;
        public IFileSystemFolderPaths FS => _fs;
        
        private AppOlObjects _olObjects;
        public IOlObjects Ol => _olObjects;
        
        private AppToDoObjects _toDoObjects;
        public IToDoObjects TD => _toDoObjects;
        
        private AppAutoFileObjects _autoFileObjects;
        public IAppAutoFileObjects AF => _autoFileObjects;
        
        private AppEvents _events;
        public IAppEvents Events => _events;

        private AppQuickFilerSettings _quickFilerSettings;
        public IAppQuickFilerSettings QfSettings => _quickFilerSettings;
        internal AppQuickFilerSettings InternalQfSettings => _quickFilerSettings;

        #region Legacy Definitions and Constants


        #endregion

    }
}