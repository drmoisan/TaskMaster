using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;
using UtilitiesCS;

namespace TaskTree
{
    /// <summary>
    /// Coordinates the task-tree UI against the <see cref="ITaskTreeForm"/> facade. Host-neutral
    /// drag/drop move and tree-data logic lives in the <c>TaskTreeController.MoveLogic.cs</c> partial.
    /// </summary>
    public partial class TaskTreeController
    {
        #region Constructors and Initializers

        public TaskTreeController(
            IApplicationGlobals AppGlobals,
            ITaskTreeForm Viewer,
            TreeOfToDoItems DataModel,
            Action<string> showMessage = null
        )
        {
            _globals = AppGlobals;
            _viewer = Viewer;
            _dataModel = DataModel;
            _showMessage = showMessage ?? (m => MessageBox.Show(m));
            _viewer.SetController(this);
        }

        public void InitializeTreeListView()
        {
            _viewer.InitializeTreeView(
                _dataModel.Roots,
                x => ((TreeNode<ToDoItem>)x).Value.Complete == false
            );
            _viewer.ResizeControls();
        }

        #endregion Constructors and Initializers

        #region Private Fields

        private bool _expanded = false;
        private bool _filterCompleted = true;
        private readonly ITaskTreeForm _viewer;
        private readonly IApplicationGlobals _globals;
        private readonly Action<string> _showMessage;
        public TreeOfToDoItems _dataModel = new TreeOfToDoItems(new List<TreeNode<ToDoItem>>());

        #endregion Private Fields

        #region UI Helper Functions

        // Exemption site E4. Direct Outlook Explorer interaction via a late-bound `dynamic` item.
        // The `dynamic item` parameter forces runtime binding of `activeExplorer.IsItemSelectableInView(item)`,
        // `AddToSelection(item)`, and `item.Display()`; that late-bound dispatch cannot resolve against a
        // Moq interop proxy (RuntimeBinderException) and has no injectable seam, so it requires a live
        // Outlook Explorer (ratified COM exemption, category c).
        [ExcludeFromCodeCoverage]
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
                else
                {
                    item.Display();
                }
            }
        }

        // Exemption site E5. Async counterpart of E4; same late-bound `dynamic` Explorer dispatch with
        // no mockable seam (ratified COM exemption, category c).
        [ExcludeFromCodeCoverage]
        internal async Task ActivateOlItemAsync(dynamic item)
        {
            if (item is not null)
            {
                var activeExplorer = _globals.Ol.App.ActiveExplorer();
                await Task.Run(() =>
                {
                    if (activeExplorer.IsItemSelectableInView(item))
                    {
                        activeExplorer.ClearSelection();
                        activeExplorer.AddToSelection(item);
                    }
                    else
                    {
                        item.Display();
                    }
                });
                await Task.Run(activeExplorer.Activate);
            }
        }

        // Exemption site E3. Residual event-handler wrapper: reads e.Model (get-only) and assigns
        // e.Item.Font; FormatRowEventArgs/OLVListItem are not constructible from TaskTree.Test in
        // ObjectListView 2.9.1 and require a live TreeListView row item. The strikeout DECISION is
        // extracted into the covered, host-neutral ResolveRowStyle below.
        [ExcludeFromCodeCoverage]
        internal void FormatRow(object sender, FormatRowEventArgs e)
        {
            var node = (TreeNode<ToDoItem>)e.Model;
            var todo = node.Value;
            e.Item.Font = new Font(e.Item.Font, ResolveRowStyle(e.Item.Font.Style, todo.Complete));
        }

        /// <summary>
        /// Host-neutral strikeout decision for a row font style. Adds <see cref="FontStyle.Strikeout"/>
        /// when the item is complete and removes it otherwise. Extracted from <see cref="FormatRow"/>
        /// so the decision is unit-testable without constructing a live row (the residual
        /// <see cref="FormatRow"/> wrapper is covered by exemption E3).
        /// </summary>
        internal static FontStyle ResolveRowStyle(FontStyle baseStyle, bool complete) =>
            complete ? (baseStyle | FontStyle.Strikeout) : (baseStyle & ~FontStyle.Strikeout);

        internal void ToggleExpandCollapseAll()
        {
            if (_expanded)
            {
                _viewer.CollapseAllNodes();
            }
            else
            {
                _viewer.ExpandAllNodes();
            }
            _expanded = !_expanded;
        }

        internal void ResizeForm()
        {
            _viewer.ResizeControls();
            _viewer.AutoSizeTreeColumns();
        }

        internal void RebuildTreeVisual()
        {
            _viewer.RebuildTree(_dataModel.Roots);
        }

        internal void ToggleHideComplete()
        {
            if (_filterCompleted)
            {
                _viewer.SetModelFilter(null);
                _filterCompleted = false;
            }
            else
            {
                _viewer.SetModelFilter(x => ((TreeNode<ToDoItem>)x).Value.Complete == false);
                _filterCompleted = true;
            }
        }

        internal void TreeLvActivateItem()
        {
            var node = GetSelectedTreeNode();
            if (node is not null)
            {
                var objItem = node.Value.OlItem.InnerObject;
                if (IsValidType(objItem))
                {
                    ActivateOlItem(objItem);
                }
                else
                {
                    _showMessage($"Unsupported type. Selection is of type {objItem.GetType()}");
                }
            }
        }

        internal async Task TreeLvActivateItemAsync()
        {
            var node = GetSelectedTreeNode();
            if (node is not null)
            {
                var objItem = node.Value.OlItem.InnerObject;
                if (IsValidType(objItem))
                {
                    await ActivateOlItemAsync(objItem);
                }
                else
                {
                    _showMessage($"Unsupported type. Selection is of type {objItem.GetType()}");
                }
            }
        }

        internal TreeNode<ToDoItem> GetSelectedTreeNode()
        {
            return _viewer.GetSelectedNode();
        }

        #endregion UI Helper Functions
    }
}
