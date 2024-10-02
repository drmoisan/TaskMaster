using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

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
            Engines = new AppItemEngines(this);
        }

        async public Task LoadAsync()
        {
            //logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is beginning.");
            //IdleAsyncQueue.AddEntry(false, () => Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync()));
            await Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync());
            await Engines.InitAsync();
            //IdleAsyncQueue.AddEntry(false, Engines.InitAsync);
            await _events.LoadAsync();
            //IdleAsyncQueue.AddEntry(false, _events.LoadAsync);
            //logger.Debug($"{nameof(ApplicationGlobals)}.{nameof(LoadAsync)} is complete.");
            //await Task.CompletedTask;
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
        
        public IAppItemEngines Engines { get; private set; } 

        public List<Type> GetClasses()
            {
                return ReflectionHelper.GetAllClassesInSolution();
            }
        
        public string[] GetProjectNames()
        {
            //ProjectCollection.GlobalProjectCollection.LoadedProjects
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetName().Name)
                .ToArray();
        }

        #region Legacy Definitions and Constants


        #endregion

    }
}