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
        public AppFileSystemFolderPaths()
        {
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
                    () => [Environment.GetEnvironmentVariable("OneDriveConsumer")]
                )
            )
            {
                TryAddSpecialFolder(
                    "OneDrivePersonal",
                    () => [Environment.GetEnvironmentVariable("OneDrivePersonal")]
                );
            }
            if (
                !TryAddSpecialFolder(
                    "OneDrive",
                    () => [Environment.GetEnvironmentVariable("OneDriveCommercial")]
                )
            )
            {
                if (
                    !TryAddSpecialFolder(
                        "OneDrive",
                        () => [Environment.GetEnvironmentVariable("OneDrive")]
                    )
                )
                {
                    if (
                        !TryAddSpecialFolder(
                            "OneDrive",
                            () => [Environment.GetEnvironmentVariable("OneDrivePersonal")]
                        )
                    )
                    {
                        if (SpecialFolders.Count > 0)
                        {
                            if (SpecialFolders.TryGetValue("AppData", out var appData))
                            {
                                TryAddSpecialFolder("OneDrive", [appData]);
                            }
                            else
                            {
                                TryAddSpecialFolder("OneDrive", [SpecialFolders.First().Value]);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                "No know network or local folders set in environment variables"
                            );
                        }
                    }
                }
            }
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
