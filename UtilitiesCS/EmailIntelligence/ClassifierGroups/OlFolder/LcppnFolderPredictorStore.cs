using System.IO;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder
{
    /// <summary>
    /// Encapsulates the dedicated on-disk location for the hierarchy-aware LCPPN folder predictor.
    /// The predictor is persisted to its OWN file, distinct from the flat <c>Folder.json</c>, in the
    /// same <c>AppData/Bayesian</c> directory used by the classifier build path. Centralizing the
    /// file name as a single named constant keeps the serialize (build) and deserialize (startup
    /// load) paths in agreement and makes the location unit-test reachable without a live filesystem.
    /// </summary>
    public static class LcppnFolderPredictorStore
    {
        /// <summary>
        /// The dedicated file name for the serialized LCPPN predictor. Deliberately distinct from
        /// <c>Folder.json</c> (the flat <c>Manager["Folder"]</c> document) so the two never collide.
        /// </summary>
        public const string FileName = "LcppnFolder.json";

        /// <summary>
        /// The sub-folder (under the resolved <c>AppData</c> special folder) that holds the Bayesian
        /// classifier artifacts, matching the classifier build path.
        /// </summary>
        public const string BayesianSubFolder = "Bayesian";

        /// <summary>
        /// Builds a <see cref="NewSmartSerializableConfig"/> whose <c>Disk</c> targets the dedicated
        /// LCPPN file inside the Bayesian folder under <paramref name="appDataFolder"/>, with the
        /// JSON settings the predictor serializes and rehydrates with (see <see cref="BuildSettings"/>).
        /// </summary>
        /// <param name="appDataFolder">The resolved AppData special folder; must not be null/empty.</param>
        /// <returns>A serialization config pointing at the dedicated LCPPN file.</returns>
        public static NewSmartSerializableConfig BuildConfig(string appDataFolder)
        {
            appDataFolder.ThrowIfNullOrEmpty();
            var bayesianFolder = Path.Combine(appDataFolder, BayesianSubFolder);
            var config = new NewSmartSerializableConfig
            {
                Disk = new FilePathHelper(FileName, bayesianFolder),
            };
            config.JsonSettings = BuildSettings();
            return config;
        }

        /// <summary>
        /// Builds the JSON settings used for both the serialize (build) and deserialize (startup
        /// load) paths so they agree. <c>PreserveReferencesHandling.Objects</c> is required because
        /// the per-parent shared token base holds a back-reference to its owner. The runtime-only
        /// <c>Config</c> property is excluded from the document: it carries a populated
        /// <see cref="FilePathHelper"/> whose property-change re-entrancy is not deserialization-safe,
        /// and per the <see cref="SmartSerializable{T}"/> contract the loader supplies the Config
        /// (file name/path) on load, so it never needs to live in the file.
        /// </summary>
        /// <returns>The shared serialize/deserialize settings.</returns>
        public static JsonSerializerSettings BuildSettings()
        {
            var settings = SmartSerializable<LcppnFolderPredictor>.GetDefaultSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.ContractResolver = new DoNotSerializeContractResolver("Config");
            return settings;
        }
    }
}
