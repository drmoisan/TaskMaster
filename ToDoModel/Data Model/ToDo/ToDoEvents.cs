using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UtilitiesCS.Extensions;
//using Microsoft.VisualBasic;

namespace ToDoModel
{

    public static class ToDoEvents
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ConcurrentDictionary<string, int> _editing = new();
        public static ConcurrentDictionary<string, int> Editing => _editing;

        private static void Debug_OutputNsStores(Outlook.Application OlApp)
        {
            string storeList = string.Empty;

            var ns = OlApp.Session;
            var stores = ns.Stores;

            for (int i = 1, loopTo = stores.Count; i <= loopTo; i++)
            {
                var store = stores[i];
                if (Path.GetExtension(store.FilePath) == "pst")
                {
                    Folder fldrtmp = (Folder)store.GetRootFolder();

                    Debug.WriteLine(fldrtmp.FolderPath);
                    var fldrs = store.GetSearchFolders();
                    foreach (Folder fldr in fldrs)
                        // \\03 LATAM CCO\search folders\FLAGGED
                        Debug.WriteLine(fldr.FolderPath);
                    // Dim FolderName As Outlook.Folder = store.GetSearchFolders.
                    // Dim items As Outlook.Items
                    // storeList += String.Format("{0} - {1}{2}", store.DisplayName, (If(store.IsDataFileStore, ".pst", ".ost")), Environment.NewLine)
                }
            }

            Debug.WriteLine(storeList);

        }

        public static void WriteToCSV(string filename, string[] strOutput, bool overwrite = false)
        {
            // CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
            if (overwrite | File.Exists(filename) == false)
            {
                using (var sw = new StreamWriter(filename))
                {
                    for (int i = 0; i < strOutput.Length; i++)
                        sw.WriteLine(strOutput[i]);
                }
            }
            else
            {
                using (var sw = new StreamWriter(filename, append: true))
                {
                    for (int i = 0; i < strOutput.Length; i++)
                        sw.WriteLine(strOutput[i]);
                }
            }

        }

