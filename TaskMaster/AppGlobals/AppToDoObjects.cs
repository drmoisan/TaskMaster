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

        async public Task LoadAsync(bool parallel = true)
        {
            if (parallel) { await LoadParallelAsync(); }
            else { await LoadSequentialAsync(); }
        }


        async public Task LoadParallelAsync() 
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
        }
        
        async public Task LoadSequentialAsync() 
        {
            await LoadPrefixAndDictPeopleAsync();
            await LoadDictRemapAsync();
            await LoadIdListAsync();
            await LoadProgramInfoAsync();
            await LoadProjInfoAsync();
            await LoadCategoryFiltersAsync();
            await LoadFilteredFolderScrapingAsync();
            await LoadFolderRemapAsync();
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
            _projInfo = await Task.Run(() => 
            {
                if (_parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
                {
                    return new ProjectData(filename: _defaults.FileName_ProjInfo, folderpath: appData); 
                }
                else { return null; }
            });
            
            if (_projInfo?.Count == 0) 
            {
                await Task.Run(() => _projInfo.Rebuild(Parent.Ol.App));
            }
        }
        private IProjectData LoadProjInfo()
        {

            if (_parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var projectInfo = new ProjectData(filename: _defaults.FileName_ProjInfo,
                                                  folderpath: appData);
                if (projectInfo.Count == 0) { projectInfo.Rebuild(Parent.Ol.App); }
                return projectInfo;
            }
            else { return null; }
        }

        private NewScDictionary<string, string> _programInfo;
        public NewScDictionary<string, string> ProgramInfo => Initialized(_programInfo, LoadProgramInfo);
        private NewScDictionary<string, string> LoadProgramInfo() 
        {
            if (_parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                return NewScDictionary<string, string>.Static.Deserialize(_defaults.FileName_ProgramDictionary, appData); 
            }
            else { return null; }
        }

        async private Task LoadProgramInfoAsync() => _programInfo = await Task.Run(LoadProgramInfo);

        //public ProgramData

        private PeopleScoDictionary _dictPPL;
        public IPeopleScoDictionary DictPPL => Initialized(_dictPPL, () => LoadDictPPL());
        private PeopleScoDictionary LoadDictPPL()
        {
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var dictPPL = new PeopleScoDictionary(filename: _defaults.FilenameDictPpl,
                                                  folderpath: pythonStaging,
                                                  appGlobals: Parent,
                                                  prefix: PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People));

                return dictPPL;
            }
            else { return null; }
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
            if (_parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var idList = new IDList(FnameIDList,
                                    appData,
                                    Parent.Ol.App);
                if (idList.Count == 0) { idList.RefreshIDList(); }
                return idList;

            }
            else { return null;}
        }

        private string _fnameDictRemap;
        public string FnameDictRemap => Initialized(_fnameDictRemap, () => _fnameDictRemap = _defaults.FileName_DictRemap);

        private ScoDictionary<string, string> _dictRemap;
        public IScoDictionary<string, string> DictRemap => Initialized(_dictRemap, () => LoadDictRemap());
        private ScoDictionary<string, string> LoadDictRemap()
        {
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var dictRemap = new ScoDictionary<string, string>(filename: FnameDictRemap,
                                                              folderpath: pythonStaging);
                return dictRemap;

            }
            else { return null;}
        }
        async private Task LoadDictRemapAsync() => _dictRemap = await Task.Run(LoadDictRemap, default);

        //TODO: Convert CategoryFilters to ScoCollection
        private ISerializableList<string> _catFilters;
        public ISerializableList<string> CategoryFilters
        {
            get => Initialized(_catFilters, () =>
            {
                if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    return new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                        folderpath: pythonStaging);
                }
                else { return null;}
            });
            set
            {
                _catFilters = value;
                
                if (_catFilters.Folderpath == "" && _parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    _catFilters.Folderpath = pythonStaging;
                    _catFilters.Filename = _defaults.FileName_CategoryFilters;
                }
                if (_catFilters.Folderpath != "") { _catFilters.Serialize(); }
            }
        }
        async private Task LoadCategoryFiltersAsync()
        {
            
            _catFilters = await Task.Run(() =>
            {
                if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    return new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                       folderpath: pythonStaging);
                }
                else { return null; }
            }, default(CancellationToken));
        }

        // Prefix List
        private ScoCollection<IPrefix> _prefixList;
        public ScoCollection<IPrefix> PrefixList => Initialized(_prefixList, () => LoadPrefixList());
        public ScoCollection<IPrefix> LoadPrefixList()
        {
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var prefixList = new ScoCollection<IPrefix>(fileName: _defaults.FileName_PrefixList,
                                                            folderPath: pythonStaging);

                if (prefixList.Count == 0) 
                { 
                    var tdDefaults = new ToDoDefaults();
                    foreach (var prefix in tdDefaults.PrefixList) { prefixList.Add(prefix); }
                    prefixList.Serialize();
                }
                return prefixList;
            }
            else { return null;}
        }
        async private Task LoadPrefixListAsync()
        {
            _prefixList = await Task.Run(LoadPrefixList);
        }

        private ScoDictionary<string, int> _filteredFolderScraping;
        public ScoDictionary<string, int> FilteredFolderScraping => Initialized(_filteredFolderScraping, () => LoadFilteredFolderScraping());
        public ScoDictionary<string, int> LoadFilteredFolderScraping()
        {
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var filteredFolderScraping = new ScoDictionary<string, int>(_defaults.FileName_FilteredFolderScraping,
                                                                            pythonStaging);
                return filteredFolderScraping;
            }
            else { return null; }
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
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var folderRemap = new ScoDictionary<string, string>(_defaults.FileName_FolderRemap,
                                                                    pythonStaging);
                return folderRemap;
            }
            else { return null; }
        }
        async private Task LoadFolderRemapAsync()
        {
            _folderRemap = await Task.Run(LoadFolderRemap);
        }




    }
}