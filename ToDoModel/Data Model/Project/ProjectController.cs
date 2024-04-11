using System;
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
            ProjectData = projectData;
        }

        protected IProjectData _projectData;
        internal IProjectData ProjectData { get => _projectData; set => _projectData = value; }
    }
}
