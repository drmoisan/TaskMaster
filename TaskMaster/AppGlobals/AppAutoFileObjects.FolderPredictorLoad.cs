using System;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;

namespace TaskMaster
{
    /// <summary>
    /// Folder-predictor (LCPPN) members of <see cref="AppAutoFileObjects"/>. Split into this partial
    /// file so the persisted-setting accessor and the load/rehydration logic do not grow the already
    /// over-cap <c>AppAutoFileObjects.cs</c> beyond the minimal wiring lines. The settings access
    /// lives here in the TaskMaster implementation; only the resolved <see cref="bool"/> crosses the
    /// <see cref="IAppAutoFileObjects"/> boundary, so <c>UtilitiesCS</c> never references
    /// <c>TaskMaster.Properties.Settings</c>.
    /// </summary>
    public partial class AppAutoFileObjects
    {
        private static readonly log4net.ILog _folderPredictorLogger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// The persisted production default that selects the LCPPN folder predictor. Sourced from
        /// <see cref="Properties.Settings.Default"/> so the default is ON and is honored by all
        /// production callers without a per-call flag; toggling the setting OFF restores the flat
        /// <c>Manager["Folder"]</c> path (AC13 parity).
        /// </summary>
        public bool UseLcppnPredictor => _defaults.UseLcppnPredictor;

        /// <summary>
        /// Deserialization seam for the LCPPN load path. Defaults to the production
        /// <see cref="SmartSerializable{T}"/> static deserialize (reads the dedicated file via the
        /// loader's <c>Config.Disk</c>), preserving runtime behavior. Tests override it with a
        /// deterministic, in-memory delegate so the rehydration path is verifiable without touching
        /// the filesystem or creating temporary files.
        /// </summary>
        internal Func<
            LcppnFolderPredictor,
            Task<LcppnFolderPredictor>
        > FolderPredictorDeserializer { get; set; } =
            loader => LcppnFolderPredictor.Static.DeserializeAsync(loader);

        /// <summary>
        /// Rehydrates <see cref="FolderPredictor"/> from the dedicated LCPPN file on startup when the
        /// persisted setting is ON, so the predictor survives a restart without a manual rebuild. The
        /// load is fail-soft: a missing or unreadable file leaves the holder null (logged), and the
        /// Folder seam then falls back to the flat <c>Manager["Folder"]</c> group (AC22). When the
        /// setting is OFF, no load is attempted and the flat path remains active (AC13).
        /// </summary>
        public async Task LoadFolderPredictorAsync()
        {
            if (!UseLcppnPredictor)
            {
                return;
            }

            if (!_parent.FS.SpecialFolders.TryGetValue("AppData", out var appDataFolder))
            {
                _folderPredictorLogger.Warn(
                    "AppData special folder not resolved; LCPPN predictor not loaded. "
                        + "Folder seam falls back to the flat classifier."
                );
                return;
            }

            try
            {
                var loader = new LcppnFolderPredictor
                {
                    Config = LcppnFolderPredictorStore.BuildConfig(appDataFolder),
                };

                // DeserializeAsync returns null when the dedicated file is absent (fail-soft); the
                // holder then stays null and the accessor falls back to flat. A genuine read/parse
                // failure is caught below and surfaced through logging without throwing on startup.
                var predictor = await FolderPredictorDeserializer(loader);
                if (predictor is null)
                {
                    _folderPredictorLogger.Warn(
                        $"LCPPN predictor file '{LcppnFolderPredictorStore.FileName}' not found "
                            + "or empty; Folder seam falls back to the flat classifier."
                    );
                    return;
                }

                FolderPredictor = predictor;
            }
            catch (Exception e)
            {
                // Fail-soft: leave FolderPredictor null so the flat fallback is used. The error is
                // logged so genuine corruption is visible rather than silently swallowed.
                _folderPredictorLogger.Error(
                    $"Failed to load LCPPN predictor file "
                        + $"'{LcppnFolderPredictorStore.FileName}'. Folder seam falls back to the "
                        + "flat classifier.",
                    e
                );
            }
        }
    }
}
