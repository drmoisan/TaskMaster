using System;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// Scalar auto-file settings of <see cref="AppAutoFileObjects"/>, read and persisted through an
    /// injected per-instance <see cref="ISettingsStore"/>.
    /// </summary>
    public partial class AppAutoFileObjects
    {
        private readonly ISettingsStore _settingsStore;

        /// <summary>
        /// Initializes the auto-file objects with the scalar settings held in
        /// <paramref name="settingsStore"/>.
        /// </summary>
        /// <param name="parent">The owning application globals.</param>
        /// <param name="settingsStore">The store that holds the scalar auto-file settings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settingsStore"/> is null.</exception>
        internal AppAutoFileObjects(IApplicationGlobals parent, ISettingsStore settingsStore)
        {
            _settingsStore =
                settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
            _parent = parent;
        }

        public int LngConvCtPwr
        {
            get => (int)_settingsStore[nameof(Properties.Settings.ConversationExponent)];
            set
            {
                _settingsStore[nameof(Properties.Settings.ConversationExponent)] = value;
                _settingsStore.Save();
            }
        }

        public int Conversation_Weight
        {
            get => (int)_settingsStore[nameof(Properties.Settings.ConversationWeight)];
            set
            {
                _settingsStore[nameof(Properties.Settings.ConversationWeight)] = value;
                _settingsStore.Save();
            }
        }

        public int SmithWatterman_MatchScore
        {
            get => (int)_settingsStore[nameof(Properties.Settings.SmithWatterman_MatchScore)];
            set
            {
                _settingsStore[nameof(Properties.Settings.SmithWatterman_MatchScore)] = value;
                _settingsStore.Save();
            }
        }

        public int SmithWatterman_MismatchScore
        {
            get => (int)_settingsStore[nameof(Properties.Settings.SmithWatterman_MismatchScore)];
            set
            {
                _settingsStore[nameof(Properties.Settings.SmithWatterman_MismatchScore)] = value;
                _settingsStore.Save();
            }
        }

        public int SmithWatterman_GapPenalty
        {
            get => (int)_settingsStore[nameof(Properties.Settings.SmithWatterman_GapPenalty)];
            set
            {
                _settingsStore[nameof(Properties.Settings.SmithWatterman_GapPenalty)] = value;
                _settingsStore.Save();
            }
        }

        public int MaxRecents
        {
            get => (int)_settingsStore[nameof(Properties.Settings.MaxRecents)];
            set
            {
                _settingsStore[nameof(Properties.Settings.MaxRecents)] = value;
                _settingsStore.Save();
            }
        }
    }
}
