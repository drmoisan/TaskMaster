using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesVB;

namespace TaskMaster
{

    public class AppAutoFileObjects : IAppAutoFileObjects
    {

        private bool _sugFilesLoaded = false;
        private int _smithWatterman_MatchScore;
        private int _smithWatterman_MismatchScore;
        private int _smithWatterman_GapPenalty;
        private IRecentsList<string> _recentsList;
        private IApplicationGlobals _parent;
        private CtfIncidenceList _ctfList;
        private ISerializableList<string> _commonWords;
        private Properties.Settings _defaults = Properties.Settings.Default;
        
        public AppAutoFileObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

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

        public CtfIncidenceList CTFList
        {
            get
            {
                if (_ctfList is null)
                    _ctfList = new CtfIncidenceList(filename: _defaults.File_CTF_Inc,
                                                    folderpath: _parent.FS.FldrPythonStaging,
                                                    backupFilepath: _defaults.BackupFile_CTF_Inc);
                return _ctfList;
            }
            set
            {
                _ctfList = value;
                if (_ctfList.Filepath == "")
                {
                    _ctfList.Folderpath = _parent.FS.FldrPythonStaging;
                    _ctfList.Filename = _defaults.File_CTF_Inc;
                }
                _ctfList.Serialize();
            }
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

        private IList<string> CommonWordsBackupLoader(string filepath)
        {
            string[] cw = FileIO2.CSV_Read(filename: Path.GetFileName(filepath), fileaddress: Path.GetDirectoryName(filepath), SkipHeaders: false);
            return cw.ToList();
        }

        private ISubjectMapEncoder _encoder;
        public ISubjectMapEncoder Encoder 
        { 
            get 
            {
                if (_encoder is null) 
                {
                    _encoder = new SubjectMapEncoder(filename: _defaults.FileName_SubjectEncoding,
                                                     folderpath: _parent.FS.FldrPythonStaging);
                    _encoder.Encoder.Deserialize();
                    if (_encoder.Encoder.Count == 0) { _encoder.RebuildEncoding(SubjectMap); }
                }
                
                return _encoder; 
            }
        }

        private ISubjectMapSL _subjectMap;
        public ISubjectMapSL SubjectMap
        {
            get
            {
                if (_subjectMap is null)
                    _subjectMap = new SubjectMapSL(filename: _defaults.File_Subject_Map,
                                                   folderpath: _parent.FS.FldrPythonStaging,
                                                   backupLoader: SubjectMapBackupLoader,
                                                   backupFilepath: Path.Combine(_parent.FS.FldrPythonStaging,
                                                                                _defaults.BackupFile_SubjectMap),
                                                   askUserOnError: false,
                                                   commonWords: CommonWords);

                _subjectMap.PropertyChanged += SubjectMap_PropertyChanged;

                return _subjectMap;
            }

        }

        private IList<ISubjectMapEntry> SubjectMapBackupLoader(string filepath)
        {
            var subjectMapEntries = new List<ISubjectMapEntry>();

            string[] fileContents = FileIO2.CSV_Read(filename: Path.GetFileName(filepath), fileaddress: Path.GetDirectoryName(filepath), SkipHeaders: true);
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