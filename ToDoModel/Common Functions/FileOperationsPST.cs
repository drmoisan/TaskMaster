using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesVB;

namespace ToDoModel
{

    public class FileOperationsPST
    {
        private readonly IApplicationGlobals _globals;
        private readonly Folder _emailFolderPST;
        private readonly List<PSTEvents> _handlerList;

        public FileOperationsPST(IApplicationGlobals appGlobals, string EmailFolderpathPST)
        {
            _globals = appGlobals;
            _emailFolderPST = GetOutlookPSTFolderByPath(EmailFolderpathPST, _globals.Ol.App);
            _handlerList = InstantiateHandlers();
        }

        private Folder GetOutlookPSTFolderByPath(string FolderPath, Application Application)
        {
            if (Strings.Left(FolderPath, 2) == @"\\")
            {
                FolderPath = Strings.Right(FolderPath, Strings.Len(FolderPath) - 2);
            }
            string[] FoldersArray = FolderPath.Split('\\');

            try
            {
                Folder OlFolder = (Folder)Application.Session.Folders[FoldersArray[0]];
                var OlFolders = OlFolder.Folders;
                Debug.WriteLine(OlFolder.FolderPath + " has " + OlFolders.Count.ToString() + " folders");
                foreach (Folder currentOlFolder in OlFolders)
                {
                    OlFolder = currentOlFolder;
                    Debug.WriteLine(OlFolder.FolderPath);
                }

                for (int i = 1, loopTo = Information.UBound(FoldersArray); i <= loopTo; i++)
                    OlFolder = (Folder)OlFolder.Folders[FoldersArray[i]];
                return OlFolder;
            }
            catch
            {
                Debug.WriteLine(Information.Err().Description);
                Debug.WriteLine("Folder Does Not Exist");
                return null;
            }

        }

        public void HookEvents()
        {
            foreach (PSTEvents handler in _handlerList)
                handler.HookEvents();
        }

        public void UnHookEvents()
        {
            foreach (PSTEvents handler in _handlerList)
                handler.UnHookEvents();
        }

        private List<PSTEvents> InstantiateHandlers()
        {
            var olSession = _globals.Ol.App.Session;
            var stores = olSession.Stores;
            var handlerList = new List<PSTEvents>();

            foreach (Store store in stores)
            {
                if (Strings.Right(store.FilePath, 3) == "pst")
                {
                    var OlFolder = GetSearchFolder(store, "FLAGGED");
                    var items = OlFolder.Items;
                    var handlerPST = new PSTEvents(store, items, _globals);
                    handlerList.Add(handlerPST);
                }
            }

            return handlerList;
        }

        private bool __itemsPST_ItemChange_blIsRunning = default;

        private class PSTEvents
        {
            private Items __itemsPST;

