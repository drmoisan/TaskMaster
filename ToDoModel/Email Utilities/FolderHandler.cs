using System;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;


using UtilitiesCS;


namespace ToDoModel
{

    public class FolderHandler
    {
        public FolderHandler(IApplicationGlobals AppGlobals)
        {
            _globals = AppGlobals;
            _olApp = AppGlobals.Ol.App;
            _options = Options.NoSuggestions;
            Suggestions = new Suggestions();
            //_folderArray = new string[0];

        }

        public FolderHandler(IApplicationGlobals appGlobals, object objItem, Options options)
        {
            _globals = appGlobals;
            _olApp = appGlobals.Ol.App;
            _options = options;

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


        private Folder _matchedFolder;
        private string _searchString;
        private bool _wildcardFlag;
        //private string[] _folderArray;
        private List<string> _folderList;
        private Suggestions _suggestions;
        public int SaveCounter;
        private int _upBound;
        private Application _olApp;

        private const bool SpeedUp = true;
        private const bool StopAtFirstMatch = false;
        private bool _blUpdateSuggestions;
        public bool WhConv;
        private IApplicationGlobals _globals;
        private Options _options;
        private Regex _regex;
        private string _searchPattern;

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
            
            LoadFromFolderKeyField(false, OlMail);
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

        public string[] FolderArray
        {
            get
            {
                if ((_folderList is null) || (_folderList.Count == 0))
                {
                    _folderList = new List<string>();
                    if (Suggestions.Count > 0)
                        AddSuggestions();
                    if (_globals.AF.RecentsList.Count > 0) 
                        AddRecents();
                }
                
                return _folderList.ToArray();
            }
        }

        public Suggestions Suggestions { get => _suggestions; set => _suggestions = value; }
        
        public bool BlUpdateSuggestions { get => _blUpdateSuggestions; set => _blUpdateSuggestions = value; }
        
        /// <summary>
        /// Function returns a list of Outlook folders that meet search criteria and appends a list of suggested folders 
        /// as well as appending a list of recently used folders
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="ReloadCTFStagingFiles"></param>
        /// <param name="EmailSearchRoot"></param>
        /// <param name="ReCalcSuggestions"></param>
        /// <param name="objItem"></param>
        /// <returns></returns>
        public string[] FindFolder(string SearchString, object objItem, bool ReloadCTFStagingFiles = true, string EmailSearchRoot = "ARCHIVEROOT", bool ReCalcSuggestions = false)
        {

            if (EmailSearchRoot == "ARCHIVEROOT")
            {
                EmailSearchRoot = _globals.Ol.ArchiveRootPath;
            }
            _folderList = new List<string>();
            
            var matchingFolders = GetMatchingFolders(SearchString, EmailSearchRoot);
            if (matchingFolders is not null && matchingFolders.Count > 0)
            {
                _folderList.Add("======= SEARCH RESULTS =======");
                _folderList.AddRange(matchingFolders);
            }
            
            // TODO: Either use the embedded UBound or pass as reference. It is hard to know where it is changed
            _upBound = 0;


            if (ReCalcSuggestions)
            {
                RecalculateSuggestions(objItem, ReloadCTFStagingFiles);
            }
            AddSuggestions();
            AddRecents();

            return FolderArray;


        }

        /// <summary>
        /// Function returns a list of Outlook folders that meet search criteria and appends a list of suggested folders 
        /// as well as appending a list of recently used folders
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="ReloadCTFStagingFiles"></param>
        /// <param name="EmailSearchRoot"></param>
        /// <param name="ReCalcSuggestions"></param>
        /// <param name="objItem"></param>
        /// <returns></returns>
        //public string[] FindFolderOld(string SearchString, object objItem, bool ReloadCTFStagingFiles = true, string EmailSearchRoot = "ARCHIVEROOT", bool ReCalcSuggestions = false)
        //{

        //    if (EmailSearchRoot == "ARCHIVEROOT")
        //    {
        //        EmailSearchRoot = _globals.Ol.ArchiveRootPath;
        //    }
        //    _folderArray = new string[1];
        //    _folderArray[0] = "======= SEARCH RESULTS =======";
        //    // TODO: Either use the embedded UBound or pass as reference. It is hard to know where it is changed
        //    _upBound = 0;

        //    GetMatchingFolders(SearchString, EmailSearchRoot);

        //    if (ReCalcSuggestions)
        //    {
        //        RecalculateSuggestions(objItem, ReloadCTFStagingFiles);
        //    }
        //    AddSuggestions();
        //    AddRecents();

        //    return _folderArray;


        //}

        private void AddRecents()
        {
            if (_globals.AF.RecentsList.Count > 0)
            {
                _folderList.Add("======= RECENT SELECTIONS ========");
                _folderList.AddRange(_globals.AF.RecentsList);
            }
        }
                
        private void RecalculateSuggestions(object ObjItem, bool ReloadCTFStagingFiles)
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

        private void LoadFromFolderKeyField(bool ReloadCTFStagingFiles, MailItem OlMail)
        {
            int i;
            string strTmp;

            int intVarCt;

            var objProperty = OlMail.UserProperties.Find("FolderKey");
            if (objProperty is null)
            {
                Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
            }
            else
            {
                var varFldrs = objProperty.Value;

                if (varFldrs is not Array)
                {
                    if ((varFldrs as string) == "Error")
                    {
                        Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
                    }
                    else
                    {
                        strTmp = (string)varFldrs;
                        Suggestions.AddSuggestion(strTmp, 1L);
                    }
                }
                else
                {
                    string[] strFolders = (string[])varFldrs;
                    intVarCt = strFolders.Length -1;
                    if (intVarCt == 0)
                    {
                        if (strFolders[0] == "Error")
                        {
                            Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
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

        private void AddSuggestions()
        {
            _folderList.Add("========= SUGGESTIONS =========");
            _folderList.AddRange(Suggestions.ToArray());
        }
                
        private List<string> GetMatchingFolders(string Name, string strEmailFolderPath)
        {
            _matchedFolder = null;
            _searchString = "";
            _wildcardFlag = false;

            if (Name.Trim().Length != 0)
            {
                _searchString = Name;
                (_regex, _searchPattern) = SimpleRegex.MakeRegex(_searchString);
                
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

        public Folder GetFolder(string FolderPath)
        {
            Folder TestFolder;
            string[] FoldersArray;
            int i;

            if (FolderPath.Substring(0,2) == @"\\")
            {
                FolderPath = FolderPath.Substring(2);
            }
            // Convert folderpath to array
            FoldersArray = FolderPath.Split(@"\");
            TestFolder = (Folder)_olApp.Session.Folders[FoldersArray[0]];
            if (TestFolder is not null)
            {
                var loopTo = FoldersArray.Length - 1;
                for (i = 1; i <= loopTo; i++)
                {
                    Folders SubFolders;
                    SubFolders = TestFolder.Folders;
                    TestFolder = (Folder)SubFolders[FoldersArray[i]];
                    if (TestFolder is null)
                    {
                        return null;
                    }
                }
            }

            return TestFolder;

        }

        private void LoopFolders(Folders folders, ref List<string> matchingFolders, string strEmailFolderPath = "")
        {
            bool found;
            int intRootLen;

            if (string.IsNullOrEmpty(strEmailFolderPath))
            {
                strEmailFolderPath = _globals.Ol.ArchiveRootPath;
            }

            intRootLen = strEmailFolderPath.Length;
            foreach (Folder f in folders)
            {
                found = _regex.IsMatch(f.FolderPath);
                

                if (found)
                {
                    if (StopAtFirstMatch == false)
                    {
                        found = false;
                        _upBound = _upBound + 1;
                        matchingFolders.Add(f.FolderPath.Substring(intRootLen));
                        
                    }
                }
                if (found)
                {
                    _matchedFolder = f;
                    break;
                }
                else
                {
                    LoopFolders(f.Folders, ref matchingFolders, strEmailFolderPath);
                    if (_matchedFolder is not null)
                        break;
                }
            }
        }

    }
}