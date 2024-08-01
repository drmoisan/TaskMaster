using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppToDoObjects : IToDoObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppToDoObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

        async public Task LoadAsync()
        {
            var tasks = new List<Task>
            {
                LoadPrefixAndDictPeopleAsync(),
                LoadDictRemapAsync(),
                LoadIdListAsync(),
                LoadProgramInfoAsync(),
                LoadProjInfoAsync(),
                LoadCategoryFiltersAsync(),
                LoadFilteredFolderScrapingAsync(),
                LoadFolderRemapAsync()
            };
            await Task.WhenAll(tasks);


            //await LoadPrefixAndDictPeopleAsync();
            //await LoadDictRemapAsync();
            //await LoadProgramInfoAsync();
            //await LoadProjInfoAsync();
            //await LoadIdListAsync();
            //await LoadCategoryFiltersAsync();
            //await LoadFilteredFolderScrapingAsync();
            //await LoadFolderRemapAsync();

            //logger.Debug($"{nameof(AppToDoObjects)}.{nameof(LoadAsync)} is complete.");
        }

        private Properties.Settings _defaults = Properties.Settings.Default;
        
        private T Initialized<T>(T obj, Func<T> initializer)
        {
            if (obj is null)
            {
                obj = initializer.Invoke();
            }
            return obj;
        }

        private readonly ApplicationGlobals _parent;
        public IApplicationGlobals Parent
        {
            get
            {
                return _parent;
            }
        }

        private string _projInfo_Filename;
        public string ProjInfo_Filename => Initialized(_projInfo_Filename, () => _projInfo_Filename = _defaults.FileName_ProjInfo);
        private ProjectData _projInfo;
        public IProjectData ProjInfo => Initialized(_projInfo, () => LoadProjInfo());
        async private Task LoadProjInfoAsync()
        {
            _projInfo = await Task.Run(() => new ProjectData(filename: _defaults.FileName_ProjInfo, folderpath: Parent.FS.FldrAppData));
            
            if (_projInfo.Count == 0) 
            {
                await Task.Run(() => _projInfo.Rebuild(Parent.Ol.App));
            }
        }
        private IProjectData LoadProjInfo()
        {
            var projectInfo = new ProjectData(filename: _defaults.FileName_ProjInfo,
                                              folderpath: Parent.FS.FldrAppData);
            if (projectInfo.Count == 0) { projectInfo.Rebuild(Parent.Ol.App); }
            return projectInfo;
        }

        private ScDictionary<string, string> _programInfo;
        public ScDictionary<string, string> ProgramInfo => Initialized(_programInfo, LoadProgramInfo);
        private ScDictionary<string, string> LoadProgramInfo() => ScDictionary<string, string>.Static.Deserialize(_defaults.FileName_ProgramDictionary, Parent.FS.FldrAppData);
        async private Task LoadProgramInfoAsync() => _programInfo = await Task.Run(LoadProgramInfo);

        //public ProgramData

        private PeopleScoDictionary _dictPPL;
        public IPeopleScoDictionary DictPPL => Initialized(_dictPPL, () => LoadDictPPL());
        private PeopleScoDictionary LoadDictPPL()
        {
            var dictPPL = new PeopleScoDictionary(filename: _defaults.FilenameDictPpl,
                                                  folderpath: Parent.FS.FldrPythonStaging,
                                                  appGlobals: Parent,
                                                  prefix: PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People));
            
            return dictPPL;
        }
        async private Task LoadDictPPLAsync() => _dictPPL = await Task.Run(LoadDictPPL);
        async private Task LoadPrefixAndDictPeopleAsync()
        {
            await LoadPrefixListAsync();
            await LoadDictPPLAsync();
        }

        public string FnameIDList => _defaults.FileName_IDList;
        
        private IIDList _idList;
        //TODO: Convert IDList to ScoCollection
        public IIDList IDList => Initialized(_idList, () => LoadIDList());
        async private Task LoadIdListAsync() => _idList = await Task.Run(() => LoadIDList());
        
        private IIDList LoadIDList()
        {
            var idList = new IDList(FnameIDList,
                                    Parent.FS.FldrAppData,
                                    Parent.Ol.App);
            if (idList.Count == 0) { idList.RefreshIDList(); }
            return idList;
        }

        private string _fnameDictRemap;
        public string FnameDictRemap => Initialized(_fnameDictRemap, () => _fnameDictRemap = _defaults.FileName_DictRemap);

        private ScoDictionary<string, string> _dictRemap;
        public IScoDictionary<string, string> DictRemap => Initialized(_dictRemap, () => LoadDictRemap());
        private ScoDictionary<string, string> LoadDictRemap()
        {
            var dictRemap = new ScoDictionary<string, string>(filename: FnameDictRemap,
                                                              folderpath: Parent.FS.FldrPythonStaging);
            return dictRemap;
        }
        async private Task LoadDictRemapAsync() => _dictRemap = await Task.Run(LoadDictRemap, default);

        //TODO: Convert CategoryFilters to ScoCollection
        private ISerializableList<string> _catFilters;
        public ISerializableList<string> CategoryFilters
        {
            get => Initialized(_catFilters, () => new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                                               folderpath: _parent.FS.FldrPythonStaging));
            set
            {
                _catFilters = value;
                if (_catFilters.Folderpath == "")
                {
                    _catFilters.Folderpath = _parent.FS.FldrPythonStaging;
                    _catFilters.Filename = _defaults.FileName_CategoryFilters;
                }
                _catFilters.Serialize();
            }
        }
        async private Task LoadCategoryFiltersAsync()
        {
            _catFilters = await Task.Factory.StartNew(
                () => new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                   folderpath: _parent.FS.FldrPythonStaging),
                default(CancellationToken));
        }

        // Prefix List
        private ScoCollection<IPrefix> _prefixList;
        public ScoCollection<IPrefix> PrefixList => Initialized(_prefixList, () => LoadPrefixList());
        public ScoCollection<IPrefix> LoadPrefixList()
        {
            var prefixList = new ScoCollection<IPrefix>(fileName: _defaults.FileName_PrefixList,
                                                        folderPath: Parent.FS.FldrPythonStaging);


            if (prefixList.Count == 0) 
            { 
                var tdDefaults = new ToDoDefaults();
                foreach (var prefix in tdDefaults.PrefixList) { prefixList.Add(prefix); }
                prefixList.Serialize();
            }
            return prefixList;
        }
        async private Task LoadPrefixListAsync()
        {
            _prefixList = await Task.Run(LoadPrefixList);
        }

        private ScoDictionary<string, int> _filteredFolderScraping;
        public ScoDictionary<string, int> FilteredFolderScraping => Initialized(_filteredFolderScraping, () => LoadFilteredFolderScraping());
        public ScoDictionary<string, int> LoadFilteredFolderScraping()
        {
            var filteredFolderScraping = new ScoDictionary<string, int>(_defaults.FileName_FilteredFolderScraping,
                                                                        Parent.FS.FldrPythonStaging);
            return filteredFolderScraping;
        }
        async private Task LoadFilteredFolderScrapingAsync()
        {
            _filteredFolderScraping = await Task.Factory.StartNew(
                                      () => LoadFilteredFolderScraping(),
                                      default(CancellationToken));
        }

        private ScoDictionary<string, string> _folderRemap;
        public ScoDictionary<string, string> FolderRemap => Initializer.GetOrLoad(ref _folderRemap, () => LoadFolderRemap());
        public ScoDictionary<string, string> LoadFolderRemap()
        {
            var folderRemap = new ScoDictionary<string, string>(_defaults.FileName_FolderRemap,
                                                                Parent.FS.FldrPythonStaging);
            return folderRemap;
        }
        async private Task LoadFolderRemapAsync()
        {
            _folderRemap = await Task.Run(LoadFolderRemap);
        }




    }
}