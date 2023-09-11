using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly AutoAssign _autoAssign;
        private readonly TaskController.FlagsToSet _flagsToSet;
        private readonly IApplicationGlobals _globals;
        private string _userEmailAddress;


        public FlagTasks(IApplicationGlobals AppGlobals, IList ItemList = null, bool blFile = true, IntPtr hWndCaller = default, string strNameOfFunctionCalling = "")
        {
            _globals = AppGlobals;
            _olExplorer = AppGlobals.Ol.App.ActiveExplorer();
            _todoSelection = InitializeToDoList(ItemList);
            _flagsToSet = GetFlagsToSet(_todoSelection.Count);
            _viewer = new TaskViewer();
            // _defaultsToDo = New ToDoDefaults()
            _autoAssign = new AutoAssign(AppGlobals);
            _controller = new TaskController(formInstance: _viewer,
                                             olCategories: AppGlobals.Ol.NamespaceMAPI.Categories,
                                             toDoSelection: _todoSelection,
                                             defaults: _defaultsToDo,
                                             autoAssign: _autoAssign,
                                             flagOptions: _flagsToSet,
                                             userEmailAddress: AppGlobals.Ol.UserEmailAddress);
            _userEmailAddress = AppGlobals.Ol.UserEmailAddress;
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

        private List<ToDoItem> InitializeToDoList(IList ItemList)
        {
            if (ItemList is null)
                ItemList = GetSelection();
            var ToDoSelection = new List<ToDoItem>();
            foreach (var ObjItem in ItemList)
            {
                ToDoItem tmpToDo;
                if (ObjItem is MailItem)
                {
                    MailItem OlMail = (MailItem)ObjItem;
                    tmpToDo = new ToDoItem(OlMail);
                }
                else if (ObjItem is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)ObjItem;
                    tmpToDo = new ToDoItem(OlTask);
                }
                else
                {
                    tmpToDo = new ToDoItem(ObjItem, OnDemand: true);
                }
                ToDoSelection.Add(tmpToDo);
            }
            return ToDoSelection;
        }

        /// <summary>
    /// Adds the Selection from the ActiveExplorer to a new Collection
    /// </summary>
    /// <returns>Collection of Outlook Items</returns>
        private IList GetSelection()
        {
            var ItemList = new List<object>();
            foreach (var obj in _olExplorer.Selection)
                ItemList.Add(obj);
            return ItemList;
        }

        private TaskController.FlagsToSet GetFlagsToSet(int selectionCount)
        {
            if (selectionCount > 1)
            {

                TaskController.FlagsToSet[] excludedMembers = new[] { TaskController.FlagsToSet.all, TaskController.FlagsToSet.none };
                var symbolsDict = Enum.GetValues(typeof(TaskController.FlagsToSet)).Cast<TaskController.FlagsToSet>().ToList().AsEnumerable().Where(x => excludedMembers.Contains(x) == false).Select(x => x).ToDictionary(x => Enum.GetName(typeof(TaskController.FlagsToSet), x), x => x);






                var symbolSelectionDict = (from x in symbolsDict
                                           select x.Key).ToDictionary(x => x, x => false).ToSortedDictionary();

                var listSelections = new List<string>();

                using (var optionsViewer = new TagViewer())
                {
                    var flagController = new TagController(viewerInstance: optionsViewer, dictOptions: symbolSelectionDict, autoAssigner: null, prefixes: _defaultsToDo.PrefixList, userEmailAddress: _userEmailAddress);
                    optionsViewer.ShowDialog();
                    if (flagController.ExitType != "Cancel")
                    {
                        listSelections = flagController.GetSelections();
                    }
                }
                if (listSelections.Count == 0)
                {
                    return TaskController.FlagsToSet.all;
                }
                else
                {
                    TaskController.FlagsToSet flag;
                    var flagsList = (from x in listSelections
                                     where Enum.TryParse(x, out flag)
                                     select Enum.Parse(typeof(TaskController.FlagsToSet), x)).ToList().OfType<TaskController.FlagsToSet>();
                    // Dim flagsList2 = flagsList.OfType(Of TaskController.FlagsToSet)()
                    // Dim flagsList = (From x In symbolsDict Where listSelections.Contains(x.Key) Select x.Value).ToList()
                    // Dim selectedFlags As TaskController.FlagsToSet = GenericBitwise(Of TaskController.FlagsToSet).And(flagsList)
                    TaskController.FlagsToSet selectedFlags = (TaskController.FlagsToSet)Conversions.ToInteger(GenericBitwise<TaskController.FlagsToSet>.Or(flagsList));
                    return selectedFlags;
                }
            }
            else
            {
                return TaskController.FlagsToSet.all;
            }
        }

        private class AutoAssign : IAutoAssign
        {

            private readonly IApplicationGlobals _globals;

            public AutoAssign(IApplicationGlobals globals)
            {
                _globals = globals;
            }

            public IList<string> FilterList
            {
                get => _globals.TD.CategoryFilters.ToList();
            }

            public IList<string> AutoFind(object objItem)
            {
                return AutoFile.AutoFindPeople(objItem: objItem,
                                               ppl_dict: _globals.TD.DictPPL,
                                               emailRootFolder: _globals.Ol.EmailRootPath,
                                               dictRemap: _globals.TD.DictRemap,
                                               userAddress: _globals.Ol.UserEmailAddress,
                                               blExcludeFlagged: false);

            }

            public IList<string> AddChoicesToDict(MailItem olMail, IList<IPrefix> prefixes, string prefixKey, string currentUserEmail)
            {
                return _globals.TD.DictPPL.AddMissingEntries(olMail);
                //return AutoFile.dictPPL_AddMissingEntries(olMail: olMail,
                //                                          ppl_dict: _globals.TD.DictPPL,
                //                                          dictRemap: _globals.TD.DictRemap,
                //                                          prefixes: prefixes,
                //                                          prefixKey: prefixKey,
                //                                          emailRootFolder: _globals.Ol.EmailRootPath,
                //                                          stagingPath: _globals.FS.FldrStaging,
                //                                          currentUserEmail: currentUserEmail);

            }

            public Category AddColorCategory(IPrefix prefix, string categoryName)
            {
                return CreateCategoryModule.CreateCategory(olNS: _globals.Ol.NamespaceMAPI, prefix: prefix, newCatName: categoryName);
            }
        }

    }
}