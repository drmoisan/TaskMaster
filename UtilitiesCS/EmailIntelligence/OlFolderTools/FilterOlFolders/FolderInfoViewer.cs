using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders
{
    public partial class FolderInfoViewer : Form
    {
        public FolderInfoViewer()
        {
            InitializeComponent();
        }

        internal OlFolderTree FolderTree { get; set; }

        public void SetFolderTree(OlFolderTree folderTree)
        {
            FolderTree = folderTree;
            Tlv.CanExpandGetter = x => ((TreeNode<OlFolderWrapper>)x).Children.Count > 0;
            Tlv.ChildrenGetter = x => ((TreeNode<OlFolderWrapper>)x).Children;
            Tlv.ParentGetter = x => ((TreeNode<OlFolderWrapper>)x).Parent;
            Tlv.Roots = FolderTree.Roots;
            
        }
    }
}
