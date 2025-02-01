using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.Properties;

namespace UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders
{
    public partial class FolderInfoViewer : Form
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FolderInfoViewer()
        {
            InitializeComponent();
        }

        internal OlFolderTree FolderTree { get; set; }

        public void SetFolderTree(OlFolderTree folderTree)
        {
            //var settings = new JsonSerializerSettings()
            //{
            //    TypeNameHandling = TypeNameHandling.Auto,
            //    Formatting = Formatting.Indented,
            //    PreserveReferencesHandling = PreserveReferencesHandling.All
            //};            
            //var tree = JsonConvert.SerializeObject(folderTree, settings);
            //logger.Debug($"SetFolderTree: \n{tree}");
            FolderTree = folderTree;
            Tlv.CanExpandGetter = x => ((TreeNode<OlFolderWrapper>)x).Children.Count > 0;
            Tlv.ChildrenGetter = x => ((TreeNode<OlFolderWrapper>)x).Children;
            Tlv.ParentGetter = x => ((TreeNode<OlFolderWrapper>)x).Parent;
            Tlv.Roots = FolderTree.Roots;
        }

    }
}
