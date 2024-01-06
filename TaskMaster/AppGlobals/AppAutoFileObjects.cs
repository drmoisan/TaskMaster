using Microsoft.Office.Core;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using QuickFiler;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppAutoFileObjects : IAppAutoFileObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppAutoFileObjects(ApplicationGlobals parent)
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

        async public Task LoadAsync()
        {
            var tasks = new List<Task> 
            {
                LoadRecentsListAsync(),
                LoadCtfMapAsync(),
                LoadCommonWordsAsync(),
                LoadSubjectMapAndEncoderAsync(),
                LoadMovedMailsAsync(),
                LoadFiltersAsync(),
                LoadManagerAsync(),
            };
            await Task.WhenAll(tasks);
            //logger.Debug($"{nameof(AppAutoFileObjects)}.{nameof(LoadAsync)} is complete.");
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
            var movedMails = new ScoStack<IMovedMailInfo>(filename: _defaults.FileName_MovedEmails,
                                                          folderpath: _parent.FS.FldrPythonStaging,
                                                          askUserOnError: false);
            return movedMails;
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
                    _recentsList = new RecentsList<string>(_defaults.FileName_Recents, _parent.FS.FldrPythonStaging, max: MaxRecents);
                return _recentsList;
            }
            set
            {
                _recentsList = value;
                if (_recentsList.FolderPath == "")
                {
                    _recentsList.FolderPath = _parent.FS.FldrPythonStaging;
                    _recentsList.FileName = Properties.Settings.Default.FileName_Recents;
                }
                _recentsList.Serialize();
            }
        }
        async private Task LoadRecentsListAsync()
        {   
            await Task.Factory.StartNew(
                () => _recentsList = new RecentsList<string>(_defaults.FileName_Recents, _parent.FS.FldrPythonStaging, max: MaxRecents),
                CancelLoad,
                TaskCreationOptions.None, 
                TaskScheduler.Current);
        }   
        
        private CtfMap _ctfMap;
        public CtfMap CtfMap
        {
            get
            {
                if (_ctfMap is null)
                    _ctfMap = new CtfMap(filename: _defaults.File_CTF_Inc,
                                         folderpath: _parent.FS.FldrPythonStaging,
                                         backupFilepath: Path.Combine(
                                             _parent.FS.FldrPythonStaging, 
                                             _defaults.BackupFile_CTF_Inc),
                                         askUserOnError: true);
                return _ctfMap;
            }
            set
            {
                _ctfMap = value;
                if (_ctfMap.FilePath == "")
                {
                    _ctfMap.FolderPath = _parent.FS.FldrPythonStaging;
                    _ctfMap.FileName = _defaults.File_CTF_Inc;
                }
                _ctfMap.Serialize();
            }
        }
        public CtfMap LoadCtfMap()
        {
            var map = new CtfMap(
                filename: _defaults.File_CTF_Inc,
                folderpath: _parent.FS.FldrPythonStaging,
                backupFilepath: Path.Combine(
                    _parent.FS.FldrPythonStaging,
                    _defaults.BackupFile_CTF_Inc),
                askUserOnError: true);
            return map;
        }
        async private Task LoadCtfMapAsync()
        {
            await Task.Factory.StartNew(
                () => _ctfMap = LoadCtfMap(),
                CancelLoad);
                //default,
                //TaskCreationOptions.None,
                //PriorityScheduler.BelowNormal);
        }

        public ISerializableList<string> CommonWords
        {
            get
            {
                if (_commonWords is null)
                    _commonWords = new SerializableList<string>(filename: _defaults.File_Common_Words,
                                                                folderpath: _parent.FS.FldrPythonStaging,
                                                                backupLoader: CommonWordsBackupLoader,
                                                                backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
                                                                                             _defaults.BackupFile_CommonWords),
                                                                askUserOnError: false);
                return _commonWords;
            }
            set
            {
                _commonWords = value;
                if (_commonWords.Folderpath == "")
                {
                    _commonWords.Folderpath = _parent.FS.FldrFlow;
                    _commonWords.Filename = _defaults.FileName_Recents;
                }
                _commonWords.Serialize();
            }
        }
        async private Task LoadCommonWordsAsync()
        {
            await Task.Factory.StartNew(
                () => _commonWords = new SerializableList<string>(filename: _defaults.File_Common_Words,
                                                                  folderpath: _parent.FS.FldrPythonStaging,
                                                                  backupLoader: CommonWordsBackupLoader,
                                                                  backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
                                                                                               _defaults.BackupFile_CommonWords),
                                                                  askUserOnError: false),
                default(CancellationToken));
                //default,
                //TaskCreationOptions.None,
                //PriorityScheduler.BelowNormal);
        }
        private IList<string> CommonWordsBackupLoader(string filepath)
        {
            string[] cw = FileIO2.CsvRead(filename: Path.GetFileName(filepath), folderpath: Path.GetDirectoryName(filepath), skipHeaders: false);
            return cw.ToList();
        }

        private ISubjectMapEncoder _encoder;
        public ISubjectMapEncoder Encoder => Initialized(_encoder, LoadEncoder);
        private ISubjectMapEncoder LoadEncoder()
        {
            var encoder = new SubjectMapEncoder(filename: _defaults.FileName_SubjectEncoding,
                                                folderpath: _parent.FS.FldrPythonStaging,
                                                subjectMap: SubjectMap);
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
            var filters = new ScoCollection<FilterEntry>(
                fileName: _defaults.FileName_Filters,
                folderPath: _parent.FS.FldrPythonStaging);
            _filterObserver = new ObserverHelper<NotifyCollectionChangedEventArgs>("FilterObserver", (x)=>Filters.Serialize());
            filters.Subscribe(_filterObserver);
            //filters.CollectionChanged += ScoFilterEntry_CollectionChanged;
            return filters;
        }
        async private Task LoadFiltersAsync()
        {
            await Task.Factory.StartNew(
                () => _filters = LoadFilters(),
                CancelLoad);
        }
        private void ScoFilterEntry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection= (ScoCollection<FilterEntry>)sender;
            collection.Serialize();
        }

        private SubjectMapSco LoadSubjectMap()
        {
            var subMap = new SubjectMapSco(filename: _defaults.File_Subject_Map,
                                          folderpath: _parent.FS.FldrPythonStaging,
                                          backupLoader: SubjectMapBackupLoader,
                                          backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
                                          _defaults.BackupFile_SubjectMap),
                                          askUserOnError: false,
                                          commonWords: CommonWords);

            subMap.CollectionChanged += SubjectMap_CollectionChanged;
            return subMap;
        }

        async private Task LoadSubjectMapAndEncoderAsync()
        {
            await Task.Factory.StartNew(
                 () => _subjectMap = LoadSubjectMap(),
                 CancelLoad,
                 TaskCreationOptions.LongRunning,
                 TaskScheduler.Current);
            //default,
            //TaskCreationOptions.None,
            //PriorityScheduler.BelowNormal);

            await Task.Factory.StartNew(
                 () => _encoder = LoadEncoder(),
                 default(CancellationToken));
            //default,
            //TaskCreationOptions.None,
            //PriorityScheduler.BelowNormal);

            //await TaskPriority.Run(
            //    PriorityScheduler.BelowNormal,
            await Task.Run(
                () =>
                {
                    var toRecode = this.SubjectMap.Where(x => x.Encoder is null || 
                                                              x.FolderEncoded is null || 
                                                              x.SubjectEncoded is null );
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
            var rowQueue = new Queue<string>(fileContents);

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

        private ScDictionary<string, ClassifierGroup> _manager;
        public ScDictionary<string, ClassifierGroup> Manager => Initialized(_manager, LoadManager);
        private ScDictionary<string, ClassifierGroup> LoadManager()
        {
            var network = new FilePathHelper(_defaults.File_ClassifierManager, _parent.FS.FldrPythonStaging);
            var networkDt = File.Exists(network.FilePath) ? File.GetLastWriteTimeUtc(network.FilePath) : default;
            
            var local = new FilePathHelper(_defaults.File_ClassifierManager, _parent.FS.FldrAppData);
            var localDt = File.Exists(local.FilePath) ? File.GetLastWriteTimeUtc(local.FilePath) : default;
           
            var localSettings = GetSettings(false);
            var networkSettings = GetSettings(true);
            
            var manager = GetManager(local, localSettings);
            manager.NetDisk = network;
            manager.NetJsonSettings = networkSettings;
            manager.LocalDisk = local;
            manager.LocalJsonSettings = localSettings;

            if (networkDt != default && (localDt == default || networkDt > localDt))
            {
                IdleActionQueue.AddEntry(async () =>
                    await Task.Run(() =>
                    {
                        _manager = GetManager(network, networkSettings);
                        _manager.NetDisk = network;
                        _manager.NetJsonSettings = networkSettings;
                        _manager.LocalDisk = local;
                        _manager.LocalJsonSettings = localSettings;
                        _manager.ActivateLocalDisk();
                        IdleActionQueue.AddEntry(() => _manager.Serialize());
                    }
                    ));
            }

            return manager;
        }

        private JsonSerializerSettings GetSettings(bool compress)
        {
            var settings = ScDictionary<string, ClassifierGroup>.GetDefaultSettings();
            settings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            settings.Converters.Add(new AppGlobalsConverter(_parent));
            if (compress)
                settings.ContractResolver = new DoNotSerializeContractResolver("Prob","Negative");
            return settings;
        }
        
        private ScDictionary<string, ClassifierGroup> GetManager(
            FilePathHelper disk, 
            JsonSerializerSettings settings)
        {
            return ScDictionary<string, ClassifierGroup>.Deserialize(
                fileName: disk.FileName,
                folderPath: disk.FolderPath,
                askUserOnError: false,
                settings: settings);
        }

        private async Task LoadManagerAsync()
        {
            LoadProgressPane(_tokenSource);
            await Task.Run(
                () => _manager = LoadManager(),
                CancelLoad);
        }

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

        private CancellationTokenSource _tokenSource = new();
        public CancellationToken CancelLoad => Initialized(_token, LoadToken);
        private CancellationToken _token;
        private CancellationToken LoadToken()
        {
            return _tokenSource.Token;
        }
    }
}