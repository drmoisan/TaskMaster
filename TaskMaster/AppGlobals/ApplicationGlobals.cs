using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class ApplicationGlobals : IApplicationGlobals
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Application _outlookApp;

        public ApplicationGlobals(Application olApp)
        {
            _outlookApp = olApp;
            BasicLoaded = new Lazy<bool>(() => { LoadBasicMethod(); return true; });
        }

        public ApplicationGlobals(Application olApp, bool loadBasic)
        {
            _outlookApp = olApp;
            BasicLoaded = new Lazy<bool>(() => { LoadBasicMethod(); return true; });
            if (loadBasic) { ForceBasicLoad(); }
        }

        async public Task LoadAsync(bool parallel = true)
        {
            ForceBasicLoad();
            if (parallel) { await LoadParallelAsync(); }
            else { await LoadSequentialAsync(); }
        }

        internal Lazy<bool> BasicLoaded;

        private void ForceBasicLoad() 
        { 
            _ = BasicLoaded.Value;
        }

        private void LoadBasicMethod()
        {
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(_outlookApp, this);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
            _events = new AppEvents(this);
            _quickFilerSettings = new AppQuickFilerSettings();
            Engines = new AppItemEngines(this);
        }

        async public Task LoadParallelAsync()
        {
            await LoadIntelConfigAsync();
            await Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync(), _olObjects.LoadAsync());
            await Engines.InitAsync();
            await _events.LoadAsync();
        }

        async public Task LoadSequentialAsync() 
        {
            await LoadIntelConfigAsync();
            await _olObjects.LoadAsync();
            await _toDoObjects.LoadAsync(false);
            await _autoFileObjects.LoadAsync(false);
            await Engines.InitAsync();
            await _events.LoadAsync();
        }

        public void LoadWhenIdle()
        {
            IdleAsyncQueue.AddEntry(false, () => Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync()));
            IdleAsyncQueue.AddEntry(false, Engines.InitAsync);
            IdleAsyncQueue.AddEntry(false, _events.LoadAsync);
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

        public IntelligenceConfig IntelRes { get; private set; }
        async private Task LoadIntelConfigAsync() => await Task.Run(async () => IntelRes = await IntelligenceConfig.LoadAsync(this), default);


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