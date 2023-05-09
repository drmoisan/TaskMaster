using System;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesCS;
using UtilitiesVB;

namespace ToDoModel
{

    public class FolderHandler
    {
        private Folder _matchedFolder;
        private string _searchString;
        private bool _wildcardFlag;
        private string[] _folderList;
        private cSuggestions _suggestions;
        public int SaveCounter;
        private int _upBound;
        private Application _olApp;

        private const bool SpeedUp = true;
        private const bool StopAtFirstMatch = false;
        private bool _blUpdateSuggestions;
        public bool WhConv;
        private IApplicationGlobals _globals;
        private Options _options;

        public enum Options
        {
            NoSuggestions = 0,
            FromArrayOrString = 1,
            FromField = 2,
            Recalculate = 4
        }

        public FolderHandler(IApplicationGlobals AppGlobals)
        {
            _globals = AppGlobals;
            _olApp = AppGlobals.Ol.App;
            _options = Options.NoSuggestions;
            Suggestions = new cSuggestions();
            _folderList = new string[0];
        }

        public FolderHandler(IApplicationGlobals AppGlobals, object ObjItem, Options Options)
        {
            _globals = AppGlobals;
            _olApp = AppGlobals.Ol.App;
            _options = Options;

            Suggestions = new cSuggestions();

            if (Options == Options.FromArrayOrString)
            {
                InitializeFromArrayOrString(ObjItem);
            }
            else if (Options == Options.FromField)
            {
                InitializeFromEmail(ObjItem);
            }
            else if (Options == Options.Recalculate)
            {
                bool argReloadCTFStagingFiles = false;
                RecalculateSuggestions(ObjItem, ref argReloadCTFStagingFiles);
            }
            else if (Options == Options.NoSuggestions)
            {
            }
            else
            {
                throw new ArgumentException((Conversions.ToDouble("Unknown option value ") + (double)Options).ToString());
            }

            _folderList = new string[1];
            AddSuggestions();
            AddRecents();
        }

        private void InitializeFromEmail(object ObjItem)
        {
            var OlMail = MailResolution.TryResolveMailItem(ObjItem);
            if (OlMail is null)
            {
                throw new ArgumentException("Constructor Requires the Email Object to be passed as MailItem to use this flag");
            }
            else
            {
                LoadFromFolderKeyField(false, OlMail);
            }
        }

        private void InitializeFromArrayOrString(object ObjItem)
        {
            if (ObjItem is null)
            {
                throw new ArgumentException("Cannot initialize suggestions from array or string because reference is null");
            }
            else if (ObjItem.GetType().IsArray && "".GetType().IsAssignableFrom((Type)ObjItem.GetElementType()))
            {
                Suggestions.FolderSuggestionsArray = (string[])ObjItem;
            }
            else if (ObjItem is string)
            {
                string tmpString = (string)ObjItem;
                Suggestions.ADD_END(tmpString);
            }
            else
            {
                throw new ArgumentException("ObjItem is of type " + Information.TypeName(ObjItem) + ", but selected option requires a string or string array");
            }
        }

        public string[] FolderList
        {
            get
            {
                if (_folderList.Length == -1)
                {
                    if (Suggestions.Count > 0)
                        AddSuggestions();
                    if (_globals.AF.RecentsList.Count > 0)
                        AddRecents();
                }
                return _folderList;
            }
        }

        public cSuggestions Suggestions
        {
            get
            {
                return _suggestions;
            }
            set
            {
                _suggestions = value;
            }
        }

