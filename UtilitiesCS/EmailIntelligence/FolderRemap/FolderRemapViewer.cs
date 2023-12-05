using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    public partial class FolderRemapViewer : Form
    {
        public FolderRemapViewer()
        {
            InitializeComponent();
        }
        private FolderRemapController _controller;
        public void SetController(FolderRemapController controller)
        {
            _controller = controller;
            //SetupDragAndDrop();
            SetupTree();
            SetupRenderer(this.TlvOriginal.TreeColumnRenderer);
            
        }

        

        private void SetupRenderer(TreeListView.TreeRenderer renderer)
        {
            var penSize = 2.0f * UIThreadExtensions.AutoScaleFactor.Width;
            renderer.LinePen = new Pen(Color.Firebrick, penSize);
            renderer.LinePen.DashStyle = DashStyle.Dot;
            renderer.UseTriangles = true;
            renderer.IsShowGlyphs = true;
        }

        private void SetupTree()
        {
            this.TlvOriginal.CanExpandGetter = x => ((TreeNode<OlFolderRemap>)x).ChildCount > 0;
            this.TlvOriginal.ChildrenGetter = x => ((TreeNode<OlFolderRemap>)x).Children;
            this.TlvOriginal.ParentGetter = x => ((TreeNode<OlFolderRemap>)x).Parent;
            this.OlvNameNotFiltered.ImageGetter = x => 0;
            this.TlvOriginal.Roots = _controller.RemapTree.Roots;

            this.OlvMap.SetObjects(_controller.Mappings2);
            this.OlvMap.AlwaysGroupByColumn = this.OlvMap.GetColumn(1);
            
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

        private void TlvOriginal_ModelDropped(object sender, ModelDropEventArgs e)
        {
            _controller.HandleModelDropped(sender, e);
        }

        private void TlvOriginal_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            _controller.HandleModelCanDrop(sender, e);
        }
    }
}
