using System;
using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesVB;

namespace ToDoModel
{

    public partial class ProjectInfoWindow
    {
        public ProjectInfo pi;
        private readonly Resizer rs = new Resizer();
        private bool blEditingCell = false;

        public ProjectInfoWindow(ProjectInfo ProjInfo)
        {


            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            pi = ProjInfo;

        }

        private void ProjectInfoWindow_Load(object sender, EventArgs e)
        {
            olvProjInfo.SetObjects(pi);

            rs.FindAllControls(this);
            rs.SetResizeDimensions(SplitContainer1.Panel2, Resizer.ResizeDimensions.Position, true);
            rs.SetResizeDimensions(SplitContainer1, Resizer.ResizeDimensions.None, true);
            rs.SetResizeDimensions(SplitContainer1.Panel1, Resizer.ResizeDimensions.Position | Resizer.ResizeDimensions.Size, true);
            rs.PrintDict();
        }

        private void BTN_OK_Click(object sender, EventArgs e)
        {
            pi.Save();
            Close();
        }

        private void BTN_CANCEL_Click(object sender, EventArgs e)
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
                        foreach (ToDoProjectInfoEntry entry in selection)
                            pi.Remove(entry);
                        pi.Save();
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