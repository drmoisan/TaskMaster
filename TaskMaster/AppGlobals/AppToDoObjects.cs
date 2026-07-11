using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Tags;
using TaskVisualization;
using ToDoModel;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Interfaces;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using UtilitiesCS.Threading;

namespace TaskMaster
{
    public class AppToDoObjects(IApplicationGlobals parentInstance) : IToDoObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public async Task LoadAsync(bool parallel = true)
        {
            if (parallel)
            {
                await LoadParallelAsync();
            }
            else
            {
                await LoadSequentialAsync();
            }
        }

        public async Task LoadParallelAsync()
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
                LoadFolderRemapAsync(),
                LoadFlagChangeTrainingQueueAsync(),
                LoadSelectFromListAsync(),
            };
            await Task.WhenAll(tasks);
        }

        public async Task LoadSequentialAsync()
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
            await LoadFlagChangeTrainingQueueAsync();
            await LoadSelectFromListAsync();
        }

        private readonly Properties.Settings _defaults = Properties.Settings.Default;

        private T Initialized<T>(T obj, Func<T> initializer)
        {
            obj ??= initializer.Invoke();
            return obj;
        }

        public IApplicationGlobals Parent { get; protected set; } = parentInstance;
        internal ISmartSerializableNonTyped SmartSerializable { get; set; } =
            new SmartSerializableNonTyped();
        internal Func<string, bool> FileExists { get; set; } = File.Exists;
        internal Func<string, string> ReadAllText { get; set; } = File.ReadAllText;

        private string _projInfo_Filename;
        public string ProjInfo_Filename =>
            Initialized(_projInfo_Filename, () => _projInfo_Filename = _defaults.FileName_ProjInfo);
        private ProjectData _projInfo;
        public IProjectData ProjInfo => Initialized(_projInfo, () => LoadProjInfo());

        private async Task LoadProjInfoAsync()
        {
            var outlookApplication = Parent.Ol.App;

            _projInfo = await Task.Run(() =>
            {
                if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
                {
                    var proj = new ProjectData(
                        filename: _defaults.FileName_ProjInfo,
                        folderpath: appData
                    );
                    proj.Sort();
                    return proj;
                }
                else
                {
                    return null;
                }
            });

            if (_projInfo?.Count == 0 && outlookApplication is not null)
            {
                _projInfo.Rebuild(outlookApplication);
            }
        }

        private IProjectData LoadProjInfo()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var projectInfo = new ProjectData(
                    filename: _defaults.FileName_ProjInfo,
                    folderpath: appData
                );
                if (projectInfo.Count == 0)
                {
                    projectInfo.Rebuild(Parent.Ol.App);
                }
                return projectInfo;
            }
            else
            {
                return null;
            }
        }

        private ScDictionary<string, string> _programInfo;
        public ScDictionary<string, string> ProgramInfo =>
            Initialized(_programInfo, LoadProgramInfo);

        private ScDictionary<string, string> LoadProgramInfo()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                return ScDictionary<string, string>.Static.Deserialize(
                    _defaults.FileName_ProgramDictionary,
                    appData
                );
            }
            else
            {
                return null;
            }
        }

        private async Task LoadProgramInfoAsync() => _programInfo = await Task.Run(LoadProgramInfo);

        //public ProgramData

        internal async Task LoadPeopleAsync() =>
            await Task.Run(
                async () =>
                {
                    if (Parent.IntelRes.Config.TryGetValue("People", out var config))
                    {
                        People = await SmartSerializable.DeserializeAsync(
                            config,
                            true,
                            () => new PeopleScoDictionaryNew(Parent)
                        );
                        People.Prefix = _prefixList?.Find(x =>
                            x.PrefixType == PrefixTypeEnum.People
                        );
                        People.CollectionChanged += People_CollectionChanged;
                    }
                    else
                    {
                        logger.Error("People config not found.");
                    }
                },
                Parent.AF.CancelToken
            );

        public IPeopleScoDictionaryNew People { get; private set; }

        public void People_CollectionChanged(
            object Sender,
            DictionaryChangedEventArgs<string, string> args
        )
        {
            People.Serialize();
            //var dict = (PeopleScoDictionaryNew)Sender;
            //dict.Serialize();
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

        private async Task LoadIdListAsync()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var outlookApplication = Parent.Ol.App;
                _idList = await Task.Run(() => (IIDList)LoadIdListFromDisk(appData));

                if (_idList.Count == 0 && outlookApplication is not null)
                {
                    _idList.RefreshIDList(outlookApplication);
                }
            }
            else
            {
                _idList = null;
            }
        }

        private IDList LoadIdListFromDisk(string appData)
        {
            var filePath = Path.Combine(appData, FnameIDList);
            List<string> ids;

            try
            {
                ids = FileExists(filePath)
                    ? JsonConvert.DeserializeObject<List<string>>(ReadAllText(filePath)) ?? []
                    : [];
            }
            catch (JsonException)
            {
                ids = [];
            }
            catch (IOException)
            {
                ids = [];
            }

            var idList = new IDList(ids) { Filename = FnameIDList, Folderpath = appData };
            return idList;
        }

        private IIDList LoadIDList()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("AppData", out var appData))
            {
                var idList = new IDList(FnameIDList, appData, Parent.Ol.App);
                if (idList.Count == 0)
                {
                    idList.RefreshIDList();
                }
                return idList;
            }
            else
            {
                return null;
            }
        }

        private string _fnameDictRemap;
        public string FnameDictRemap =>
            Initialized(_fnameDictRemap, () => _fnameDictRemap = _defaults.FileName_DictRemap);

        private ScoDictionaryNew<string, string> _dictRemap;
        public IScoDictionaryNew<string, string> DictRemap =>
            Initialized(_dictRemap, () => LoadDictRemap());

        private ScoDictionaryNew<string, string> LoadDictRemap()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var dictRemap = ScoDictionaryNew<string, string>.Static.Deserialize(
                    FnameDictRemap,
                    pythonStaging
                );
                return dictRemap;
            }
            else
            {
                return null;
            }
        }

        private async Task LoadDictRemapAsync() =>
            _dictRemap = await Task.Run(LoadDictRemap, default);

        //TODO: Convert CategoryFilters to ScoCollection
        private ISerializableList<string> _catFilters;
        public ISerializableList<string> CategoryFilters
        {
            get =>
                Initialized(
                    _catFilters,
                    () =>
                    {
                        if (
                            Parent.FS.SpecialFolders.TryGetValue(
                                "PythonStaging",
                                out var pythonStaging
                            )
                        )
                        {
                            return new SerializableList<string>(
                                filename: _defaults.FileName_CategoryFilters,
                                folderpath: pythonStaging
                            );
                        }
                        else
                        {
                            return null;
                        }
                    }
                );
            set
            {
                _catFilters = value;

                if (
                    _catFilters.Folderpath == ""
                    && Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging)
                )
                {
                    _catFilters.Folderpath = pythonStaging;
                    _catFilters.Filename = _defaults.FileName_CategoryFilters;
                }
                if (_catFilters.Folderpath != "")
                {
                    _catFilters.Serialize();
                }
            }
        }

        private async Task LoadCategoryFiltersAsync()
        {
            _catFilters = await Task.Run(
                () =>
                {
                    if (
                        Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging)
                    )
                    {
                        return new SerializableList<string>(
                            filename: _defaults.FileName_CategoryFilters,
                            folderpath: pythonStaging
                        );
                    }
                    else
                    {
                        return null;
                    }
                },
                default
            );
        }

        // Prefix List
        private ConcurrentObservableCollection<IPrefix> _prefixList;
        public ConcurrentObservableCollection<IPrefix> PrefixList =>
            Initialized(_prefixList, () => LoadPrefixList());

        public ConcurrentObservableCollection<IPrefix> LoadPrefixList()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var prefixList = new ConcurrentObservableCollection<IPrefix>(
                    fileName: _defaults.FileName_PrefixList,
                    folderPath: pythonStaging
                );

                if (prefixList.Count == 0)
                {
                    var tdDefaults = new ToDoDefaults();
                    foreach (var prefix in tdDefaults.PrefixList)
                    {
                        prefixList.Add(prefix);
                    }
                    prefixList.Serialize();
                }
                return prefixList;
            }
            else
            {
                return null;
            }
        }

        private async Task LoadPrefixListAsync()
        {
            _prefixList = await Task.Run(LoadPrefixList);
        }

        private ScoDictionaryNew<string, int> _filteredFolderScraping;
        public ScoDictionaryNew<string, int> FilteredFolderScraping =>
            Initialized(_filteredFolderScraping, () => LoadFilteredFolderScraping());

        public ScoDictionaryNew<string, int> LoadFilteredFolderScraping()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var filteredFolderScraping = ScoDictionaryNew<string, int>.Static.Deserialize(
                    _defaults.FileName_FilteredFolderScraping,
                    pythonStaging
                );
                return filteredFolderScraping;
            }
            else
            {
                return null;
            }
        }

        private async Task LoadFilteredFolderScrapingAsync()
        {
            _filteredFolderScraping = await Task.Run(
                () => LoadFilteredFolderScraping(),
                default(CancellationToken)
            );
        }

        private ScoDictionaryNew<string, string> _folderRemap;
        public ScoDictionaryNew<string, string> FolderRemap =>
            Initializer.GetOrLoad(ref _folderRemap, () => LoadFolderRemap());

        public ScoDictionaryNew<string, string> LoadFolderRemap()
        {
            if (Parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                var folderRemap = ScoDictionaryNew<string, string>.Static.Deserialize(
                    _defaults.FileName_FolderRemap,
                    pythonStaging
                );
                return folderRemap;
            }
            else
            {
                return null;
            }
        }

        private async Task LoadFolderRemapAsync()
        {
            _folderRemap = await Task.Run(LoadFolderRemap);
        }

        public Func<IEnumerable<string>, IPrefix, string, string, string> FindMatchingTag
        {
            get;
            internal set;
        } = TagLauncher.LaunchAndFindMatch;

        //IEnumerable<string> options, IApplicationGlobals appGlobals
        public Func<IEnumerable<string>, List<string>> SelectFromList { get; internal set; }

        private async Task LoadSelectFromListAsync()
        {
            await Task.Run(() =>
                SelectFromList = (options) => TagLauncher.LaunchAndSelect(options, this.Parent)
            );
        }

        public IFlagChangeTrainingQueue FlagChangeTrainingQueue { get; set; }

        private async Task LoadFlagChangeTrainingQueueAsync()
        {
            await Task.Run(() =>
            {
                FlagChangeTrainingQueue = new FlagChangeTrainingQueue().Init();
            });
        }
    }
}
