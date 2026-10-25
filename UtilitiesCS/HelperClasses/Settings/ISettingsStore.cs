#nullable enable

namespace UtilitiesCS
{
    /// <summary>
    /// Narrow persistence seam over an application settings collection. Consumers read and write
    /// named settings through the indexer and persist pending values with <see cref="Save"/>, so a
    /// test can supply an in-memory store instead of the per-host settings file.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>
        /// Gets or sets the value of the named setting.
        /// </summary>
        /// <param name="propertyName">The setting name, as declared by the settings class.</param>
        /// <returns>The current value of the setting.</returns>
        object this[string propertyName] { get; set; }

        /// <summary>
        /// Persists every pending setting value, mirroring
        /// <see cref="System.Configuration.ApplicationSettingsBase.Save"/>.
        /// </summary>
        void Save();
    }
}
