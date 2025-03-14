using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel.Data_Model.Project;
using UtilitiesCS;

namespace ToDoModel
{
    public partial class ProjectViewer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ProjectViewer()
        {
            // This call is required by the designer.
            InitializeComponent();
        }

        private bool _isEditing = false;

        private ProjectController _controller;
        public ProjectController Controller { get => _controller; set => _controller = value; }

        protected readonly ControlResizer _resizer = new ControlResizer();
        internal ControlResizer Resizer { get => _resizer; }

        private void ButtonOk_Click(object sender, EventArgs e) => Controller.SaveAndClose();
        
        private void ButtonCancel_Click(object sender, EventArgs e) => Controller.Cancel();
        
        private void ProjectInfoWindow_Resize(object sender, EventArgs e)
        {
            _resizer.ResizeAllControls(this);
            OlvProjectData.AutoScaleColumnsToContainer();
        }

        private void OlvProjInfo_KeyUp(object sender, KeyEventArgs e)
        {
            if (_isEditing == false)
            {
                if (e.KeyData == Keys.Delete)
                {
                    Controller.DeleteSelection();
                }
            }
        }

        private void OlvProjInfo_CellEditStarting(object sender, CellEditEventArgs e)
        {
            _isEditing = true;
        }

        private void OlvProjInfo_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            _isEditing = false;
        }

        private void OlvProjectData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedItems();
            }
        }

        private void CopySelectedItems()
        {
            if (OlvProjectData.SelectedObjects.Count > 0)
            {
                var selectedItems = OlvProjectData.SelectedObjects;
                
                try
                {
                    var clipboardText = string
                        .Join("\n",selectedItems?
                        .CastNullSafe<IProjectEntry>()
                        .Where(x => x is not null)
                        .Select(x => x.ToCSV()));
                    Clipboard.SetDataObject(selectedItems, true);
                }
                catch (Exception e)
                {
                    logger.Error($"Copy to clipboard failed. {e.Message}", e);
                }
                
            }
        }
    }
}
