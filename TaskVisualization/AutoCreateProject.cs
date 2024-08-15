using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace TaskVisualization
{
    public class AutoCreateProject(IApplicationGlobals globals) : IAutoAssign
    {
        private readonly IApplicationGlobals _globals = globals;
        //private readonly IList<IPrefix> _prefixes;

        public IList<string> FilterList => [.. _globals.TD.CategoryFilters];

        public IList<string> AddChoicesToDict(MailItem olMail, IList<IPrefix> prefixes, string prefixKey, string currentUserEmail)
        {            
            throw new NotImplementedException();
        }

        public Category AddColorCategory(IPrefix prefix, string projectName)
        {
            projectName = StripPrefix(prefix?.Value, projectName);
            
            if (!_globals.TD.ProjInfo.Contains_ProjectName(projectName))
            {
                if (!TryAutoExtractProgram(projectName, out var programName))
                {
                    programName = ChooseOrCreateProgramName();
                }
                if (programName.IsNullOrEmpty()) { return null; }
                var programID = _globals.TD.ProgramInfo[programName];
                var projectID = GetNextProjectID(programID);
                _globals.TD.ProjInfo.Add(new ProjectEntry(projectName, projectID, programName, programID));
                _globals.TD.ProjInfo.Serialize();
                var cat = CreateCategoryModule.CreateCategory(olNS: _globals.Ol.NamespaceMAPI, prefix: prefix, newCatName: projectName);
                CreateProjectTaskItem(projectName, projectID);
                return cat;
            }
            return null;
        }

        public string GetNextProjectID(string programID) 
        {
            programID.ThrowIfNullOrEmpty();
            var projects = _globals.TD.ProjInfo.Where(entry => entry.ProgramID == programID).OrderByDescending(entry => entry.ProjectID).FirstOrDefault();
            var seedId = projects?.ProjectID ?? $"{programID}00";
            return _globals.TD.IDList.GetNextToDoID(seedId);
        }
        
        internal string ChooseOrCreateProgramName() 
        {
            var chooser = new TagLauncher(_globals.TD.ProgramInfo.Keys, _globals);
            chooser.Viewer.ShowDialog();
            var selection = chooser.Controller.GetSelections().FirstOrDefault();
            if (selection.IsNullOrEmpty()) { return null; }
            else if (_globals.TD.ProgramInfo.TryGetValue(selection, out var programID))
            {
                return selection;
            }
            else
            {
                var seedID = _globals.TD.ProgramInfo.Values.OrderByDescending(x => x).FirstOrDefault();
                var newProgramID = _globals.TD.IDList.GetNextToDoID(seedID);
                _globals.TD.ProgramInfo[selection] = newProgramID;
                _globals.TD.ProgramInfo.Serialize();
                return selection;
            }
            
        }

        internal bool TryAutoExtractProgram(string projectName, out string programName) 
        {
            programName = null;
            //var programs = _globals.TD.ProjInfo.Select(entry => entry.ProgramName).OrderByDescending(x => x.Length).ToList();
            var programs = _globals.TD.ProgramInfo.Keys.OrderByDescending(x => x.Length).ToList();
            foreach (var program in programs)
            {
                if (projectName.Contains(program))
                {
                    programName = program;
                    return true;
                }
            }
            return false;
        }

        public void CreateProjectTaskItem(string projectName, string projectID) 
        {
            var taskItems = GetTaskItems();
            var taskItem = (TaskItem)taskItems.Add(OlItemType.olTaskItem);
            var todo = new ToDoItem(new OutlookItem(taskItem));
            todo.ToDoID = projectID;
            todo.TaskSubject = projectName;
            todo.Projects.AsStringNoPrefix = projectName;
            todo.Context.AsStringNoPrefix = "PROJECTS";            
        }

        internal Items GetTaskItems()
        {
            var olTasksFolder = _globals.Ol.App.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks);
            return olTasksFolder?.Items;
        }

        internal string StripPrefix(string prefix, string categoryName)
        {
            if (!prefix.IsNullOrEmpty() && !categoryName.IsNullOrEmpty())
            {
                return categoryName.Replace(prefix, "");
            }
            else
            {
                return categoryName;
            }
        }

        public IList<string> AutoFind(object objItem)
        {
            // TODO: Link this to the Bayesian project prediction model
            throw new NotImplementedException();
        }
    }

}
