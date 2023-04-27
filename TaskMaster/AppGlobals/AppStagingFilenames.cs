using UtilitiesVB;

namespace TaskMaster
{

    public class AppStagingFilenames : IAppStagingFilenames
    {

        private string _recentsFile = "9999999RecentsFile.txt";
        private string _emailMoves = "999999EmailMoves.tsv";
        private string _emailSession = "99999EmailSession.csv";
        private string _emailSessionTemp = "99999EmailSession_Tmp.csv";
        private string _ctfMap = "9999999CTF_Map.txt";
        private string _ctfInc = "9999999CTF_Inc.txt";
        private string _subjectMap = "9999999Subject_Map.txt";
        private string _commonWords = "9999999CommonWords.txt";
        private string _conditionalReminders = "999999ConditionalReminders.txt";

        public string ConditionalReminders
        {
            get
            {
                return _conditionalReminders;
            }
            set
            {
                _conditionalReminders = value;
            }
        }

        public string CommonWords
        {
            get
            {
                return _commonWords;
            }
            set
            {
                _commonWords = value;
            }
        }

        public string SubjectMap
        {
            get
            {
                return _subjectMap;
            }
            set
            {
                _subjectMap = value;
            }
        }

        public string CtfInc
        {
            get
            {
                return _ctfInc;
            }
            set
            {
                _ctfInc = value;
            }
        }

        public string CtfMap
        {
            get
            {
                return _ctfMap;
            }
            set
            {
                _ctfMap = value;
            }
        }

        public string EmailSessionTemp
        {
            get
            {
                return _emailSessionTemp;
            }
            set
            {
                _emailSessionTemp = value;
            }
        }

        public string EmailSession
        {
            get
            {
                return _emailSession;
            }
            set
            {
                _emailSession = value;
            }
        }

        public string EmailMoves
        {
            get
            {
                return _emailMoves;
            }
            set
            {
                _emailMoves = value;
            }
        }

        public string RecentsFile
        {
            get
            {
                return _recentsFile;
            }
            set
            {
                _recentsFile = value;
            }
        }
    }
}