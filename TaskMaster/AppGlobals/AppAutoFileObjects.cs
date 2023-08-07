using Microsoft.VisualBasic;
using QuickFiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppAutoFileObjects : IAppAutoFileObjects
    {
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
                LoadSubjectMapAndEncoderAsync()
            };
            await Task.WhenAll(tasks);
            Debug.WriteLine($"{nameof(AppAutoFileObjects)}.{nameof(LoadAsync)} is complete.");
        }
        
        private bool _sugFilesLoaded = false;
        private IRecentsList<string> _recentsList;
        private IApplicationGlobals _parent;
        private CtfMap _ctfMap;
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

        public IRecentsList<string> RecentsList
        {
            get
            {
                if (_recentsList is null)
                    _recentsList = new RecentsList<string>(_defaults.FileName_Recents, _parent.FS.FldrFlow, max: MaxRecents);
                return _recentsList;
            }
            set
            {
                _recentsList = value;
                if (_recentsList.Folderpath == "")
                {
                    _recentsList.Folderpath = _parent.FS.FldrFlow;
                    _recentsList.Filename = Properties.Settings.Default.FileName_Recents;
                }
                _recentsList.Serialize();
            }
        }
        async private Task LoadRecentsListAsync()
        {
            await Task.Factory.StartNew(
                () => _recentsList = new RecentsList<string>(_defaults.FileName_Recents, _parent.FS.FldrFlow, max: MaxRecents),
                default,
                TaskCreationOptions.None,
                PriorityScheduler.BelowNormal);
        }   

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
                if (_ctfMap.Filepath == "")
                {
                    _ctfMap.Folderpath = _parent.FS.FldrPythonStaging;
                    _ctfMap.Filename = _defaults.File_CTF_Inc;
                }
                _ctfMap.Serialize();
            }
        }
        async private Task LoadCtfMapAsync()
        {
            await Task.Factory.StartNew(
                () => _ctfMap = new CtfMap(filename: _defaults.File_CTF_Inc,
                                           folderpath: _parent.FS.FldrPythonStaging,
                                           backupFilepath: Path.Combine(
                                               _parent.FS.FldrPythonStaging,
                                               _defaults.BackupFile_CTF_Inc),
                                           askUserOnError: true),
                default,
                TaskCreationOptions.None,
                PriorityScheduler.BelowNormal);
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
                default,
                TaskCreationOptions.None,
                PriorityScheduler.BelowNormal);
        }

        private IList<string> CommonWordsBackupLoader(string filepath)
        {
            string[] cw = FileIO2.CsvRead(filename: Path.GetFileName(filepath), folderpath: Path.GetDirectoryName(filepath), skipHeaders: false);
            return cw.ToList();
        }

        private ISubjectMapEncoder _encoder;
        public ISubjectMapEncoder Encoder => Initialized(_encoder, LoadEncoder);
        //{ 
        //    get 
        //    {
        //        if (_encoder is null) 
        //        {
        //            _encoder = new SubjectMapEncoder(filename: _defaults.FileName_SubjectEncoding,
        //                                             folderpath: _parent.FS.FldrPythonStaging,
        //                                             subjectMap: SubjectMap);
        //            if (_encoder.Encoder.Count == 0) { _encoder.RebuildEncoding(SubjectMap); }
        //        }
                
        //        return _encoder; 
        //    }
        //}
        private ISubjectMapEncoder LoadEncoder()
        {
            var encoder = new SubjectMapEncoder(filename: _defaults.FileName_SubjectEncoding,
                                                folderpath: _parent.FS.FldrPythonStaging,
                                                subjectMap: SubjectMap);
            if (encoder.Encoder.Count == 0) { encoder.RebuildEncoding(SubjectMap); }
            return encoder;
        }

        private ISubjectMapSL _subjectMap;
        public ISubjectMapSL SubjectMap => Initialized(_subjectMap, LoadSubjectMap);
        //{
        //    get
        //    {
        //        if (_subjectMap is null)
        //        {
        //            _subjectMap = new SubjectMapSL(filename: _defaults.File_Subject_Map,
        //                                           folderpath: _parent.FS.FldrPythonStaging,
        //                                           backupLoader: SubjectMapBackupLoader,
        //                                           backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
        //                                                                        _defaults.BackupFile_SubjectMap),
        //                                           askUserOnError: false,
        //                                           commonWords: CommonWords);

        //            _subjectMap.PropertyChanged += SubjectMap_PropertyChanged;
        //        }
        //        return _subjectMap;
        //    }

        //}
        private SubjectMapSL LoadSubjectMap()
        {
            var subMap = new SubjectMapSL(filename: _defaults.File_Subject_Map,
                                          folderpath: _parent.FS.FldrPythonStaging,
                                          backupLoader: SubjectMapBackupLoader,
                                          backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
                                          _defaults.BackupFile_SubjectMap),
                                          askUserOnError: false,
                                          commonWords: CommonWords);

            subMap.PropertyChanged += SubjectMap_PropertyChanged;
            return subMap;
        }
        async private Task LoadSubjectMapAndEncoderAsync()
        {
            await Task.Factory.StartNew(
                 () => _subjectMap = LoadSubjectMap(),
                 default,
                 TaskCreationOptions.None,
                 PriorityScheduler.BelowNormal);

            await Task.Factory.StartNew(
                 () => _encoder = LoadEncoder(),
                 default,
                 TaskCreationOptions.None,
                 PriorityScheduler.BelowNormal);

            await TaskPriority.Run(
                PriorityScheduler.BelowNormal,
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

        private IList<ISubjectMapEntry> SubjectMapBackupLoader(string filepath)
        {
            var subjectMapEntries = new List<ISubjectMapEntry>();

            string[] fileContents = FileIO2.CsvRead(filename: Path.GetFileName(filepath), folderpath: Path.GetDirectoryName(filepath), skipHeaders: true);
            var rowQueue = new Queue<string>(fileContents);

            while (rowQueue.Count > 0)
            {
                subjectMapEntries.Add(
                    new SubjectMapEntry(emailFolder: rowQueue.Dequeue(),
                                        emailSubject: rowQueue.Dequeue(),
                                        emailSubjectCount: int.Parse(rowQueue.Dequeue()),
                                        commonWords: CommonWords));
            }
            return subjectMapEntries;
        }
        
        internal void SubjectMap_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISubjectMapSL map = (ISubjectMapSL)sender;
            if (e.PropertyName == "Add")
            {
                var entry = map[map.Count - 1];
                entry.Encode(Encoder);
            }
            else if (e.PropertyName == "BackupLoader") 
            {
                Encoder.RebuildEncoding(map);
            }
        }

    }
}