        public bool BlUpdateSuggestions
        {
            get
            {
                return _blUpdateSuggestions;
            }
            set
            {
                _blUpdateSuggestions = value;
            }
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
        public string[] FindFolder(string SearchString, object objItem, bool ReloadCTFStagingFiles = true, string EmailSearchRoot = "ARCHIVEROOT", bool ReCalcSuggestions = false)
        {

            if (EmailSearchRoot == "ARCHIVEROOT")
            {
                EmailSearchRoot = _globals.Ol.ArchiveRootPath;
            }
            _folderList = new string[1];
            _folderList[0] = "======= SEARCH RESULTS =======";
            // TODO: Either use the embedded UBound or pass as reference. It is hard to know where it is changed
            _upBound = 0;

            GetMatchingFolders(SearchString, EmailSearchRoot);

            if (ReCalcSuggestions)
            {
                RecalculateSuggestions(objItem, ref ReloadCTFStagingFiles);
            }
            AddSuggestions();
            AddRecents();

            return _folderList;


        }

        private void AddRecents()
        {
            _upBound = _upBound + 1;
            Array.Resize(ref _folderList, _upBound + 1);
            _folderList[_upBound] = "======= RECENT SELECTIONS ========";  // Seperator between search and recent selections

            foreach (string folderName in _globals.AF.RecentsList)
            {
                _upBound = _upBound + 1;
                Array.Resize(ref _folderList, _upBound + 1);
                _folderList[_upBound] = folderName;
            }
        }

        private void RecalculateSuggestions(object ObjItem, ref bool ReloadCTFStagingFiles)
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
                throw new ArgumentException("ObjItem passed as " + Information.TypeName(ObjItem) + ", but should have been MailItem");
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
                var varFldrs = objProperty;

                if (varFldrs is Array == false)
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(varFldrs, "Error", false)))
                    {
                        Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
                    }
                    else
                    {
                        strTmp = Conversions.ToString(varFldrs);
                        Suggestions.Add(strTmp, 1L);
                    }
                }
                else
                {
                    intVarCt = Information.UBound((Array)varFldrs);
                    if (intVarCt == 0)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(varFldrs((object)0), "Error", false)))
                        {
                            Suggestions.RefreshSuggestions(OlMail, _globals, ReloadCTFStagingFiles);
                        }
                        else
                        {
                            strTmp = Conversions.ToString(varFldrs((object)0));
                            Suggestions.ADD_END(strTmp);
                        }
                    }
                    else
                    {
                        var loopTo = intVarCt;
                        for (i = 0; i <= loopTo; i++)
                        {
                            strTmp = Conversions.ToString(varFldrs((object)i));
                            Suggestions.ADD_END(strTmp);
                        }
                    }
                }
            }
        }

        private void AddSuggestions()
        {
            if (Suggestions.Count > 0)
            {
                if (_upBound > 0)
                    _upBound = _upBound + 1;
                _upBound = _upBound + Suggestions.Count;
                Array.Resize(ref _folderList, _upBound + 1);
                _folderList[_upBound - Suggestions.Count] = "========= SUGGESTIONS =========";
                for (int i = 1, loopTo = Suggestions.Count; i <= loopTo; i++)
                    _folderList[_upBound - Suggestions.Count + i] = Suggestions.get_FolderList_ItemByIndex(i);
            }
        }

        private Folders GetMatchingFolders(string Name, string strEmailFolderPath)
        {
            _matchedFolder = null;
            _searchString = "";
            _wildcardFlag = false;


            if (Strings.Len(Strings.Trim(Name)) != 0)
            {
                _searchString = Name;

                _searchString = Strings.LCase(_searchString);
                _searchString = Strings.Replace(_searchString, "%", "*");
                _wildcardFlag = Conversions.ToBoolean(Strings.InStr(_searchString, "*"));

                var folders = GetFolder(strEmailFolderPath).Folders;
                LoopFolders(folders, strEmailFolderPath);

                return folders;
            }
            else
            {
                return null;
            }


        }

        public Folder GetFolder(string FolderPath)
        {
            Folder TestFolder;
            object FoldersArray;
            int i;

            if (Strings.Left(FolderPath, 2) == @"\\")
            {
                FolderPath = Strings.Right(FolderPath, Strings.Len(FolderPath) - 2);
            }
            // Convert folderpath to array
            FoldersArray = Strings.Split(FolderPath, @"\");
            TestFolder = (Folder)_olApp.Session.Folders[FoldersArray((object)0)];
            if (TestFolder is not null)
            {
                var loopTo = Information.UBound((Array)FoldersArray, 1);
                for (i = 1; i <= loopTo; i++)
                {
                    Folders SubFolders;
                    SubFolders = TestFolder.Folders;
                    TestFolder = (Folder)SubFolders[FoldersArray((object)i)];
                    if (TestFolder is null)
                    {
                        return null;
                    }
                }
            }

            return TestFolder;

        }

        private void LoopFolders(Folders folders, string strEmailFolderPath = "")
        {
            bool found;
            int intRootLen;

            if (string.IsNullOrEmpty(strEmailFolderPath))
            {
                strEmailFolderPath = _globals.Ol.ArchiveRootPath;
            }

            if (SpeedUp == false)
                _olApp.DoEvents();

            intRootLen = Strings.Len(strEmailFolderPath);
            foreach (Folder f in folders)
            {
                if (_wildcardFlag)
                {
                    found = LikeOperator.LikeString(Strings.LCase(f.FolderPath), _searchString, CompareMethod.Binary);
                }
                else
                {
                    found = (Strings.LCase(f.FolderPath) ?? "") == (_searchString ?? "");
                }

                if (found)
                {
                    if (StopAtFirstMatch == false)
                    {
                        found = false;
                        _upBound = _upBound + 1;
                        Array.Resize(ref _folderList, _upBound + 1);
                        // _folderList(_upBound - 1) = Right(f.FolderPath, Len(f.FolderPath) - 36) 'If starting at 0 in folder list
                        _folderList[_upBound] = Strings.Right(f.FolderPath, Strings.Len(f.FolderPath) - intRootLen - 1); // If starting at 1 in folder list
                    }
                }
                if (found)
                {
                    _matchedFolder = f;
                    break;
                }
                else
                {
                    LoopFolders(f.Folders, strEmailFolderPath);
                    if (_matchedFolder is not null)
                        break;
                }
            }
        }



    }
}