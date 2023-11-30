using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;

namespace TaskTree
{
    public partial class TaskTreeForm : Form
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
        public void SetController(TaskTreeController Controller)
        {
            _controller = Controller;
        }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }

        #endregion Public Methods and Properties

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
                
        private void TLV_ItemActivate(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.TreeLvActivateItem();
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