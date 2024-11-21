using Microsoft.Office.Core;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using QuickFiler;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskMaster.AppGlobals;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppAutoFileObjects : IAppAutoFileObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppAutoFileObjects(IApplicationGlobals parent)
        {
            _parent = parent;
        }

        private T Initialized<T>(T obj, Func<T> initializer)
        {
            if (obj is null)
            {
                obj = initializer.Invoke();
            }
            return obj;
        }

        async public Task LoadAsync(bool parallel = true)
        {
            if (parallel) { await LoadParallelAsync(); }
            else { await LoadSequentialAsync(); }

            LoadProgressPane(CancelSource);
        }

        async public Task LoadParallelAsync()
        {
            Manager = new ManagerAsyncLazy(_parent);
            var tasks = new List<Task>
            {
                LoadRecentsListAsync(),
                LoadCtfMapAsync(),
                LoadCommonWordsAsync(),
                LoadSubjectMapAndEncoderAsync(),
                LoadMovedMailsAsync(),
                LoadFiltersAsync(),
                Manager.InitAsync(),
            };
            await Task.WhenAll(tasks);
        }

        async public Task LoadSequentialAsync()
        {
            Manager = new ManagerAsyncLazy(_parent);

            await LoadRecentsListAsync();
            await LoadCtfMapAsync();
            await LoadCommonWordsAsync();
            await LoadSubjectMapAndEncoderAsync();
            await LoadMovedMailsAsync();
            await LoadFiltersAsync();
            await Manager.InitAsync();            
        }

        private bool _sugFilesLoaded = false;
        private IApplicationGlobals _parent;
        private ISerializableList<string> _commonWords;
        private Properties.Settings _defaults = Properties.Settings.Default;

        private System.Action _maximizeQuickFileWindow = null;
        public System.Action MaximizeQuickFileWindow { get => _maximizeQuickFileWindow; set => _maximizeQuickFileWindow = value; }

        public int LngConvCtPwr
        {
            get => _defaults.ConversationExponent;
            set { _defaults.ConversationExponent = value; _defaults.Save(); }
        }

        public int Conversation_Weight
        {
            get => _defaults.ConversationWeight;
            set { _defaults.ConversationWeight = value; _defaults.Save(); }
        }

        public bool SuggestionFilesLoaded { get => _sugFilesLoaded; set => _sugFilesLoaded = value; }

        public int SmithWatterman_MatchScore
        {
            get => _defaults.SmithWatterman_MatchScore;
            set { _defaults.SmithWatterman_MatchScore = value; _defaults.Save(); }
        }

        public int SmithWatterman_MismatchScore
        {
            get => _defaults.SmithWatterman_MismatchScore;
            set { _defaults.SmithWatterman_MismatchScore = value; _defaults.Save(); }
        }

        public int SmithWatterman_GapPenalty
        {
            get => _defaults.SmithWatterman_GapPenalty;
            set { _defaults.SmithWatterman_GapPenalty = value; _defaults.Save(); }
        }

        public int MaxRecents { get => _defaults.MaxRecents; set { _defaults.MaxRecents = value; _defaults.Save(); } }

        private ScoStack<IMovedMailInfo> _movedMails;
        public ScoStack<IMovedMailInfo> MovedMails { get => Initialized(_movedMails, LoadMovedMails); }
        private ScoStack<IMovedMailInfo> LoadMovedMails()
        {
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging)) 
            { 
                var movedMails = new ScoStack<IMovedMailInfo>(filename: _defaults.FileName_MovedEmails,
                                                              folderpath: pythonStaging,
                                                              askUserOnError: false);
                return movedMails;
            }
            else { return null; }
        }
        async private Task LoadMovedMailsAsync()
        {
            //await TaskPriority.Run(
            //    PriorityScheduler.BelowNormal,
            //    () => _movedMails = LoadMovedMails());
            await Task.Run(() => _movedMails = LoadMovedMails());
        }

        private RecentsList<string> _recentsList;
        public RecentsList<string> RecentsList
        {
            get
            {
                if (_recentsList is null)
                {
                    if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                    {
                        _recentsList = new RecentsList<string>(_defaults.FileName_Recents, pythonStaging, max: MaxRecents);
                    }
                }
                return _recentsList;
            }
            set
            {
                _recentsList = value;
                if (_recentsList.FolderPath == "")
                {
                    if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                    {
                        _recentsList.FolderPath = pythonStaging;
                        _recentsList.FileName = Properties.Settings.Default.FileName_Recents;
                    }
                }
                _recentsList.Serialize();
            }
        }
        async private Task LoadRecentsListAsync()
        {
            await Task.Run(() => 
            {
                if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    _recentsList = new RecentsList<string>(_defaults.FileName_Recents, pythonStaging, max: MaxRecents); 
                }
            }, CancelToken);
        }

        private CtfMap _ctfMap;
        public CtfMap CtfMap
        {
            get
            {
                if (_ctfMap is null)
                {
                    if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                    {
                        _ctfMap = new CtfMap(filename: _defaults.File_CTF_Inc, folderpath: pythonStaging, 
                            backupFilepath: Path.Combine(pythonStaging, _defaults.BackupFile_CTF_Inc),
                            askUserOnError: true);
                    }

                }
                return _ctfMap;
            }
            set
            {
                _ctfMap = value;
                if (_ctfMap.FilePath == "")
                {
                    if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                    {
                        _ctfMap.FolderPath = pythonStaging;
                        _ctfMap.FileName = _defaults.File_CTF_Inc;
                    }
                }
                _ctfMap.Serialize();
            }
        }
        public CtfMap LoadCtfMap()
        {
            CtfMap map = null;
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                map = new CtfMap(
                    filename: _defaults.File_CTF_Inc,
                    folderpath: pythonStaging,
                    backupFilepath: Path.Combine(pythonStaging, _defaults.BackupFile_CTF_Inc),
                    askUserOnError: true);
            }
            return map;
        }
        async private Task LoadCtfMapAsync()
        {
            await Task.Run(() => _ctfMap = LoadCtfMap(), CancelToken);
        }

        public ISerializableList<string> CommonWords
        {
            get
            {
                if (_commonWords is null)
                    if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                    {
                        _commonWords = new SerializableList<string>(filename: _defaults.File_Common_Words,
                                                                folderpath: pythonStaging,
                                                                backupLoader: CommonWordsBackupLoader,
                                                                backupFilepath: Path.Combine(pythonStaging,
                                                                                             _defaults.BackupFile_CommonWords),
                                                                askUserOnError: false);

                    }
                return _commonWords;
            }
            set
            {
                _commonWords = value;
                if (_commonWords.Folderpath == "")
                {
                    if (_parent.FS.SpecialFolders.TryGetValue("Flow", out var flow))
                    {
                        _commonWords.Folderpath = flow;
                        _commonWords.Filename = _defaults.FileName_Recents;
                    }
                }
                if (_commonWords.Folderpath != "") { _commonWords.Serialize(); }
            }
        }
        async private Task LoadCommonWordsAsync()
        {
            await Task.Run(() =>
            {
                if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    _commonWords = new SerializableList<string>(filename: _defaults.File_Common_Words,
                                                                  folderpath: pythonStaging,
                                                                  backupLoader: CommonWordsBackupLoader,
                                                                  backupFilepath: Path.Combine(pythonStaging,
                                                                                               _defaults.BackupFile_CommonWords),
                                                                  askUserOnError: false);
                }
            }, default(CancellationToken)); 
            
            
        }
        private IList<string> CommonWordsBackupLoader(string filepath)
        {
            if (File.Exists(filepath))
            {
                string[] cw = FileIO2.CsvRead(filename: Path.GetFileName(filepath), folderpath: Path.GetDirectoryName(filepath), skipHeaders: false);
                return cw.ToList();
            }
            else 
            {
                logger.Error($"File not found {filepath}");
                return []; 
            }
        }

        private ISubjectMapEncoder _encoder;
        public ISubjectMapEncoder Encoder => Initialized(_encoder, LoadEncoder);
        private ISubjectMapEncoder LoadEncoder()
        {
            SubjectMapEncoder encoder = null;
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                encoder = new SubjectMapEncoder(filename: _defaults.FileName_SubjectEncoding,
                                                folderpath: pythonStaging,
                                                subjectMap: SubjectMap);

            }
            if (encoder.Encoder.Count == 0) { encoder.RebuildEncoding(SubjectMap); }
            return encoder;
        }


        private SubjectMapSco _subjectMap;
        public SubjectMapSco SubjectMap => Initialized(_subjectMap, LoadSubjectMap);

        private ObserverHelper<NotifyCollectionChangedEventArgs> _filterObserver;
        private ScoCollection<FilterEntry> _filters;
        public ScoCollection<FilterEntry> Filters => Initializer.GetOrLoad(ref _filters, LoadFilters);
        private ScoCollection<FilterEntry> LoadFilters()
        {
            ScoCollection<FilterEntry> filters = null;
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                filters = new ScoCollection<FilterEntry>(
                    fileName: _defaults.FileName_Filters,
                    folderPath: pythonStaging);
            }
            _filterObserver = new ObserverHelper<NotifyCollectionChangedEventArgs>("FilterObserver", (x) => Filters.Serialize());
            filters.Subscribe(_filterObserver);
            
            return filters;
        }
        async private Task LoadFiltersAsync()
        {
            await Task.Factory.StartNew(
                () => _filters = LoadFilters(),
                CancelToken);
        }
        private void ScoFilterEntry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = (ScoCollection<FilterEntry>)sender;
            collection.Serialize();
        }

        private SubjectMapSco LoadSubjectMap()
        {
            SubjectMapSco subMap = null;
            if (_parent.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
            {
                subMap = new SubjectMapSco(filename: _defaults.File_Subject_Map,
                                          folderpath: pythonStaging,
                                          backupLoader: SubjectMapBackupLoader,
                                          backupFilepath: Path.Combine(pythonStaging, _defaults.BackupFile_SubjectMap),
                                          askUserOnError: false,
                                          commonWords: CommonWords);
                subMap.CollectionChanged += SubjectMap_CollectionChanged;
            }

            return subMap;
        }

        async private Task LoadSubjectMapAndEncoderAsync()
        {
            await Task.Run(
                 () => _subjectMap = LoadSubjectMap(),
                 CancelToken);

            await Task.Run(
                 () => _encoder = LoadEncoder(),
                 CancelToken);

            await Task.Run(
                () =>
                {
                    var toRecode = this.SubjectMap
                        .Where(
                        x => x.Encoder is null || x.FolderEncoded is null || x.SubjectEncoded is null)
                        .ToArray();

                    if (toRecode.Any())
                    {
                        toRecode.ForEach(x => x.Encoder = this.Encoder);
                        this.SubjectMap.Serialize();
                    }
                });

        }

        private IList<SubjectMapEntry> SubjectMapBackupLoader(string filepath)
        {
            var subjectMapEntries = new List<SubjectMapEntry>();

            string[] fileContents = FileIO2.CsvRead(filename: Path.GetFileName(filepath), folderpath: Path.GetDirectoryName(filepath), skipHeaders: true);

            var rowQueue = fileContents.IsNullOrEmpty() ? new Queue<string>() : new Queue<string>(fileContents);

            while (rowQueue.Count > 0)
            {
                string emailFolderPath = "not set";
                string emailSubject = "not set";
                int emailSubjectCount = -1;
                try
                {
                    emailFolderPath = rowQueue.Dequeue();
                    emailSubject = rowQueue.Dequeue();
                    emailSubjectCount = int.Parse(rowQueue.Dequeue());

                    subjectMapEntries.Add(
                        new SubjectMapEntry(
                            emailFolder: emailFolderPath,
                            emailSubject: emailSubject,
                            emailSubjectCount: emailSubjectCount,
                            commonWords: CommonWords));

                }
                catch (Exception e)
                {
                    logger.Error($"Error loading subject map from backup file on item \n " +
                        $"Email Folder: {emailFolderPath} \n" +
                        $"Email Subject: {emailSubject} \n" +
                        $"Email Count {emailSubjectCount} \n" +
                        $"{e.Message}", e);
                }
            }
            return subjectMapEntries;
        }

        internal void SubjectMap_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SubjectMapSco map = (SubjectMapSco)sender;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var entry = map.Last();
                entry.Encode(Encoder);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Encoder.RebuildEncoding(map);
            }
        }

        public ManagerAsyncLazy Manager { get; internal set; } 
        
        private ProgressTrackerPane _progressTracker;
        public ProgressTrackerPane ProgressTracker => _progressTracker;
        private Microsoft.Office.Tools.CustomTaskPane _progressPane;
        public Microsoft.Office.Tools.CustomTaskPane ProgressPane => _progressPane;
        private void LoadProgressPane(CancellationTokenSource tokenSource)
        {
            if (_progressTracker is null)
            {
                _progressTracker = new ProgressTrackerPane(tokenSource);
                _progressPane = Globals.ThisAddIn.CustomTaskPanes.Add(
                    _progressTracker.ProgressViewer, "Progress Tracker");
                _progressPane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionBottom;
            }
        }

        public CancellationTokenSource CancelSource { get; private set; } = new();
        public CancellationToken CancelToken => Initializer.GetOrLoad(ref _token, LoadToken);
        private CancellationToken _token;
        private CancellationToken LoadToken() => CancelSource.Token;
        
        #region Unused Commented Code

        //using (Stream stream = new MemoryStream(Properties.Resources.manager_config))
        //public async Task<string> LoadResourceFileToStringAsync(string resourceName)
        //{
        //    string result = string.Empty;
        //    var assembly = Assembly.GetExecutingAssembly();

        //    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        //    {
        //        if (stream is null)
        //        {
        //            logger.Error($"Resource '{resourceName}' not found.");
        //            return result;
        //        }
        //        using StreamReader reader = new(stream);
        //        result = await reader.ReadToEndAsync();
        //    }

        //    return result;
        //}


        #endregion Unused Commented Code

        #region Obsolete And Commented

        //[Obsolete]
        //private Lazy<ConcurrentDictionary<string, byte[]>> _binaryResources = new(() =>
        //{
        //    var rsMgr = Properties.Resources.ResourceManager;
        //    var rsSet = rsMgr.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
        //    var rsDict = rsSet
        //        .Cast<DictionaryEntry>()
        //        .Where(x => x.Value is byte[])
        //        .ToDictionary<string, byte[]>()
        //        .ToConcurrentDictionary();
        //    return rsDict;
        //});
        //[Obsolete]
        //public ConcurrentDictionary<string, byte[]> BinaryResources => _binaryResources.Value;

        //[Obsolete]
        //public string[] GetManifestResourceNames()
        //{
        //    //var rsMgr = Properties.Resources.ResourceManager;
        //    //var rsSet = rsMgr.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
        //    //var rsDict = rsSet.Cast<DictionaryEntry>().Where(x => x.Value is byte[]).ToDictionary<string, byte[]>();
        //    var rsDict = BinaryResources;
        //    var configBin = rsDict["manager_config"];
        //    var configStr = System.Text.Encoding.UTF8.GetString(configBin);
        //    var configLoader = new SmartSerializableConfig(_parent);
        //    //var config = configLoader.DeserializeConfig(configStr);
        //    return rsDict.Keys.ToArray();
        //    //return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        //}

        //[Obsolete]
        //private AsyncLazy<ManagerClass> _manager2;
        //[Obsolete]
        //public AsyncLazy<ManagerClass> Manager2 => _manager2;
        //[Obsolete]
        //public void ResetLoadManager()
        //{
        //    _manager2 = new AsyncLazy<ManagerClass>(async () =>
        //    {
        //        if (BinaryResources.TryGetValue("ConfigManager", out byte[] configBin))
        //        {
        //            var config = await SmartSerializableConfig.DeserializeAsync(_parent, configBin);
        //            return await ManagerClass.Static.DeserializeAsync(config);
        //        }
        //        else { return null; }
        //    });
        //    //_manager2 = mgr;
        //}

        //[Obsolete]
        //public AsyncLazy<BayesianClassifierGroup> GetClassifierAsyncLazy(string classifierName, string configName)
        //{
        //    return new AsyncLazy<BayesianClassifierGroup>(async () =>
        //    {
        //        if (BinaryResources.TryGetValue(configName, out byte[] configBin))
        //        {
        //            var config = await NewSmartSerializableLoader.DeserializeAsync(_parent, configBin);
        //            return await BayesianClassifierGroup.Static.DeserializeAsync(config);                    
        //        }
        //        else { return null; }                
        //    });
        //}

        //[Obsolete]
        //public void ResetLoadManagerLazyOld()
        //{
        //    var classifierConfigs = new Dictionary<string, string>()
        //    {
        //        {"Spam", "Spam" },
        //        {"Folder", "Folder"},
        //        {"Triage", "Triage" }
        //    };

        //    foreach (var classifier in classifierConfigs)
        //    {
        //        var value = GetClassifierAsyncLazy(classifier.Key, classifier.Value);
        //        if (value != null) { _managerLazy[classifier.Key] = value; }
        //    }
        //}

        //[Obsolete]
        //private ScDictionary<string, BayesianClassifierGroup> _manager;
        //[Obsolete]
        //public ScDictionary<string, BayesianClassifierGroup> Manager => Initialized(_manager, LoadManager);
        //[Obsolete]
        //internal ScDictionary<string, BayesianClassifierGroup> LoadManager()
        //{
        //    var network = new FilePathHelper(_defaults.File_ClassifierManager, _parent.FS.FldrPythonStaging);
        //    var networkDt = File.Exists(network.FilePath) ? File.GetLastWriteTimeUtc(network.FilePath) : default;

        //    var local = new FilePathHelper(_defaults.File_ClassifierManager, _parent.FS.FldrAppData);
        //    var localDt = File.Exists(local.FilePath) ? File.GetLastWriteTimeUtc(local.FilePath) : default;

        //    //var config = new SmartSerializableConfig(_parent);
        //    //config.Local = local;
        //    //config.Network = network;
        //    //var configFP = new FilePathHelper("manager.config", _parent.FS.FldrAppData);
        //    //var configSettings = config.LocalSettings;
        //    //using (StreamWriter sw = File.CreateText(configFP.FilePath))
        //    //{
        //    //    var serializer = JsonSerializer.Create(configSettings);
        //    //    serializer.Serialize(sw, config);
        //    //    sw.Close();
        //    //}

        //    var localSettings = GetSettings(false);
        //    var networkSettings = GetSettings(true);

        //    var manager = GetManager(local, localSettings);
        //    manager.NetDisk = network;
        //    manager.NetJsonSettings = networkSettings;
        //    manager.LocalDisk = local;
        //    manager.LocalJsonSettings = localSettings;

        //    if (networkDt != default && (localDt == default || networkDt > localDt))
        //    {
        //        IdleActionQueue.AddEntry(async () =>
        //            await Task.Run(() =>
        //            {
        //                _manager = GetManager(network, networkSettings);
        //                _manager.NetDisk = network;
        //                _manager.NetJsonSettings = networkSettings;
        //                _manager.LocalDisk = local;
        //                _manager.LocalJsonSettings = localSettings;
        //                _manager.ActivateLocalDisk();
        //                IdleActionQueue.AddEntry(() => _manager.Serialize());
        //            }
        //            ));
        //    }

        //    return manager;
        //}
        //[Obsolete]
        //private JsonSerializerSettings GetSettings(bool compress)
        //{
        //    var settings = ScDictionary<string, BayesianClassifierGroup>.GetDefaultSettings();
        //    //var settings = ScDictionary<string, BayesianClassifierGroup>.Factory.GetDefaultSettings();
        //    settings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
        //    settings.Converters.Add(new AppGlobalsConverter(_parent));
        //    settings.TraceWriter = new NLogTraceWriter();
        //    if (compress)
        //        settings.ContractResolver = new DoNotSerializeContractResolver("Prob","NotMatch");
        //    return settings;
        //}
        //[Obsolete]
        //private ScDictionary<string, BayesianClassifierGroup> GetManager(
        //    FilePathHelper disk, 
        //    JsonSerializerSettings settings)
        //{
        //    return new ScDictionary<string, BayesianClassifierGroup>().Deserialize(
        //        fileName: disk.FileName,
        //        folderPath: disk.FolderPath,
        //        askUserOnError: false,
        //        settings: settings);
        //}
        //[Obsolete]
        //private async Task LoadManagerAsync()
        //{
        //    LoadProgressPane(_tokenSource);
        //    await Task.Run(
        //        () => _manager = LoadManager(),
        //        CancelToken);
        //}
        //[Obsolete]
        //public void SaveManagerLocal()
        //{
        //    _manager.ActivateLocalDisk();
        //    _manager.Serialize();
        //}
        //[Obsolete]
        //public void SaveManagerNetwork()
        //{
        //    _manager.ActivateNetDisk();
        //    _manager.Serialize();
        //}

        #endregion Obsolete And Commented

    }
}