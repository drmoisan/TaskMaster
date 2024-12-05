using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel.Data_Model.Project
{
    public class ProjectController
    {
        public ProjectController(IProjectData projectData)
        {
            DataModel = projectData;
            Viewer = new() { Controller = this };
        }

        public void Run()
        {
            Viewer.OlvProjectData.SetObjects(DataModel);
            InitResizer();
            Viewer.ShowDialog();
        }

        internal void InitResizer()
        {
            Viewer.Resizer.FindAllControls(_viewer);
            Viewer.Resizer.SetResizeDimensions(_viewer.SplitContainer1.Panel2, ControlResizer.ResizeDimensions.Position, true);
            Viewer.Resizer.SetResizeDimensions(_viewer.SplitContainer1, ControlResizer.ResizeDimensions.None, true);
            Viewer.Resizer.SetResizeDimensions(_viewer.SplitContainer1.Panel1, ControlResizer.ResizeDimensions.Position | ControlResizer.ResizeDimensions.Size, true);
            Viewer.Resizer.PrintDict();
        }

        #region Properties

        protected IProjectData _dataModel;
        internal IProjectData DataModel { get => _dataModel; set => _dataModel = value; }

        protected ProjectViewer _viewer;
        internal ProjectViewer Viewer { get => _viewer; set => _viewer = value; }
        
        #endregion Properties

        #region Actions and Events

        public void SaveAndClose() 
        { 
            DataModel.Save();
            Viewer.Close();
            Cleanup();
        }

        public void Cancel()
        {
            Viewer.Close();
            Cleanup();
        }

        public void Cleanup() 
        { 
            DataModel = null;
            Viewer = null;
        }
        
        public void DeleteSelection()
        {
            ArrayList selection = (ArrayList)Viewer.OlvProjectData.SelectedObjects;
            if (selection is not null)
            {
                foreach (ProjectEntry entry in selection)
                    DataModel.Remove(entry);
                DataModel.Save();
                Viewer.OlvProjectData.RemoveObjects(Viewer.OlvProjectData.SelectedObjects);
            }
        }

        #endregion Actions and Events
    }
}
