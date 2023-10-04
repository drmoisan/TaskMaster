using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.IO;
using UtilitiesCS;
using System.Windows.Forms;
using UtilitiesCS.OutlookExtensions;

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

        private Folder GetOutlookPSTFolderByPath(string FolderPath, Outlook.Application Application)
        {
            if (FolderPath.Substring(0, 2) == @"\\")
            {
                FolderPath = FolderPath.Substring(2);
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

                for (int i = 0; i < FoldersArray.Length; i++)
                    OlFolder = (Folder)OlFolder.Folders[FoldersArray[i]];
                return OlFolder;
            }
            catch (System.Exception ex) 
            {
                Debug.WriteLine(ex.Message);
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
                if (Path.GetExtension(store.FilePath) == "pst")
                {
                    var OlFolder = GetSearchFolder(store, "FLAGGED");
                    var items = OlFolder.Items;
                    var handlerPST = new PSTEvents(store, items, _globals);
                    handlerList.Add(handlerPST);
                }
            }

            return handlerList;
        }

        private static bool __itemsPST_ItemChange_blIsRunning = default;

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
            //private readonly Store _store;
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
                var olItem = new OutlookItem(Item);
                // TODO: Morph Functionality to handle proactively rather than reactively
                if (__itemsPST_ItemChange_blIsRunning == false)
                {

                    __itemsPST_ItemChange_blIsRunning = true;
                    var todo = new ToDoItem(olItem, OnDemand: true);
                    UserProperty objProperty_ToDoID = olItem.GetUdf("ToDoID");
                    UserProperty objProperty_Project = olItem.GetUdf("TagProject");


                    // AUTOCODE ToDoID based on Project
                    // Check to see if the project exists before attempting to autocode the id
                    if (objProperty_Project is not null)
                    {

                        string strProject;
                        string strProjectToDo;
                        // Check to see whether there is an existing ID
                        if (objProperty_ToDoID is not null)
                        {
                            string strToDoID = (string)objProperty_ToDoID.Value;

                            // Don't autocode branches that existed to another project previously
                            if (strToDoID.Length != 0 & strToDoID.Length <= 4)
                            {

                                // Get Project Name
                                strProject = todo.Projects.AsStringNoPrefix;

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
                                            foreach (var context in todo.Context.AsListWithPrefix)
                                            {
                                                if (context != "Tag PROJECTS")
                                                {
                                                    // Change the Item's todoid to be a node of the project
                                                    strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                                    todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                                    todo.ToDoID = _globals.TD.IDList.GetNextToDoID(strProjectToDo + "00");
                                                    // strToDoID = IDList.GetNextToDoID(strProjectToDo & "00")
                                                    // SetUdf("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                                    _globals.TD.IDList.Serialize(_globals.TD.FnameIDList);
                                                    // Split_ToDoID(objItem:=Item)
                                                    todo.SplitID();
                                                }
                                            }
                                            
                                        }
                                    }


                                    else if (strToDoID.Length == 4) // If it is not in the dictionary, see if this is a project we should add
                                    {
                                        var response = MessageBox.Show($"Add Project {strProject} to the Master List?", "", MessageBoxButtons.YesNo);
                                        if (response == DialogResult.Yes)
                                        {
                                            // ProjDict.ProjectDictionary.Add(strProject, strToDoID)
                                            // SaveDict()
                                            string strProgram = InputBox.ShowDialog($"What is the program name for {strProject}?", DefaultResponse: "");
                                            _globals.TD.ProjInfo.Add(new ToDoProjectInfoEntry(strProject, strToDoID, strProgram));
                                            _globals.TD.ProjInfo.Save();
                                        }
                                    }
                                }
                            }

                            else if (strToDoID.Length == 0)
                            {
                                strProject = todo.Projects.AsStringNoPrefix;
                                // If IsArray(objProperty_Project.Value) Then
                                // strProject = FlattenStringTree(objProperty_Project.Value)
                                // Else
                                // strProject = objProperty_Project.Value
                                // End If
                                if (_globals.TD.ProjInfo.Contains_ProjectName(strProject))
                                {
                                    strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                    todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                    // If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                    // strProjectToDo = ProjDict.ProjectDictionary(strProject)
                                    todo.ToDoID = _globals.TD.IDList.GetNextToDoID(strProjectToDo + "00");
                                    // strToDoID = IDList.GetNextToDoID(strProjectToDo & "00")
                                    // SetUdf("ToDoID", Value:=strToDoID, SpecificItem:=Item)
                                    _globals.TD.IDList.Serialize(_globals.TD.FnameIDList);
                                    // Split_ToDoID(objItem:=Item)
                                    todo.SplitID();
                                }

                            }
                        }
                        else // In this case, the project name exists but the todo id does not
                        {
                            // Get Project Name
                            strProject = objProperty_Project.GetUdfString();

                            // If the project name is in our dictionary, autoadd the ToDoID to this item
                            if (strProject.Length != 0)
                            {
                                // If ProjDict.ProjectDictionary.ContainsKey(strProject) Then
                                if (_globals.TD.ProjInfo.Contains_ProjectName(strProject))
                                {
                                    strProjectToDo = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                    // Add the next ToDoID available in that branch
                                    todo.ToDoID = _globals.TD.IDList.GetNextToDoID(strProjectToDo + "00");
                                    todo.TagProgram = _globals.TD.ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                    _globals.TD.IDList.Serialize(_globals.TD.FnameIDList);
                                    
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
                        if (olItem.GetCategories().Contains("Tag KB Completed"))
                        {
                            string strCats = olItem.GetCategories().Replace("Tag KB Backlog", "").Replace(",,", ",");
                            strCats = strCats.Replace("Tag KB InProgress", "").Replace(",,", ",");
                            strCats = strCats.Replace("Tag KB Planned", "").Replace(",,", ",");
                            
                            while (strCats.Substring(0, 1) == ",")
                                strCats = strCats.Substring(1);
                            if (strCats.Length > 0)
                            {
                                strCats += ", Tag KB Completed";
                            }
                            else
                            {
                                strCats += "Tag KB Completed";
                            }
                            olItem.SetCategories(strCats);
                            todo.KB = "Completed";
                        }
                    }
                    else if (todo.KB == "Completed")
                    {
                        string strCats = olItem.GetCategories();

                        // Strip Completed from categories
                        if (strCats.Contains("Tag KB Completed"))
                        {
                            strCats = strCats.Replace("Tag KB Completed", "").Replace(",,", ",");
                        }

                        string strReplace;
                        string strKB;
                        if (strCats.Contains("Tag A Top Priority Today"))
                        {
                            strReplace = "Tag KB InProgress";
                            strKB = "InProgress";
                        }
                        else if (strCats.Contains("Tag Bullpin Priorities"))
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
                        olItem.SetCategories(strCats);
                        todo.KB = strKB;

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
            catch (System.Exception ex) 
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }


    }
}