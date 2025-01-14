using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Newtonsoft.Json;
using ToDoModel;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppToDoObjects(IApplicationGlobals parentInstance) : IToDoObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        async public Task LoadAsync(bool parallel = true)
        {
            if (parallel) { await LoadParallelAsync(); }
            else { await LoadSequentialAsync(); }
        }


        async public Task LoadParallelAsync() 
        {
            var tasks = new List<Task>
            {
                LoadPrefixListAsync(),                
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
            await LoadPrefixListAsync();
            await LoadPeopleAsync();
            await LoadDictRemapAsync();
            await LoadIdListAsync();
            await LoadProgramInfoAsync();
            await LoadProjInfoAsync();
            await LoadCategoryFiltersAsync();
            await LoadFilteredFolderScrapingAsync();
            await LoadFolderRemapAsync();
        }

        private readonly Properties.Settings _defaults = Properties.Settings.Default;
        
        private T Initialized<T>(T obj, Func<T> initializer)
        {
            obj ??= initializer.Invoke();
            return obj;
        }

        public IApplicationGlobals Parent { get; protected set; } = parentInstance;
        internal ISmartSerializableNonTyped SmartSerializable { get; set; } = new SmartSerializableNonTyped();

        private string _projInfo_Filename;
        public string ProjInfo_Filename => Initialized(_projInfo_Filename, () => _projInfo_Filename = _defaults.FileName_ProjInfo);
        private ProjectData _projInfo;
        public IProjectData ProjInfo => Initialized(_projInfo, () => LoadProjInfo());
        async private Task LoadProjInfoAsync()
        {
            _projInfo = await Task.Run(() => 
            {
                if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
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

            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var projectInfo = new ProjectData(filename: _defaults.FileName_ProjInfo,
                                                  folderpath: appData);
                if (projectInfo.Count == 0) { projectInfo.Rebuild(Parent.Ol.App); }
                return projectInfo;
            }
            else { return null; }
        }

        private ScDictionary<string, string> _programInfo;
        public ScDictionary<string, string> ProgramInfo => Initialized(_programInfo, LoadProgramInfo);
        private ScDictionary<string, string> LoadProgramInfo() 
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                return ScDictionary<string, string>.Static.Deserialize(_defaults.FileName_ProgramDictionary, appData); 
            }
            else { return null; }
        }

        async private Task LoadProgramInfoAsync() => _programInfo = await Task.Run(LoadProgramInfo);

        //public ProgramData

        async internal Task LoadPeopleAsync() => await Task.Run(async () =>
        {
            if (Parent.IntelRes.Config.TryGetValue("People", out var config))
            {                
                People = await SmartSerializable.DeserializeAsync(config, true, () => new PeopleScoDictionaryNew(Parent));
                People.CollectionChanged += People_CollectionChanged;
            }
            else { logger.Error("People config not found."); }
        }, Parent.AF.CancelToken);

        public IPeopleScoDictionaryNew People {  get; private set; }
        public void People_CollectionChanged(object Sender, DictionaryChangedEventArgs<string, string> args)
        {
            var dict = (PeopleScoDictionaryNew)Sender;
            dict.Serialize();
        }


        //private PeopleScoDictionary _dictPPL;
        //public IPeopleScoDictionary DictPPL => Initialized(_dictPPL, () => LoadDictPPL());
        //private PeopleScoDictionary LoadDictPPL()
        //{
        //    if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
        //    {
        //        var dictPPL = new PeopleScoDictionary(filename: _defaults.FilenameDictPpl,
        //                                          folderpath: pythonStaging,
        //                                          appGlobals: Parent,
        //                                          prefix: PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People));

        //        return dictPPL;
        //    }
        //    else { return null; }
        //}
        //async private Task LoadDictPPLAsync() => _dictPPL = await Task.Run(LoadDictPPL);
        //async private Task LoadPrefixAndDictPeopleAsync()
        //{
        //    await LoadPrefixListAsync();
        //    await LoadDictPPLAsync();
        //}

        public string FnameIDList => _defaults.FileName_IDList;
        
        private IIDList _idList;
        //TODO: Convert IDList to ScoCollection
        public IIDList IDList => Initialized(_idList, () => LoadIDList());
        async private Task LoadIdListAsync() => _idList = await Task.Run(() => LoadIDList());
        
        private IIDList LoadIDList()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
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
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
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
                if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    return new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                        folderpath: pythonStaging);
                }
                else { return null;}
            });
            set
            {
                _catFilters = value;
                
                if (_catFilters.Folderpath == "" && Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
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
                if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    return new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                       folderpath: pythonStaging);
                }
                else { return null; }
            }, default);
        }

        // Prefix List
        private ScoCollection<IPrefix> _prefixList;
        public ScoCollection<IPrefix> PrefixList => Initialized(_prefixList, () => LoadPrefixList());
        public ScoCollection<IPrefix> LoadPrefixList()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
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
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
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
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
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