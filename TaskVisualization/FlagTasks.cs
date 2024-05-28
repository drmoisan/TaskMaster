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
        private readonly AutoAssignPeople _autoAssign;
        private readonly Enums.FlagsToSet _flagsToSet;
        private readonly IApplicationGlobals _globals;
        private string _userEmailAddress;


        public FlagTasks(IApplicationGlobals AppGlobals, IList ItemList = null, bool blFile = true, IntPtr hWndCaller = default, string strNameOfFunctionCalling = "")
        {
            _globals = AppGlobals;
            _olExplorer = AppGlobals.Ol.App.ActiveExplorer();
            _todoSelection = InitializeToDoList(ItemList, _olExplorer);
            _flagsToSet = GetFlagsToSet(_todoSelection.Count);
            _viewer = new TaskViewer();
            // _defaultsToDo = New ToDoDefaults()
            _autoAssign = new AutoAssignPeople(AppGlobals);
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

        public static List<ToDoItem> InitializeToDoList(IList itemList, Explorer olExplorer)
        {
            itemList ??= GetSelection(olExplorer);
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
                    tmpToDo = new ToDoItem(objItem, OnDemand: true);
                }
                ToDoSelection.Add(tmpToDo);
            }
            return ToDoSelection;
        }

        public static void PopulateUdf(IList itemList, Explorer olExplorer) 
        { 
            var toDoSelection = InitializeToDoList(itemList, olExplorer);
            var flagsToSet = GetFlagsToSet(toDoSelection.Count);
            toDoSelection.ForEach(x => x.WriteFlagsBatch(flagsToSet));
        }

        /// <summary>
        /// Adds the Selection from the ActiveExplorer to a new Collection
        /// </summary>
        /// <returns>Collection of Outlook Items</returns>
        private static IList GetSelection(Explorer olExplorer)
        {
            var itemList = new List<object>();
            foreach (var obj in olExplorer.Selection)
                itemList.Add(obj);
            return itemList;
        }

        private static Enums.FlagsToSet GetFlagsToSet(int selectionCount)
        {
            if (selectionCount > 1)
            {

                Enums.FlagsToSet[] excludedMembers = new[] { Enums.FlagsToSet.all, Enums.FlagsToSet.none };
                var symbolsDict = Enum.GetValues(typeof(Enums.FlagsToSet)).Cast<Enums.FlagsToSet>().ToList().AsEnumerable().Where(x => excludedMembers.Contains(x) == false).Select(x => x).ToDictionary(x => Enum.GetName(typeof(Enums.FlagsToSet), x), x => x);






                var symbolSelectionDict = (from x in symbolsDict
                                           select x.Key).ToDictionary(x => x, x => false).ToSortedDictionary();

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
                if (listSelections.Count == 0)
                {
                    return Enums.FlagsToSet.all;
                }
                else
                {
                    Enums.FlagsToSet flag;
                    var flagsList = (from x in listSelections
                                     where Enum.TryParse(x, out flag)
                                     select Enum.Parse(typeof(Enums.FlagsToSet), x)).ToList().OfType<Enums.FlagsToSet>();
                    // Dim flagsList2 = flagsList.OfType(Of Enums.FlagsToSet)()
                    // Dim flagsList = (From x In symbolsDict Where listSelections.Contains(x.Key) Select x.Value).ToList()
                    // Dim selectedFlags As Enums.FlagsToSet = GenericBitwise(Of Enums.FlagsToSet).And(flagsList)
                    Enums.FlagsToSet selectedFlags = (Enums.FlagsToSet)Conversions.ToInteger(GenericBitwiseStatic<Enums.FlagsToSet>.Or(flagsList));
                    return selectedFlags;
                }
            }
            else
            {
                return Enums.FlagsToSet.all;
            }
        }

        
    }
}