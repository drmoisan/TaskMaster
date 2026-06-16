using System.IO;
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
        /// LCPPN file inside the Bayesian folder under <paramref name="appDataFolder"/>.
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
            return config;
        }
    }
}
