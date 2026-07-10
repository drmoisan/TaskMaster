using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;
using UtilitiesCS.Extensions;

namespace TaskVisualization
{
    public class AutoCreateProject : IAutoAssign
    {
        private readonly IApplicationGlobals _globals;

        // Interop/dialog seams. Production defaults preserve the original live-host
        // behavior; tests inject stubs so host-neutral branches are measured without
        // a live Outlook process or popup.
        private readonly Func<IEnumerable<string>, string> _chooseProgram;
        private readonly Func<IPrefix, string, Category> _createCategory;
        private readonly Func<Items> _getTaskItems;

        /// <summary>
        /// Creates the project auto-assigner. The single-argument form remains valid
        /// for existing callers; optional seams default to the live-host
        /// implementations.
        /// </summary>
        public AutoCreateProject(
            IApplicationGlobals globals,
            Func<IEnumerable<string>, string> chooseProgram = null,
            Func<IPrefix, string, Category> createCategory = null,
            Func<Items> getTaskItems = null
        )
        {
            _globals = globals;
            _chooseProgram = chooseProgram ?? DefaultChooseProgram;
            _createCategory = createCategory ?? DefaultCreateCategory;
            _getTaskItems = getTaskItems ?? GetTaskItems;
        }

        public IList<string> FilterList => [.. _globals.TD.CategoryFilters];

        public IList<string> AddChoicesToDict(
            MailItem olMail,
            IList<IPrefix> prefixes,
            string prefixKey,
            string currentUserEmail
        )
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
                if (programName.IsNullOrEmpty())
                {
                    return null;
                }
                var programID = _globals.TD.ProgramInfo[programName];
                var projectID = GetNextProjectID(programID);
                _globals.TD.ProjInfo.Add(
                    new ProjectEntry(projectName, projectID, programName, programID)
                );
                _globals.TD.ProjInfo.Serialize();
                var cat = _createCategory(prefix, projectName);
                CreateProjectTaskItem(projectName, projectID);
                return cat;
            }
            return null;
        }

        // MAPI-bound default: creates a live Outlook category. Not unit-testable
        // without a running Outlook process.
        [ExcludeFromCodeCoverage]
        private Category DefaultCreateCategory(IPrefix prefix, string projectName)
        {
            return CreateCategoryModule.CreateCategory(
                olNS: _globals.Ol.NamespaceMAPI,
                prefix: prefix,
                newCatName: projectName
            );
        }

        public string GetNextProjectID(string programID)
        {
            programID.ThrowIfNullOrEmpty();
            var projects = _globals
                .TD.ProjInfo.Where(entry => entry.ProgramID == programID)
                .OrderByDescending(entry => entry.ProjectID)
                .FirstOrDefault();
            var seedId = projects?.ProjectID ?? $"{programID}00";
            return _globals.TD.IDList.GetNextToDoID(seedId);
        }

        internal string ChooseOrCreateProgramName()
        {
            var selection = _chooseProgram(_globals.TD.ProgramInfo.Keys);
            if (selection.IsNullOrEmpty())
            {
                return null;
            }
            else if (_globals.TD.ProgramInfo.TryGetValue(selection, out var programID))
            {
                return selection;
            }
            else
            {
                var seedID =
                    _globals.TD.ProgramInfo.Values.OrderByDescending(x => x).FirstOrDefault()
                    ?? "00";
                var newProgramID = _globals.TD.IDList.GetNextToDoID(seedID);
                _globals.TD.ProgramInfo[selection] = newProgramID;
                _globals.TD.ProgramInfo.Serialize();
                return selection;
            }
        }

        // UI-bound default: shows the TagLauncher program-selection dialog. Not
        // unit-testable without a live form.
        [ExcludeFromCodeCoverage]
        private string DefaultChooseProgram(IEnumerable<string> programKeys)
        {
            string userEmail = _globals
                .Ol.StoresWrapper.Stores.FirstOrDefault(x => !x.UserEmailAddress.IsNullOrEmpty())
                ?.UserEmailAddress;
            var chooser = new TagLauncher(programKeys, null, userEmail);

            chooser.Viewer.Text = "Select or Create Program";
            chooser.Viewer.ShowDialog();
            return chooser.Controller.GetSelections().FirstOrDefault();
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

        // Outlook-bound: creates a live Outlook task item. Not unit-testable without
        // a running Outlook process.
        [ExcludeFromCodeCoverage]
        public void CreateProjectTaskItem(string projectName, string projectID)
        {
            var taskItems = _getTaskItems();
            var taskItem = (TaskItem)taskItems.Add(OlItemType.olTaskItem);
            var todo = new ToDoItem(new OutlookItem(taskItem));
            todo.IdAutoCoding = false;
            todo.ToDoID = projectID;
            todo.TaskSubject = projectName;
            todo.Projects.AsStringNoPrefix = projectName;
            todo.Context.AsStringNoPrefix = "PROJECTS";
        }

        // Outlook-bound: resolves the live default Tasks folder. Not unit-testable
        // without a running Outlook process. Also the default for the _getTaskItems seam.
        [ExcludeFromCodeCoverage]
        internal Items GetTaskItems()
        {
            var olTasksFolder = _globals.Ol.App.Session.GetDefaultFolder(
                OlDefaultFolders.olFolderTasks
            );
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

        // Outlook/classifier-bound: builds a MailItemHelper from a live MailItem and
        // runs the classifier engine. Not unit-testable without a running Outlook.
        [ExcludeFromCodeCoverage]
        public async Task<IList<string>> AutoFindAsync(object objItem)
        {
            var helper = await ToHelper(objItem);
            if (helper is null)
            {
                return [];
            }

            var project = await CategoryClassifierGroup
                .CreateEngineAsync(_globals, "Project", default)
                .ConfigureAwait(true);
            project.ProbabilityThreshold = 0.2;

            var results = (await project.GetMatchingCategoriesAsync(helper)).ToList();
            return results;
        }

        // Outlook-bound: constructs a MailItemHelper from a live MailItem. Not
        // unit-testable without a running Outlook process.
        [ExcludeFromCodeCoverage]
        private async Task<MailItemHelper> ToHelper(object objItem)
        {
            MailItemHelper helper = null;
            if (objItem is MailItemHelper mailItemHelper)
            {
                helper = mailItemHelper;
            }
            else if (objItem is OutlookItem olItem)
            {
                if (olItem.InnerObject is MailItem mailItem)
                {
                    helper = await MailItemHelper
                        .FromMailItemAsync(mailItem, _globals, default, false)
                        .ConfigureAwait(true);
                }
            }
            else if (objItem is MailItem mailItem)
            {
                helper = await MailItemHelper
                    .FromMailItemAsync(mailItem, _globals, default, false)
                    .ConfigureAwait(true);
            }
            if (helper is null)
            {
                return null;
            }
            else
            {
                await Task.Run(() => _ = helper.Tokens).ConfigureAwait(true);
                return helper;
            }
        }
    }
}
