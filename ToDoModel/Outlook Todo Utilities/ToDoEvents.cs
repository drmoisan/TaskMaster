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
using UtilitiesVB;
//using Microsoft.VisualBasic;

namespace ToDoModel
{

    public static class ToDoEvents
    {
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
                    // Dim fldr As Outlook.Folder = store.GetSearchFolders.
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
        
        public static List<object> GetListOfToDoItemsInView(Outlook.Application OlApp)
        {
            Items OlItems;
            Outlook.View objView;
            Folder OlFolder;
            string strFilter;
            // QUESTION: ThisAddin.GetListOfToDoItemsInView When is this called? Is it needed?
            // CLEANUP: ThisAddin.GetListOfToDoItemsInView Move to a Class, Module or a Library depending on how it is used. 

            objView = (Outlook.View)OlApp.ActiveExplorer().CurrentView;
            strFilter = "@SQL=" + objView.Filter;

            OlItems = null;
            foreach (Store oStore in OlApp.Session.Stores)
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

        public static void Refresh_ToDoID_Splits(Outlook.Application OlApp)
        {
            ToDoItem todo;
            var OlItems = GetToDoItemsInView(OlApp);
            // QUESTION: Duplicate? If not, move to a class, module or library.
            foreach (var objItem in OlItems)
            {
                todo = new ToDoItem(objItem, OnDemand: true);
                todo.SplitID();
            }
        }

        private static bool _blItemChangeRunning = false;

        public static void OlToDoItems_ItemChange(object Item, Items OlToDoItems, IApplicationGlobals AppGlobals)
        {

            // TODO: Morph Functionality to handle proactively rather than reactively

            if (_blItemChangeRunning == false)
            {

                _blItemChangeRunning = true;

                var ProjInfo = AppGlobals.TD.ProjInfo;
                var IDList = AppGlobals.TD.IDList;

                var todo = new ToDoItem(Item, OnDemand: true);
                UserProperty objProperty_ToDoID = ((dynamic)Item).UserProperties.Find("ToDoID");
                UserProperty objProperty_Project = ((dynamic)Item).UserProperties.Find("TagProject");

                bool blTmp = todo.EC2; // This reads the button and keeps the other field in sync if there is a change
                                       // Check to see if change was in the EC
                if (todo.EC_Change)
                {
                    string strEC = todo.ExpandChildren;
                    // Extremely expensive. I wonder why it is done this way?
                    if (!string.IsNullOrEmpty(todo.ToDoID))
                    {
                        string strChFilter = "@SQL=" + '"' + "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID" + '"' + " like '" + todo.ToDoID + "%'";
                        var OlChildren = OlToDoItems.Restrict(strChFilter);

                        // Identify the tree depth of the current ToDoID (Length of ToDoID / 2)
                        int intLVL = (int)Math.Round(Math.Truncate(todo.ToDoID.Length / 2d));
                        foreach (var objItem in OlChildren)
                        {
                            var todoTmp = new ToDoItem(objItem, OnDemand: true);

                            // Set the toggle for that level to + or - for all descendants on the binary number
                            if ((todoTmp.ToDoID ?? "") != (todo.ToDoID ?? ""))
                            {
                                // Added if statement to correct for the fact that Restrict is not case sensitive
                                if ((todoTmp.ToDoID.Substring(0,todo.ToDoID.Length) ?? "") == (todo.ToDoID ?? ""))
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

                // AUTOCODE ToDoID based on Project
                // Check to see if the project exists before attempting to autocode the id
                if (objProperty_Project is not null)
                {

                    // Get Project Name
                    string strProject = todo.get_Project();

                    // Code the Program name
                    if (ProjInfo.Contains_ProjectName(strProject))
                    {
                        string strProgram = ProjInfo.Programs_ByProjectNames(strProject);
                        if ((todo.TagProgram ?? "") != (strProgram ?? ""))
                        {
                            todo.TagProgram = strProgram;
                        }
                    }

                    string strProjectToDo;
                    // Check to see whether there is an existing ID
                    if (objProperty_ToDoID is not null)
                    {
                        string strToDoID = objProperty_ToDoID.Value;

                        // Don't autocode branches that existed in another project previously
                        if (strToDoID.Length != 0 & strToDoID.Length <= 4)
                        {
                            if (strProject.Length != 0)
                            {

                                // Check to ensure it is in the dictionary before autocoding
                                if (ProjInfo.Contains_ProjectName(strProject))
                                {

                                    if (strToDoID.Length == 2)
                                    {
                                        // Change the item's todoid to be a node of the project
                                        if (todo.get_Context() != "@PROJECTS")
                                        {
                                            strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                            todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                            IDList.Save();
                                            todo.SplitID();
                                            todo.EC2 = true;
                                        }
                                    }
                                }


                                else if (strToDoID.Length == 4) // If it is not in the dictionary, see if this is a project we should add
                                {
                                    var response = MessageBox.Show("Add Project " + strProject + " to the Master List?", "Dialog", MessageBoxButtons.YesNo);
                                    if (response == DialogResult.Yes)
                                    {
                                        string strProgram = Interaction.InputBox("What is the program name for " + strProject + "?", DefaultResponse: "");
                                        ProjInfo.Add(new ToDoProjectInfoEntry(strProject, strToDoID, strProgram));
                                        ProjInfo.Save();
                                    }
                                }
                            }
                        }

                        else if (strToDoID.Length == 0)
                        {
                            strProject = todo.get_Project();
                            if (ProjInfo.Contains_ProjectName(strProject))
                            {
                                strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                IDList.Save();
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
                            if (ProjInfo.Contains_ProjectName(strProject))
                            {
                                strProjectToDo = ProjInfo.Find_ByProjectName(strProject).First().ProjectID;
                                // Add the next ToDoID available in that branch
                                todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                                todo.TagProgram = ProjInfo.Find_ByProjectName(strProject).First().ProgramName;
                                IDList.Save();
                                todo.SplitID();
                                todo.EC2 = true;
                            }
                        }
                    }


                }

                // If OlToDoItem_IsMarkedComplete(item) Then
                // Check to see if todo was just marked complete 
                // If So, adjust Kan Ban fields and categories
                if (todo.Complete)
                {
                    if (Strings.InStr(Conversions.ToString(Item.Categories), "Tag KB Completed") == Conversions.ToInteger(false))
                    {
                        string strCats = Conversions.ToString(Item.Categories);
                        strCats = strCats.Replace("Tag KB Backlog", "").Replace(",,", ",");
                        strCats = strCats.Replace("Tag KB InProgress", "").Replace(",,", ",");
                        strCats = strCats.Replace("Tag KB Planned", "").Replace(",,", ",");
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
                    string strCats = (string)(Item.Categories);

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
                _blItemChangeRunning = false;
            }

        }

        private static bool OlToDoItem_IsMarkedComplete(object Item)
        {
            // QUESTION: Duplicate Function??? I beleive this is already in the ToDoItem class
            if (Item is MailItem)
            {
                var OlMail = Item;
                return Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(OlMail.FlagStatus, OlFlagStatus.olFlagComplete, false));
            }
            else if (Item is TaskItem)
            {
                var OlTask = Item;
                return Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(OlTask.Complete, true, false));
            }
            else
            {
                return false;
            }

        }

        public static void OlToDoItems_ItemAdd(object Item, IApplicationGlobals AppGlobals)
        {
            var todo = new ToDoItem(Item, OnDemand: true);
            var ProjInfo = AppGlobals.TD.ProjInfo;
            var IDList = AppGlobals.TD.IDList;
            if (todo.ToDoID.Length == 0)
            {
                if (todo.get_Project().Length != 0)
                {
                    if (ProjInfo.Contains_ProjectName(todo.get_Project()))
                    {
                        string strProjectToDo = ProjInfo.Find_ByProjectName(todo.get_Project()).First().ProjectID;
                        // Add the next ToDoID available in that branch
                        todo.ToDoID = IDList.GetNextAvailableToDoID(strProjectToDo + "00");
                        todo.TagProgram = ProjInfo.Find_ByProjectName(todo.get_Project()).First().ProgramName;
                        IDList.Save();
                        todo.SplitID();
                    }
                }
                else
                {
                    todo.ToDoID = IDList.GetMaxToDoID();
                }
            }
            todo.VisibleTreeState = 63;


        }

        /// <summary>
    /// This is a helper procedure to migrate ToDoIDs from one framework to another
    /// </summary>
        public static void MigrateToDoIDs(Application OlApp)
        {
            // TODO: Move MigrateToDoIDs to a class, module, or library
            var ToDoItems = OlApp.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderToDo).Items;
            long max = ToDoItems.Count;
            long j = 0L;
            for (long i = 1L, loopTo = max; i <= loopTo; i++)
            {
                var Item = new ToDoItem(ToDoItems[i], true);
                j += 1L;
                if ((string)Item.olItem.GetUdf("NewID") != "Done")
                {
                    string strToDoID = Item.ToDoID;
                    if (strToDoID.Length > 0)
                    {
                        string strToDoIDnew = SubstituteCharsInID(strToDoID);
                        Item.ToDoID = strToDoIDnew;
                        Item.olItem.SetUdf("NewID", value: "Done");
                    }
                }
                if (j == 40L)
                {
                    j = 0L;
                    System.Windows.Forms.Application.DoEvents();
                }
            }



        }

        private static string SubstituteCharsInID(string strToDoID)
        {
            // Dim charsorig As String = "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
            // Dim charsnew As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            // "0123456789AaÁáÀàÂâÄäÃãÅåÆæBbCcÇçDdÐðEeÉéÈèÊêËëFfƒGgHhIiÍíÌìÎîÏïJjKkLlMmNnÑñOoÓóÒòÔôÖöÕõØøŒœPpQqRrSsŠšßTtÞþUuÚúÙùÛûÜüVvWwXxYyÝýÿŸZzŽž"
            // "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            string charsorig = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            string charsnew = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";

            string strBuild = "";
            foreach (var c in strToDoID)
            {
                int intLoc = Strings.InStr(charsorig, Conversions.ToString(c));
                strBuild += Strings.Mid(charsnew, intLoc, 1);
            }

            return strBuild;

        }




    }
}