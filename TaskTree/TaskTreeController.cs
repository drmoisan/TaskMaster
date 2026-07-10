using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

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

        // Selects the supplied Outlook item in the active Explorer when it is visible there, otherwise
        // opens it in its own inspector. The parameter is typed as <see cref="object"/> (not `dynamic`)
        // so the Explorer selection calls bind statically against the mockable
        // <see cref="Outlook.Explorer"/> interface. The caller (<see cref="TreeLvActivateItem"/>) gates
        // this via <see cref="IsValidType"/>, so <paramref name="item"/> is always a
        // <see cref="Outlook.MailItem"/> or <see cref="Outlook.TaskItem"/>; the strongly-typed
        // <see cref="DisplayOutlookItem"/> dispatch covers the display branch without late binding.
        internal void ActivateOlItem(object item)
        {
            if (item is null)
            {
                return;
            }
            var activeExplorer = _globals.Ol.App.ActiveExplorer();
            if (activeExplorer.IsItemSelectableInView(item))
            {
                activeExplorer.ClearSelection();
                activeExplorer.AddToSelection(item);
            }
            else
            {
                DisplayOutlookItem(item);
            }
        }

        // Async counterpart of <see cref="ActivateOlItem"/>. Uses the same statically-bound
        // <see cref="object"/> seam so the Explorer selection is mockable; the <c>Task.Run</c> wrapping
        // only offloads the synchronous COM calls and does not affect testability of the routing.
        internal async Task ActivateOlItemAsync(object item)
        {
            if (item is null)
            {
                return;
            }
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
                    DisplayOutlookItem(item);
                }
            });
            await Task.Run(activeExplorer.Activate);
        }

        /// <summary>
        /// Opens the supplied Outlook item in its own inspector via strongly-typed dispatch. Only the
        /// two item kinds admitted by <see cref="IsValidType"/> (<see cref="Outlook.MailItem"/> and
        /// <see cref="Outlook.TaskItem"/>) are handled; any other type is ignored. Typed dispatch
        /// replaces the former late-bound `dynamic` call so the display branch binds against the
        /// mockable interop interfaces.
        /// </summary>
        private static void DisplayOutlookItem(object item)
        {
            switch (item)
            {
                case Outlook.MailItem mail:
                    mail.Display();
                    break;
                case Outlook.TaskItem task:
                    task.Display();
                    break;
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
