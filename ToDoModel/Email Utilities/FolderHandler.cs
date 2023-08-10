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

        public FolderHandler(IApplicationGlobals appGlobals, object objItem, Options options)
        {
            _globals = appGlobals;
            _olApp = appGlobals.Ol.App;

            Suggestions = new Suggestions();

            switch(options)
            {
                case Options.NoSuggestions:
                    break;
                case Options.FromArrayOrString:
                    InitializeFromArrayOrString(objItem);
                    break;
                case Options.FromField:
                    InitializeFromEmail(objItem);
                    break;
                case Options.Recalculate:
                    RecalculateSuggestions(objItem, false);
                    break;
                default:
                    throw new ArgumentException($"Unknown option value {options}");
            }
            
        }

        public enum Options
        {
            NoSuggestions = 0,
            FromArrayOrString = 1,
            FromField = 2,
            Recalculate = 4
        }
                
        private void InitializeFromEmail(object objItem)
        {
            var OlMail = MailResolution.TryResolveMailItem(objItem);
            if (OlMail is null) { throw new ArgumentException("Constructor Requires the Email Object to be passed as MailItem to use this flag"); }
            
            InitializeFromFolderKeyField(false, OlMail);
        }

        private void InitializeFromArrayOrString(object obj)
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
        }
        
        private void InitializeFromFolderKeyField(bool reloadCTFStagingFiles, MailItem olMail)
        {
            int i;
            string strTmp;

            int intVarCt;

            var objProperty = olMail.UserProperties.Find("FolderKey");
            if (objProperty is null)
            {
                Suggestions.RefreshSuggestions(olMail, _globals, reloadCTFStagingFiles);
            }
            else
            {
                var foldersObject = objProperty.Value;

                if (foldersObject is not Array)
                {
                    if ((foldersObject as string) == "Error")
                    {
                        Suggestions.RefreshSuggestions(olMail, _globals, reloadCTFStagingFiles);
                    }
                    else
                    {
                        strTmp = (string)foldersObject;
                        Suggestions.AddSuggestion(strTmp, 1L);
                    }
                }
                else
                {
                    string[] strFolders = (string[])foldersObject;
                    intVarCt = strFolders.Length -1;
                    if (intVarCt == 0)
                    {
                        if (strFolders[0] == "Error")
                        {
                            Suggestions.RefreshSuggestions(olMail, _globals, reloadCTFStagingFiles);
                        }
                        else
                        {
                            strTmp = strFolders[0];
                            Suggestions.AddSuggestion(strTmp, 0);
                        }
                    }
                    else
                    {
                        var loopTo = intVarCt;
                        for (i = 0; i <= loopTo; i++)
                        {
                            strTmp = strFolders[i];
                            Suggestions.AddSuggestion(strTmp, 0);
                        }
                    }
                }
            }
        }
        
        #endregion

        #region Private Fields

        private IApplicationGlobals _globals;
        private Folder _matchedFolder;
        private Outlook.Application _olApp;
        private Regex _regex;
        private string _searchString;
        

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
        /// <param name="emailSearchRoot"></param>
        /// <param name="reCalcSuggestions"></param>
        /// <param name="objItem"></param>
        /// <returns></returns>
        public string[] FindFolder(string searchString, object objItem, bool reloadCTFStagingFiles = true, string emailSearchRoot = "ARCHIVEROOT", bool reCalcSuggestions = false)
        {
            if (emailSearchRoot == "ARCHIVEROOT") { emailSearchRoot = _globals.Ol.ArchiveRootPath; }

            _folderList = new List<string>();

            // Add search results
            var matchingFolders = GetMatchingFolders(searchString, emailSearchRoot).OrderBy(x => x).ToList();
            AddMatches(matchingFolders);

            // Add suggestions
            if (reCalcSuggestions)
            {
                RecalculateSuggestions(objItem, reloadCTFStagingFiles);
            }
            AddSuggestions(ref _folderList);

            // Add recents
            AddRecents(ref _folderList);

            return FolderArray;
        }

        public static Folder GetFolder(string folderpath, Outlook.Application olApp)
        {
            Folder TestFolder;
            string[] foldersArray;
            int i;

            if (folderpath.Substring(0, 2) == @"\\")
            {
                folderpath = folderpath.Substring(2);
            }
            if (folderpath.Substring(0, 1) == @"\")
            {
                folderpath = folderpath.Substring(1);
            }
            // Convert folderpath to array
            foldersArray = folderpath.Split(@"\");
            TestFolder = (Folder)olApp.Session.Folders[foldersArray[0]];
            if (TestFolder is not null)
            {
                var loopTo = foldersArray.Length - 1;
                for (i = 1; i <= loopTo; i++)
                {
                    Folders SubFolders;
                    SubFolders = TestFolder.Folders;
                    TestFolder = (Folder)SubFolders[foldersArray[i]];
                    if (TestFolder is null)
                    {
                        return null;
                    }
                }
            }

            return TestFolder;

        }

        public Folder GetFolder(string folderpath)
        {
            Folder TestFolder;
            string[] foldersArray;
            int i;

            if (folderpath.Substring(0,2) == @"\\")
            {
                folderpath = folderpath.Substring(2);
            }
            if (folderpath.Substring(0, 1) == @"\")
            {
                folderpath = folderpath.Substring(1);
            }
            // Convert folderpath to array
            foldersArray = folderpath.Split(@"\");
            TestFolder = (Folder)_olApp.Session.Folders[foldersArray[0]];
            if (TestFolder is not null)
            {
                var loopTo = foldersArray.Length - 1;
                for (i = 1; i <= loopTo; i++)
                {
                    Folders SubFolders;
                    SubFolders = TestFolder.Folders;
                    TestFolder = (Folder)SubFolders[foldersArray[i]];
                    if (TestFolder is null)
                    {
                        return null;
                    }
                }
            }

            return TestFolder;

        }

        public Folder GetFolder(string selectedValue, bool mustMatch, bool throwEx)
        {
            var olFolder = this.GetFolder(selectedValue);
            if (olFolder is null && mustMatch)
            {
                string message = $"Selected folder {selectedValue} does not exist. " +
                    "Staging Files out of sync with current directory state.";
                if (throwEx) { throw new ArgumentException(message, nameof(selectedValue)); }
                else { MessageBox.Show(message); }

            }
            return olFolder;
        }

        public string InputNewFoldername(Folder parentFolder) //Internal
        {
            string newFolderName = " ";
            while (newFolderName == " ")
            {
                newFolderName = InputBox.ShowDialog(
                    $"Please enter a new subfolder name for {parentFolder.Name}",
                    "New folder dialog", " ");

                if (newFolderName != " " && newFolderName != "")
                {
                    if (!IsLegalFolderName(newFolderName))
                    {
                        MessageBox.Show("Folder name contains illegal characters. Please choose a different name.");
                        newFolderName = " ";
                    }
                    else if (newFolderName.Length > 30)
                    {
                        MessageBox.Show("Outlook limits folder names to 30 characters. Please choose a different name.");
                        newFolderName = " ";
                    }
                    else if (this.GetFolder(newFolderName) is not null)
                    {
                        MessageBox.Show("Folder already exists. Please choose a different name.");
                        newFolderName = " ";
                    }
                }
            }
            return newFolderName;
        }

        private static char[] IllegalFolderCharacters { get => @"[\/:*?""<>|].".ToCharArray(); }

        private bool IsLegalFolderName(string folderName)
        {
            return !folderName.Any(c => IllegalFolderCharacters.Contains(c));
        }
        
        public MAPIFolder CreateFolder(string parentBranchPath, string olRoot, string fsRoot)
        {
            if (olRoot.IsNullOrEmpty()) { olRoot = _globals.Ol.ArchiveRootPath; }
            var parentFolderpath = $"{olRoot}{parentBranchPath}";
            //var parentFolderpath = Path.Combine(olRoot, parentBranchPath);
            var parentFolder = this.GetFolder(parentFolderpath, true, false);
            if (parentFolder is null) { return null; }

            string newFolderName = InputNewFoldername(parentFolder);
            if (newFolderName == "") { return null; }

            var olFolder = parentFolder.Folders.Add(newFolderName);

            var fsFolderName = olFolder.ToFsFolderpath(olRoot, fsRoot);
            
            var fsFolder = Directory.CreateDirectory(fsFolderName);

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
                _folderList.Add("======= SEARCH RESULTS =======");
                _folderList.AddRange(matchingFolders);
            }
        }
                
        public void AddSuggestions(ref List<string> folderList) // internal
        {
            folderList.Add("========= SUGGESTIONS =========");
            folderList.AddRange(Suggestions.ToArray());
        }
        
        public List<string> GetMatchingFolders(string searchString, string strEmailFolderPath, bool includeChildren = true) // Internal
        {
            _matchedFolder = null;

            if (searchString.Trim().Length != 0)
            {
                (_regex, _) = SimpleRegex.MakeRegex(searchString);
                
                var matchingFolders = new List<string>();
                var folders = GetFolder(strEmailFolderPath).Folders;
                LoopFolders(folders, ref matchingFolders, strEmailFolderPath);

                return matchingFolders;
            }
            else
            {
                return null;
            }
        }

        public void LoopFolders(Folders folders, ref List<string> matchingFolders, string strEmailFolderPath = "", bool includeChildren = true) //Internal
        {
            if (string.IsNullOrEmpty(strEmailFolderPath))
            {
                strEmailFolderPath = _globals.Ol.ArchiveRootPath;
            }

            foreach (Folder f in folders)
            {
                var relevantPath = GetRelevantOlPathPortion(f.FolderPath, strEmailFolderPath, includeChildren);

                if (_regex.IsMatch(relevantPath))
                {
                    matchingFolders.Add(relevantPath);    
                }
                
                LoopFolders(f.Folders, ref matchingFolders, strEmailFolderPath, includeChildren);
            }
        }
        
        public string GetRelevantOlPathPortion(string path, string root, bool includeChildren)
        {
            if (includeChildren)
            {
                return path.Substring(root.Length);
            }
            else
            {
                var pathParts = path.Substring(root.Length).Split(@"\");
                return pathParts[pathParts.Count() - 1];
            }
        }

        public void RecalculateSuggestions(object ObjItem, bool ReloadCTFStagingFiles) // Internal
        {
            var OlMail = MailResolution.TryResolveMailItem(ObjItem);
            if (OlMail is not null)
            {
                if (_globals.AF.SuggestionFilesLoaded == false)
                    ReloadCTFStagingFiles = true;
                Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
                BlUpdateSuggestions = false;
            }
            else
            {
                throw new ArgumentException($"Obj passed as {ObjItem.GetType().Name} but should have been MailItem");
            }
        }

        #endregion
    }
}