using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using System.Collections;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS;

namespace ToDoModel
{
    public class TreeOfToDoItems
    {
        #region constructors 

        public TreeOfToDoItems()
        {
            ListOfToDoTree = new List<TreeNode<ToDoItem>>();
        }

        public TreeOfToDoItems(List<TreeNode<ToDoItem>> todoTree)
        {
            _todoTree = todoTree;
        }

        #endregion

        #region Initialize and Access Encapsulated Tree

        private List<TreeNode<ToDoItem>> _todoTree = new List<TreeNode<ToDoItem>>();
        
        public List<TreeNode<ToDoItem>> ListOfToDoTree { get => _todoTree; private set => _todoTree = value; }
        
        public enum LoadOptions
        {
            vbLoadAll = 0,
            vbLoadInView = 1
        }

        public void LoadTree(LoadOptions LoadType, Application Application)
        {
            string strTemp;
            string strPrev;
            IList colItems;
            strPrev = "";
            strTemp = "";

            try
            {
                // ***STEP 1: LOAD RAW [ITEMS] TO A LIST AND SORT THEM***
                var TreeItems = GetToDoList(LoadType, Application);
                TreeItems = (List<object>)this.MergeSort<object>(TreeItems, this.CompareItemsByToDoID);

                colItems = new List<object>();
                var colNoID = new List<object>();
                ToDoItem tmpToDo = null;
                TreeNode<ToDoItem> ToDoNode;
                TreeNode<ToDoItem> NodeParent;


                // ***STEP 2: ADD ITEMS TO A FLAT TREE & ASSIGN IDs TO THOSE THAT DON'T HAVE THEM***
                // Iterate through ToDo items in List
                foreach (var objItem in TreeItems)
                {
                    // Cast objItem to temporary ToDoItem
                    if (objItem is MailItem)
                    {
                        tmpToDo = new ToDoItem((MailItem)objItem);
                    }
                    else if (objItem is TaskItem)
                    {
                        tmpToDo = new ToDoItem((TaskItem)objItem);
                    }

                    // Add the temporary ToDoItem to the tree, assigning an ID if missing
                    // If tmpToDo.ToDoID = "nothing" Then
                    // ToDoTree.AddChild(tmpToDo)
                    ListOfToDoTree.Add(new TreeNode<ToDoItem>(tmpToDo));
                    // Else
                    // ToDoTree.AddChild(tmpToDo, tmpToDo.ToDoID)
                    // ToDoTree.Add(New TreeNode(Of ToDoItem)(tmpToDo, tmpToDo.ToDoID))
                    // End If
                }

                // ***STEP 3: MAKE TREE HIERARCHICAL
                int max = ListOfToDoTree.Count - 1;
                int i;

                // Loop through the tree from the end to the beginning
                for (i = max; i >= 0; i -= 1)
                {
                    ToDoNode = ListOfToDoTree[i];

                    // If the ID is bigger than 2 digits, it is a child of someone. 
                    // So in that case link it to the proper _parent
                    // First try cutting off the last two digits, but in the case of
                    // Filtered Items, it is possible that the _parent is not visible.
                    // If the _parent is not visible, work recursively to find the next 
                    // closest visible _parent until you get to the root
                    if (ToDoNode.Value.ToDoID.Length > 2)
                    {
                        string strID = ToDoNode.Value.ToDoID;
                        string strParentID = strID.Substring(1, strID.Length - 2);
                        bool blContinue = true;

                        while (blContinue)
                        {
                            NodeParent = FindChildByID(strParentID, ListOfToDoTree);
                            // NodeParent = F
                            if (NodeParent is not null)
                            {
                                var unused2 = NodeParent.InsertChild(ToDoNode);
                                bool unused1 = ListOfToDoTree.Remove(ToDoNode);
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
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public List<object> GetToDoList(LoadOptions LoadType, Application Application)
        {
            Items OlItems;
            View objView;
            Folder OlFolder;
            string strFilter;
            var ListObjects = new List<object>();

            objView = (View)Application.ActiveExplorer().CurrentView;
            strFilter = "@SQL=" + objView.Filter;

            foreach (Store oStore in Application.Session.Stores)
            {
                OlItems = null;
                OlFolder = (Folder)oStore.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                OlItems = strFilter == "@SQL=" | LoadType == LoadOptions.vbLoadAll ? OlFolder.Items : OlFolder.Items.Restrict(strFilter);

                foreach (var objItem in OlItems)
                    ListObjects.Add(objItem);
            }

            return ListObjects;
        }

        #endregion

        #region ToDoId

        public bool CompareToDoID(ToDoItem item, string strToDoID)
        {
            return (item.ToDoID ?? "") == (strToDoID ?? "");
        }

        internal int CompareItemsByToDoID(ToDoItem left, ToDoItem right)
        {
            string todoIDLeft = left.ToDoID;
            string todoIDRight = right.ToDoID;

            return CompareItemsByToDoID(todoIDLeft, todoIDRight);
        }

        internal int CompareItemsByToDoID(object objItemLeft, object objItemRight)
        {
            string todoIDLeft = objItemLeft.GetUdfString("ToDoID");
            string todoIDRight = objItemRight.GetUdfString("ToDoID");

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
                if (idx == -1) { return 0; }
                var left = BaseChanger.ConvertToDecimal(36, todoIDLeft[idx]);
                var right = BaseChanger.ConvertToDecimal(36, todoIDRight[idx]);
                if (left < right) { return -1; }
                else { return 1; }
            }
        }

        public void ReNumberIDs(ListOfIDsLegacy IDList)
        {
            foreach (var RootNode in ListOfToDoTree)
            {
                foreach (var Child in RootNode.Children)
                {
                    if (Child.Children.Count > 0)
                        ReNumberChildrenIDs(Child.Children, IDList);
                }
            }
        }
        
        public void ReNumberChildrenIDs(List<TreeNode<ToDoItem>> Children, ListOfIDsLegacy IDList)
        {
            var i = default(int);
            int max = Children.Count - 1;
            if (max >= 0)
            {
                string strParentID = Children[i].Parent.Value.ToDoID;
                var loopTo = max;
                for (i = 0; i <= loopTo; i++)
                {
                    if (IDList.UsedIDList.Contains(Children[i].Value.ToDoID))
                        IDList.UsedIDList.Remove(Children[i].Value.ToDoID);
                }
                var loopTo1 = max;
                for (i = 0; i <= loopTo1; i++)
                {
                    string NextID = IDList.GetNextAvailableToDoID(strParentID + "00");
                    // Dim LevelChange As Boolean = (Children(i).Value.ToDoID.Length = NextID.Length)
                    Children[i].Value.ToDoID = NextID;
                    // Children(i).Value.VisibleTreeState = 67
                    // Children(i).Value.ToDoID = Children(i).Value.ToDoID
                    if (Children[i].Children.Count > 0)
                        ReNumberChildrenIDs(Children[i].Children, IDList);
                }
                IDList.Save();
            }
        }

        public TreeNode<ToDoItem> FindChildByID(string ID, List<TreeNode<ToDoItem>> nodes)
        {
            TreeNode<ToDoItem> rnode;

            foreach (var node in nodes)
            {
                if ((node.Value.ToDoID ?? "") == (ID ?? ""))
                {
                    return node;
                }
                else
                {
                    rnode = FindChildByID(ID, node.Children);
                    if (rnode is not null)
                    {
                        return rnode;
                    }
                }
            }

            return null;

        }

        #endregion region

        public void AddChild(TreeNode<ToDoItem> Child, TreeNode<ToDoItem> Parent, ListOfIDsLegacy IDList)
        {
            Parent.Children.Add(Child);
            string strSeed = Parent.Children.Count > 1 ? Parent.Children[Parent.Children.Count - 2].Value.ToDoID : Parent.Value.ToDoID + "00";

            if (IDList.UsedIDList.Contains(Child.Value.ToDoID))
            {
                bool unused = IDList.UsedIDList.Remove(Child.Value.ToDoID);
            }
            Child.Value.ToDoID = IDList.GetNextAvailableToDoID(strSeed);
            if (Child.Children.Count > 0)
            {
                ReNumberChildrenIDs(Child.Children, IDList);
            }
            IDList.Save();
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
            Action<TreeNode<ToDoItem>> action = node => { if (node.ChildCount == 0) { if (IsHeader(node.Value.get_Context())) { node.Value.ActiveBranch = false; } } };

            foreach (TreeNode<ToDoItem> node in ListOfToDoTree)
                node.Traverse(action);
        }

        
        internal IList<T> MergeSort<T>(IList<T> coll, Comparison<T> comparison)
        {
            var Result = new List<T>();
            var Left = new Queue<T>();
            var Right = new Queue<T>();
            if (coll.Count <= 1)
                return coll;
            int midpoint = (int)Math.Round(coll.Count / 2d);

            for (int i = 0, loopTo = midpoint - 1; i <= loopTo; i++)
                Left.Enqueue(coll[i]);

            for (int i = midpoint, loopTo1 = coll.Count - 1; i <= loopTo1; i++)
                Right.Enqueue(coll[i]);


            Left = new Queue<T>(MergeSort(Left.ToList(), comparison));
            Right = new Queue<T>(MergeSort(Right.ToList(), comparison));
            Result = Merge(Left, Right, comparison);
            return Result;
        }
        
        internal List<T> Merge<T>(Queue<T> Left, Queue<T> Right, Comparison<T> comparison)
        {
            var Result = new List<T>();

            while (Left.Count > 0 && Right.Count > 0)
            {
                int cmp = comparison(Left.Peek(), Right.Peek());
                if (cmp < 0)
                {
                    Result.Add(Left.Dequeue());
                }
                else
                {
                    Result.Add(Right.Dequeue());
                }
            }

            while (Left.Count > 0)
                Result.Add(Left.Dequeue());

            while (Right.Count > 0)
                Result.Add(Right.Dequeue());

            return Result;
        }

        #region Debugging Helper Functions

        public void WriteTreeToCSVDebug(string FilePath)
        {

            using (var sw = new StreamWriter(FilePath))
            {
                sw.WriteLine("File Dump");
            }

            LoopTreeToWrite(ListOfToDoTree, FilePath, "");
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

        #endregion

    }
}