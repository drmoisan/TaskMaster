using UtilitiesCS;
using UtilitiesVB;

namespace TaskMaster
{

    public class AppAutoFileObjects : IAppAutoFileObjects
    {

        private bool _suggestionFilesLoaded = false;
        private int _smithWatterman_MatchScore;
        private int _smithWatterman_MismatchScore;
        private int _smithWatterman_GapPenalty;
        private IRecentsList<string> _recentsList;
        private IApplicationGlobals _parent;
        private CtfIncidenceList _ctfList;

        public AppAutoFileObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

        public long LngConvCtPwr
        {
            get
            {
                return Properties.Settings.Default.ConversationExponent;
            }
            set
            {
                Properties.Settings.Default.ConversationExponent = value;
                Properties.Settings.Default.Save();
            }
        }

        public long Conversation_Weight
        {
            get
            {
                return Properties.Settings.Default.ConversationWeight;
            }
            set
            {
                Properties.Settings.Default.ConversationWeight = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool SuggestionFilesLoaded
        {
            get
            {
                return _suggestionFilesLoaded;
            }
            set
            {
                _suggestionFilesLoaded = value;
            }
        }

        public int SmithWatterman_MatchScore
        {
            get
            {
                return Properties.Settings.Default.SmithWatterman_MatchScore;
            }
            set
            {
                Properties.Settings.Default.SmithWatterman_MatchScore = value;
                Properties.Settings.Default.Save();
            }
        }

        public int SmithWatterman_MismatchScore
        {
            get
            {
                return Properties.Settings.Default.SmithWatterman_MismatchScore;
            }
            set
            {
                Properties.Settings.Default.SmithWatterman_MismatchScore = value;
                Properties.Settings.Default.Save();
            }
        }

        public int SmithWatterman_GapPenalty
        {
            get
            {
                return Properties.Settings.Default.SmithWatterman_GapPenalty;
            }
            set
            {
                Properties.Settings.Default.SmithWatterman_GapPenalty = value;
                Properties.Settings.Default.Save();
            }
        }

        public long MaxRecents
        {
            get
            {
                return Properties.Settings.Default.MaxRecents;
            }
            set
            {
                Properties.Settings.Default.MaxRecents = value;
                Properties.Settings.Default.Save();
            }
        }

        public IRecentsList<string> RecentsList
        {
            get
            {
                if (_recentsList is null)
                {
                    _recentsList = new RecentsList<string>(Properties.Settings.Default.FileName_Recents, _parent.FS.FldrFlow, max: (int)MaxRecents);
                }
                return _recentsList;
            }
            set
            {
                _recentsList = value;
                {
                    ref var withBlock = ref _recentsList;
                    if (string.IsNullOrEmpty(withBlock.Folderpath))
                    {
                        withBlock.Folderpath = _parent.FS.FldrFlow;
                        withBlock.Filename = Properties.Settings.Default.FileName_Recents;
                    }
                }
                _recentsList.Serialize();
            }
        }

        public CtfIncidenceList CTFList
        {
            get
            {
                if (_ctfList is null)
                {
                    _ctfList = new CtfIncidenceList(filename: Properties.Settings.Default.File_CTF_Inc, folderpath: _parent.FS.FldrPythonStaging, backupFilepath: Properties.Settings.Default.BackupFile_CTF_Inc);
                }
                return _ctfList;
            }
            set
            {

            }
        }

    }
}