        public static void WriteToCSV(string filename, string strOutput, bool overwrite = false)
        {
            // CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
            if (overwrite | File.Exists(filename) == false)
            {
                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine(strOutput);
                }
            }
            else
            {
                using (var sw = new StreamWriter(filename, append: true))
                {
                    sw.WriteLine(strOutput);
                }
            }

        }
        
        public static List<object> GetListOfToDoItemsInView(Outlook.Application olApp)
        {
            Items OlItems;
            Outlook.View objView;
            Folder OlFolder;
            string strFilter;
            // QUESTION: ThisAddin.GetListOfToDoItemsInView When is this called? Is it needed?
            // CLEANUP: ThisAddin.GetListOfToDoItemsInView Move to a Class, Module or a Library depending on how it is used. 

            objView = (Outlook.View)olApp.ActiveExplorer().CurrentView;
            strFilter = "@SQL=" + objView.Filter;

            OlItems = null;
            foreach (Store oStore in olApp.Session.Stores)
            {
                OlFolder = (Folder)oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                OlItems = strFilter == "@SQL=" ? OlFolder.Items : OlFolder.Items.Restrict(strFilter);
            }
            var ListObjects = new List<object>();
            foreach (var objItem in OlItems)
                ListObjects.Add(objItem);
            // GetToDoItemsInView = OlItems
            return ListObjects;
        }
                
        public static IAsyncEnumerable<object> GetAsyncEnumerableOfToDoItemsInView(Outlook.Application olApp) 
        { 
            var olView = (Outlook.View)olApp.ActiveExplorer().CurrentView;
            var strFilter = "@SQL=" + olView.Filter;
            var items = olApp.Session.Stores
                ?.Cast<Store>()
                ?.ToAsyncEnumerable()
                ?.Select(store => store.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                ?.SelectMany(folder => 
                    strFilter == "@SQL=" ? 
                    folder?.Items?.Cast<object>()?.ToAsyncEnumerable() : 
                    folder?.Items?.Restrict(strFilter)?.Cast<object>()?.ToAsyncEnumerable());
            return items;   
        }

        public static Items GetToDoItemsInView(Outlook.Application OlApp)
        {
            Items GetItemsInView_ToDoRet = default;
            Items OlItems;
            Outlook.View objView;
            Folder OlFolder;
            string strFilter;

            // QUESTION: Depricated? Previous function was GetList. Do we need both?
            objView = (Outlook.View)OlApp.ActiveExplorer().CurrentView;
            strFilter = "@SQL=" + objView.Filter;

            OlItems = null;
            foreach (Store oStore in OlApp.Session.Stores)
            {
                OlFolder = (Folder)oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                OlItems = strFilter == "@SQL=" ? OlFolder.Items : OlFolder.Items.Restrict(strFilter);
            }
            GetItemsInView_ToDoRet = OlItems;
            return GetItemsInView_ToDoRet;
        }

        public static int IsChild(string strParent, string strChild)
        {
            int IsChildRet = default;
            int count = 0;
            bool unbroken = true;
            int i;
            // QUESTION: Duplicate? If not, move to a class, module or library.
            var loopTo = (int)Math.Round(strParent.Length / 2d);
            for (i = 1; i <= loopTo; i++)
            {
                if (unbroken)
                {
                    if ((strParent.Substring(i * 2 - 1, 2) ?? "") == (strChild.Substring(i * 2 - 1, 2) ?? ""))
                    {
                        count = i;
                    }
                    else
                    {
                        unbroken = false;
                    }
                }
            }
            IsChildRet = count;
            return IsChildRet;
        }

        //public static object FindParent(Collection itms, string strChild)
        //{
        //    string strParent;
        //    // QUESTION: Duplicate? If not, move to a class, module or library.
        //    try
        //    {
        //        strParent = strChild.Substring(2);
        //        return itms[strParent];
        //    }
        //    catch (System.Exception)
        //    {
        //        return null;
        //    }
        //}

        public static async Task RefreshToDoIdSplitsAsync(Outlook.Application olApp)
        {
            var itemsAsyncEnum = GetAsyncEnumerableOfToDoItemsInView(olApp);
            await itemsAsyncEnum.ForEachAwaitAsync(
                async item => await Task.Run(()=>TrySplitToDoID(item)));
        }

        private static void TrySplitToDoID(object item) 
        {
            try
            {
                new ToDoItem(new OutlookItem(item)).SplitID();
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            
        }

        public static async Task OlToDoItems_ItemChange(object item, Items olToDoItems, IApplicationGlobals globals)
        {
            // TODO: Morph Functionality to handle proactively rather than reactively
            string entryId = null;
            try
            {
                entryId = ((dynamic)item).EntryID;
            }
            catch (System.Exception e) 
            {  
                logger.Error($"Error in {nameof(ToDoEvents)}.{nameof(OlToDoItems_ItemChange)} getting EntryID from item\n{e.Message}");
            }
            
            if (entryId is not null && Editing.TryAdd(entryId, 1))
            {
                var projInfo = globals.TD.ProjInfo;
                var idList = globals.TD.IDList;

                var olItem = new OutlookItem(item);
                var todo = new ToDoItem(olItem);
                
                todo.Identifier = $"ItemChangeEvent: {todo.ToDoID}";
                todo.ProjectData = projInfo;
                todo.IdList = idList;
                todo.ProjectsToPrograms = projInfo.Programs_ByProjectNames;

                await Task.Run(() => SynchronizeEC(olToDoItems, todo));
                await Task.Run(() => AutoCodeId(projInfo, idList, todo));                
                await Task.Run(() => SynchronizeKanban(item, todo));
                await Task.Delay(500);

                Editing.TryRemove(olItem.EntryID, out _);
            }

        }

        private static void SynchronizeKanban(object Item, ToDoItem todo)
        {
            // If OlToDoItem_IsMarkedComplete(item) Then
            // Check to see if todo was just marked complete 
            // If So, adjust Kan Ban fields and categories
            if (todo.Complete)
            {
                dynamic olItem = Item;
                if (((string)olItem.Categories).Contains("Tag KB Completed"))
                {
                    string strCats = olItem.Categories;
                    strCats = strCats.Replace("Tag KB Backlog", "").Replace(",,", ",");
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
                    olItem.Categories = strCats;
                    olItem.Save();
                    todo.KB.AsStringNoPrefix = "Completed";
                    //todo.SetKB(value: "Completed");
                }
            }
            else if (todo.KB.AsStringNoPrefix == "Completed")
            {
                dynamic olItem = Item;
                string strCats = (string)(olItem.Categories);

                // Strip Completed from categories
                if (((string)strCats).Contains("Tag KB Completed"))
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
                olItem.Categories = strCats;
                olItem.Save();
                todo.KB.AsStringNoPrefix = strKB;
                //todo.SetKB(value: strKB);

            }
        }

        private static void SynchronizeEC(Items OlToDoItems, ToDoItem todo)
        {
            bool blTmp = todo.EC2; // This reads the button and keeps the Other field in sync if there is a change
                                   // Check to see if change was in the EC
            if (todo.EC_Change)
            {
                string strEC = todo.ExpandChildren;
                // Extremely expensive. I wonder why it is done this way?
                if (!string.IsNullOrEmpty(todo.ToDoID))
                {
                    string strChFilter = "@SQL=" + '"' + "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID" + '"' + " like '" + todo.ToDoID + "%'";
                    var OlChildren = OlToDoItems.Restrict(strChFilter);
                    OlChildren.Cast<object>()
                        .Select(item => ((dynamic)item).EntryID as string)
                        .ForEach(entryId => Editing.AddOrUpdate(entryId, 1, (key, existing) => existing + 1));

                    // Identify the tree depth of the current ToDoID (Length of ToDoID / 2)
                    int intLVL = (int)Math.Round(Math.Truncate(todo.ToDoID.Length / 2d));
                    foreach (var objItem in OlChildren)
                    {
                        var todoTmp = new ToDoItem(new OutlookItem(objItem), onDemand: true);

                        // Set the toggle for that level to + or - for all descendants on the binary number
                        if ((todoTmp.ToDoID ?? "") != (todo.ToDoID ?? ""))
                        {
                            // Added if statement to correct for the fact that Restrict is not case sensitive
                            if ((todoTmp.ToDoID.Substring(0, todo.ToDoID.Length) ?? "") == (todo.ToDoID ?? ""))
                            {
                                if (strEC == "-")
                                {
                                    todoTmp.set_VisibleTreeStateLVL(intLVL + 1, true);
                                }
                                else if (strEC == "+")
                                {
                                    todoTmp.set_VisibleTreeStateLVL(intLVL + 1, false);
                                }
                                // Check to see if visible
                                int VisibleMask = (int)Math.Round(Math.Pow(2d, todoTmp.ToDoID.Length / 2d) - 1d);
                                bool blnewAB = (todoTmp.VisibleTreeState & VisibleMask) == VisibleMask;
                                if (blnewAB != todoTmp.ActiveBranch)
                                {
                                    todoTmp.ActiveBranch = blnewAB;
                                }
                            }
                        }

                    }
                }
                todo.EC_Change = false;
            }
        }

        private static bool ParamsAreValid(IProjectData ProjInfo, IIDList idList, ToDoItem todo)
        {
            try
            {
                ProjInfo.ThrowIfNull();
                idList.ThrowIfNull();
                todo.ThrowIfNull();
            }
            catch (ArgumentNullException e)
            {
                logger.Warn($"Cannot autocode ToDoId because {e.ParamName} in {nameof(ToDoEvents)}.{nameof(AutoCodeId)} was null");
                return false;
            }
            if (todo.Projects?.AsListNoPrefix?.IsNullOrEmpty() ?? true)
            {
                logger.Warn($"Cannot autocode ToDoId because {nameof(todo.Projects.AsListNoPrefix)} in {nameof(ToDoEvents)}.{nameof(AutoCodeId)} is null or empty");
                return false;
            }
            return true;
        }

        private static void AutoCodeId(IProjectData ProjInfo, IIDList idList, ToDoItem todo)
        {            
            if (!ParamsAreValid(ProjInfo, idList, todo)) { return; }
            
            // Get Project Name
            string project = todo.Projects.AsStringNoPrefix;
            var toDoId = todo.ToDoID;

            if (ProjInfo.Contains_ProjectName(project))
            {
                // Since project is found, get the project ID
                var projectId = ProjInfo.Find_ByProjectName(project).First().ProjectID;

                // Auto code based on the state of the existing ToDoID
                if (toDoId.IsNullOrEmpty())
                {
                    // If the todo id is not set, set it to the next available id in the project branch
                    MakeChildOfProject(projectId, idList, todo);
                }                
                else if (projectId == toDoId) { return; } // Exit if the item is the Project header
                else if (toDoId.Length == 2) { return; } // Exit if the item is a Program header
                else if (toDoId.Length > 4) { return;  } // Exit if the item is a child of another branch
                else if (toDoId.Length == 4) // If the item has a placeholder ID but should be a child of the project
                {
                    MakeChildOfProject(projectId, idList, todo);
                }
            }
            else if (toDoId.Length == 4) // If it is not in the dictionary, see if this is a project we should add
            {
                var response = MessageBox.Show("Add Project " + project + " to the Master List?", "Dialog", MessageBoxButtons.YesNo);
                if (response == DialogResult.Yes)
                {
                    string program = InputBox.ShowDialog("What is the program name for " + project + "?", DefaultResponse: "");
                    ProjInfo.Add(new ProjectEntry(project, toDoId, program));
                    ProjInfo.Save();
                }
            }                       
        }

        private static string MakeChildOfProject(string projectId, IIDList idList, ToDoItem todo)
        {                        
            // Get the next available first-branch ID within the project and assign it
            todo.ToDoID = idList.GetNextToDoID($"{projectId}00");
            // Save the ID list
            idList.Serialize();
            // Split the ID
            todo.SplitID();
            // Mark the item as an active branch
            todo.EC2 = true;

            return projectId;
        }

        private static bool OlToDoItem_IsMarkedComplete(object Item)
        {
            // QUESTION: Duplicate Function??? I beleive this is already in the ToDoItem class
            if (Item is MailItem)
            {
                MailItem OlMail = (MailItem)Item;
                return (OlMail.FlagStatus == OlFlagStatus.olFlagComplete);
            }
            else if (Item is TaskItem)
            {
                TaskItem OlTask = (TaskItem)Item;
                return OlTask.Complete;
            }
            else
            {
                return false;
            }

        }

        public static void OlToDoItems_ItemAdd(object item, IApplicationGlobals AppGlobals)
        {
            var olItem = new OutlookItem(item);
            if (Editing.TryAdd(olItem.EntryID, 1)) 
            {
                var todo = new ToDoItem(olItem);
                var ProjInfo = AppGlobals.TD.ProjInfo;
                var IDList = AppGlobals.TD.IDList;
                if (todo.ToDoID.Length == 0)
                {
                    if (todo.Projects.AsListWithPrefix.Count != 0)
                    {
                        foreach (var projectName in todo.Projects.AsListWithPrefix)
                        {
                            if (ProjInfo.Contains_ProjectName(projectName))
                            {
                                string strProjectToDo = ProjInfo.Find_ByProjectName(projectName).First().ProjectID;
                                // Add the next ToDoID available in that branch
                                todo.ToDoID = IDList.GetNextToDoID(strProjectToDo + "00");
                                todo.Program.AsStringNoPrefix = ProjInfo.Find_ByProjectName(projectName).First().ProgramName;
                                IDList.Serialize();
                                todo.SplitID();
                            }
                        }
                    }
                    else
                    {
                        todo.ToDoID = IDList.GetNextToDoID();
                    }
                }
                todo.VisibleTreeState = 63;

                Editing.TryRemove(olItem.EntryID, out _);
            }            
        }

        private static string SubstituteCharsInID(string strToDoID)
        {
            // Dim charsorig As String = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
            // Dim charsnew As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            // "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
            // "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            // string charsorig = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            // string charsnew = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
            // 20230606
            string charsorig = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
            string charsnew = "0123456789abcdefghijklmnopqrstuvwxyzZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";

            string strBuild = "";
            foreach (var c in strToDoID)
            {
                int intLoc = charsorig.IndexOf(c);
                strBuild += charsnew.Substring(intLoc, 1);
            }

            return strBuild;

        }




    }
}