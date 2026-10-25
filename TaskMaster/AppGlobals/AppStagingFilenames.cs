using System;
using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster
{
    public class AppStagingFilenames : IAppStagingFilenames
    {
        private readonly ISettingsStore _settingsStore;

        /// <summary>
        /// Initializes file names that read and persist through <see cref="Settings.Default"/>.
        /// </summary>
        public AppStagingFilenames()
            : this(new ApplicationSettingsStore(Settings.Default)) { }

        /// <summary>
        /// Initializes file names that read and persist through <paramref name="settingsStore"/>.
        /// </summary>
        /// <param name="settingsStore">The store that holds the staging file-name settings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settingsStore"/> is null.</exception>
        internal AppStagingFilenames(ISettingsStore settingsStore)
        {
            _settingsStore =
                settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        private string _conditionalReminders;
        public string ConditionalReminders
        {
            get =>
                _conditionalReminders
                ?? InitProp(
                    ref _conditionalReminders,
                    Read(nameof(Settings.File_ConditionalReminders))
                );
            set
            {
                _conditionalReminders = value;
                _settingsStore[nameof(Settings.File_ConditionalReminders)] = value;
                _settingsStore.Save();
            }
        }

        private string _commonWords;
        public string CommonWords
        {
            get =>
                _commonWords
                ?? InitProp(ref _commonWords, Read(nameof(Settings.File_Common_Words)));
            set
            {
                _commonWords = value;
                _settingsStore[nameof(Settings.File_Common_Words)] = value;
                _settingsStore.Save();
            }
        }

        private string _subjectMap;
        public string SubjectMap
        {
            get =>
                _subjectMap ?? InitProp(ref _subjectMap, Read(nameof(Settings.File_Subject_Map)));
            set
            {
                _subjectMap = value;
                _settingsStore[nameof(Settings.File_Subject_Map)] = value;
                _settingsStore.Save();
            }
        }

        private string _ctfInc;
        public string CtfInc
        {
            get => _ctfInc ?? InitProp(ref _ctfInc, Read(nameof(Settings.File_CTF_Inc)));
            set
            {
                _ctfInc = value;
                _settingsStore[nameof(Settings.File_CTF_Inc)] = value;
                _settingsStore.Save();
            }
        }

        private string _ctfMap;
        public string CtfMap
        {
            get => _ctfMap ?? InitProp(ref _ctfMap, Read(nameof(Settings.File_CTF_Map)));
            set
            {
                _ctfMap = value;
                _settingsStore[nameof(Settings.File_CTF_Map)] = value;
                _settingsStore.Save();
            }
        }

        private string _emailSessionTemp;
        public string EmailSessionTemp
        {
            get =>
                _emailSessionTemp
                ?? InitProp(ref _emailSessionTemp, Read(nameof(Settings.FileName_EmailSessionTmp)));
            set
            {
                _emailSessionTemp = value;
                _settingsStore[nameof(Settings.FileName_EmailSessionTmp)] = value;
                _settingsStore.Save();
            }
        }

        private string _emailSession;
        public string EmailSession
        {
            get =>
                _emailSession
                ?? InitProp(ref _emailSession, Read(nameof(Settings.FileName_EmailSession)));
            set
            {
                _emailSession = value;
                _settingsStore[nameof(Settings.FileName_EmailSession)] = value;
                _settingsStore.Save();
            }
        }

        private string _movedMails;
        public string MovedMails
        {
            get =>
                _movedMails
                ?? InitProp(ref _movedMails, Read(nameof(Settings.FileName_MovedEmailsBackup)));
            set
            {
                _movedMails = value;
                _settingsStore[nameof(Settings.FileName_MovedEmailsBackup)] = value;
                _settingsStore.Save();
            }
        }

        private string _recentsFile;
        public string RecentsFile
        {
            get =>
                _recentsFile ?? InitProp(ref _recentsFile, Read(nameof(Settings.FileName_Recents)));
            set
            {
                _recentsFile = value;
                _settingsStore[nameof(Settings.FileName_Recents)] = value;
                _settingsStore.Save();
            }
        }

        private string _emailInfoStagingFile;
        public string EmailInfoStagingFile
        {
            get =>
                _emailInfoStagingFile
                ?? InitProp(
                    ref _emailInfoStagingFile,
                    Read(nameof(Settings.FileName_EmailInfoStaging))
                );
            set => _emailInfoStagingFile = value;
        }

        internal string InitProp(ref string prop, string value)
        {
            prop = value;
            return value;
        }

        private string Read(string settingName) => (string)_settingsStore[settingName];
    }
}