            private Items _itemsPST
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    return __itemsPST;
                }

                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    __itemsPST = value;
                }
            }
            private readonly Store _store;
            private readonly IApplicationGlobals _globals;

            public PSTEvents(Store Store, Items ItemsPST, IApplicationGlobals Globals)
            {
                _itemsPST = ItemsPST;
                _globals = Globals;
            }

            public void HookEvents()
            {
                _itemsPST.ItemChange += _itemsPST_ItemChange;
                _itemsPST.ItemAdd += _itemsPST_ItemChange;
            }

            public void UnHookEvents()
            {
                _itemsPST.ItemChange -= _itemsPST_ItemChange;
                _itemsPST.ItemAdd -= _itemsPST_ItemChange;
            }

            // Handles _itemsPST.ItemChange

            private void _itemsPST_ItemChange(object Item)
            {
                // TODO: Morph Functionality to handle proactively rather than reactively
                if (__itemsPST_ItemChange_blIsRunning == false)
                {

                    __itemsPST_ItemChange_blIsRunning = true;
                    var todo = new ToDoItem(Item, OnDemand: true);
                    UserProperty objProperty_ToDoID = (UserProperty)Item.UserProperties.Find("ToDoID");
                    UserProperty objProperty_Project = (UserProperty)Item.UserProperties.Find("TagProject");


                    // AUTOCODE ToDoID based on Project
                    // Check to see if the project exists before attempting to autocode the id
                    if (objProperty_Project is not null)
                    {

                        string strProject;
                        string strProjectToDo;
                        // Check to see whether there is an existing ID
                        if (objProperty_ToDoID is not null)
                        {
                            string strToDoID = Conversions.ToString(objProperty_ToDoID);

                            // Don't autocode branches that existed to another project previously
                            if (strToDoID.Length != 0 & strToDoID.Length <= 4)
                            {

                                // Get Project Name
                                strProject = todo.get_Project();

                                // If IsArray(objProperty_Project.Value) Then
                                // strProject = FlattenArry(objProperty_Project.Value)
                                // Else
                                // strProject = objProperty_Project.Value
                                // End If

                                // Check to see if the Project name returned a value before attempting to autocode
                                if (strProject.Length != 0)
                                {

                                    // Check to ensure it is in the dictionary before autocoding
                                    if (_globals.TD.ProjInfo.Contains_ProjectName(strProject))
                                    {
                                        // If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                        // strProjectToDo = ProjDict.ProjectDictionary(strProject)

                                        if (strToDoID.Length == 2)
                                        {
                                            // Change the Item's todoid to be a node of the project
                                            if (todo.get_Context() != "Tag PROJECTS")
                                            {
                                                strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                                todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                                todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                                // strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                                // SetUdf("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                                _globals.TD.IDList.Save(_globals.TD.FnameIDList);
                                                // Split_ToDoID(objItem:=Item)
                                                todo.SplitID();
                                            }
                                        }
                                    }


                                    else if (strToDoID.Length == 4) // If it is not in the dictionary, see if this is a project we should add
                                    {
                                        var response = Interaction.MsgBox("Add Project " + strProject + " to the Master List?", Constants.vbYesNo);
                                        if (response == Constants.vbYes)
                                        {
                                            // ProjDict.ProjectDictionary.Add(strProject, strToDoID)
                                            // SaveDict()
                                            string strProgram = Interaction.InputBox("What is the program name for " + strProject + "?", DefaultResponse: "");
                                            int unused2 = _globals.TD.ProjInfo.Add(new ToDoProjectInfoEntry(strProject, strToDoID, strProgram));
                                            _globals.TD.ProjInfo.Save();
                                        }
                                    }
                                }
                            }

                            else if (strToDoID.Length == 0)
                            {
                                strProject = todo.get_Project();
                                // If IsArray(objProperty_Project.Value) Then
                                // strProject = FlattenArry(objProperty_Project.Value)
                                // Else
                                // strProject = objProperty_Project.Value
                                // End If
                                if (_globals.TD.ProjInfo.Contains_ProjectName(strProject))
                                {
                                    strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                    todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                    // If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                    // strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                    todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                    // strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                    // SetUdf("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                    _globals.TD.IDList.Save(_globals.TD.FnameIDList);
                                    // Split_ToDoID(objItem:=Item)
                                    todo.SplitID();
                                }

                            }
                        }
                        else // In this case, the project name exists but the todo id does not
                        {
                            // Get Project Name
                            strProject = objProperty_Project is Array ? FlattenArray.FlattenArry((object[])objProperty_Project) : (string)objProperty_Project;

                            // If the project name is in our dictionary, autoadd the ToDoID to this item
                            if (strProject.Length != 0)
                            {
                                // If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                if (_globals.TD.ProjInfo.Contains_ProjectName(strProject))
                                {
                                    // strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                    strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                    // Add the next ToDoID available in that branch
                                    todo.ToDoID = _globals.TD.IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                    todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                    // strToDoID = IDList.GetNextAvailableToDoID(strProjectToDo & "00")
                                    // SetUdf("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                    _globals.TD.IDList.Save(_globals.TD.FnameIDList);
                                    // Split_ToDoID(objItem:=Item)
                                    todo.SplitID();
                                    // ***NEED CODE HERE***
                                    // ***NEED CODE HERE***
                                    // ***NEED CODE HERE***
                                }
                            }
                        }


                    }

                    // If OlToDoItem_IsMarkedComplete(Item) Then
                    // Check to see if todo was just marked complete 
                    // If So, adjust Kan Ban fields and categories
                    if (todo.Complete)
                    {
                        if (Strings.InStr(Conversions.ToString(Item.Categories), "Tag KB Completed") == Conversions.ToInteger(false))
                        {
                            string strCats = Strings.Replace(Strings.Replace(Conversions.ToString(Item.Categories), "Tag KB Backlog", ""), ",,", ",");
                            strCats = Strings.Replace(Strings.Replace(strCats, "Tag KB InProgress", ""), ",,", ",");
                            strCats = Strings.Replace(Strings.Replace(strCats, "Tag KB Planned", ""), ",,", ",");
                            while (Strings.Left(strCats, 1) == ",")
                                strCats = Strings.Right(strCats, strCats.Length - 1);
                            if (strCats.Length > 0)
                            {
                                strCats += ", Tag KB Completed";
                            }
                            else
                            {
                                strCats += "Tag KB Completed";
                            }
                            Item.Categories = strCats;
                            var unused1 = Item.Save;
                            todo.set_KB(value: "Completed");
                        }
                    }
                    else if (todo.get_KB() == "Completed")
                    {
                        string strCats = Conversions.ToString(Item.Categories);

                        // Strip Completed from categories
                        if (Strings.InStr(strCats, "Tag KB Completed") == Conversions.ToInteger(true))
                        {
                            strCats = Strings.Replace(Strings.Replace(strCats, "Tag KB Completed", ""), ",,", ",");
                        }

                        string strReplace;
                        string strKB;
                        if (Strings.InStr(strCats, "Tag A Top Priority Today") == Conversions.ToInteger(true))
                        {
                            strReplace = "Tag KB InProgress";
                            strKB = "InProgress";
                        }
                        else if (Strings.InStr(strCats, "Tag Bullpin Priorities") == Conversions.ToInteger(true))
                        {
                            strReplace = "Tag KB Planned";
                            strKB = "Planned";
                        }
                        else
                        {
                            strReplace = "Tag KB Backlog";
                            strKB = "Backlog";
                        }
                        if (strCats.Length > 0)
                        {
                            strCats += ", " + strReplace;
                        }
                        else
                        {
                            strCats = strReplace;
                        }
                        Item.Categories = strCats;
                        var unused = Item.Save;
                        todo.set_KB(value: strKB);

                    }
                    __itemsPST_ItemChange_blIsRunning = false;
                }


            }
        }

        private Folder GetSearchFolder(Store store, string name)
        {
            try
            {
                var searchfolders = store.GetSearchFolders();
                foreach (Folder OlFolder in searchfolders)
                {
                    Debug.WriteLine(OlFolder.Name);
                    if ((OlFolder.Name ?? "") == (name ?? ""))
                    {
                        return OlFolder;
                    }
                }
                return null;
            }
            catch
            {
                Debug.WriteLine(Information.Err().Description);
                return null;
            }
        }


    }
}