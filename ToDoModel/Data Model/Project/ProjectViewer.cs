using System;
using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesCS;

namespace ToDoModel
{

    public partial class ProjectViewer
    {
        public IProjectData _projectData;
        private readonly Resizer rs = new Resizer();
        private bool blEditingCell = false;

        public ProjectViewer()
        {
            // This call is required by the designer.
            InitializeComponent();
        }


        public ProjectViewer(IProjectData projData)
        {
            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            _projectData = projData;

        }

        public ProjectViewer(IProjectData ProjInfo, Action<string, string> action)
        {
            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            _projectData = ProjInfo;

        }

        private void ProjectInfoWindow_Load(object sender, EventArgs e)
        {
            olvProjInfo.SetObjects(_projectData);

            rs.FindAllControls(this);
            rs.SetResizeDimensions(SplitContainer1.Panel2, Resizer.ResizeDimensions.Position, true);
            rs.SetResizeDimensions(SplitContainer1, Resizer.ResizeDimensions.None, true);
            rs.SetResizeDimensions(SplitContainer1.Panel1, Resizer.ResizeDimensions.Position | Resizer.ResizeDimensions.Size, true);
            rs.PrintDict();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            _projectData.Save();
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ProjectInfoWindow_Resize(object sender, EventArgs e)
        {
            rs.ResizeAllControls(this);
            // TreeListView1.AutoResizeColumns()
            olvProjInfo.AutoScaleColumnsToContainer();
        }

        private void olvProjInfo_KeyUp(object sender, KeyEventArgs e)
        {

            if (blEditingCell == false)
            {
                if (e.KeyData == Keys.Delete)
                {
                    ArrayList selection = (ArrayList)olvProjInfo.SelectedObjects;
                    if (selection is not null)
                    {
                        foreach (ProjectEntry entry in selection)
                            _projectData.Remove(entry);
                        _projectData.Save();
                        olvProjInfo.RemoveObjects(olvProjInfo.SelectedObjects);
                    }
                }
            }
        }

        private void olvProjInfo_CellEditStarting(object sender, CellEditEventArgs e)
        {
            blEditingCell = true;
        }

        private void olvProjInfo_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            blEditingCell = false;
        }
    }
}
