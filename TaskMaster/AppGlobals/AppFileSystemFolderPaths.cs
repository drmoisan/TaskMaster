using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace TaskMaster
{
    public class AppFileSystemFolderPaths : IFileSystemFolderPaths
    {
        /// <summary>
        /// Environment-variable read seam (#614 D7). Defaults to
        /// <see cref="Environment.GetEnvironmentVariable(string)"/>. Tests inject a delegate so
        /// that no test mutates process environment state. No NEW environment variable is read.
        /// </summary>
        private Func<string, string> _readEnvironmentVariable = Environment.GetEnvironmentVariable;

        public AppFileSystemFolderPaths()
        {
            LoadFolders();
            _filenames = new AppStagingFilenames();
        }

        /// <summary>Test seam constructor: supplies the environment reader used by LoadFolders.</summary>
        /// <param name="readEnvironmentVariable">Environment reader; null selects the default.</param>
        internal AppFileSystemFolderPaths(Func<string, string> readEnvironmentVariable)
        {
            if (readEnvironmentVariable != null)
            {
                _readEnvironmentVariable = readEnvironmentVariable;
            }

            LoadFolders();
            _filenames = new AppStagingFilenames();
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region ctor

        private AppFileSystemFolderPaths(bool async) { }

        public static async Task<AppFileSystemFolderPaths> LoadAsync()
        {
            var fs = new AppFileSystemFolderPaths(true);
            await fs.LoadFoldersAsync();
            fs._filenames = new AppStagingFilenames();
            return fs;
        }

        #endregion ctor

        #region Methods

        private void CreateMissingPaths(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
        }

        private async Task CreateMissingPathsAsync(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                await Task.Run(() => Directory.CreateDirectory(filepath));
            }
        }

        public string MatchBestSpecialFolder(string path)
        {
            // Delegate to the pure static helper so the matching logic can be unit-tested with an
            // in-memory folder collection (no filesystem access / no LoadFolders). The instance
            // SpecialFolders dictionary is passed unchanged; behavior is identical.
            return MatchBestSpecialFolder(SpecialFolders, path);
        }

        /// <summary>
        /// Pure special-folder matching helper. Given a folder collection and a path, returns the
        /// key of the entry whose value is contained in <paramref name="path"/> and is the longest
        /// such value, or null when the collection is null/empty or no value is contained.
        /// </summary>
        /// <remarks>
        /// Behavior is byte-for-byte identical to the original instance method body: a
        /// null/empty collection returns null; matching uses ordinal <c>string.Contains</c>;
        /// candidates are ordered by descending value length and the first key is returned
        /// (null when no candidate matches). No filesystem access. Introduced as a pure seam to
        /// enable deterministic unit testing without changing runtime behavior.
        /// </remarks>
        internal static string MatchBestSpecialFolder(
            IReadOnlyDictionary<string, string> specialFolders,
            string path
        )
        {
            if (specialFolders.IsNullOrEmpty())
            {
                return null;
            }
            var bestMatch = specialFolders
                .Where(x => path.Contains(x.Value))
                .OrderByDescending(x => x.Value.Length)
                .FirstOrDefault();
            return bestMatch.Key;
        }

        private bool TryAddSpecialFolder(string name, string[] pathParts)
        {
            if (name.IsNullOrEmpty())
            {
                return false;
            }
            else if (pathParts.IsNullOrEmpty())
            {
                logger.Debug(
                    $"Error in {nameof(TryAddSpecialFolder)} for key {nameof(name)} because {nameof(pathParts)} is null or empty. {TraceUtility.GetMyTraceString(new System.Diagnostics.StackTrace())}"
                );
                return false;
            }
            else if (pathParts.Any(x => x is null || x.Trim().IsNullOrEmpty()))
            {
                var locations = Enumerable
                    .Range(0, pathParts.Length)
                    .Where(i => pathParts[i] is null)
                    .Select(i => i.ToString())
                    .SentenceJoin();
                logger.Debug(
                    $"Error in {nameof(TryAddSpecialFolder)} for key {nameof(name)} because {nameof(pathParts)} has null elements at {locations}. {TraceUtility.GetMyTraceString(new System.Diagnostics.StackTrace())}"
                );
                return false;
            }

            SpecialFolders ??= [];

            try
            {
                SpecialFolders[name] = Path.Combine(pathParts);
                CreateMissingPaths(SpecialFolders[name]);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }
        }

        private bool TryAddSpecialFolder(string name, Func<string[]> predicate)
        {
            try
            {
                var parts = predicate();
                return TryAddSpecialFolder(name, parts);
            }
            catch (Exception e)
            {
                logger.Error(
                    $"Error in {nameof(TryAddSpecialFolder)}. {nameof(predicate)} threw the following exception {e.Message}",
                    e
                );
                return false;
            }
        }

        /// <summary>Environment variables consulted for the OneDrive root, in priority order.</summary>
        internal static readonly string[] OneDriveVariablesInPriorityOrder =
        {
            "OneDriveCommercial",
            "OneDrive",
            "OneDrivePersonal",
        };

        /// <summary>The redacted diagnostic raised when no OneDrive root is set (#614 D7).</summary>
        internal const string OneDriveUnresolvableRule =
            "No OneDrive root is set in the environment: OneDriveCommercial, OneDrive and OneDrivePersonal are all unset or empty, so there is no filesystem root to mirror the Outlook archive into. The values are withheld from this message because they contain a user-profile path.";

        /// <summary>
        /// Resolves the OneDrive root by reading the environment variables in priority order and
        /// returning the first non-empty value (#614 D7). Pure apart from the injected reader, so
        /// it is unit-testable without mutating process environment state.
        /// </summary>
        /// <param name="readEnvironmentVariable">The environment reader seam.</param>
        /// <returns>The highest-priority non-empty OneDrive root.</returns>
        /// <exception cref="InvalidOperationException">No variable yields a value. The message
        /// names the rule only; the values are withheld because they carry a user-profile path.
        /// </exception>
        internal static string ResolveOneDriveRoot(Func<string, string> readEnvironmentVariable)
        {
            if (readEnvironmentVariable is null)
            {
                throw new ArgumentNullException(nameof(readEnvironmentVariable));
            }

            foreach (string variable in OneDriveVariablesInPriorityOrder)
            {
                string value = readEnvironmentVariable(variable);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            logger.Error(OneDriveUnresolvableRule);
            throw new InvalidOperationException(OneDriveUnresolvableRule);
        }

        private void LoadFolders()
        {
            SpecialFolders = [];
            TryAddSpecialFolder(
                "AppData",
                () =>
                    [
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        nameof(TaskMaster),
                    ]
            );
            TryAddSpecialFolder(
                "MyDocuments",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)]
            );
            TryAddSpecialFolder(
                "UserProfile",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)]
            );
            TryAddSpecialFolder(
                "MyComputer",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)]
            );
            TryAddSpecialFolder(
                "Favorites",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.Favorites)]
            );
            TryAddSpecialFolder(
                "Personal",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.Personal)]
            );
            TryAddSpecialFolder(
                "ApplicationData",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)]
            );
            TryAddSpecialFolder(
                "Desktop",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)]
            );
            TryAddSpecialFolder(
                "NetworkShortcuts",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.NetworkShortcuts)]
            );
            if (
                !TryAddSpecialFolder(
                    "OneDrivePersonal",
                    () => [_readEnvironmentVariable("OneDriveConsumer")]
                )
            )
            {
                TryAddSpecialFolder(
                    "OneDrivePersonal",
                    () => [_readEnvironmentVariable("OneDrivePersonal")]
                );
            }

            // #614 D7: the OneDrive root is resolved from the environment in priority order and
            // fails EXPLICITLY when none is set. The previous AppData and first-arbitrary-entry
            // fallbacks silently produced a filing root that had nothing to do with OneDrive.
            TryAddSpecialFolder("OneDrive", [ResolveOneDriveRoot(_readEnvironmentVariable)]);
            SpecialFolders.TryGetValue("OneDrive", out var oneDrive);
            TryAddSpecialFolder("Flow", [oneDrive, "Email attachments from Flow"]);
            SpecialFolders.TryGetValue("Flow", out var flow);
            TryAddSpecialFolder("PreReads", [oneDrive, "_  Workflow", "_ Pre-Reads"]);
            TryAddSpecialFolder(
                "System",
                () => [Environment.GetFolderPath(Environment.SpecialFolder.System)]
            );
            TryAddSpecialFolder(
                "Root",
                () =>
                    [Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))]
            );

            if (SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                _remap = Path.Combine(myDocuments, "dictRemap.csv");
            }

            TryAddSpecialFolder("PythonStaging", [flow, "Combined", "data"]);
        }

        //TODO: Cleanup Staging Files so that they are in one or two directories and not all over the place
        private async Task LoadFoldersAsync()
        {
            await Task.Run(LoadFolders);
        }

        public void Reload()
        {
            LoadFolders();
        }

        #endregion Methods

        #region Properties

        //private string _appData;
        //public string FldrAppData { get => _appData; protected set => _appData = value; }

        //private string _myDocuments;
        //public string FldrMyDocuments { get => _myDocuments; protected set => _myDocuments = value; }

        //private string _oneDrive;
        //public string FldrOneDrive { get => _oneDrive; protected set => _oneDrive = value; }

        //private string _flow;
        //public string FldrFlow { get => _flow; protected set => _flow = value; }

        //private string _prereads;
        //public string FldrPreReads { get => _prereads; protected set => _prereads = value; }

        //private string _fldrPythonStaging;
        //public string FldrPythonStaging { get => _fldrPythonStaging; protected set => _fldrPythonStaging = value; }

        private IAppStagingFilenames _filenames;
        public IAppStagingFilenames Filenames
        {
            get => _filenames;
            protected set => _filenames = value;
        }

        private ConcurrentDictionary<string, string> _specialFolders;
        public ConcurrentDictionary<string, string> SpecialFolders
        {
            get => _specialFolders;
            protected set => _specialFolders = value;
        }

        private string _remap;

        #endregion Properties
    }
}
