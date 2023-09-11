using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Outlook = Microsoft.Office.Interop.Outlook;
using ToDoModel;
using UtilitiesCS;

namespace TaskTree
{
    public class TaskTreeController
    {
        public TaskTreeController(IApplicationGlobals AppGlobals, TaskTreeForm Viewer, TreeOfToDoItems DataModel)
        {
            _globals = AppGlobals;
            _viewer = Viewer;
            _dataModel = DataModel;
            _viewer.SetController(this);
        }

        
        public List<TreeNode<ToDoItem>> ToDoTree = new List<TreeNode<ToDoItem>>();
        private readonly Resizer rs = new Resizer();
        private readonly Resizer rscol = new Resizer();
        private bool expanded = false;
        private bool filtercompleted = true;
        private TaskTreeForm _viewer;
        private IApplicationGlobals _globals;
        public TreeOfToDoItems _dataModel = new TreeOfToDoItems(new List<TreeNode<ToDoItem>>());

        public void InitializeTreeListView()
        {

            {
                _viewer.TLV.CanExpandGetter = (x => ((TreeNode<ToDoItem>)x).ChildCount > 0);
                _viewer.TLV.ChildrenGetter = (x => ((TreeNode<ToDoItem>)x).Children);
                _viewer.TLV.ModelFilter = new ModelFilter(x => ((TreeNode<ToDoItem>)x).Value.Complete == false);
                _viewer.TLV.Roots = _dataModel.ListOfToDoTree;
                _viewer.TLV.Sort(_viewer.OlvToDoID, SortOrder.Ascending);
            }

            SimpleDropSink sink1 = (SimpleDropSink)_viewer.TLV.DropSink;
            sink1.AcceptExternal = true;
            sink1.CanDropBetween = true;
            sink1.CanDropOnBackground = true;

            rs.FindAllControls(_viewer);
            rs.SetResizeDimensions(_viewer.SplitContainer1, Resizer.ResizeDimensions.None, true);
            rs.SetResizeDimensions(_viewer.SplitContainer1.Panel2, Resizer.ResizeDimensions.Position | Resizer.ResizeDimensions.Size, true);
            rs.PrintDict();
        }

