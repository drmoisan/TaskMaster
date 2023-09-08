using System;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook; 
using System.Collections.Generic;
using System.Threading.Tasks;


using UtilitiesCS;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace ToDoModel
{

    public class FolderHandler
    {
        #region Constructors and Initialization

        public FolderHandler(IApplicationGlobals AppGlobals)
        {
            _globals = AppGlobals;
            _olApp = AppGlobals.Ol.App;
            Suggestions = new Suggestions();
        }

        public FolderHandler(IApplicationGlobals appGlobals, object objItem, InitOptions options)
        {
            _globals = appGlobals;
            _olApp = appGlobals.Ol.App;

            Suggestions = new Suggestions();

            switch(options)
            {
                case InitOptions.NoSuggestions:
                    break;
                case InitOptions.FromArrayOrString:
                    FromArrayOrString(objItem);
                    break;
                case InitOptions.FromField:
                    InitializeFromEmail(objItem);
                    break;
                case InitOptions.Recalculate:
                    RefreshSuggestions(objItem);
                    break;
                default:
                    throw new ArgumentException($"Unknown option value {options}");
            }
            
        }

        public enum InitOptions
        {
            NoSuggestions = 0,
            FromArrayOrString = 1,
            FromField = 2,
            Recalculate = 4
        }
        
        public void InitializeFromEmail(object objItem) //internal
        {
            var OlMail = MailResolution.TryResolveMailItem(objItem);
            if (OlMail is null) { throw new ArgumentException("Constructor Requires the Email Object to be passed as MailItem to use this flag"); }
            
            FromFolderKey(OlMail);
        }

        public void FromArrayOrString(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentException("Cannot initialize suggestions from array or string because reference is null");
            }
            else if (obj.GetType().IsArray && typeof(string).IsAssignableFrom(obj.GetType().GetElementType()))
            {
                _folderList = new List<string>((string[])obj);
                //Suggestions.FromArray((string[])Obj);
            }
            else if (obj is string)
            {
                string tmpString = (string)obj;
                Suggestions.AddSuggestion(tmpString,0);
                
            }
            else
            {
                throw new ArgumentException($"Obj is of type {obj.GetType().Name}, but selected option requires a string or string array");
            }
        }//internal
        
        public void FromFolderKey(MailItem olMail)//internal
        {
            if (!Suggestions.LoadFromField(olMail, _globals))
            {
                Suggestions.RefreshSuggestions(olMail: olMail, appGlobals: _globals);
            }
        }
        
        #endregion

        #region Private Fields

        private IApplicationGlobals _globals;
        
        private Outlook.Application _olApp;
        private Regex _regex;
        //private string _searchString;
        

        #endregion

        #region Public Properties

        private List<string> _folderList;
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

        private Suggestions _suggestions;
        public Suggestions Suggestions { get => _suggestions; set => _suggestions = value; }
        
        private bool _blUpdateSuggestions;
        public bool BlUpdateSuggestions { get => _blUpdateSuggestions; set => _blUpdateSuggestions = value; }

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
        /// <returns></returns>
        public string[] FindFolder(string searchString,
                                   object objItem,
                                   bool reloadCTFStagingFiles = true,
                                   List<string> emailSearchRoots = null,
                                   bool recalcSuggestions = false,
                                   IEnumerable<(string root, string excludedFolder, bool excludeChildren)> exclusions = null)
        {
            if (emailSearchRoots is null) { emailSearchRoots = new() { _globals.Ol.ArchiveRootPath }; }
            if (exclusions is null) { exclusions = new List<(string root, string excludedFolder, bool excludeChildren)>(); }

            _folderList = new List<string>();

            // Add search results
            var matchingFolders = emailSearchRoots.Select(root => GetMatchingFolders(
                                                          searchString, 
                                                          root,
                                                          includeChildren: true,
                                                          exclusions.Where(x => x.root == root)
                                                                    .Select(x => (x.excludedFolder, x.excludeChildren))))
                                                  .SelectMany(x => x)
                                                  .ToList();
            
            //var matchingFolders = GetMatchingFolders(searchString, emailSearchRoots);
            AddMatches(matchingFolders);

            // Add suggestions
            if (recalcSuggestions) { RefreshSuggestions(objItem); }
            AddSuggestions(ref _folderList);

            // Add recents
            AddRecents(ref _folderList);

            return FolderArray;
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> based on a rooted <seealso cref="Folder"/>.FolderPath
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="Folder"/>.FolderPath</param>
        /// <param name="olApp">Handle on the <seealso cref="Outlook.Application"/></param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath 
        /// or <c>null</c> if not found</returns>
        public static Folder GetFolder(string folderpath, Outlook.Application olApp)
        {
            if (folderpath.Substring(0, 2) == @"\\")
            {
                folderpath = folderpath.Substring(2);
            }
            // Convert folderpath to array
            var foldersArray = folderpath.Split(@"\");

            var matchedFolder = GetFolder(olApp.Session.Folders, foldersArray[0]);
            if (matchedFolder is null) { return null; }

            for (int i = 1; i < foldersArray.Length; i++)
            {
                matchedFolder = GetFolder(matchedFolder.Folders, foldersArray[i]);
                if (matchedFolder is null) { return null; }
            }

            return matchedFolder;
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> based on a rooted <seealso cref="Folder"/>.FolderPath.
        /// Uses the <seealso cref="Outlook.Application"/> stored in the <see cref="FolderHandler"/> instance.
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="Folder.FolderPath"/></param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath 
        /// or <c>null</c> if not found</returns>
        /// <exception cref="ArgumentException"><paramref name="folderpath"/> should be rooted </exception>
        public Folder GetFolder(string folderpath)
        {
            // Check that folderpath is rooted
            var root = _globals.Ol.Root.FolderPath;
            if (!folderpath.Contains(root))
            {
                throw new ArgumentException($"The parameter {nameof(folderpath)} value {folderpath} does not contain the root {root}", nameof(folderpath));
            }
            
            return GetFolder(folderpath, _olApp);
        }

        /// <summary>
        /// Function grabs a handle on the <seealso cref="Folder"/> represented by the rooted <seealso cref="Folder"/>.FolderPath.
        /// Uses the <seealso cref="Outlook.Application"/> stored in the <see cref="FolderHandler"/> instance. If the
        /// targeted folder is not found, an exception is thrown or a message is delivered to the user based on the 
        /// value of the <paramref name="throwEx"/> parameter.
        /// </summary>
        /// <param name="folderpath"> Rooted <seealso cref="Folder.FolderPath"/></param>
        /// <param name="throwEx">Flag to determine if exception should be thrown or message delivered to user</param>
        /// <returns>The <seealso cref="Folder"/> represented by the <seealso cref="Folder"/>.FolderPath 
        /// or <c>null</c> if not found</returns>
        /// <exception cref="ArgumentException"><paramref name="folderpath"/> should be rooted </exception>
        public Folder GetFolder(string folderpath, bool throwEx)
        {
            // Check that folderpath is rooted
            var root = _globals.Ol.Root.FolderPath;
            if (!folderpath.Contains(root))
            {
                throw new ArgumentException($"The parameter {nameof(folderpath)} value {folderpath} does not contain the root {root}", nameof(folderpath)); 
            }
            
            // Get the Folder
            var olFolder = GetFolder(folderpath, _olApp);
            
            // If folder is null, throw exception or deliver message to user
            if (olFolder is null)
            {
                string message = $"Selected folder {folderpath} does not exist. " +
                    "Staging Files out of sync with current directory state.";
                if (throwEx) { throw new ArgumentException(message, nameof(folderpath)); }
                else { MessageBox.Show(message); }

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
        public static Folder GetFolder(Folders children, string childName)
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
        public string InputFoldername(Folder parent) //Internal
        {
            string name = "";
            while (name is not null && name == "")
            {
                name = InputBox.ShowDialog(
                    $"Please enter a new subfolder name for {parent.Name}",
                    "New folder dialog");

                if (name is not null)
                {
                    if (!IsLegalFolderName(name))
                    {
                        MessageBox.Show($"Folder name {name} contains the illegal characters "+
                            $"{GetIllegalFolderChars(name).SentenceJoin()}. Please choose a different name.");
                        name = "";
                    }
                    else if (name.Length > 30)
                    {
                        MessageBox.Show("Outlook limits folder names to 30 characters. Please choose a different name.");
                        name = "";
                    }
                    else if (GetFolder(parent.Folders, name) is not null)
                    {
                        MessageBox.Show("Folder already exists. Please choose a different name.");
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
        private static char[] IllegalFolderCharacters { get => @"[\/:*?""<>|].".ToCharArray(); }

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
        /// Method creates new parallel folders in Outlook Email and the File System. Combines 
        /// a relative folderpath with the fully rooted olAncestor folderpath to create an 
        /// Outlook.<seealso cref="Folder"/>. The fully qualified Outlook folderpath applies the 
        /// <seealso cref="FolderConverter.ToFsFolderpath(string, string, string)"/> extension to convert 
        /// to a parallel folderpath. System.IO.<seealso cref="DirectoryInfo"/> creates 
        /// this parallel folder in the file system.
        /// </summary>
        /// <param name="parentBranchPath">Parent FolderPath to Outlook.<seealso cref="Folder"/> 
        /// excluding the FolderPath of the Outlook ancestor in the path</param>
        /// <param name="olAncestor">Fully rooted Outlook.<seealso cref="Folder"/>.FolderPath of Ancestor <seealso cref="Folder"/></param>
        /// <param name="fsAncestor">Fully qualified File System path</param>
        /// <returns>The created Outlook.<seealso cref="Folder"/></returns>
        public MAPIFolder CreateFolder(string parentBranchPath, string olAncestor, string fsAncestor)
        {
            // Set default root if not provided
            if (olAncestor.IsNullOrEmpty()) { olAncestor = _globals.Ol.ArchiveRootPath; }
            
            // Fully root the folderpath
            var parentFolderpath = $"{olAncestor}{parentBranchPath}";
            
            // Get the parent folder and return null if not found
            var parentFolder = this.GetFolder(parentFolderpath, false);
            if (parentFolder is null) { return null; }

            // Get the new folder name from the user
            string newFolderName = InputFoldername(parentFolder);
            if (newFolderName is null) { return null; }

            // Create the new folder in Outlook 
            var olFolder = parentFolder.Folders.Add(newFolderName);

            // Convert the Outlook folderpath to a filesystem folderpath
            var fsFolderName = olFolder.ToFsFolderpath(olAncestor, fsAncestor);
            
            // Create the new folder in the filesystem
            var fsFolder = Directory.CreateDirectory(fsFolderName);

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
                _folderList.Add("======= SEARCH RESULTS =======");
                _folderList.AddRange(matchingFolders);
            }
        }
                
        public void AddSuggestions(ref List<string> folderList) // internal
        {
            folderList.Add("========= SUGGESTIONS =========");
            folderList.AddRange(Suggestions.ToArray(5));
        }
        
        public List<string> GetMatchingFolders(string searchString,
                                               string strEmailFolderPath,
                                               bool includeChildren,
                                               IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions) // Internal
        {


            var matchingFolders = new List<string>();
            if (searchString.Trim().Length != 0)
            {
                (_regex, _) = SimpleRegex.MakeRegex(searchString);
                
                var folders = GetFolder(strEmailFolderPath).Folders;
                LoopFolders(folders, ref matchingFolders, strEmailFolderPath, true, exclusions);
            }
            
            return matchingFolders;
            
        }

        public void LoopFolders(Folders folders,
                                ref List<string> matchingFolders,
                                string olAncestor,
                                bool includeChildren,
                                IEnumerable<(string excludedFolder, bool excludeChildren)> exclusions) //Internal
        {
            if (string.IsNullOrEmpty(olAncestor)) { olAncestor = _globals.Ol.ArchiveRootPath; }

            foreach (Folder f in folders)
            {
                var folderStem = GetOlSubpath(f.FolderPath, olAncestor, true);
                if (exclusions.Any(x => x.excludedFolder == folderStem))
                {
                    // If the folder is excluded, but not its children, then we need to loop through the children
                    if (!exclusions.First(x => x.excludedFolder == folderStem).excludeChildren)
                    {
                        LoopFolders(f.Folders, ref matchingFolders, olAncestor, includeChildren, exclusions);
                    }
                }
                else
                {
                    var relevantPath = GetOlSubpath(f.FolderPath, olAncestor, includeChildren);

                    if (_regex.IsMatch(relevantPath))
                    {
                        matchingFolders.Add(folderStem);
                    }
                
                    LoopFolders(f.Folders, ref matchingFolders, olAncestor, includeChildren, exclusions);
                }
            }
        }
        
        public string GetOlSubpath(string path, string olAncestor, bool includeChildren)
        {
            if (includeChildren)
            {
                return path.Substring(olAncestor.Length);
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
            if (OlMail is not null) { RefreshSuggestions(OlMail, topNfolderKeys);}
            else
            {
                throw new ArgumentException($"{nameof(objItem)} passed as {objItem.GetType().Name} could not be cast to MailItem");
            }
        }

        public void RefreshSuggestions(MailItem mailItem, int topNfolderKeys = -1) // Internal
        {
            if (mailItem is not null)
            {
                Suggestions.RefreshSuggestions(olMail: mailItem, appGlobals: _globals, topNfolderKeys: topNfolderKeys);
                BlUpdateSuggestions = false;
            }
        }

        #endregion
    }
}