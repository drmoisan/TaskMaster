using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel
{
    public class ToDoSynchronizer
    {
        public ToDoSynchronizer(IApplicationGlobals globals) 
        {
            Globals = globals;
            ProjInfo = Globals.TD.ProjInfo;
            IdList = Globals.TD.IDList;
            Ns = Globals.Ol.NamespaceMAPI;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }

        private IIDList _idList;
        internal IIDList IdList { get => _idList; set => _idList = value; }

        private NameSpace _ns;
        internal NameSpace Ns { get => _ns; set => _ns = value; }

        private IProjectData _projInfo;
        internal IProjectData ProjInfo { get => _projInfo; set => _projInfo = value; }

        private static ConcurrentDictionary<string, int> _synchronizing = new();
        internal static ConcurrentDictionary<string, int> Synchronizing => _synchronizing;

        public async Task SynchronizeItem(string entryId)
        {
            if (Synchronizing.TryAdd(entryId, 1))
            {
                var todo = ToDoFromId(entryId);

                await Task.Run(() => SynchronizeEC(Globals.Events.OlToDoItems, todo));               
                await Task.Run(() => SynchronizeKanban(todo.OlItem.InnerObject, todo));
                await Task.Delay(500);

                Synchronizing.TryRemove(todo.OlItem.EntryID, out _);
            }
        }

        
        
        private ToDoItem ToDoFromId(string entryId)
        {
            var item = Ns.GetItemFromID(entryId);
            var todo = new ToDoItem(new OutlookItem(item));
            todo.ProjectData = ProjInfo;
            todo.IdList = IdList;
            todo.ProjectsToPrograms = ProjInfo.Programs_ByProjectNames;
            return todo;
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
    }
}
