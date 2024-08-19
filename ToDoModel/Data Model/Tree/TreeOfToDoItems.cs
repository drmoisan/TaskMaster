using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using System.Collections;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS;
using System.Windows.Forms.VisualStyles;
using Deedle.Internal;

namespace ToDoModel
{
    public class TreeOfToDoItems
    {
        #region constructors 

        public TreeOfToDoItems() { }

        public TreeOfToDoItems(List<TreeNode<ToDoItem>> todoTree)
        {
            Roots = todoTree;
        }

        #endregion

        #region Initialize and Access Encapsulated Tree

        private List<TreeNode<ToDoItem>> _roots = new List<TreeNode<ToDoItem>>();        
        public List<TreeNode<ToDoItem>> Roots { get => _roots; private set => _roots = value; }
        
        public enum LoadOptions
        {
            vbLoadAll = 0,
            vbLoadInView = 1
        }

        public void LoadTree(LoadOptions LoadType, IApplicationGlobals appGlobals)
        {
            // Get the list of ToDo items from Outlook
            var items = GetToDoList(LoadType, appGlobals.Ol.App);

            // Create a flat tree of ToDo items and assign IDs to those that don't have them
            var tree = ToListTreeNode(items, appGlobals);

            // Sort the flat tree by ToDoID
            tree.MergeSort(this.CompareItemsByToDoID, inplace: true);
            
            tree = MakeTreeHierarchical(tree);

            Roots = tree;
            
            //WriteTreeToCSVDebug(@"C:\temp\TreeOfToDoItems.csv");
        }

        private List<TreeNode<ToDoItem>> MakeTreeHierarchical(List<TreeNode<ToDoItem>> tree)
        {
            int max = tree.Count - 1;
            int i;

            // Loop through the tree from the end to the beginning
            for (i = max; i >= 0; i -= 1)
            {
                var toDoNode = tree[i];

                // If the ID is bigger than 2 digits, it is a child of someone. 
                // So in that case link it to the proper _parent
                // First try cutting off the last two digits, but in the case of
                // Filtered Items, it is possible that the _parent is not visible.
                // If the _parent is not visible, work iteratively to find the next 
                // closest visible _parent until you get to the root
                if (toDoNode.Value.ToDoID.Length > 2)
                {
                    string strID = toDoNode.Value.ToDoID;
                    string strParentID = strID.Substring(0, strID.Length - 2);
                    bool blContinue = true;

                    while (blContinue)
                    {
                        var nodeParent = FindChildByID(strParentID, tree);
                        if (nodeParent is not null)
                        {
                            nodeParent.InsertChild(toDoNode);
                            tree.Remove(toDoNode);
                            blContinue = false;
                        }
                        if (strParentID.Length > 2)
                        {
                            strParentID = strParentID.Substring(1, strParentID.Length - 2);
                        }
                        else
                        {
                            blContinue = false;
                        }
                    }
                }
            }
            
            return tree;
        }

        private List<TreeNode<ToDoItem>> ToListTreeNode(List<OutlookItem> items, IApplicationGlobals appGlobals)
        {
            // Convert items to TreeNode<ToDoItem> and fill IDs for those that don't have them
            var tree = items.Select(x =>
            {
                var td = new ToDoItem(x);
                AssignId(appGlobals, td);
                return new TreeNode<ToDoItem>(new ToDoItem(x));
            }).ToList();
            return tree;
        }

        public static void AssignId(IApplicationGlobals appGlobals, ToDoItem td)
        {
            if (td.ToDoID.IsNullOrEmpty())
            {
                if (td.Projects.AsListNoPrefix.Count == 1)
                {
                    var projectName = td.Projects.AsListNoPrefix[0];
                    var projectInfos = appGlobals.TD.ProjInfo.Find_ByProjectName(projectName);
                    if (projectInfos.Count == 1)
                    {
                        td.ToDoID = appGlobals.TD.IDList.GetNextToDoID(projectInfos[0].ProjectID + "00");
                    }
                }
                if (td.ToDoID.IsNullOrEmpty())
                {
                    td.ToDoID = appGlobals.TD.IDList.GetNextToDoID();
                }
            }
        }

