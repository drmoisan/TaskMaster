using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;
using UtilitiesCS;

namespace TaskTree
{
    // Exemption site E1. Form-derived WinForms class (ratified COM/VSTO/WinForms coverage exemption,
    // category b). Every facade member below is a thin delegation to the live TreeLv/ControlResizer
    // that requires a window handle; the private event handlers forward to the controller. The
    // class-level attribute on this partial declaration also covers the TaskTreeForm.Designer.cs
    // designer-generated partial.
    [ExcludeFromCodeCoverage]
    public partial class TaskTreeForm : Form, ITaskTreeForm
    {
        #region Constructors

        public TaskTreeForm()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        #endregion Constructors

        #region Public Methods and Properties

        private TaskTreeController _controller;
        private readonly ControlResizer _rs = new ControlResizer();

        public void SetController(TaskTreeController Controller)
        {
            _controller = Controller;
        }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext
        {
            get => _context;
        }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler
        {
            get => _uiScheduler;
        }

        #endregion Public Methods and Properties

        #region ITaskTreeForm Facade Implementation

        public void InitializeTreeView(
            IEnumerable<TreeNode<ToDoItem>> roots,
            Predicate<object> incompleteFilter
        )
        {
            TreeLv.CanExpandGetter = x => ((TreeNode<ToDoItem>)x).ChildCount > 0;
            TreeLv.ChildrenGetter = x => ((TreeNode<ToDoItem>)x).Children;
            TreeLv.ParentGetter = x => ((TreeNode<ToDoItem>)x).Parent;
            TreeLv.ModelFilter = new ModelFilter(incompleteFilter);
            TreeLv.Roots = roots;
            TreeLv.Sort(OlvToDoID, SortOrder.Ascending);

            SimpleDropSink sink1 = (SimpleDropSink)TreeLv.DropSink;
            sink1.AcceptExternal = true;
            sink1.CanDropBetween = true;
            sink1.CanDropOnBackground = true;
        }

        public void SetModelFilter(Predicate<object> filter)
        {
            TreeLv.ModelFilter = filter is null ? null : new ModelFilter(filter);
        }

        public void SortTree()
        {
            TreeLv.Sort();
        }

        public void ExpandAllNodes()
        {
            TreeLv.ExpandAll();
        }

        public void CollapseAllNodes()
        {
            TreeLv.CollapseAll();
        }

        public void RebuildTree(IEnumerable<TreeNode<ToDoItem>> roots)
        {
            TreeLv.Roots = roots;
            TreeLv.RebuildAll(preserveState: false);
        }

        public void AutoSizeTreeColumns()
        {
            TreeLv.AutoScaleColumnsToContainer();
        }

        public TreeNode<ToDoItem> GetSelectedNode()
        {
            try
            {
                return TreeLv.GetItem(TreeLv.SelectedIndex).RowObject as TreeNode<ToDoItem>;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public void ResizeControls()
        {
            _rs.FindAllControls(this);
            _rs.SetResizeDimensions(SplitContainer1, ControlResizer.ResizeDimensions.None, true);
            _rs.SetResizeDimensions(
                SplitContainer1.Panel2,
                ControlResizer.ResizeDimensions.Position | ControlResizer.ResizeDimensions.Size,
                true
            );
            _rs.ResizeAllControls(this);
        }

        #endregion ITaskTreeForm Facade Implementation

        #region Event Handlers

        private void TaskTreeForm_Load(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.InitializeTreeListView();
        }

        private void HandleModelCanDrop(object sender, ModelDropEventArgs e)
        {
            if (_controller is not null)
                _controller.HandleModelCanDrop(sender, e);
        }

        private void HandleModelDropped(object sender, ModelDropEventArgs e)
        {
            if (_controller is not null)
                _controller.HandleModelDropped(sender, e);
        }

        private async void TLV_ItemActivate(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_context);

            if (_controller is not null)
                await _controller.TreeLvActivateItemAsync();
        }

        private void FormatRow(object sender, FormatRowEventArgs e)
        {
            if (_controller is not null)
                _controller.FormatRow(sender, e);
        }

        private void But_ExpandCollapse_Click(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.ToggleExpandCollapseAll();
        }

        private void TaskTreeForm_Resize(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.ResizeForm();
        }

        private void But_ShowHideComplete_Click(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.ToggleHideComplete();
        }

        private void But_ReloadTree_Click(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.RebuildTreeVisual();
        }

        #endregion Event Handlers
    }
}
