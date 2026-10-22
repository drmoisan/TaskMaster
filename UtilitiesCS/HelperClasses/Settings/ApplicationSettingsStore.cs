#nullable enable

using System;
using System.Configuration;

namespace UtilitiesCS
{
    /// <summary>
    /// Production <see cref="ISettingsStore"/> that forwards every read, write, and save to a wrapped
    /// <see cref="ApplicationSettingsBase"/> instance, such as an assembly's
    /// <c>Properties.Settings.Default</c>. Generated settings properties are implemented as indexer
    /// reads and writes, so routing through this adapter persists the same values as before.
    /// </summary>
    public sealed class ApplicationSettingsStore : ISettingsStore
    {
        private readonly ApplicationSettingsBase _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettingsStore"/> class.
        /// </summary>
        /// <param name="settings">The settings instance to wrap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
        public ApplicationSettingsStore(ApplicationSettingsBase settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public object this[string propertyName]
        {
            get => _settings[propertyName];
            set => _settings[propertyName] = value;
        }

        /// <inheritdoc />
        public void Save() => _settings.Save();
    }
}
