using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster
{

    public class AppStagingFilenames : IAppStagingFilenames
    {
        private string _conditionalReminders;
        public string ConditionalReminders
        {
            get => _conditionalReminders ?? InitProp(ref _conditionalReminders, Settings.Default.File_ConditionalReminders);
            set
            {
                _conditionalReminders = value;
                Settings.Default.File_ConditionalReminders = value;
                Settings.Default.Save();
            }
        }

        private string _commonWords;
        public string CommonWords
        {
            get => _commonWords ?? InitProp(ref _commonWords, Settings.Default.File_Common_Words);
            set
            {
                _commonWords = value;
                Settings.Default.File_Common_Words = value;
                Settings.Default.Save();
            }
        }

        private string _subjectMap;
        public string SubjectMap
        {
            get => _subjectMap ?? InitProp(ref _subjectMap, Settings.Default.File_Subject_Map);
            set
            {
                _subjectMap = value;
                Settings.Default.File_Subject_Map = value;
                Settings.Default.Save();
            }
        }

        private string _ctfInc;
        public string CtfInc
        {
            get => _ctfInc ?? InitProp(ref _ctfInc, Settings.Default.File_CTF_Inc);
            set
            {
                _ctfInc = value;
                Settings.Default.File_CTF_Inc = value;
                Settings.Default.Save();
            }
        }

        private string _ctfMap;
        public string CtfMap
        {
            get => _ctfMap ?? InitProp(ref _ctfMap, Settings.Default.File_CTF_Map);
            
            set
            {
                _ctfMap = value;
                Settings.Default.File_CTF_Map = value;
                Settings.Default.Save();
            }
        }

        private string _emailSessionTemp;
        public string EmailSessionTemp
        {
            get => _emailSessionTemp ?? InitProp(ref _emailSessionTemp, Settings.Default.FileName_EmailSessionTmp);
            
            set
            {
                _emailSessionTemp = value;
                Settings.Default.FileName_EmailSessionTmp = value;
                Settings.Default.Save();
            }
        }

        private string _emailSession;
        public string EmailSession
        {
            get => _emailSession ?? InitProp(ref _emailSession, Settings.Default.FileName_EmailSession);
            set
            {
                _emailSession = value;
                Settings.Default.FileName_EmailSession = value;
                Settings.Default.Save();
            }
        }

        private string _movedMails;
        public string MovedMails
        {
            get => _movedMails ?? InitProp(ref _movedMails, Settings.Default.FileName_MovedEmailsBackup);
            set
            {
                _movedMails = value;
                Settings.Default.FileName_MovedEmailsBackup = value;
                Settings.Default.Save();
            }
        }

        private string _recentsFile;
        public string RecentsFile
        {
            get => _recentsFile ?? InitProp(ref _recentsFile, Settings.Default.FileName_Recents);
            set
            {
                _recentsFile = value;
                Settings.Default.FileName_Recents = value;
                Settings.Default.Save();
            }
        }

        private string _emailInfoStagingFile;
        public string EmailInfoStagingFile { get => _emailInfoStagingFile ?? InitProp(ref _emailInfoStagingFile, Settings.Default.FileName_EmailInfoStaging); set => _emailInfoStagingFile = value;}

        internal string InitProp(ref string prop, string value)
        {
            prop = value;
            return value;
        }
    }
}