using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic.CompilerServices;
using Tags;
using ToDoModel;
using UtilitiesCS;

[assembly: InternalsVisibleTo("TaskVisualization.Test")]

namespace TaskVisualization
{
    public class FlagTasks
    {
        private readonly List<ToDoItem> _todoSelection;
        private readonly Explorer _olExplorer;
        private TaskViewer _viewer;

        private readonly TaskController _controller;
        private readonly ToDoDefaults _defaultsToDo = new ToDoDefaults();
        private readonly AutoAssignPeople _autoAssignPeople;
        private readonly AutoCreateProject _autoCreateProject;
        private readonly Enums.FlagsToSet _flagsToSet;
        private readonly IApplicationGlobals _globals;
        private string _userEmailAddress;

        // Flag-selection dialog seam: production default is the Tags dialog; the
        // extracted FlagCalculations.GetFlagsToSet is directly stub-testable.
        private readonly Func<SortedDictionary<string, bool>, List<string>> _flagSelector =
            GetUserInputFlagsToAdjust;

        // Outlook-bound: reads ActiveExplorer/Selection, shows MessageBox, and
        // constructs the live TaskViewer/TaskController; not unit-testable without a
        // running Outlook process. Constructor shape preserved (invariant 1).
        [ExcludeFromCodeCoverage]
        public FlagTasks(
            IApplicationGlobals globals,
            IList itemList = null,
            bool blFile = true,
            IntPtr hWndCaller = default,
            string strNameOfFunctionCalling = ""
        )
        {
            _globals = globals;
            _olExplorer = globals.Ol.App.ActiveExplorer();
            _todoSelection = InitializeToDoList(itemList, globals);
            if (_todoSelection.Count == 0)
            {
                MessageBox.Show(
                    "No items selected. Exiting.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            _flagsToSet = FlagCalculations.GetFlagsToSet(_todoSelection.Count, _flagSelector);
            _viewer = new TaskViewer();
            // _defaultsToDo = New ToDoDefaults()
            _autoAssignPeople = new AutoAssignPeople(globals);
            _autoCreateProject = new AutoCreateProject(globals);
            var autoAssignContext = new AutoAssignContext(globals);
            _controller = new TaskController(
                formInstance: _viewer,
                olCategories: globals.Ol.NamespaceMAPI.Categories,
                toDoSelection: _todoSelection,
                defaults: _defaultsToDo,
                autoAssign: _autoAssignPeople,
                projectAssign: _autoCreateProject,
                contextAssign: autoAssignContext,
                projectsToPrograms: globals.TD.ProjInfo.Programs_ByProjectNames,
                flagOptions: _flagsToSet,
                userEmailAddress: globals.Ol.UserEmailAddress,
                globals: _globals
            );
            _userEmailAddress = globals.Ol.UserEmailAddress;
        }

        // Outlook-bound: shows the live TaskViewer form; not unit-testable without a
        // running Outlook process. Behavior preserved (invariant 1).
        [ExcludeFromCodeCoverage]
        public DialogResult Run(bool modal = false)
        {
            if (_controller is not null)
            {
                _controller.Initialize();
                if (modal)
                    return _viewer.ShowDialog();
                else
                    _viewer.Show();
                return DialogResult.None;
            }
            else
            {
                return DialogResult.None;
            }
        }

        // Outlook-bound: enumerates the live ActiveExplorer selection and shows a
        // MessageBox / creates a live task item; not unit-testable without Outlook.
        [ExcludeFromCodeCoverage]
        public static List<ToDoItem> InitializeToDoList(IList itemList, IApplicationGlobals globals)
        {
            var olItems = (
                itemList?.Cast<object>() ?? GetSelection(globals.Ol.App.ActiveExplorer())
            )
                ?.Select(x => new OutlookItem(x))
                .ToList();
            if (olItems.Count() == 0)
            {
                var response = MessageBox.Show(
                    "No items selected. Would you like to create a new task?",
                    "Question",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (response == DialogResult.Yes)
                {
                    var taskItems = globals
                        .Ol.App.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks)
                        .Items;
                    olItems.Add(new OutlookItem(taskItems.Add(OlItemType.olTaskItem)));
                }
            }

            var todoList = Enumerable
                .Range(0, olItems.Count())
                .Select(i =>
                {
                    var todo = new ToDoItem(olItems[i]);
                    todo.Identifier = $"Original list index: {i}";
                    todo.ProjectsToPrograms = globals.TD.ProjInfo.Programs_ByProjectNames;
                    todo.ProjectData = globals.TD.ProjInfo;
                    todo.IdList = globals.TD.IDList;
                    return todo;
                })
                ?.ToList();

            return todoList;
        }

        // Outlook-bound: builds the ToDo selection from a live Outlook selection and
        // writes user-defined fields; not unit-testable without a running Outlook.
        [ExcludeFromCodeCoverage]
        public static void PopulateUdf(IList itemList, IApplicationGlobals globals)
        {
            var toDoSelection = InitializeToDoList(itemList, globals);
            var flagsToSet = FlagCalculations.GetFlagsToSet(
                toDoSelection.Count,
                GetUserInputFlagsToAdjust
            );
            toDoSelection.ForEach(x => x.WriteFlagsBatch(flagsToSet));
        }

        /// <summary>
        /// Adds the Selection from the ActiveExplorer to a new List of object
        /// </summary>
        /// <returns>Collection of Outlook Items</returns>
        // Outlook-bound: enumerates the live Explorer.Selection; not unit-testable.
        [ExcludeFromCodeCoverage]
        private static IList<object> GetSelection(Explorer olExplorer)
        {
            return olExplorer.Selection.Cast<object>().ToList();
        }

        /// <summary>
        /// Method asks the user which flags to set if selectionCount is greater than
        /// 1. Otherwise sets all flags. Production default supplied to
        /// <see cref="FlagCalculations.GetFlagsToSet"/>.
        /// </summary>
        // Outlook/UI-bound: constructs and shows the Tags flag-selection dialog; not
        // unit-testable without a live form.
        [ExcludeFromCodeCoverage]
        private static List<string> GetUserInputFlagsToAdjust(
            SortedDictionary<string, bool> symbolSelectionDict
        )
        {
            var listSelections = new List<string>();

            using (var optionsViewer = new TagViewer())
            {
                var flagController = new TagController(
                    viewerInstance: optionsViewer,
                    dictOptions: symbolSelectionDict,
                    autoAssigner: null,
                    prefixes: ToDoDefaults.Instance.PrefixList,
                    userEmailAddress: "UnusedFieldDiscardText"
                );

                optionsViewer.ShowDialog();
                if (flagController.ExitType != "Cancel")
                {
                    listSelections = flagController.GetSelections();
                }
            }

            return listSelections;
        }
    }
}