        internal void HandleModelCanDrop(object sender, ModelDropEventArgs e)
        {
            e.Handled = true;
            e.Effect = DragDropEffects.None;

            if (e.SourceModels.Contains(e.TargetModel))
            {
                e.InfoMessage = "Cannot drop on self";
            }
            else
            {
                var sourceModels = e.SourceModels.Cast<TreeNode<ToDoItem>>();

                if (e.DropTargetLocation == DropTargetLocation.Background)
                {
                    if (ReferenceEquals(e.SourceListView, e.ListView) && sourceModels.All(x => x.Parent is null))
                    {
                        e.InfoMessage = "Dragged objects are already roots";
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                        e.InfoMessage = "Drop on background to promote to roots";
                    }
                }
                else if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                {
                    e.Effect = DragDropEffects.Move;
                    e.InfoMessage = "Drop above item to reorder";
                }
                else
                {
                    TreeNode<ToDoItem> target = (TreeNode<ToDoItem>)e.TargetModel;

                    if (sourceModels.Any(x => target.IsAncestor(x)))
                    {
                        e.InfoMessage = "Cannot drop on descendant (think of the temporal paradoxes!)";
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                }
            }
        }

        internal void HandleModelDropped(object sender, ModelDropEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine("Fired HandleModelDropped");

            switch (e.DropTargetLocation)
            {
                case DropTargetLocation.AboveItem:
                    {
                        MoveObjectsToSibling(e.ListView as TreeListView, e.SourceListView as TreeListView, (TreeNode<ToDoItem>)e.TargetModel, e.SourceModels, 0);
                        break;
                    }
                case DropTargetLocation.BelowItem:
                    {
                        MoveObjectsToSibling(e.ListView as TreeListView, e.SourceListView as TreeListView, (TreeNode<ToDoItem>)e.TargetModel, e.SourceModels, 1);
                        break;
                    }
                case DropTargetLocation.Background:
                    {
                        MoveObjectsToRoots(e.ListView as TreeListView, e.SourceListView as TreeListView, e.SourceModels);
                        break;
                    }
                case DropTargetLocation.Item:
                    {
                        MoveObjectsToChildren(e.ListView as TreeListView, e.SourceListView as TreeListView, (TreeNode<ToDoItem>)e.TargetModel, e.SourceModels);
                        break;
                    }

                default:
                    {
                        return;
                    }
            }
            e.RefreshObjects();
            if (filtercompleted)
                _viewer.TLV.ModelFilter = new ModelFilter(x => ((TreeNode<ToDoItem>)x).Value.Complete == false);
            _viewer.TLV.Sort();
            // this.lastSortColumn = Column;
            // this.lastSortOrder = order;
        }

        internal void MoveObjectsToRoots(TreeListView targetTree, TreeListView sourceTree, IList toMove)
        {
            if (ReferenceEquals(sourceTree, targetTree))                // Data Model: Check to see if the desination tree roots are in the same tree
            {
                foreach (TreeNode<ToDoItem> x in toMove)
                {

                    if (x.Parent is not null)
                    {
                        x.Parent.RemoveChild(x);             // Data Model: Remove pointer to node from parent.children list
                                                             // x.Parent.Children.Remove(x)         'Data Model: Remove pointer to node from parent.children list
                                                             // x.Parent = Nothing                  'Data Model: Set the pointer to the parent inside the node to nothing
                        sourceTree.AddObject(x);             // TreeListView: Add the node to the source tree as a FldrRoot node
                    }
                }
            }
            else                                            // Data Model: If the destination tree is different than the source tree
            {

                foreach (TreeNode<ToDoItem> x in toMove)
                {

                    if (x.Parent is null)             // Data Model: If the node was a root in the old tree
                    {
                        sourceTree.RemoveObject(x);          // TreeListView: Delete the pointer in the tree to the node
                    }
                    else                                    // Data Model: If the node was NOT a root in the old tree
                    {
                        x.Parent.RemoveChild(x);
                    }             // Data Model: Grab the parent node and delete the pointer from the list of children

                    x.Parent = null;                      // Data Model: Delete the pointer in the node to the parent
                    targetTree.AddObject(x);                 // TreeListView: Add the node to the new tree as a root
                }
            }
        }

        internal void MoveObjectsToSibling(TreeListView targetTree, TreeListView sourceTree, TreeNode<ToDoItem> target, IList toMove, int siblingOffset)
        {
            foreach (TreeNode<ToDoItem> x in toMove)
            {
                if (x.Parent is null)
                {
                    if (_dataModel.ListOfToDoTree.Contains(x))
                    {
                        _dataModel.ListOfToDoTree.Remove(x);         // Data Model: Remove node from roots
                    }
                    else
                    {
                        MessageBox.Show("Error in MoveObjectsToSibling: TreeListView and DataModel out of sync at roots");
                    }
                }
                else
                {
                    x.Parent.RemoveChild(x);
                }                 
                x.Parent = target.Parent;                    
            }

            // Now add to the moved objects to children of their parent (or to the roots collection
            // if the target is a root)
            if (target.Parent is null)
            {
                // targetRootsChanged = True                   'TreeListview:
                // targetRoots.InsertRange(targetRoots.IndexOf(target) + siblingOffset, toMove) 'TreeListview: Inserted into new tree
                // DataModel: Nothing here. Is this dealt with?
                _dataModel.ListOfToDoTree.AddRange((IEnumerable<TreeNode<ToDoItem>>)toMove);
                string strSeed = _dataModel.ListOfToDoTree.Count > toMove.Count ? _dataModel.ListOfToDoTree[_dataModel.ListOfToDoTree.Count - toMove.Count - 2].Value.ToDoID : "00";

                var loopTo = _dataModel.ListOfToDoTree.Count - 1;
                for (int i = _dataModel.ListOfToDoTree.Count - toMove.Count - 1; i <= loopTo; i++)
                {
                    strSeed = _globals.TD.IDList.GetNextToDoID(strSeed);
                    _dataModel.ListOfToDoTree[i].Value.ToDoID = strSeed;
                }
            }
            else
            {
                // Insert moved object into DATAMODEL children of new parent
                int idx = target.Parent.Children.IndexOf(target) + siblingOffset;
                // Inconsistent with case of Parent is nothing
                target.Parent.Children.InsertRange(idx, toMove.Cast<TreeNode<ToDoItem>>()); // DataModel: Inserted into new data model tree. 
                _dataModel.ReNumberChildrenIDs(target.Parent.Children, (IDList)_globals.TD.IDList);

            }         


        }

        internal void MoveObjectsToChildren(TreeListView targetTree, TreeListView sourceTree, TreeNode<ToDoItem> target, IList toMove)
        {
            foreach (TreeNode<ToDoItem> x in toMove)
            {
                if (x.Parent is null)
                {
                    sourceTree.RemoveObject(x);              
                    if (_dataModel.ListOfToDoTree.Contains(x))
                    {
                        _dataModel.ListOfToDoTree.Remove(x);         
                    }
                    else
                    {
                        MessageBox.Show("Error in MoveObjectsToChildren: TreeListView and DataModel out of sync at roots");
                    }
                }
                else
                {
                    x.Parent.Children.Remove(x);
                }             

                x.Parent = target;                                   
                _dataModel.AddChild(x, target, _globals.TD.IDList);    
            }
        }

        private TreeNode<ToDoItem> FindChildByID(string ID, List<TreeNode<ToDoItem>> nodes)
        {
            //QUESTION: Why is this method here? Shouldn't it be part of the class ToDoTree?
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
        
        internal TreeNode<ToDoItem> GetSelectedTreeNode()
        {
            var item = _viewer.TLV.GetItem(_viewer.TLV.SelectedIndex).RowObject;
            return item as TreeNode<ToDoItem>;
        }
                
        internal bool IsValidType(object item)
        {
            return ((item is Outlook.MailItem) || (item is Outlook.TaskItem));
        }
        
        internal void ActivateOlItem(dynamic item)
        {
            if (item is not null)
            {
                var activeExplorer = _globals.Ol.App.ActiveExplorer();
                if (activeExplorer.IsItemSelectableInView(item))
                {
                    activeExplorer.ClearSelection();
                    activeExplorer.AddToSelection(item);
                }
                else { item.Display(); }
            }
        }

        internal void TlvActivateItem()
        {
            var node = GetSelectedTreeNode();
            if (node is not null) 
            {
                var objItem = node.Value.GetItem();
                if (IsValidType(objItem)) { ActivateOlItem(objItem); }
                else { MessageBox.Show($"Unsupported type. Selection is of type {objItem.GetType()}"); }
            }
        }

        internal void FormatRow(object sender, FormatRowEventArgs e)
        {
            ToDoItem objToDo = (ToDoItem)e.Model;
            e.Item.Font = objToDo.Complete ? new Font(e.Item.Font, e.Item.Font.Style | FontStyle.Strikeout) : new Font(e.Item.Font, e.Item.Font.Style & ~FontStyle.Strikeout);
        }

        internal void ToggleExpandCollapseAll()
        {
            if (expanded)
            {
                _viewer.TLV.CollapseAll();
            }
            else
            {
                _viewer.TLV.ExpandAll();
            }
            expanded = !expanded;

        }

        internal void ResizeForm()
        {
            rs.ResizeAllControls(_viewer);
            _viewer.TLV.AutoScaleColumnsToContainer();
        }

        internal void ToggleHideComplete()
        {
            if (filtercompleted)
            {
                _viewer.TLV.ModelFilter = null;
                filtercompleted = false;
            }
            else
            {
                _viewer.TLV.ModelFilter = new ModelFilter(x => ((TreeNode<ToDoItem>)x).Value.Complete == false);
                filtercompleted = true;
            }
        }

        internal void RebuildTreeVisual()
        {
            _viewer.TLV.Roots = _dataModel.ListOfToDoTree;
            _viewer.TLV.RebuildAll(preserveState: false);
        }

        #region debugging helper functions

        public void WriteTreeToDisk(string filepath)
        {
            string filename = Path.Combine(filepath, "DebugTreeDump.csv");

            using (var sw = new StreamWriter(filename))
            {
                sw.WriteLine("File Dump");
            }
            LoopTreeToWrite(ToDoTree, filename, "");
        }

        public void LoopTreeToWrite(List<TreeNode<ToDoItem>> nodes, string filename, string lineprefix)
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

        public void AppendLineToCSV(string filename, string line)
        {
            using (var sw = File.AppendText(filename))
            {
                sw.WriteLine(line);
            }
        }

        #endregion

    }
}