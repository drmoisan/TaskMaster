using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskTree
{
    /// <summary>
    /// Host-neutral drag/drop move and tree-data logic for <see cref="TaskTreeController"/>. These
    /// members operate against the data model and the narrow <see cref="ITreeVisual"/> seam only, so
    /// they are unit-testable without a live control.
    /// </summary>
    public partial class TaskTreeController
    {
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
                    if (
                        ReferenceEquals(e.SourceListView, e.ListView)
                        && sourceModels.All(x => x.Parent is null)
                    )
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
                        e.InfoMessage =
                            "Cannot drop on descendant (think of the temporal paradoxes!)";
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                }
            }
        }

        // Exemption site E6. Thin control-bound drop-event marshalling wrapper. Its only irreducible work
        // is building the E2 adapters from the live drop-event controls (e.ListView/e.SourceListView) and
        // calling e.RefreshObjects(), which throws NullReferenceException without a live ObjectListView
        // handle. The host-neutral routing decision is extracted into the fully unit-tested
        // <see cref="RouteDrop"/>, and the post-drop filter/sort re-application is delegated to the
        // unit-tested <see cref="ApplyPostDropView"/> (ratified WinForms exemption, category b/c analog).
        [ExcludeFromCodeCoverage]
        internal void HandleModelDropped(object sender, ModelDropEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine("Fired HandleModelDropped");

            var targetTree = e.ListView as TreeListView;
            var sourceTree = e.SourceListView as TreeListView;
            ITreeVisual targetVisual = new TreeListViewVisual(targetTree);
            ITreeVisual sourceVisual = ReferenceEquals(sourceTree, targetTree)
                ? targetVisual
                : new TreeListViewVisual(sourceTree);

            if (!RouteDrop(targetVisual, sourceVisual, e))
            {
                return;
            }
            e.RefreshObjects();
            ApplyPostDropView();
        }

        /// <summary>
        /// Routes a completed drop to the matching host-neutral move operation based on
        /// <see cref="ModelDropEventArgs.DropTargetLocation"/>. Operates only against the mockable
        /// <see cref="ITreeVisual"/> seam and the test-constructible <see cref="ModelDropEventArgs"/>
        /// (target model, source models, drop location), so it is fully unit-testable without a live
        /// control. Returns <c>true</c> when a move was dispatched and <c>false</c> for unhandled drop
        /// locations, letting the caller skip the post-drop refresh/sort.
        /// </summary>
        /// <returns><c>true</c> if a move operation was dispatched; otherwise <c>false</c>.</returns>
        internal bool RouteDrop(
            ITreeVisual targetVisual,
            ITreeVisual sourceVisual,
            ModelDropEventArgs e
        )
        {
            switch (e.DropTargetLocation)
            {
                case DropTargetLocation.AboveItem:
                    MoveObjectsToSibling(
                        targetVisual,
                        sourceVisual,
                        (TreeNode<ToDoItem>)e.TargetModel,
                        e.SourceModels,
                        0
                    );
                    return true;
                case DropTargetLocation.BelowItem:
                    MoveObjectsToSibling(
                        targetVisual,
                        sourceVisual,
                        (TreeNode<ToDoItem>)e.TargetModel,
                        e.SourceModels,
                        1
                    );
                    return true;
                case DropTargetLocation.Background:
                    MoveObjectsToRoots(targetVisual, sourceVisual, e.SourceModels);
                    return true;
                case DropTargetLocation.Item:
                    MoveObjectsToChildren(
                        targetVisual,
                        sourceVisual,
                        (TreeNode<ToDoItem>)e.TargetModel,
                        e.SourceModels
                    );
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Re-applies the current incomplete-item filter (when active) and re-sorts the tree after a
        /// drop. Delegates only to the mockable <see cref="ITaskTreeForm"/> seam, so it is unit-testable
        /// without a live control.
        /// </summary>
        internal void ApplyPostDropView()
        {
            if (_filterCompleted)
                _viewer.SetModelFilter(x => ((TreeNode<ToDoItem>)x).Value.Complete == false);
            _viewer.SortTree();
        }

        internal void MoveObjectsToRoots(
            ITreeVisual targetTree,
            ITreeVisual sourceTree,
            IList toMove
        )
        {
            if (ReferenceEquals(sourceTree, targetTree)) // Data Model: Check to see if the desination tree roots are in the same tree
            {
                foreach (TreeNode<ToDoItem> x in toMove)
                {
                    if (x.Parent is not null)
                    {
                        x.Parent.RemoveChild(x); // Data Model: Remove pointer to node from parent.children list
                        sourceTree.AddObject(x); // TreeListView: Add the node to the source tree as a FldrRoot node
                    }
                }
            }
            else // Data Model: If the destination tree is different than the source tree
            {
                foreach (TreeNode<ToDoItem> x in toMove)
                {
                    if (x.Parent is null) // Data Model: If the node was a root in the old tree
                    {
                        sourceTree.RemoveObject(x); // TreeListView: Delete the pointer in the tree to the node
                    }
                    else // Data Model: If the node was NOT a root in the old tree
                    {
                        x.Parent.RemoveChild(x);
                    } // Data Model: Grab the parent node and delete the pointer from the list of children

                    x.Parent = null; // Data Model: Delete the pointer in the node to the parent
                    targetTree.AddObject(x); // TreeListView: Add the node to the new tree as a root
                }
            }
        }

        internal void MoveObjectsToSibling(
            ITreeVisual targetTree,
            ITreeVisual sourceTree,
            TreeNode<ToDoItem> target,
            IList toMove,
            int siblingOffset
        )
        {
            foreach (TreeNode<ToDoItem> x in toMove)
            {
                if (x.Parent is null)
                {
                    if (_dataModel.Roots.Contains(x))
                    {
                        _dataModel.Roots.Remove(x); // Data Model: Remove node from roots
                    }
                    else
                    {
                        _showMessage(
                            "Error in MoveObjectsToSibling: TreeListView and DataModel out of sync at roots"
                        );
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
                _dataModel.Roots.AddRange((IEnumerable<TreeNode<ToDoItem>>)toMove);
                string strSeed =
                    _dataModel.Roots.Count > toMove.Count
                        ? _dataModel.Roots[_dataModel.Roots.Count - toMove.Count - 2].Value.ToDoID
                        : "00";

                var loopTo = _dataModel.Roots.Count - 1;
                for (int i = _dataModel.Roots.Count - toMove.Count - 1; i <= loopTo; i++)
                {
                    strSeed = _globals.TD.IDList.GetNextToDoID(strSeed);
                    _dataModel.Roots[i].Value.ToDoID = strSeed;
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

        internal void MoveObjectsToChildren(
            ITreeVisual targetTree,
            ITreeVisual sourceTree,
            TreeNode<ToDoItem> target,
            IList toMove
        )
        {
            foreach (TreeNode<ToDoItem> x in toMove)
            {
                if (x.Parent is null)
                {
                    sourceTree.RemoveObject(x);
                    if (_dataModel.Roots.Contains(x))
                    {
                        _dataModel.Roots.Remove(x);
                    }
                    else
                    {
                        _showMessage(
                            "Error in MoveObjectsToChildren: TreeListView and DataModel out of sync at roots"
                        );
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

        internal TreeNode<ToDoItem> FindChildByID(string ID, List<TreeNode<ToDoItem>> nodes)
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

        internal bool IsValidType(object item)
        {
            return ((item is Outlook.MailItem) || (item is Outlook.TaskItem));
        }
    }
}
