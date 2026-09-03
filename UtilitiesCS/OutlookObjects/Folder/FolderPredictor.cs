#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public partial class FolderPredictor
    {
        internal static string NormalizePredictionPath(string input)
        {
            return input ?? string.Empty;
        }

        #region Constructors and Initialization

        public FolderPredictor(Outlook.Application olApp)
        {
            _olApp = olApp;
            // Navigation-only ctor: _globals is populated by the globals-providing ctors and by Init*;
            // this overload only exposes GetFolder(folderpath, olApp). Callers that need globals-dependent
            // members must use a globals-providing ctor (pre-existing contract). _suggestions defaults via
            // its field initializer.
            _globals = null!;
        }

        public FolderPredictor(IApplicationGlobals AppGlobals)
        {
            _globals = AppGlobals;
            _olApp = AppGlobals.Ol.App;
            Suggestions = new FolderScorer();
        }

        public FolderPredictor(IApplicationGlobals appGlobals, object objItem, InitOptions options)
        {
            _globals = appGlobals;
            _olApp = appGlobals.Ol.App;

            Suggestions = new FolderScorer();
        }

        public async Task<FolderPredictor> InitAsync(object objItem, InitOptions options)
        {
            switch (options)
            {
                case InitOptions.NoSuggestions:
                    break;
                case InitOptions.FromArrayOrString:
                    FromArrayOrString(objItem);
                    break;
                case InitOptions.FromField:
                    await InitializeFromEmail(objItem);
                    break;
                case InitOptions.Recalculate:
                    RefreshSuggestions(objItem);
                    break;
                default:
                    throw new ArgumentException($"Unknown option value {options}");
            }
            return this;
        }

        public enum InitOptions
        {
            NoSuggestions = 0,
            FromArrayOrString = 1,
            FromField = 2,
            Recalculate = 4,
        }

        public async Task InitializeFromEmail(object objItem) //internal
        {
            if (objItem is null)
            {
                throw new ArgumentException(
                    "Cannot initialize suggestions from email because reference is null"
                );
            }
            else if (objItem is MailItemHelper)
            {
                var mailInfo = (MailItemHelper)objItem;
                await FromFolderKey(mailInfo);
            }
            else if (objItem is MailItem && MailResolution.TryResolveMailItem(objItem) is not null)
            {
                FromFolderKey((MailItem)objItem);
            }
            else
            {
                throw new ArgumentException(
                    $"Obj is of type {objItem.GetType().Name}, but selected option requires a MailItem or MailItemHelper"
                );
            }
        }

        public void FromArrayOrString(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentException(
                    "Cannot initialize suggestions from array or string because reference is null"
                );
            }
            else if (
                obj.GetType().IsArray
                && typeof(string).IsAssignableFrom(obj.GetType().GetElementType())
            )
            {
                _folderList = new List<string>((string[])obj);
                //Suggestions.FromArray((string[])Obj);
            }
            else if (obj is string)
            {
                string tmpString = (string)obj;
                Suggestions.AddSuggestion(tmpString, 0);
            }
            else
            {
                throw new ArgumentException(
                    $"Obj is of type {obj.GetType().Name}, but selected option requires a string or string array"
                );
            }
        } //internal

        public void FromFolderKey(MailItem olMail) //internal
        {
            if (!Suggestions.LoadFromField(olMail, _globals))
            {
                Suggestions.RefreshSuggestions(olMail: olMail, appGlobals: _globals);
            }
        }

        public async Task FromFolderKey(MailItemHelper mailInfo) //internal
        {
            if (!Suggestions.LoadFromField(mailInfo, _globals))
            {
                await Suggestions.RefreshSuggestions(mailInfo: mailInfo, appGlobals: _globals);
            }
        }

        #endregion

        #region Private Fields

        private IApplicationGlobals _globals;

        private Outlook.Application _olApp;
        private Regex? _regex;

        //private string _searchString;

        internal static Func<string, string, string> PromptForFolderNameDialog { get; set; } =
            (prompt, title) => InputBox.ShowDialog(prompt, title)!;

        internal static Func<
            string,
            string,
            string,
            string
        > PromptForFolderNameWithDefaultDialog { get; set; } =
            (prompt, title, defaultValue) => InputBox.ShowDialog(prompt, title, defaultValue)!;

        internal static Action<string> ShowPromptMessageAction { get; set; } =
            message => MessageBox.Show(message);

        internal static Func<Task> EnterUiContextAsyncAction { get; set; } =
            () =>
            {
                var taskCompletionSource = new TaskCompletionSource<bool>();
                UiThread.UiSyncContext.Post(_ => taskCompletionSource.SetResult(true), null);
                return taskCompletionSource.Task;
            };

        internal static Func<string, DirectoryInfo> CreateDirectoryPathFactory { get; set; } =
            path => Directory.CreateDirectory(path);

        internal virtual string PromptForFolderName(
            string prompt,
            string title,
            string? defaultValue = null
        )
        {
            return defaultValue is null
                ? PromptForFolderNameDialog(prompt, title)
                : PromptForFolderNameWithDefaultDialog(prompt, title, defaultValue);
        }

        internal virtual void ShowPromptMessage(string message)
        {
            ShowPromptMessageAction(message);
        }

        internal virtual Task EnterUiContextAsync()
        {
            return EnterUiContextAsyncAction();
        }

        internal virtual DirectoryInfo CreateDirectoryPath(string path)
        {
            return CreateDirectoryPathFactory(path);
        }

        #endregion

        #region Public Properties

        private List<string>? _folderList;
        public string[] FolderArray
        {
            get
            {
                if ((_folderList is null) || (_folderList.Count == 0))
                {
                    _folderList = new List<string>();
                    if (Suggestions.Count > 0)
                        AddSuggestions(ref _folderList);
                    if (_globals.AF.RecentsList.Count > 0)
                        AddRecents(ref _folderList);
                }

                return _folderList.ToArray();
            }
        }

        /// <summary>
        /// Additive row-model mirror of <see cref="FolderArray"/>. Produces the same ordered
        /// sequence of rows, each <see cref="FolderRow.Text"/> equal to the corresponding
        /// <see cref="FolderArray"/> string: the "========= SUGGESTIONS =========" separator
        /// (<see cref="FolderRowKind.Separator"/>), the top-5 scored suggestions
        /// (<see cref="FolderRowKind.Suggestion"/> with a non-null <see cref="FolderRow.Score"/>),
        /// the "======= RECENT SELECTIONS ========" separator, and the recent selections
        /// (<see cref="FolderRowKind.Recent"/>). This getter does not mutate the cached
        /// <c>_folderList</c>, so <see cref="FolderArray"/> output is unaffected.
        /// </summary>
        public FolderRow[] FolderRowArray
        {
            get
            {
                var rows = new List<FolderRow>();
                if (Suggestions.Count > 0)
                {
                    AddSuggestionRows(rows);
                }
                if (_globals.AF.RecentsList.Count > 0)
                {
                    AddRecentRows(rows);
                }
                return rows.ToArray();
            }
        }

        // Set via a globals-providing ctor / Init*; null! documents the navigation-only ctor path
        // where Suggestions is not populated (pre-existing contract). The public property stays
        // non-null to satisfy IFolderSearchHandler.Suggestions.
        private FolderScorer _suggestions = null!;
        public FolderScorer Suggestions
        {
            get => _suggestions;
            set => _suggestions = value;
        }

        private bool _blUpdateSuggestions;
        public bool BlUpdateSuggestions
        {
            get => _blUpdateSuggestions;
            set => _blUpdateSuggestions = value;
        }

        #endregion

        #region public Methods

        /// <summary>
        /// Function returns a list of Outlook folders that meet search criteria and appends a list of suggested folders
        /// as well as appending a list of recently used folders
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="reloadCTFStagingFiles"></param>
        /// <param name="emailSearchRoots"></param>
        /// <param name="recalcSuggestions"></param>
        /// <param name="objItem"></param>
        /// <param name="exclusions">Folders to exclude from the search results</param>
        /// <returns></returns>
        public string[] FindFolder(
            string searchString,
            object objItem,
            bool reloadCTFStagingFiles = true,
            List<string>? emailSearchRoots = null,
            bool recalcSuggestions = false,
            IEnumerable<(string root, string excludedFolder, bool excludeChildren)>? exclusions =
                null
        )
        {
            if (emailSearchRoots is null)
            {
                emailSearchRoots = new() { _globals.Ol.ArchiveRootPath };
            }
            if (exclusions is null)
            {
                exclusions = new List<(string root, string excludedFolder, bool excludeChildren)>();
            }

            _folderList = new List<string>();

            // Add search results
            var matchingFolders = emailSearchRoots
                .Select(root =>
                    GetMatchingFolders(
                        searchString,
                        root,
                        includeChildren: true,
                        exclusions
                            .Where(x => x.root == root)
                            .Select(x => (x.excludedFolder, x.excludeChildren))
                    )
                )
                .SelectMany(x => x)
                .ToList();

            //var matchingFolders = GetMatchingFolders(searchString, emailSearchRoots);
            AddMatches(matchingFolders);

            // Add suggestions
            if (recalcSuggestions)
            {
                RefreshSuggestions(objItem);
            }
            AddSuggestions(ref _folderList);

            // Add recents
            AddRecents(ref _folderList);

            return FolderArray;
        }

        /// <summary>
        /// Additive row-model mirror of <see cref="FindFolder"/> with the same signature and the
        /// same ordered output. Each <see cref="FolderRow.Text"/> equals the corresponding
        /// <see cref="FindFolder"/> string: the "======= SEARCH RESULTS =======" separator
        /// (<see cref="FolderRowKind.Separator"/>) and matching folders
        /// (<see cref="FolderRowKind.SearchResult"/>), the "========= SUGGESTIONS =========" separator
        /// and the top-5 scored suggestions (<see cref="FolderRowKind.Suggestion"/> with a non-null
        /// <see cref="FolderRow.Score"/>), then the "======= RECENT SELECTIONS ========" separator and
        /// the recents (<see cref="FolderRowKind.Recent"/>). Only <see cref="FolderRowKind.Suggestion"/>
        /// rows carry a <see cref="FolderRow.Score"/>. This method does not mutate the cached
        /// <c>_folderList</c>, so <see cref="FindFolder"/> output is unaffected.
        /// </summary>
        /// <param name="searchString"><inheritdoc cref="FindFolder"/></param>
        /// <param name="objItem"><inheritdoc cref="FindFolder"/></param>
        /// <param name="reloadCTFStagingFiles"><inheritdoc cref="FindFolder"/></param>
        /// <param name="emailSearchRoots"><inheritdoc cref="FindFolder"/></param>
        /// <param name="recalcSuggestions"><inheritdoc cref="FindFolder"/></param>
        /// <param name="exclusions">Folders to exclude from the search results</param>
        /// <returns>The assembled folder rows in the same order as <see cref="FindFolder"/>.</returns>
        public FolderRow[] FindFolderRows(
            string searchString,
            object objItem,
            bool reloadCTFStagingFiles = true,
            List<string>? emailSearchRoots = null,
            bool recalcSuggestions = false,
            IEnumerable<(string root, string excludedFolder, bool excludeChildren)>? exclusions =
                null
        )
        {
            if (emailSearchRoots is null)
            {
                emailSearchRoots = new() { _globals.Ol.ArchiveRootPath };
            }
            if (exclusions is null)
            {
                exclusions = new List<(string root, string excludedFolder, bool excludeChildren)>();
            }

            var rows = new List<FolderRow>();

            // Add search results (mirrors the FindFolder search block + AddMatches)
            var matchingFolders = emailSearchRoots
                .Select(root =>
                    GetMatchingFolders(
                        searchString,
                        root,
                        includeChildren: true,
                        exclusions
                            .Where(x => x.root == root)
                            .Select(x => (x.excludedFolder, x.excludeChildren))
                    )
                )
                .SelectMany(x => x)
                .ToList();

            AddMatchRows(rows, matchingFolders);

            // Add suggestions (unconditional, mirroring the FindFolder AddSuggestions call)
            if (recalcSuggestions)
            {
                RefreshSuggestions(objItem);
            }
            AddSuggestionRows(rows);

            // Add recents (mirrors AddRecents, which gates internally on the recents count)
            AddRecentRows(rows);

            return rows.ToArray();
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> based on a rooted <seealso cref="Folder"/>.FolderPath
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="Folder"/>.FolderPath</param>
        /// <param name="olApp">Handle on the <seealso cref="Outlook.Application"/></param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath
        /// or <c>null</c> if not found</returns>
        public Folder? GetFolder(string folderpath, Outlook.Application olApp)
        {
            if (folderpath.Substring(0, 2) == @"\\")
            {
                folderpath = folderpath.Substring(2);
            }
            // Convert folderpath to array
            var foldersArray = folderpath.Split(@"\");

            var matchedFolder = GetFolder(olApp.Session.Folders, foldersArray[0]);
            if (matchedFolder is null)
            {
                return null;
            }

            for (int i = 1; i < foldersArray.Length; i++)
            {
                matchedFolder = GetFolder(matchedFolder.Folders, foldersArray[i]);
                if (matchedFolder is null)
                {
                    return null;
                }
            }

            return matchedFolder;
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> based on a rooted <seealso cref="Folder"/>.FolderPath.
        /// Uses the <seealso cref="Outlook.Application"/> stored in the <see cref="FolderPredictor"/> instance.
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="MAPIFolder.FolderPath"/></param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath
        /// or <c>null</c> if not found</returns>
        /// <exception cref="ArgumentException"><paramref name="folderpath"/> should be rooted </exception>
        public Folder? GetFolder(string folderpath)
        {
            // Check that folderpath is rooted
            var root = _globals.Ol.Root.FolderPath;
            if (!folderpath.Contains(root))
            {
                throw new ArgumentException(
                    $"The parameter {nameof(folderpath)} value {folderpath} does not contain the root {root}",
                    nameof(folderpath)
                );
            }

            return GetFolder(folderpath, _olApp);
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> represented by the rooted <seealso cref="Folder"/>.FolderPath.
        /// Uses the <seealso cref="Outlook.Application"/> stored in the <see cref="FolderPredictor"/> instance. If the
        /// targeted folder is not found, an exception is thrown or a message is delivered to the user based on the
        /// value of the <paramref name="throwEx"/> parameter.
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="MAPIFolder.FolderPath"/></param>
        /// <param name="throwEx">Flag to determine if exception should be thrown or message delivered to user</param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath
        /// or <c>null</c> if not found</returns>
        /// <exception cref="ArgumentException"><paramref name="folderpath"/> should be rooted </exception>
        public Folder? GetFolder(string folderpath, bool throwEx)
        {
            // Check that folderpath is rooted
            var root = _globals.Ol.Root.FolderPath;
            if (!folderpath.Contains(root))
            {
                throw new ArgumentException(
                    $"The parameter {nameof(folderpath)} value {folderpath} does not contain the root {root}",
                    nameof(folderpath)
                );
            }

            // Get the Folder
            var olFolder = GetFolder(folderpath, _olApp);

            // If folder is null, throw exception or deliver message to user
            if (olFolder is null)
            {
                string message =
                    $"Selected folder {folderpath} does not exist. "
                    + "Staging Files out of sync with current directory state.";
                if (throwEx)
                {
                    throw new ArgumentException(message, nameof(folderpath));
                }
                else
                {
                    ShowPromptMessage(message);
                }
            }
            return olFolder;
        }

        /// <summary>
        /// Function selects the <seealso cref="Folder"/> in the <seealso cref="Folders"/> collection whose
        /// Name property matches the argument <paramref name="childName"/>.
        /// </summary>
        /// <param name="children"><seealso cref="Folders"/> collection to search</param>
        /// <param name="childName">Name of the <seealso cref="Folder"/> to match</param>
        /// <returns>The <seealso cref="Folder"/> if found or <c>null</c></returns>
        public Folder? GetFolder(Folders children, string childName)
        {
            var folderLevelNames = children.Cast<MAPIFolder>().Select(x => x.Name).ToList();
            if (folderLevelNames.Contains(childName))
            {
                return (Folder)children[childName];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method asks the user to input a name for a new child folder of the parent folder
        /// supplied as an argument. Utilizes <seealso cref="InputBox"/> to get the user input.
        /// User is notified if name contains illegal characters, is too long, or represents an
        /// Outlook.<seealso cref="Folder"/> that already exists
        /// </summary>
        /// <param name="parent">The parent Outlook.<seealso cref="Folder"/> under which the
        /// new Outlook.<seealso cref="Folder"/> will be created</param>
        /// <returns>The name of the new Outlook.<seealso cref="Folder"/> to create</returns>
        public string? InputFoldername(Folder parent) //Internal
        {
            string? name = "";
            while (name is not null && name == "")
            {
                name = PromptForFolderName(
                    $"Please enter a new subfolder name for {parent.Name}",
                    "New folder dialog"
                );

                if (name is not null)
                {
                    if (!IsLegalFolderName(name))
                    {
                        ShowPromptMessage(
                            $"Folder name {name} contains the illegal characters "
                                + $"{GetIllegalFolderChars(name).SentenceJoin()}. Please choose a different name."
                        );
                        name = "";
                    }
                    else if (name.Length > 30)
                    {
                        ShowPromptMessage(
                            "Outlook limits folder names to 30 characters. Please choose a different name."
                        );
                        name = "";
                    }
                    else if (GetFolder(parent.Folders, name) is not null)
                    {
                        ShowPromptMessage("Folder already exists. Please choose a different name.");
                        name = "";
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// Async version of <see cref="InputFoldername(Folder)"/> which does the following:
        /// <inheritdoc cref="InputFoldername(Folder)"/>
        /// </summary>
        /// <param name="parent"><inheritdoc cref="InputFoldername(Folder)"/></param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A task with the name of the new Outlook.<seealso cref="Folder"/> to create</returns>
        public async Task<string?> InputFoldernameAsync(Folder parent, CancellationToken token) //Internal
        {
            token.ThrowIfCancellationRequested();
            string? name = "";
            while (name is not null && name == "")
            {
                await EnterUiContextAsync();
                name = PromptForFolderName(
                    $"Please enter a new subfolder name for {parent.Name}",
                    "New folder dialog"
                );

                token.ThrowIfCancellationRequested();
                if (name is not null)
                {
                    if (!IsLegalFolderName(name))
                    {
                        ShowPromptMessage(
                            $"Folder name {name} contains the illegal characters "
                                + $"{GetIllegalFolderChars(name).SentenceJoin()}. Please choose a different name."
                        );
                        name = "";
                    }
                    else if (name.Length > 30)
                    {
                        ShowPromptMessage(
                            "Outlook limits folder names to 30 characters. Please choose a different name."
                        );
                        name = "";
                    }
                    else if (GetFolder(parent.Folders, name) is not null)
                    {
                        ShowPromptMessage("Folder already exists. Please choose a different name.");
                        name = "";
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// Character array of illegal characters for either Outlook.<seealso cref="Folder"/>
        /// names or for System.IO.<seealso cref="DirectoryInfo"/> names.
        /// </summary>
        private static char[] IllegalFolderCharacters
        {
            get => @"[\/:*?""<>|].".ToCharArray();
        }

        /// <summary>
        /// Method is used for error reporting to identify which characters in a string cannot
        /// be used in either an Outlook.<seealso cref="Folder"/> name or a
        /// System.IO.<seealso cref="DirectoryInfo"/> name. See also <see cref="IllegalFolderCharacters"/>
        /// </summary>
        /// <param name="foldername">Name to check for illegal characters</param>
        /// <returns>Array of characters in the foldername that are illegal</returns>
        private char[] GetIllegalFolderChars(string foldername)
        {
            return foldername.Where(c => IllegalFolderCharacters.Contains(c)).ToArray();
        }

        /// <summary>
        /// Identifies if a foldername contains any illegal characters for either an
        /// Outlook.<seealso cref="Folder"/> name or a System.IO.<seealso cref="DirectoryInfo"/> name.
        /// </summary>
        /// <param name="foldername">Name to check for illegal characters</param>
        /// <returns><c>true</c> if no characters found. <c>false</c> if illegal
        /// characters are present</returns>
        private bool IsLegalFolderName(string foldername)
        {
            return !foldername.Any(c => IllegalFolderCharacters.Contains(c));
        }

        /// <summary>
        /// Method creates new parallel folders in Outlook Email and the File System.
        /// <list type="bullet">
        /// <item>Combines a relative folderpath with the fully rooted olAncestor folderpath
        /// to create an Outlook.<seealso cref="Folder"/>. </item>
        /// <item>The fully qualified Outlook folderpath applies the
        /// <seealso cref="FolderConverter.ToFsFolderpath(string, string, string)"/> extension
        /// to convert to a parallel folderpath.</item>
        /// <item>System.IO.<seealso cref="DirectoryInfo"/> creates this parallel folder in the file system.</item>
        /// </list>
        /// </summary>
        /// <param name="parentBranchPath">Parent FolderPath to Outlook.<seealso cref="Folder"/>
        /// excluding the FolderPath of the Outlook ancestor in the path</param>
        /// <param name="olAncestor">Fully rooted Outlook.<seealso cref="Folder"/>.FolderPath of Ancestor <seealso cref="Folder"/></param>
        /// <param name="fsAncestor">Fully qualified File System path</param>
        /// <returns>The created Outlook.<seealso cref="Folder"/></returns>
        public MAPIFolder? CreateFolder(
            string parentBranchPath,
            string olAncestor,
            string fsAncestor
        )
        {
            // Set default root if not provided
            if (olAncestor.IsNullOrEmpty())
            {
                olAncestor = _globals.Ol.ArchiveRootPath;
            }

            // Fully root the folderpath
            string parentFolderpath;
            if (
                olAncestor.EndsWith("\\", StringComparison.Ordinal)
                || (parentBranchPath.Length > 0 && parentBranchPath[0] == '\\')
            )
            {
                parentFolderpath = $"{olAncestor}{parentBranchPath}";
            }
            else
            {
                parentFolderpath = $"{olAncestor}\\{parentBranchPath}";
            }

            // Get the parent folder and return null if not found
            var parentFolder = this.GetFolder(parentFolderpath, false);
            if (parentFolder is null)
            {
                return null;
            }

            // Get the new folder name from the user
            string? newFolderName = InputFoldername(parentFolder);
            if (newFolderName is null)
            {
                return null;
            }

            // Create the new folder in Outlook
            var olFolder = parentFolder.Folders.Add(newFolderName);

            // Convert the Outlook folderpath to a filesystem folderpath
            var fsFolderName = olFolder.ToFsFolderpath(olAncestor, fsAncestor);

            // Create the new folder in the filesystem
            var fsFolder = CreateDirectoryPath(fsFolderName);

            // Return the new Outlook folder
            return olFolder;
        }

        /// <summary>
        /// <para>Async version of <see cref="CreateFolder(string, string, string)"/> which does the following:</para>
        /// <inheritdoc cref="CreateFolder(string, string, string)"/>
        /// </summary>
        /// <param name="parentBranchPath"><inheritdoc cref="CreateFolder(string, string, string)"/></param>
        /// <param name="olAncestor"><inheritdoc cref="CreateFolder(string, string, string)"/></param>
        /// <param name="fsAncestor"><inheritdoc cref="CreateFolder(string, string, string)"/></param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A Task of the created Outlook.<seealso cref="MAPIFolder"/> returned as object</returns>
        public async Task<object?> CreateFolderAsync(
            string parentBranchPath,
            string olAncestor,
            string fsAncestor,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            // Set default root if not provided
            if (olAncestor.IsNullOrEmpty())
            {
                olAncestor = _globals.Ol.ArchiveRootPath;
            }

            // Fully root the folderpath
            var parentFolderpath = $"{olAncestor}\\{parentBranchPath}";

            // Get the parent folder and return null if not found
            var parentFolder = this.GetFolder(parentFolderpath, false);
            if (parentFolder is null)
            {
                return null;
            }

            // Get the new folder name from the user
            string? newFolderName = await InputFoldernameAsync(parentFolder, token);
            if (newFolderName is null)
            {
                return null;
            }

            // Create the new folder in Outlook
            var olFolder = parentFolder.Folders.Add(newFolderName);

            // Convert the Outlook folderpath to a filesystem folderpath
            var fsFolderName = olFolder.ToFsFolderpath(olAncestor, fsAncestor);

            // Create the new folder in the filesystem
            var fsFolder = CreateDirectoryPath(fsFolderName);

            // Return the new Outlook folder
            return olFolder;
        }

        #endregion

        #region Helper Functions

        public void AddRecents(ref List<string> folderList) // internal
        {
            if (_globals.AF.RecentsList.Count > 0)
            {
                folderList.Add("======= RECENT SELECTIONS ========");
                folderList.AddRange(_globals.AF.RecentsList);
            }
        }

        public void AddMatches(List<string> matchingFolders) // internal
        {
            if (matchingFolders is not null && matchingFolders.Count > 0)
            {
                matchingFolders = matchingFolders.OrderBy(x => x).ToList();
                _folderList!.Add("======= SEARCH RESULTS =======");
                _folderList!.AddRange(matchingFolders);
            }
        }

        public void AddSuggestions(ref List<string> folderList) // internal
        {
            folderList.Add("========= SUGGESTIONS =========");
            folderList.AddRange(Suggestions.ToArray(5).Select(ProjectSuggestionPath));
        }

        // Row-model mirror of AddMatches: the SEARCH RESULTS separator (Separator, no score)
        // followed by the ordered matching folders tagged SearchResult. Uses the same OrderBy(x =>
        // x) as AddMatches so the Text sequence is identical.
        private static void AddMatchRows(List<FolderRow> rows, List<string> matchingFolders)
        {
            if (matchingFolders is not null && matchingFolders.Count > 0)
            {
                matchingFolders = matchingFolders.OrderBy(x => x).ToList();
                rows.Add(
                    new FolderRow("======= SEARCH RESULTS =======", FolderRowKind.Separator, null)
                );
                foreach (var folder in matchingFolders)
                {
                    rows.Add(new FolderRow(folder, FolderRowKind.SearchResult, null));
                }
            }
        }

        // Row-model mirror of AddSuggestions: the SUGGESTIONS separator (Separator, no score)
        // followed by the top-5 scored suggestions as Suggestion rows carrying their FolderScore.
        // Text parity with AddSuggestions holds because Suggestions.ToScoredArray(5) shares the same
        // ordered enumeration as Suggestions.ToArray(5).
        private void AddSuggestionRows(List<FolderRow> rows)
        {
            rows.Add(
                new FolderRow("========= SUGGESTIONS =========", FolderRowKind.Separator, null)
            );
            foreach (var score in Suggestions.ToScoredArray(5))
            {
                var folderPath = ProjectSuggestionPath(score.FolderPath);
                var projectedScore = new FolderScore(folderPath, score.Score, score.Probability);
                rows.Add(new FolderRow(folderPath, FolderRowKind.Suggestion, projectedScore));
            }
        }

        private string ProjectSuggestionPath(string folderPath)
        {
            if (_globals is null)
            {
                return folderPath;
            }

            var archivePrefix = _globals.Ol.ArchiveRootPath + "\\";
            return
                folderPath.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase)
                && folderPath.Length > archivePrefix.Length
                ? folderPath.Substring(archivePrefix.Length)
                : folderPath;
        }

        // Row-model mirror of AddRecents: the RECENT SELECTIONS separator (Separator, no score)
        // followed by each recent selection tagged Recent. Gated internally on the recents count,
        // exactly as AddRecents is.
        private void AddRecentRows(List<FolderRow> rows)
        {
            if (_globals.AF.RecentsList.Count > 0)
            {
                rows.Add(
                    new FolderRow(
                        "======= RECENT SELECTIONS ========",
                        FolderRowKind.Separator,
                        null
                    )
                );
                foreach (var recent in _globals.AF.RecentsList)
                {
                    rows.Add(new FolderRow(recent, FolderRowKind.Recent, null));
                }
            }
        }

        public List<string> GetMatchingFolders(
            string searchString,
            string strEmailFolderPath,
            bool includeChildren,
            IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions
        ) // Internal
        {
            var matchingFolders = new List<string>();
            if (searchString.Trim().Length != 0)
            {
                (_regex, _) = SimpleRegex.MakeRegex(searchString);

                var folders = GetFolder(strEmailFolderPath)!.Folders;
                LoopFolders(folders, ref matchingFolders, strEmailFolderPath, true, exclusions);
            }

            return matchingFolders;
        }

        public void LoopFolders(
            Folders folders,
            ref List<string> matchingFolders,
            string olAncestor,
            bool includeChildren,
            IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions
        ) //Internal
        {
            if (string.IsNullOrEmpty(olAncestor))
            {
                olAncestor = _globals.Ol.ArchiveRootPath;
            }

            foreach (Folder f in folders)
            {
                var folderStem = GetOlSubpath(f.FolderPath, olAncestor, true);
                if (exclusions.Any(x => x.excludedFolder == folderStem))
                {
                    // If the folder is excluded, but not its children, then we need to loop through the children
                    if (!exclusions.First(x => x.excludedFolder == folderStem).excludeChildren)
                    {
                        LoopFolders(
                            f.Folders,
                            ref matchingFolders,
                            olAncestor,
                            includeChildren,
                            exclusions
                        );
                    }
                }
                else
                {
                    var relevantPath = GetOlSubpath(f.FolderPath, olAncestor, includeChildren);

                    if (_regex!.IsMatch(relevantPath))
                    {
                        matchingFolders.Add(folderStem);
                    }

                    LoopFolders(
                        f.Folders,
                        ref matchingFolders,
                        olAncestor,
                        includeChildren,
                        exclusions
                    );
                }
            }
        }

        public string GetOlSubpath(string path, string olAncestor, bool includeChildren)
        {
            if (includeChildren)
            {
                if (olAncestor.EndsWith('\\'.ToString()))
                {
                    return path.Substring(olAncestor.Length);
                }
                else
                {
                    return path.Substring(olAncestor.Length + 1);
                }
            }
            else
            {
                var pathParts = path.Substring(olAncestor.Length).Split(@"\");
                return pathParts[pathParts.Count() - 1];
            }
        }

        public void RefreshSuggestions(object objItem, int topNfolderKeys = -1) // Internal
        {
            var OlMail = MailResolution.TryResolveMailItem(objItem);
            if (OlMail is not null)
            {
                RefreshSuggestions(OlMail, topNfolderKeys);
            }
            else
            {
                throw new ArgumentException(
                    $"{nameof(objItem)} passed as {objItem.GetType().Name} could not be cast to MailItem"
                );
            }
        }

        public void RefreshSuggestions(MailItem mailItem, int topNfolderKeys = -1) // Internal
        {
            if (mailItem is not null)
            {
                Suggestions.RefreshSuggestions(
                    olMail: mailItem,
                    appGlobals: _globals,
                    topNfolderKeys: topNfolderKeys
                );
                BlUpdateSuggestions = false;
            }
        }

        #endregion
    }
}
