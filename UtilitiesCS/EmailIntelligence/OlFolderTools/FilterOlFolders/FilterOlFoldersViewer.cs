using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.Office.Interop.Outlook;
using ObjectListViewDemo;

namespace UtilitiesCS
{
    public partial class FilterOlFoldersViewer : Form
    {
        public FilterOlFoldersViewer()
        {
            InitializeComponent();
        }

        private FilterOlFoldersController _controller;
        public void SetController(FilterOlFoldersController controller)
        {
            _controller = controller;
            //SetupDragAndDrop();
            SetupTree();
            SetupRenderer(this.TlvNotFiltered.TreeColumnRenderer);
            SetupRenderer(this.TlvFiltered.TreeColumnRenderer);
        }
        
        private void SetupDragAndDrop()
        {

            // Setup the tree so that it can drop and drop.

            // Dropping doesn't do anything, but it does show how it works

            TlvNotFiltered.IsSimpleDragSource = true;
            TlvNotFiltered.IsSimpleDropSink = true;

            TlvNotFiltered.ModelCanDrop += delegate (object sender, ModelDropEventArgs e) {
                e.Effect = DragDropEffects.None;
                if (e.TargetModel == null)
                    return;

                if (e.TargetModel is DirectoryInfo)
                    e.Effect = e.StandardDropActionFromKeys;
                else
                    e.InfoMessage = "Can only drop on directories";
            };

            TlvNotFiltered.ModelDropped += delegate (object sender, ModelDropEventArgs e) {
                String msg = String.Format("{2} items were dropped on '{1}' as a {0} operation.",
                    e.Effect, ((DirectoryInfo)e.TargetModel).Name, e.SourceModels.Count);
                MessageBox.Show(msg, "Object List View Demo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void SetupRenderer(TreeListView.TreeRenderer renderer)
        {
            var penSize = 2.0f * UiThread.AutoScaleFactor.Width;
            renderer.LinePen = new Pen(Color.Firebrick, penSize);
            renderer.LinePen.DashStyle = DashStyle.Dot;
            renderer.UseTriangles = true;
            renderer.IsShowGlyphs = true;
        }
        
        private void SetupTree()
        {
            this.TlvNotFiltered.CanExpandGetter = x => ((TreeNode<FolderWrapper>)x).ChildCount>0;
            this.TlvNotFiltered.ChildrenGetter = x => ((TreeNode<FolderWrapper>)x).Children;
            this.TlvNotFiltered.ParentGetter = x => ((TreeNode<FolderWrapper>)x).Parent;
            this.OlvNameNotFiltered.ImageGetter = x => 0;
            this.TlvNotFiltered.Roots = _controller.OlFolderTree.FilterSelected(false);
            //this.TlvNotFiltered.Roots = _controller.OlFolderTree.Roots;
            //this.TlvNotFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == false);


            this.TlvFiltered.CanExpandGetter = x => ((TreeNode<FolderWrapper>)x).ChildCount > 0;
            this.TlvFiltered.ChildrenGetter = x => ((TreeNode<FolderWrapper>)x).Children;
            this.TlvFiltered.ParentGetter = x => ((TreeNode<FolderWrapper>)x).Parent;
            this.OlvNameFiltered.ImageGetter = x => 0;
            this.TlvFiltered.Roots = _controller.OlFolderTree.FilterSelected(true);
            //this.TlvFiltered.Roots = _controller.OlFolderTree.Roots;
            //this.TlvFiltered.ModelFilter = new ModelFilter(x => ((TreeNode<OlFolderInfo>)x).Value.Selected == true);
        }

        

        /// <summary>
        /// Format a file size into a more intelligible value
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string FormatFileSize(long size)
        {
            int[] limits = new int[] { 1024 * 1024 * 1024, 1024 * 1024, 1024 };
            string[] units = new string[] { "GB", "MB", "KB" };

            for (int i = 0; i < limits.Length; i++)
            {
                if (size >= limits[i])
                    return String.Format("{0:#,##0.##} " + units[i], ((double)size / limits[i]));
            }

            return String.Format("{0} bytes", size);
        }

        private void BtnDiscard_Click(object sender, EventArgs e) => _controller?.Discard();
        private void BtnSave_Click(object sender, EventArgs e) => _controller?.Save();
    }
}