        public List<OutlookItem> GetToDoList(LoadOptions LoadType, Application Application)
        {
            
            View objView;
            
            string strFilter;

            objView = (View)Application.ActiveExplorer().CurrentView;
            strFilter = "@SQL=" + objView.Filter;

            var stores = Application.Session.Stores.Cast<Store>();
            var result = stores.Where(store => store.ExchangeStoreType != OlExchangeStoreType.olExchangePublicFolder).Select(store =>
            {
                try
                {
                    var folder = (Folder)store.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                    var olObjects = (strFilter == "@SQL=" | LoadType == LoadOptions.vbLoadAll) ? folder.Items : folder.Items.Restrict(strFilter);
                    return olObjects.Cast<object>().Select(x => new OutlookItem(x)).ToList();
                }
                catch (System.Exception)
                {
                    return new List<OutlookItem>();
                }
                
            }).SelectMany(x=>x).ToList();
            
            return result;
        }

        public IAsyncEnumerable<object> GetToDoListAsync(LoadOptions loadType, Application olApp)
        {
            var olView = (View)olApp.ActiveExplorer().CurrentView;
            var strFilter = "@SQL=" + olView.Filter;
            var items = olApp.Session.Stores
                ?.Cast<Store>()
                ?.ToAsyncEnumerable()
                ?.Select(store => store.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                ?.SelectMany(folder =>
                    (strFilter == "@SQL=" | loadType == LoadOptions.vbLoadAll) ?
                    folder?.Items?.Cast<object>()?.ToAsyncEnumerable() :
                    folder?.Items?.Restrict(strFilter)?.Cast<object>()?.ToAsyncEnumerable())
                ?.Select(x => new OutlookItem(x));
            items ??= new List<OutlookItem>().ToAsyncEnumerable();
            return items;
        }

        #endregion

        #region ToDoId

        public bool CompareToDoID(ToDoItem item, string strToDoID)
        {
            return (item.ToDoID ?? "") == (strToDoID ?? "");
        }

        internal int CompareItemsByToDoID(TreeNode<ToDoItem> left, TreeNode<ToDoItem> right)
        {
            return CompareItemsByToDoID(left.Value, right.Value);
        }

        internal int CompareItemsByToDoID(ToDoItem left, ToDoItem right)
        {
            string todoIDLeft = left.ToDoID.ToUpper();
            string todoIDRight = right.ToDoID.ToUpper();

            return CompareItemsByToDoID(todoIDLeft, todoIDRight);
        }

        internal int CompareItemsByToDoID(OutlookItem objItemLeft, OutlookItem objItemRight)
        {
            string todoIDLeft = objItemLeft.GetUdfString("ToDoID").ToUpper();
            string todoIDRight = objItemRight.GetUdfString("ToDoID").ToUpper();

            return CompareItemsByToDoID(todoIDLeft, todoIDRight);
        }

        internal int CompareItemsByToDoID(string todoIDLeft, string todoIDRight)
        {
            if (todoIDRight.Length == 0)
            {
                return -1;
            }
            else if (todoIDLeft.Length == 0)
            {
                return 1;
            }
            else
            {
                var idx = todoIDLeft.FirstDiffIndex(todoIDRight);
                
                // Identical IDs
                if (idx == -1) { return 0; }
                
                // Left ID is prefix of Right ID
                if (idx == todoIDLeft.Length) { return -1; }

                // Right ID is prefix of Left ID
                if (idx == todoIDRight.Length) { return 1; }

                // Compare the two characters that differ
                var left = todoIDLeft[idx].ToBase10(36);
                var right = todoIDRight[idx].ToBase10(36);
                if (left < right) { return -1; }
                else { return 1; }
            }
        }

        public void ReNumberIDs(IDList idList)
        {
            foreach (var RootNode in Roots)
            {
                foreach (var Child in RootNode.Children)
                {
                    if (Child.Children.Count > 0)
                        ReNumberChildrenIDs(Child.Children, idList);
                }
            }
        }
        
        public void ReNumberChildrenIDs(List<TreeNode<ToDoItem>> Children, IIDList idList)
        {
            int max = Children.Count - 1;
            if (max >= 0)
            {
                string strParentID = Children[0].Parent.Value.ToDoID;
                var loopTo = max;
                for (int i = 0; i <= loopTo; i++)
                {
                    if (idList.Contains(Children[i].Value.ToDoID))
                        idList.Remove(Children[i].Value.ToDoID);
                }
                var loopTo1 = max;
                for (int i = 0; i <= loopTo1; i++)
                {
                    string NextID = idList.GetNextToDoID(strParentID + "00");
                    // Dim LevelChange As Boolean = (Children(i).Value.ToDoID.Length = NextID.Length)
                    Children[i].Value.ToDoID = NextID;
                    // Children(i).Value.VisibleTreeState = 67
                    // Children(i).Value.ToDoID = Children(i).Value.ToDoID
                    if (Children[i].Children.Count > 0)
                        ReNumberChildrenIDs(Children[i].Children, idList);
                }
                idList.Serialize();
            }
        }

        public TreeNode<ToDoItem> FindChildByID(string Id, List<TreeNode<ToDoItem>> nodes)
        {
            if (Id.IsNullOrEmpty()) { return null; }
            foreach (var node in nodes)
            {
                if ((node.Value.ToDoID ?? "") == Id)
                {
                    return node;
                }
                else
                {
                    var rnode = FindChildByID(Id, node.Children);
                    if (rnode is not null)
                    {
                        return rnode;
                    }
                }
            }

            return null;

        }

        #endregion ToDoId

        public void AddChild(TreeNode<ToDoItem> Child, TreeNode<ToDoItem> Parent, IIDList idList)
        {
            Parent.Children.Add(Child);
            string strSeed = Parent.Children.Count > 1 ? Parent.Children[Parent.Children.Count - 2].Value.ToDoID : Parent.Value.ToDoID + "00";

            if (idList.Contains(Child.Value.ToDoID))
            {
                bool unused = idList.Remove(Child.Value.ToDoID);
            }
            Child.Value.ToDoID = idList.GetNextToDoID(strSeed);
            if (Child.Children.Count > 0)
            {
                ReNumberChildrenIDs(Child.Children, idList);
            }
            idList.Serialize();
        }

        internal bool IsHeader(string TagContext)
        {
            if (TagContext.Contains("@PROJECTS") || TagContext.Contains("HEADER") || TagContext.Contains("DELIVERABLE") || TagContext.Contains("@PROGRAMS"))
            {
                return true;
            }
            return false;
        }

        public void HideEmptyHeadersInView()
        {
            Action<TreeNode<ToDoItem>> action = node => { if (node.ChildCount == 0) { if (IsHeader(node.Value.Context.AsStringNoPrefix)) { node.Value.ActiveBranch = false; } } };

            foreach (TreeNode<ToDoItem> node in Roots)
                node.Traverse(action);
        }

        #region Debugging Helper Functions

        public void WriteTreeToCSVDebug(string FilePath)
        {

            using (var sw = new StreamWriter(FilePath))
            {
                sw.WriteLine("File Dump");
            }

            LoopTreeToWrite(Roots, FilePath, "");
        }
        
        internal void LoopTreeToWrite(List<TreeNode<ToDoItem>> nodes, string filename, string lineprefix)
        {
            if (nodes is not null)
            {
                foreach (TreeNode<ToDoItem> node in nodes)
                {
                    AppendLineToCSV(filename, lineprefix + node.Value.ToDoID + " " + node.Value.TaskSubject);
                    LoopTreeToWrite(node.Children, filename, lineprefix + node.Value.ToDoID + ",");
                }
            }
        }

            

        internal void AppendLineToCSV(string filename, string line)
        {
            using (var sw = File.AppendText(filename))
            {
                sw.WriteLine(line);
            }
        }

        

        #endregion Debugging Helper Functions

    }
}