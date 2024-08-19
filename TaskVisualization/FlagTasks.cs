using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

        public FlagTasks(IApplicationGlobals globals, IList itemList = null, bool blFile = true, IntPtr hWndCaller = default, string strNameOfFunctionCalling = "")
        {
            _globals = globals;
            _olExplorer = globals.Ol.App.ActiveExplorer();
            _todoSelection = InitializeToDoList(itemList, globals);
            _flagsToSet = GetFlagsToSet(_todoSelection.Count);
            _viewer = new TaskViewer();
            // _defaultsToDo = New ToDoDefaults()
            _autoAssignPeople = new AutoAssignPeople(globals);
            _autoCreateProject = new AutoCreateProject(globals);
            _controller = new TaskController(formInstance: _viewer,
                                             olCategories: globals.Ol.NamespaceMAPI.Categories,
                                             toDoSelection: _todoSelection,
                                             defaults: _defaultsToDo,
                                             autoAssign: _autoAssignPeople,
                                             projectAssign: _autoCreateProject,
                                             projectsToPrograms: globals.TD.ProjInfo.Programs_ByProjectNames,
                                             flagOptions: _flagsToSet,
                                             userEmailAddress: globals.Ol.UserEmailAddress);
            _userEmailAddress = globals.Ol.UserEmailAddress;
        }

        public DialogResult Run(bool modal = false)
        {
            _controller.Initialize();
            if (modal)
                return _viewer.ShowDialog();
            else
                _viewer.Show();
                return DialogResult.None;
        }

        public static List<ToDoItem> InitializeToDoList(IList itemList, IApplicationGlobals globals)
        {
            var olItems = (itemList?.Cast<object>() ?? GetSelection(globals.Ol.App.ActiveExplorer()))
                ?.Select(x => new OutlookItem(x)).ToArray();
            if (olItems is null) { return null;  }

            var todoList = Enumerable.Range(0, olItems.Count())
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

            //var todoList = (itemList?.Cast<object>() ?? GetSelection(globals.Ol.App.ActiveExplorer()))
            //    ?.Select(x => new OutlookItem(x))
            //    .Select(x =>
            //    {
            //        var todo = new ToDoItem(x);
            //        todo.ProjectsToPrograms = globals.TD.ProjInfo.Programs_ByProjectNames;
            //        todo.ProjectData = globals.TD.ProjInfo;
            //        return todo;
            //    })?
            //    .ToList();

            return todoList;
        }

        public static void PopulateUdf(IList itemList, IApplicationGlobals globals) 
        { 
            var toDoSelection = InitializeToDoList(itemList, globals);
            var flagsToSet = GetFlagsToSet(toDoSelection.Count);
            toDoSelection.ForEach(x => x.WriteFlagsBatch(flagsToSet));
        }
                
        /// <summary>
        /// Adds the Selection from the ActiveExplorer to a new List of object
        /// </summary>
        /// <returns>Collection of Outlook Items</returns>
        private static IList<object> GetSelection(Explorer olExplorer)
        {
            return olExplorer.Selection.Cast<object>().ToList();
        }

        /// <summary>
        /// Method asks the user which flags to set if selectionCount is greater than 1. Otherwise sets all flags.
        /// </summary>
        /// <param name="selectionCount">Count of outlook object items selected</param>
        /// <returns></returns>
        private static Enums.FlagsToSet GetFlagsToSet(int selectionCount)
        {
            // If more than one item selected, ask user which flags to set
            if (selectionCount > 1)
            {
                var symbolSelectionDict = GetSymbolsDictionary();
                var flagStrings = GetUserInputFlagsToAdjust(symbolSelectionDict);
                return ConvertFlagStringsToEnum(flagStrings);
            }
            // Else set them All
            else
            {
                return Enums.FlagsToSet.All;
            }
        }

        private static Enums.FlagsToSet ConvertFlagStringsToEnum(List<string> flagStrings)
        {
            if (flagStrings.Count == 0)
            {
                return Enums.FlagsToSet.All;
            }
            else
            {
                Enums.FlagsToSet flag;
                var flagsList = (from x in flagStrings
                                 where Enum.TryParse(x, out flag)
                                 select Enum.Parse(typeof(Enums.FlagsToSet), x)).ToList().OfType<Enums.FlagsToSet>();

                Enums.FlagsToSet selectedFlags = (Enums.FlagsToSet)Conversions.ToInteger(GenericBitwiseStatic<Enums.FlagsToSet>.Or(flagsList));
                return selectedFlags;
            }
        }

        private static SortedDictionary<string, bool> GetSymbolsDictionary()
        {
            Enums.FlagsToSet[] excludedMembers = new[] { Enums.FlagsToSet.All, Enums.FlagsToSet.None };
            var symbolsDict = Enum.GetValues(typeof(Enums.FlagsToSet)).Cast<Enums.FlagsToSet>().ToList().AsEnumerable().Where(x => excludedMembers.Contains(x) == false).Select(x => x).ToDictionary(x => Enum.GetName(typeof(Enums.FlagsToSet), x), x => x);

            var symbolSelectionDict = (from x in symbolsDict
                                       select x.Key).ToDictionary(x => x, x => false).ToSortedDictionary();
            return symbolSelectionDict;
        }

        private static List<string> GetUserInputFlagsToAdjust(SortedDictionary<string, bool> symbolSelectionDict)
        {
            var listSelections = new List<string>();

            using (var optionsViewer = new TagViewer())
            {
                var flagController = new TagController(
                    viewerInstance: optionsViewer,
                    dictOptions: symbolSelectionDict,
                    autoAssigner: null,
                    prefixes: ToDoDefaults.Instance.PrefixList,
                    userEmailAddress: "UnusedFieldDiscardText");

                optionsViewer.ShowDialog();
                if (flagController.ExitType != "Cancel")
                {
                    listSelections = flagController.GetSelections();
                }
            }

            return listSelections;
        }

        #region Obsolete Methods
        
        [Obsolete("Use the other InitializeToDoList method")]
        public static List<ToDoItem> InitializeToDoList2(IList itemList, Explorer olExplorer, Func<string, string> projectsToPrograms)
        {
            itemList ??= GetSelectionIList(olExplorer);
            var ToDoSelection = new List<ToDoItem>();
            foreach (var objItem in itemList)
            {
                ToDoItem tmpToDo;
                if (objItem is MailItem)
                {
                    MailItem olMail = (MailItem)objItem;
                    tmpToDo = new ToDoItem(olMail);
                }
                else if (objItem is TaskItem)
                {
                    TaskItem olTask = (TaskItem)objItem;
                    tmpToDo = new ToDoItem(olTask);
                }
                else
                {
                    tmpToDo = new ToDoItem(objItem, onDemand: true);
                }
                tmpToDo.ProjectsToPrograms = projectsToPrograms;
                ToDoSelection.Add(tmpToDo);
            }
            return ToDoSelection;
        }

        /// <summary>
        /// Adds the Selection from the ActiveExplorer to a new Collection
        /// </summary>
        /// <returns>Collection of Outlook Items</returns>
        [Obsolete("Use the other GetSelection method")]
        private static IList GetSelectionIList(Explorer olExplorer)
        {
            var itemList = new List<object>();
            foreach (var obj in olExplorer.Selection)
                itemList.Add(obj);
            return itemList;
        }


        #endregion Obsolete Methods

    }
}