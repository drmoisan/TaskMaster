using System;
using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel;

namespace TaskTree
{
    public partial class TaskTreeForm: Form
    {
        private TaskTreeController _controller;

        public TaskTreeForm()
        {

            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

        }

        public void SetController(TaskTreeController Controller)
        {
            _controller = Controller;
        }

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

        private void MoveObjectsToRoots(TreeListView targetTree, TreeListView sourceTree, IList toMove)
        {
            if (_controller is not null)
                _controller.MoveObjectsToRoots(targetTree, sourceTree, toMove);
        }

        private void MoveObjectsToSibling(TreeListView targetTree, TreeListView sourceTree, TreeNode<ToDoItem> target, IList toMove, int siblingOffset)
        {

            if (_controller is not null)
                _controller.MoveObjectsToSibling(targetTree, sourceTree, target, toMove, siblingOffset);
        }

        private void MoveObjectsToChildren(TreeListView targetTree, TreeListView sourceTree, TreeNode<ToDoItem> target, IList toMove)
        {

            if (_controller is not null)
                _controller.MoveObjectsToChildren(targetTree, sourceTree, target, toMove);
        }



        private void TLV_ItemActivate(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.TlvActivateItem();
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
    }
}