using System;
using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster
{
    public class AppQuickFilerSettings : IAppQuickFilerSettings
    {
        private readonly ISettingsStore _settingsStore;

        /// <summary>
        /// Initializes settings that read and persist through <see cref="Settings.Default"/>.
        /// </summary>
        public AppQuickFilerSettings()
            : this(new ApplicationSettingsStore(Settings.Default)) { }

        /// <summary>
        /// Initializes settings that read and persist through <paramref name="settingsStore"/>.
        /// </summary>
        /// <param name="settingsStore">The store that holds the QuickFiler settings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settingsStore"/> is null.</exception>
        internal AppQuickFilerSettings(ISettingsStore settingsStore)
        {
            _settingsStore =
                settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        public bool MoveEntireConversation
        {
            get => (bool)_settingsStore[nameof(Settings.MoveEntireConversations)];
            internal set
            {
                _settingsStore[nameof(Settings.MoveEntireConversations)] = value;
                _settingsStore.Save();
            }
        }

        public bool SaveAttachments
        {
            get => (bool)_settingsStore[nameof(Settings.SaveAttachments)];
            internal set
            {
                _settingsStore[nameof(Settings.SaveAttachments)] = value;
                _settingsStore.Save();
            }
        }

        public bool SavePictures
        {
            get => (bool)_settingsStore[nameof(Settings.SavePictures)];
            internal set
            {
                _settingsStore[nameof(Settings.SavePictures)] = value;
                _settingsStore.Save();
            }
        }

        public bool SaveEmailCopy
        {
            get => (bool)_settingsStore[nameof(Settings.SaveEmailCopy)];
            internal set
            {
                _settingsStore[nameof(Settings.SaveEmailCopy)] = value;
                _settingsStore.Save();
            }
        }

        public bool HighConfidenceModeEnabled
        {
            get => (bool)_settingsStore[nameof(Settings.HighConfidenceModeEnabled)];
            internal set
            {
                _settingsStore[nameof(Settings.HighConfidenceModeEnabled)] = value;
                _settingsStore.Save();
            }
        }

        public double HighConfidenceThreshold
        {
            get => (double)_settingsStore[nameof(Settings.HighConfidenceThreshold)];
            internal set
            {
                _settingsStore[nameof(Settings.HighConfidenceThreshold)] = value;
                _settingsStore.Save();
            }
        }
    }
}
