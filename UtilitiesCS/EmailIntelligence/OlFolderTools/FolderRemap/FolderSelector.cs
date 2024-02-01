using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    public partial class FolderSelector : Form
    {
        public FolderSelector()
        {
            InitializeComponent();
        }

        public static OlFolderRemap SelectFolder(IList<TreeNode<OlFolderRemap>> roots)
        {
            var selector = new FolderSelector();
            selector.Initialize(roots);
            selector.ShowDialog();
            var result = selector.Selection;
            selector.Dispose();
            return result;
        }

        internal void Initialize(IList<TreeNode<OlFolderRemap>> roots)
        {
            this.TlvOriginal.CanExpandGetter = x => ((TreeNode<OlFolderRemap>)x).ChildCount > 0;
            this.TlvOriginal.ChildrenGetter = x => ((TreeNode<OlFolderRemap>)x).Children;
            this.TlvOriginal.ParentGetter = x => ((TreeNode<OlFolderRemap>)x).Parent;
            this.OlvNameNotFiltered.ImageGetter = x => 0;
            this.TlvOriginal.Roots = roots;
            this.TlvOriginal.CheckStatePutter = delegate (object rowObject, CheckState newValue)
            {
                _selection = ((TreeNode<OlFolderRemap>)rowObject).Value;
                this.Hide(); 
                return CheckState.Checked;
            };

            this.TlvOriginal.CheckStateGetter = delegate (object rowObject) { return CheckState.Unchecked; };
        }

        internal OlFolderRemap Selection => _selection;
        private OlFolderRemap _selection = null;
    }
}
