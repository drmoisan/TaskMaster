using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ToDoModel.Data_Model.Project;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

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

        public ScDictionary<string, string> ProgramData { get => Controller?.ProgramData; }

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

        private ComboBox GetCombo(object sender, CellEditEventArgs e, string[] options)
        {
            ComboBox cb = new ComboBox();
            cb.Bounds = e.CellBounds;
            cb.Font = ((ObjectListView)sender).Font;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.Items.AddRange(options);
            var currentValue = e.Value as string;
            if (currentValue.IsNullOrEmpty())
            {
                currentValue = options.FirstOrDefault();
            }
            var index = Math.Max(cb.Items.IndexOf(currentValue), 0);
            cb.SelectedIndex = index;
            return cb;
        }

        private void OlvProjInfo_CellEditStarting(object sender, CellEditEventArgs e)
        {
            _isEditing = true;
            if (e.Column == this.OlvProgramID && !ProgramData.IsNullOrEmpty())
            { 
                // Grab handle on the row and cast to IProjectData
                IProjectEntry projectEntry = (IProjectEntry)e.RowObject;
                
                var cb = GetCombo(sender, e, ProgramData.Values.OrderBy(x=>x).ToArray());
                
                cb.SelectedIndexChanged += (sender, args) =>
                {
                    var id = cb.SelectedItem as string;
                    
                    var kvp = id.IsNullOrEmpty() ? default : ProgramData.FirstOrDefault(x => x.Value == id);
                    if (!kvp.Key.IsNullOrEmpty() && !kvp.Value.IsNullOrEmpty())
                    {
                        projectEntry.ProgramID = kvp.Value;
                        projectEntry.ProgramName = kvp.Key;
                    }                    
                    e.Cancel = true;
                };
                e.Control = cb;
            }
            else if (e.Column == this.OlvProgramName && !ProgramData.IsNullOrEmpty())
            {
                // Grab handle on the row and cast to IProjectData
                IProjectEntry projectEntry = (IProjectEntry)e.RowObject;

                var cb = GetCombo(sender, e, ProgramData.Keys.OrderBy(x=>x).ToArray());

                cb.SelectedIndexChanged += (sender, args) =>
                {
                    var name = cb.SelectedItem as string;

                    if (!name.IsNullOrEmpty() && ProgramData.TryGetValue(name, out var id))                     
                    {
                        projectEntry.ProgramID = id;
                        projectEntry.ProgramName = name;
                    }
                    e.Cancel = true;
                };
                e.Control = cb;
            }


            // Create the ComboBox to get the selection
            //ComboBox cb = new ComboBox();
            //cb.Bounds = e.CellBounds;
            //cb.Font = ((ObjectListView)sender).Font;
            //cb.DropDownStyle = ComboBoxStyle.DropDownList;
            //var keys = ProgramData.Keys.ToArray();
            //cb.Items.AddRange(keys);
            //var currentValue = e.Value as string;
            //var index = Math.Max(cb.Items.IndexOf(currentValue), 0);
            //cb.SelectedIndex = index;

        }

        private void OlvProjInfo_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            _isEditing = false;
            if ((e.Column == this.OlvProgramID || e.Column == this.OlvProgramName) && !ProgramData.IsNullOrEmpty()) 
            { 
                // Any updating will have been down in the SelectedIndexChanged event handler
                // Here we simply make the list redraw the involved ListViewItem
                ((ObjectListView)sender).RefreshItem(e.ListViewItem);

                // We have updated the model object, so we cancel the auto update
                e.Cancel = true;
            }            
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
