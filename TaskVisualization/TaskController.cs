using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Tags;
using ToDoModel;
using UtilitiesCS;


namespace TaskVisualization
{


    public class TaskController
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public const int WM_LBUTTONDOWN = 0x201;

        private TaskViewer __viewer;

        private TaskViewer _viewer
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __viewer;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                __viewer = value;
            }
        }
        private readonly List<ToDoItem> _todo_list;
        private readonly ToDoItem _active;

        private readonly SortedDictionary<string, bool> _dict_categories;
        private string _exit_type = "Cancel";
        private readonly Dictionary<Label, string> _xlCtrlCaptions;
        private readonly Dictionary<Label, Control> _xlCtrlLookup;
        private readonly Dictionary<Label, bool> _xlCtrlOptions;
        private Dictionary<Label, char> _xlCtrlsActive;
        private Dictionary<Label, char> _xlCtrlsNav;
        private int _activeNavGroup = -1;
        private bool _altActive = false;
        private int _altLevel = 0;
        private readonly string _keyCapture = "";
        private readonly ToDoDefaults _defaults;
        private readonly IAutoAssign _autoAssign;
        private string _userEmailAddress;


        [Flags]
        public enum FlagsToSet
        {
            none = 0,
            context = 1,
            people = 2,
            projects = 4,
            topics = 8,
            priority = 16,
            taskname = 32,
            worktime = 64,
            today = 128,
            bullpin = 256,
            kbf = 512,
            duedate = 1024,
            reminder = 2048,
            all = 4095
        }

        internal enum ForceState
        {
            none = 0,
            force_on = 1,
            force_off = 2
        }

        #region Constructors and Initializers

        /// <summary>
        /// Constructor initializes the controller for the TaskViewer
        /// </summary>
        /// <param name="formInstance">Instance of TaskViewer</param>
        /// <param name="toDoSelection">List of ToDoItems</param>
        /// <param name="flagOptions">Enumeration of fields to activate</param>
        public TaskController(TaskViewer formInstance, Categories olCategories, List<ToDoItem> toDoSelection, ToDoDefaults defaults, IAutoAssign autoAssign, string userEmailAddress, FlagsToSet flagOptions = FlagsToSet.all)
        {

            // Save parameters to internal variables
            _viewer = formInstance;
            _todo_list = toDoSelection;
            _options = flagOptions;
            _defaults = defaults;
            _autoAssign = autoAssign;
            _userEmailAddress = userEmailAddress;

            // Activate this controller within the viewer
            formInstance.SetController(this);
            formInstance.AcceptButton = formInstance.OKButton;
            formInstance.CancelButton = formInstance.Cancel_Button;


            // First ToDoItem in list is cloned to _active and set to readonly
            _active = (ToDoItem)_todo_list[0].Clone();
            _active.IsReadOnly = true;

            // All color categories in Outlook.Namespace are loaded to a sorted dictionary
            _dict_categories = new SortedDictionary<string, bool>();
            foreach (Category cat in olCategories)
                _dict_categories.Add(cat.Name, false);

            _xlCtrlLookup = GetControlLookup();
            _xlCtrlOptions = GetOptionsLookup();
            _xlCtrlCaptions = GetCaptionLookup();
            _xlCtrlsNav = (from controlCaption in GetCaptionLookup(0)
                           where GetOptionsLookup(0)[controlCaption.Key]
                           select controlCaption)
                                .ToDictionary(
                                    controlCaption => controlCaption.Key,
                                    controlCaption => controlCaption.Value[0]);

        }

        /// <summary>
        /// Function prepares task viewer by activating desired controls and loading values to them
        /// </summary>
        public void Initialize()
        {
            _viewer.TaskName.Text = _active.TaskSubject;
            if (!_active.Context.IsNullOrEmpty())
                _viewer.CategorySelection.Text = _active.Context;
            if (!_active.People.IsNullOrEmpty())
                _viewer.PeopleSelection.Text = _active.People;
            if (!_active.Project.IsNullOrEmpty())
                _viewer.ProjectSelection.Text = _active.Project;
            if (!_active.Topic.IsNullOrEmpty())
                _viewer.TopicSelection.Text = _active.Topic;

            switch (_active.Priority)
            {
                case OlImportance.olImportanceHigh:
                    {
                        _viewer.PriorityBox.SelectedItem = "High";
                        break;
                    }
                case OlImportance.olImportanceLow:
                    {
                        _viewer.PriorityBox.SelectedItem = "Low";
                        break;
                    }
                case OlImportance.olImportanceNormal:
                    {
                        _viewer.PriorityBox.SelectedItem = "Normal";
                        break;
                    }
            }

            _viewer.KbSelector.SelectedItem = _active.KB.IsNullOrEmpty() ? "Backlog" : _active.KB;

            if (_active.TotalWork == 0)
                _active.TotalWork = _defaults.DefaultTaskLength;
            _viewer.Duration.Text = _active.TotalWork.ToString();

            if (_active.ReminderTime != DateAndTime.DateValue("1/1/4501"))
            {
                _viewer.DtReminder.Value = _active.ReminderTime;
                _viewer.DtReminder.Checked = true;
            }
            if (_active.DueDate != DateAndTime.DateValue("1/1/4501"))
            {
                _viewer.DtDuedate.Value = _active.DueDate;
                _viewer.DtDuedate.Checked = true;
            }

            // Deactivate accelerator controls
            ToggleXl(
                (from x in _xlCtrlLookup select x).ToDictionary(x => x.Key, x => 'A'),
                ForceState.force_off);

            // Deactivate controls that are not set in _options
            if (_options != FlagsToSet.all)
                ActivateOptions();

        }

        /// <summary>
        /// Activates or deactivates controls on _viewer based on _options set in class
        /// </summary>
        private void ActivateOptions()
        {
            foreach (var optionGroup in OptionsGroups)
            {
                foreach (var control in optionGroup.Value)
                {
                    control.Enabled = _options.HasFlag(optionGroup.Key);
                    control.Visible = _options.HasFlag(optionGroup.Key);
                }
            }
        }

        #endregion

        #region Public Properties

        private FlagsToSet _options;
        /// <summary>
        /// Sets options for which controls / fields to activate using FlagsToSet enumeration
        /// </summary>
        /// <returns></returns>
        public FlagsToSet Options
        {
            get => _options;
            set
            {
                _options = value;
                ActivateOptions();
            }
        }

        #endregion

        #region Public Major Actions

        /// <summary>
        /// Loads a TagViewer with categories relevant to People for assigment
        /// </summary>
        public void AssignPeople()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "People");

            var filtered_cats = (from x in _dict_categories
                                 where x.Key.Contains(prefix.Value)
                                 select x).ToSortedDictionary();

            List<string> selections = Array.ConvertAll(_active.People.Split(','), x => x.Trim()).ToList();

            selections.Remove("");

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewerInstance: viewer,
                                                   dictOptions: filtered_cats,
                                                   autoAssigner: _autoAssign,
                                                   prefixes: _defaults.PrefixList,
                                                   selections: selections,
                                                   prefixKey: prefix.Key,
                                                   objItemObject: _active.OlItem,
                                                   userEmailAddress: _userEmailAddress);
                viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    _active.People = controller.SelectionString();
                    _viewer.PeopleSelection.Text = _active.People;
                }
            }
        }

        /// <summary>
        /// Loads a TagViewer with categories relevant to Context for assigment
        /// </summary>
        public void AssignContext()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Context");

            var filtered_cats = (from x in _dict_categories
                                 where x.Key.Contains(prefix.Value)
                                 select x).ToSortedDictionary();

            List<string> selections = Array.ConvertAll(_active.Context.Split(','), x => x.Trim()).ToList();
            bool unused1 = selections.Remove("");

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewerInstance: viewer,
                                                   dictOptions: filtered_cats,
                                                   autoAssigner: _autoAssign,
                                                   prefixes: _defaults.PrefixList,
                                                   selections: selections,
                                                   prefixKey: prefix.Key,
                                                   objItemObject: _active.OlItem,
                                                   userEmailAddress: _userEmailAddress);
                viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    _active.Context = controller.SelectionString();
                    _viewer.CategorySelection.Text = _active.Context;
                }
            }

        }

        public void AssignProject()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Project");

            var filtered_cats = (from x in _dict_categories
                                 where x.Key.Contains(prefix.Value)
                                 select x).ToSortedDictionary();

            List<string> selections = Array.ConvertAll(_active.Project.Split(','), x => x.Trim()).ToList();
            bool unused1 = selections.Remove("");

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewerInstance: viewer,
                                                   dictOptions: filtered_cats,
                                                   autoAssigner: _autoAssign,
                                                   prefixes: _defaults.PrefixList,
                                                   selections: selections,
                                                   prefixKey: prefix.Key,
                                                   objItemObject: _active.OlItem,
                                                   userEmailAddress: _userEmailAddress);
                var result = viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    _active.Project = controller.SelectionString();
                    _viewer.ProjectSelection.Text = _active.Project;
                }
            }
        }

        /// <summary>
        /// Loads a TagViewer with categories relevant to Topics for assigment
        /// </summary>
        public void AssignTopic()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Topic");

            var filtered_cats = (from x in _dict_categories
                                 where x.Key.Contains(prefix.Value)
                                 select x).ToSortedDictionary();

            List<string> selections = Array.ConvertAll(_active.Topic.Split(','), x => x.Trim()).ToList();
            bool unused1 = selections.Remove("");

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(viewerInstance: viewer,
                                                   dictOptions: filtered_cats,
                                                   autoAssigner: _autoAssign,
                                                   prefixes: _defaults.PrefixList,
                                                   selections: selections,
                                                   prefixKey: prefix.Key,
                                                   objItemObject: _active.OlItem,
                                                   userEmailAddress: _userEmailAddress);
                var result = viewer.ShowDialog();
                if (controller.ExitType != "Cancel")
                {
                    _active.Topic = controller.SelectionString();
                    _viewer.TopicSelection.Text = _active.Topic;
                }
            }
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Assign_KB()
        {
            _active.KB = _viewer.KbSelector.SelectedItem.ToString();
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Assign_Priority()
        {
            string TmpStr = _viewer.PriorityBox.SelectedItem.ToString();
            if (TmpStr == "High")
            {
                _active.Priority = OlImportance.olImportanceHigh;
            }
            else if (TmpStr == "Low")
            {
                _active.Priority = OlImportance.olImportanceLow;
            }
            else
            {
                _active.Priority = OlImportance.olImportanceNormal;
            }
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Today_Change()
        {
            _active.Today = _viewer.CbxToday.Checked;
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Bullpin_Change()
        {
            _active.Bullpin = _viewer.CbxBullpin.Checked;
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void FlagAsTask_Change()
        {
            _active.FlagAsTask = _viewer.CbxFlagAsTask.Checked;
        }

        public void MouseFilter_FormClicked(object sender, EventArgs e)
        {
            if (_altActive)
            {
                _altActive = false;
                ToggleXl(_xlCtrlsActive, ForceState.force_off);
            }
        }

        /// <summary>
        /// Method determines if any category has been selected and copies the flags from the 
        /// sample _active item to all members of _todo_list based on flags set in _options
        /// </summary>
        public void OK_Action()
        {
            if (AnyCategorySelected)
            {

                // Capture the value of the task subject and if not empty write to ToDoItem
                if (_options.HasFlag(FlagsToSet.taskname))
                {
                    if (!string.IsNullOrEmpty(_viewer.TaskName.Text))
                        _active.TaskSubject = _viewer.TaskName.Text;
                }

                // Capture the worktime, validate and write to ToDoItem
                CaptureDuration();

                _viewer.Hide();

                // Apply values captured in _active to each member of _todo_list for flags in _options
                ApplyChanges();

                _viewer.DialogResult = DialogResult.OK;

                _viewer.Dispose();
            }
        }

        /// <summary>
        /// Handles cancel button click. Sets the controller exit type to 
        /// "Cancel" and disposes of the viewer
        /// </summary>
        public void Cancel_Action()
        {
            _viewer.Hide();
            _exit_type = "Cancel";
            _viewer.DialogResult = DialogResult.Cancel;
            _viewer.Dispose();
        }

        #endregion

        #region Public Shortcuts

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Personal()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Context");
            _viewer.CategorySelection.Text = prefix.Value + "Personal";
            _active.Context = prefix.Value + "Personal";

            prefix = _defaults.PrefixList.Find(x => x.Key == "Project");
            _viewer.ProjectSelection.Text = prefix.Value + "Personal - Other";
            _active.Project = prefix.Value + "Personal - Other";
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Meeting()
        {
            SetFlag("Meeting", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Email()
        {
            SetFlag("Email", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Calls()
        {
            SetFlag("Calls", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_PreRead()
        {
            SetFlag("PreRead", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_WaitingFor()
        {
            SetFlag("Waiting For", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Unprocessed()
        {
            SetFlag("Reading - .Unprocessed > 2 Minutes", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_ReadingBusiness()
        {
            SetFlag("Reading - Business", FlagsToSet.context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_ReadingNews()
        {
            SetFlag("Reading - News | Articles | Other", FlagsToSet.context);
            SetFlag("Routine - Reading", FlagsToSet.projects);
            SetFlag("READ: " + _viewer.TaskName.Text, FlagsToSet.taskname);
            SetFlag("15", FlagsToSet.worktime);
            bool unused = _viewer.Duration.Focus();
        }

        #endregion

        #region Public Keyboard Events and Properties
        public bool KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                if (_altActive) 
                { 
                    var tup = RecurseXl(_xlCtrlsActive, _altActive, Conversions.ToChar(""), _altLevel);
                    _xlCtrlsActive = tup.dictActive;
                    _altActive = tup.altActive;
                    _altLevel = tup.level;
                    return true;               
                }
                else
                {
                    ToggleXlGroupNav(ForceState.force_on);
                    _altActive = true;
                    return true;
                }
            }
            else if (_altActive)
            {
                if (e.KeyCode >= Keys.A & e.KeyCode <= Keys.Z)
                {
                    var tup = RecurseXl(_xlCtrlsActive, _altActive, e.KeyCode.ToString().ToUpper()[0], _altLevel);
                    _xlCtrlsActive = tup.dictActive;
                    _altActive = tup.altActive;
                    _altLevel = tup.level;
                }
                else if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) || (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
                {
                    var tup = ActivateXlGroup(e.KeyCode.ToString()[0]);
                    _xlCtrlsActive = tup.dictActive;
                    _altActive = tup.altActive;
                    _altLevel = tup.level;                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool SuppressKeystrokes
        {
            get
            {
                return _altActive;
            }
        }
        #endregion

        #region Private Helper Properties and Functions

        /// <summary>
        /// Property determines whether any category contains a value
        /// </summary>
        /// <returns>True if any value set in Context, People, Project or Topic. Else returns False</returns>
        private bool AnyCategorySelected
        {
            //TODO: Rewrite AnyCategorySelected property to be more stable
            get
            {
                return _viewer.CategorySelection.Text != "[Category Label]" | _viewer.PeopleSelection.Text != "[Assigned People Flagged]" | _viewer.ProjectSelection.Text != "[ Projects Flagged ]" | _viewer.TopicSelection.Text != "[Other Topics Tagged]";
            }
        }
                
        //private void ActivateOptions()
        //{
        //    if (_options.HasFlag(FlagsToSet.all))
        //    {
        //        _viewer.ShortcutMeeting.Enabled = true;
        //        _viewer.ShortcutCalls.Enabled = true;
        //        _viewer.ShortcutPersonal.Enabled = true;
        //        _viewer.ShortcutEmail.Enabled = true;
        //        _viewer.ShortcutInternet.Enabled = true;
        //        _viewer.ShortcutReadingBusiness.Enabled = true;
        //        _viewer.ShortcutNews.Enabled = true;
        //        _viewer.ShortcutUnprocessed.Enabled = true;
        //        _viewer.ShortcutWaitingFor.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.ShortcutMeeting.Enabled = false;
        //        _viewer.ShortcutCalls.Enabled = false;
        //        _viewer.ShortcutPersonal.Enabled = false;
        //        _viewer.ShortcutEmail.Enabled = false;
        //        _viewer.ShortcutInternet.Enabled = false;
        //        _viewer.ShortcutReadingBusiness.Enabled = false;
        //        _viewer.ShortcutNews.Enabled = false;
        //        _viewer.ShortcutUnprocessed.Enabled = false;
        //        _viewer.ShortcutWaitingFor.Enabled = false;
        //        _viewer.ShortcutPreRead.Enabled = false;

        //        _viewer.ShortcutMeeting.Visible = false;
        //        _viewer.ShortcutCalls.Visible = false;
        //        _viewer.ShortcutPersonal.Visible = false;
        //        _viewer.ShortcutEmail.Visible = false;
        //        _viewer.ShortcutInternet.Visible = false;
        //        _viewer.ShortcutReadingBusiness.Visible = false;
        //        _viewer.ShortcutNews.Visible = false;
        //        _viewer.ShortcutUnprocessed.Visible = false;
        //        _viewer.ShortcutWaitingFor.Visible = false;
        //        _viewer.ShortcutPreRead.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.context))
        //    {
        //        _viewer.CategorySelection.Enabled = true;
        //        _viewer.LblContext.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.CategorySelection.Enabled = false;
        //        _viewer.LblContext.Enabled = false;

        //        _viewer.CategorySelection.Visible = false;
        //        _viewer.LblContext.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.people))
        //    {
        //        _viewer.PeopleSelection.Enabled = true;
        //        _viewer.LblPeople.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.PeopleSelection.Enabled = false;
        //        _viewer.LblPeople.Enabled = false;

        //        _viewer.PeopleSelection.Visible = false;
        //        _viewer.LblPeople.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.projects))
        //    {
        //        _viewer.ProjectSelection.Enabled = true;
        //        _viewer.LblProject.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.ProjectSelection.Enabled = false;
        //        _viewer.LblProject.Enabled = false;

        //        _viewer.ProjectSelection.Visible = false;
        //        _viewer.LblProject.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.topics))
        //    {
        //        _viewer.TopicSelection.Enabled = true;
        //        _viewer.LblTopic.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.TopicSelection.Enabled = false;
        //        _viewer.LblTopic.Enabled = false;

        //        _viewer.TopicSelection.Visible = false;
        //        _viewer.LblTopic.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.priority))
        //    {
        //        _viewer.PriorityBox.Enabled = true;
        //        _viewer.LblPriority.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.PriorityBox.Enabled = false;
        //        _viewer.LblPriority.Enabled = false;

        //        _viewer.PriorityBox.Visible = false;
        //        _viewer.LblPriority.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.taskname))
        //    {
        //        _viewer.TaskName.Enabled = true;
        //        _viewer.LblTaskname.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.TaskName.Enabled = false;
        //        _viewer.LblTaskname.Enabled = false;

        //        _viewer.TaskName.Visible = false;
        //        _viewer.LblTaskname.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.worktime))
        //    {
        //        _viewer.Duration.Enabled = true;
        //        _viewer.LblDuration.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.Duration.Enabled = false;
        //        _viewer.LblDuration.Enabled = false;

        //        _viewer.Duration.Visible = false;
        //        _viewer.LblDuration.Visible = false;
        //    }

        //    _viewer.CbxToday.Enabled = _options.HasFlag(FlagsToSet.today);
        //    _viewer.CbxToday.Visible = _options.HasFlag(FlagsToSet.today);

        //    _viewer.CbxBullpin.Enabled = _options.HasFlag(FlagsToSet.bullpin);
        //    _viewer.CbxBullpin.Visible = _options.HasFlag(FlagsToSet.bullpin);


        //    if (_options.HasFlag(FlagsToSet.kbf))
        //    {
        //        _viewer.KbSelector.Enabled = true;
        //        _viewer.LblKbf.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.KbSelector.Enabled = false;
        //        _viewer.LblKbf.Enabled = false;

        //        _viewer.KbSelector.Visible = false;
        //        _viewer.LblKbf.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.duedate))
        //    {
        //        _viewer.DtDuedate.Enabled = true;
        //        _viewer.LblDuedate.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.DtDuedate.Enabled = false;
        //        _viewer.LblDuedate.Enabled = false;

        //        _viewer.DtDuedate.Visible = false;
        //        _viewer.LblDuedate.Visible = false;
        //    }

        //    if (_options.HasFlag(FlagsToSet.reminder))
        //    {
        //        _viewer.DtReminder.Enabled = true;
        //        _viewer.LblReminder.Enabled = true;
        //    }
        //    else
        //    {
        //        _viewer.DtReminder.Enabled = false;
        //        _viewer.LblReminder.Enabled = false;

        //        _viewer.DtReminder.Visible = false;
        //        _viewer.LblReminder.Visible = false;
        //    }


        //}

        /// <summary>
        /// Sets value based on the flag type and value
        /// </summary>
        /// <param name="value">Comma separated list of tags</param>
        /// <param name="flagType">Used to identify field names and tag Prefix</param>
        private void SetFlag(string value, FlagsToSet flagType)
        {
            switch (flagType)
            {
                case FlagsToSet.context:
                    {
                        _active.Context = value;
                        _viewer.CategorySelection.Text = _active.Context;
                        break;
                    }
                case FlagsToSet.people:
                    {
                        _active.People = value;
                        _viewer.PeopleSelection.Text = _active.People;
                        break;
                    }
                case FlagsToSet.projects:
                    {
                        _active.Project = value;
                        _viewer.ProjectSelection.Text = _active.Project;
                        break;
                    }
                case FlagsToSet.topics:
                    {
                        _active.Topic = value;
                        _viewer.TopicSelection.Text = _active.Topic;
                        break;
                    }
                case FlagsToSet.taskname:
                    {
                        _active.TaskSubject = value;
                        _viewer.TaskName.Text = value;
                        break;
                    }
                case FlagsToSet.worktime:
                    {
                        _viewer.Duration.Text = value;
                        break;
                    }
                    // Note that _active is set after OK click
            }

        }

        /// <summary>
        /// Method grabs the work Duration out of a text box, converts to an integer, 
        /// and sets totalwork on the ToDoItem. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Duration must be >= 0 </exception>
        /// <exception cref="InvalidCastException">Value must be an integer </exception>
        private void CaptureDuration()
        {
            int duration;
            try
            {
                duration = Conversions.ToInteger(_viewer.Duration.Text);
                if (duration < 0)
                {
                    throw new ArgumentOutOfRangeException("Duration cannot be negative");
                }
            }
            catch (InvalidCastException ex)
            {
                var unused1 = Interaction.MsgBox("Could not convert to integer. Please put a positive integer in the duration box");
                duration = -1;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                var unused = Interaction.MsgBox(ex.Message);
                duration = -1;
            }

            if (duration >= 0)
            {
                _active.TotalWork = duration;
            }
        }

        /// <summary>
        /// Iterates through _todo_list and applies the values in _active for the fields in _options
        /// </summary>
        private void ApplyChanges()
        {
            foreach (ToDoItem c in _todo_list)
            {
                c.FlagAsTask = true;
                c.IsReadOnly = true;

                if (_options.HasFlag(FlagsToSet.context))
                    c.Context = _active.Context;
                if (_options.HasFlag(FlagsToSet.people))
                    c.People = _active.People;
                if (_options.HasFlag(FlagsToSet.projects))
                    c.Project = _active.Project;
                if (_options.HasFlag(FlagsToSet.topics))
                    c.Topic = _active.Topic;
                if (_options.HasFlag(FlagsToSet.today))
                    c.Today = _active.Today;
                if (_options.HasFlag(FlagsToSet.bullpin))
                    c.Bullpin = _active.Bullpin;
                if (_options.HasFlag(FlagsToSet.kbf))
                    c.KB = _active.KB;

                c.WriteFlagsBatch();
                c.IsReadOnly = false;

                if (_options.HasFlag(FlagsToSet.priority))
                    c.Priority = _active.Priority;
                if (_options.HasFlag(FlagsToSet.taskname))
                    c.TaskSubject = _active.TaskSubject;
                if (_options.HasFlag(FlagsToSet.worktime))
                    c.TotalWork = _active.TotalWork;
                if (_options.HasFlag(FlagsToSet.duedate))
                    c.DueDate = _active.DueDate;
                if (_options.HasFlag(FlagsToSet.reminder))
                    c.ReminderTime = _active.ReminderTime;
            }
        }

        #endregion

        #region Keyboard UI

        private void ToggleXl(Dictionary<Label, char> dictLabels, ForceState state)
        {
            switch (state)
            {
                case ForceState.none:
                    {
                        foreach (var row in dictLabels)
                            row.Key.Visible = !row.Key.Visible;
                        break;
                    }

                case ForceState.force_on:
                    {
                        foreach (var row in dictLabels)
                            row.Key.Visible = true;
                        break;
                    }

                case ForceState.force_off:
                    {
                        foreach (var row in dictLabels)
                            row.Key.Visible = false;
                        break;
                    }
            }

        }

        private void UpdateCaptions(Dictionary<Label, char> dictLabels)
        {
            foreach (var row in dictLabels)
                row.Key.Text = Conversions.ToString(row.Value);
        }

        private void ExecuteXlAction(Label lbl)
        {
            var ctrl = _xlCtrlLookup[lbl];
            if (ctrl is Button)
            {
                Button btn = ctrl as Button;
                btn.PerformClick();
            }

            else if (ctrl is CheckBox)
            {
                CheckBox checkBox = ctrl as CheckBox;
                checkBox.Checked = !checkBox.Checked;
            }

            else if (ctrl is TextBox)
            {
                TextBox txt = ctrl as TextBox;
                txt.Select();
                txt.SelectionStart = txt.Text.Length;
            }

            else if (ctrl is ComboBox)
            {
                ComboBox combo = (ComboBox)ctrl;
                combo.Select();
                combo.DroppedDown = true;
            }

            else if (ctrl is DateTimePicker)
            {
                DateTimePicker dt = (DateTimePicker)ctrl;

                int x = dt.Width - 10;
                int y = (int)Math.Round(dt.Height / 2d);
                int lParam = x + y * 0x10000;
                bool unused = PostMessage(dt.Handle, WM_LBUTTONDOWN, 1, lParam);
            }

            else if (ctrl is Label)
            {

                if (lbl.Equals(_viewer.XlPeople))
                {
                    AssignPeople();
                }
                else if (lbl.Equals(_viewer.XlProject))
                {
                    AssignProject();
                }
                else if (lbl.Equals(_viewer.XlTopic))
                {
                    AssignTopic();
                }
                else if (lbl.Equals(_viewer.XlContext))
                {
                    AssignContext();
                }
                else
                {
                    throw new ArgumentException("lbl not assigned properly to control", nameof(lbl));
                }
            }
            else
            {
                throw new ArgumentException("lbl not assigned properly to control", nameof(lbl));
            }

        }

        internal void ToggleXlGroupNav(ForceState desiredState) => ToggleXl(_xlCtrlsNav, desiredState);

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) ActivateXlGroup(char selectedChar)
        {
            int groupNumber = int.TryParse(selectedChar.ToString(), out groupNumber) ? groupNumber : 0;
            if ((groupNumber != 0)&&(groupNumber!=_activeNavGroup))
            {
                var dictActivate = (from controlCaption in GetCaptionLookup(groupNumber)
                                    where GetOptionsLookup(0)[controlCaption.Key]
                                    select controlCaption)
                                    .ToDictionary(
                                        controlCaption => controlCaption.Key,
                                        controlCaption => controlCaption.Value[groupNumber]);
                ToggleXl(dictActivate, ForceState.force_on);
                UpdateCaptions(dictActivate);
                return (dictActivate, true, 1);
            }
            else { return (null, true, 0); }
        }

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) RecurseXl(Dictionary<Label, char> dictSeed, bool altActive, char selectedChar, int level)
        {

            Dictionary<Label, char> dictDeactivate;
            Dictionary<Label, char> dictActivate;

            if (!altActive)
            {

                dictActivate = (from x in _xlCtrlCaptions
                                where _xlCtrlOptions[x.Key]
                                select x).ToDictionary(x => x.Key, x => char.ToUpper(x.Value[0]));

                ToggleXl(dictActivate, ForceState.force_on);
                UpdateCaptions(dictActivate);

                return (dictActivate, true, 1);
            }


            else if (dictSeed is null)
            {
                // Ensure that dictSeed is assigned. Alt key should not be
                // active if there is no seed value
                throw new ArgumentNullException(nameof(dictSeed));
            }

            else if (selectedChar == '\0')
            {
                // Empty character is only passed if Alt key is pressed again.
                // In this case, we should deactivate the accelerator dialogue

                ToggleXl(dictSeed, ForceState.force_off);
                return (null, false, 0);
            }

            else
            {
                // Get accelerator labels that match the key stroke
                dictActivate = (from x in dictSeed
                                where x.Value == selectedChar
                                select x).ToDictionary(x => x.Key, x => char.ToUpper(_xlCtrlCaptions[x.Key][level]));

                switch (dictActivate.Count)
                {
                    case 0:
                        {
                            // If character doesn't match, ignore it
                            return (dictSeed, true, 0);
                        }

                    case 1:
                        {
                            // If only 1 element, we have found a match. 

                            // Turn off all remaining accelerator labels, including the match
                            ToggleXl(dictSeed, ForceState.force_off);

                            // Execute the designated action for the control
                            ExecuteXlAction(dictActivate.First().Key);

                            // Return values to reset the seed values
                            return (null, false, 0);
                        }

                    default:
                        {
                            // If more than 1 element, we need to keep searching letters

                            // Get controls to deactivate
                            dictDeactivate = (from x in dictSeed
                                              where x.Value != selectedChar
                                              select x).ToDictionary(x => x.Key, x => x.Value);
                            ToggleXl(dictDeactivate, ForceState.force_off);
                            UpdateCaptions(dictActivate);

                            // Return values to seed the next recursion
                            return (dictActivate, true, level + 1);
                        }

                }


            }

        }

        #endregion

        #region Data Groupings

        //private Dictionary<Label, bool> CreateOptionsLookup()
        //{
        //    var xlCtrlOptions = new Dictionary<Label, bool>();
        //    {
        //        xlCtrlOptions.Add(_viewer.XlTopic, _options.HasFlag(FlagsToSet.topics));
        //        xlCtrlOptions.Add(_viewer.XlProject, _options.HasFlag(FlagsToSet.projects));
        //        xlCtrlOptions.Add(_viewer.XlPeople, _options.HasFlag(FlagsToSet.people));
        //        xlCtrlOptions.Add(_viewer.XlContext, _options.HasFlag(FlagsToSet.context));
        //        xlCtrlOptions.Add(_viewer.XlTaskname, _options.HasFlag(FlagsToSet.taskname));
        //        xlCtrlOptions.Add(_viewer.XlImportance, _options.HasFlag(FlagsToSet.priority));
        //        xlCtrlOptions.Add(_viewer.XlKanban, _options.HasFlag(FlagsToSet.kbf));
        //        xlCtrlOptions.Add(_viewer.XlWorktime, _options.HasFlag(FlagsToSet.worktime));
        //        xlCtrlOptions.Add(_viewer.XlOk, true);
        //        xlCtrlOptions.Add(_viewer.XlCancel, true);
        //        xlCtrlOptions.Add(_viewer.XlReminder, _options.HasFlag(FlagsToSet.reminder));
        //        xlCtrlOptions.Add(_viewer.XlDuedate, _options.HasFlag(FlagsToSet.duedate));
        //        xlCtrlOptions.Add(_viewer.XlScWaiting, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScUnprocessed, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScNews, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScEmail, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScReadingbusiness, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScCalls, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScInternet, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScPreread, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScMeeting, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScPersonal, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScBullpin, _options.HasFlag(FlagsToSet.all));
        //        xlCtrlOptions.Add(_viewer.XlScToday, _options.HasFlag(FlagsToSet.all));
        //    }
        //    return xlCtrlOptions;
        //}

        internal Dictionary<Label, bool> GetOptionsLookup(int group)
        {
            return GetControlRelationships().Where(x => x.Group == group)
                                            .Select(x => new KeyValuePair<Label, bool>(x.Accelerator, x.Active))
                                            .ToDictionary();
        }

        internal Dictionary<Label, bool> GetOptionsLookup()
        {
            return GetControlRelationships().Select(x => new KeyValuePair<Label, bool>(x.Accelerator, x.Active))
                                            .ToDictionary();
        }

        //internal Dictionary<Label, string> CreateCaptionLookup()
        //{
        //    var xlCtrlCaptions = new Dictionary<Label, string>();
        //    {
        //        xlCtrlCaptions.Add(_viewer.XlTopic, _viewer.LblTopic.Text);
        //        xlCtrlCaptions.Add(_viewer.XlProject, _viewer.LblProject.Text);
        //        xlCtrlCaptions.Add(_viewer.XlPeople, _viewer.LblPeople.Text);
        //        xlCtrlCaptions.Add(_viewer.XlContext, _viewer.LblContext.Text);
        //        xlCtrlCaptions.Add(_viewer.XlTaskname, _viewer.LblTaskname.Text);
        //        xlCtrlCaptions.Add(_viewer.XlImportance, _viewer.LblPriority.Text);
        //        xlCtrlCaptions.Add(_viewer.XlKanban, _viewer.LblKbf.Text);
        //        xlCtrlCaptions.Add(_viewer.XlWorktime, _viewer.LblDuration.Text);
        //        xlCtrlCaptions.Add(_viewer.XlOk, _viewer.OKButton.Text);
        //        xlCtrlCaptions.Add(_viewer.XlCancel, _viewer.Cancel_Button.Text);
        //        xlCtrlCaptions.Add(_viewer.XlReminder, _viewer.LblReminder.Text);
        //        xlCtrlCaptions.Add(_viewer.XlDuedate, _viewer.LblDuedate.Text);

        //        xlCtrlCaptions.Add(_viewer.XlScWaiting, _viewer.ShortcutWaitingFor.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScUnprocessed, _viewer.ShortcutUnprocessed.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScNews, _viewer.ShortcutNews.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScEmail, _viewer.ShortcutEmail.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScReadingbusiness, _viewer.ShortcutReadingBusiness.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScCalls, _viewer.ShortcutCalls.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScInternet, _viewer.ShortcutInternet.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScPreread, _viewer.ShortcutPreRead.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScMeeting, _viewer.ShortcutMeeting.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScPersonal, _viewer.ShortcutPersonal.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScBullpin, _viewer.CbxBullpin.Text);
        //        xlCtrlCaptions.Add(_viewer.XlScToday, _viewer.CbxToday.Text);
        //    }
        //    return xlCtrlCaptions;
        //}

        internal Dictionary<Label, string> GetCaptionLookup(int group)
        {
            return GetControlRelationships().Where(x => x.Group == group)
                                            .Select(x => new KeyValuePair<Label, string>(x.Accelerator, x.Caption))
                                            .ToDictionary();
        }

        internal Dictionary<Label, string> GetCaptionLookup()
        {
            return GetControlRelationships().Select(x => new KeyValuePair<Label, string>(x.Accelerator, x.Caption))
                                            .ToDictionary();
        }

        //internal Dictionary<Label, Control> CreateControlLookup()
        //{
        //    var xlCtrlLookup = new Dictionary<Label, Control>();
        //    {
        //        xlCtrlLookup.Add(_viewer.XlTopic, _viewer.LblTopic);
        //        xlCtrlLookup.Add(_viewer.XlProject, _viewer.LblProject);
        //        xlCtrlLookup.Add(_viewer.XlPeople, _viewer.LblPeople);
        //        xlCtrlLookup.Add(_viewer.XlContext, _viewer.LblContext);
        //        xlCtrlLookup.Add(_viewer.XlTaskname, _viewer.TaskName);
        //        xlCtrlLookup.Add(_viewer.XlImportance, _viewer.PriorityBox);
        //        xlCtrlLookup.Add(_viewer.XlKanban, _viewer.KbSelector);
        //        xlCtrlLookup.Add(_viewer.XlWorktime, _viewer.Duration);
        //        xlCtrlLookup.Add(_viewer.XlOk, _viewer.OKButton);
        //        xlCtrlLookup.Add(_viewer.XlCancel, _viewer.Cancel_Button);
        //        xlCtrlLookup.Add(_viewer.XlReminder, _viewer.DtReminder);
        //        xlCtrlLookup.Add(_viewer.XlDuedate, _viewer.DtDuedate);

        //        xlCtrlLookup.Add(_viewer.XlScWaiting, _viewer.ShortcutWaitingFor);
        //        xlCtrlLookup.Add(_viewer.XlScUnprocessed, _viewer.ShortcutUnprocessed);
        //        xlCtrlLookup.Add(_viewer.XlScNews, _viewer.ShortcutNews);
        //        xlCtrlLookup.Add(_viewer.XlScEmail, _viewer.ShortcutEmail);
        //        xlCtrlLookup.Add(_viewer.XlScReadingbusiness, _viewer.ShortcutReadingBusiness);
        //        xlCtrlLookup.Add(_viewer.XlScCalls, _viewer.ShortcutCalls);
        //        xlCtrlLookup.Add(_viewer.XlScInternet, _viewer.ShortcutInternet);
        //        xlCtrlLookup.Add(_viewer.XlScPreread, _viewer.ShortcutPreRead);
        //        xlCtrlLookup.Add(_viewer.XlScMeeting, _viewer.ShortcutMeeting);
        //        xlCtrlLookup.Add(_viewer.XlScPersonal, _viewer.ShortcutPersonal);
        //        xlCtrlLookup.Add(_viewer.XlScBullpin, _viewer.CbxBullpin);
        //        xlCtrlLookup.Add(_viewer.XlScToday, _viewer.CbxToday);
        //    }
        //    return xlCtrlLookup;
        //}

        internal Dictionary<Label, Control> GetControlLookup(int group)
        {
            return GetControlRelationships().Where(x => x.Group == group)
                                            .Select(x => new KeyValuePair<Label, Control>(x.Accelerator, x.Control))
                                            .ToDictionary();
        }

        internal Dictionary<Label, Control> GetControlLookup()
        {
            return GetControlRelationships().Select(x => new KeyValuePair<Label, Control>(x.Accelerator, x.Control))
                                            .ToDictionary();
        }

        private List<ControlRelationship> GetControlRelationships()
        {
            var list = new List<ControlRelationship>
            {
                new ControlRelationship(0, _viewer.XlSector1,  true,  _viewer.XlSector1.Text,  _viewer.XlSector1),
                new ControlRelationship(0, _viewer.XlSector2,  true,  _viewer.XlSector2.Text,  _viewer.XlSector2),
                new ControlRelationship(0, _viewer.XlSector3,  _options.HasFlag(FlagsToSet.all),  _viewer.XlSector3.Text,  _viewer.XlSector3),
                new ControlRelationship(0, _viewer.XlSector4,  true,  _viewer.XlSector4.Text,  _viewer.XlSector4),
                new ControlRelationship(2, _viewer.XlTopic,  _options.HasFlag(FlagsToSet.topics),  _viewer.LblTopic.Text,  _viewer.LblTopic),
                new ControlRelationship(2, _viewer.XlProject,  _options.HasFlag(FlagsToSet.projects),  _viewer.LblProject.Text,  _viewer.LblProject),
                new ControlRelationship(2, _viewer.XlPeople,  _options.HasFlag(FlagsToSet.people),  _viewer.LblPeople.Text,  _viewer.LblPeople),
                new ControlRelationship(2, _viewer.XlContext,  _options.HasFlag(FlagsToSet.context),  _viewer.LblContext.Text,  _viewer.LblContext),
                new ControlRelationship(1, _viewer.XlTaskname,  _options.HasFlag(FlagsToSet.taskname),  _viewer.LblTaskname.Text,  _viewer.TaskName),
                new ControlRelationship(1, _viewer.XlImportance,  _options.HasFlag(FlagsToSet.priority),  _viewer.LblPriority.Text,  _viewer.PriorityBox),
                new ControlRelationship(1, _viewer.XlKanban,  _options.HasFlag(FlagsToSet.kbf),  _viewer.LblKbf.Text,  _viewer.KbSelector),
                new ControlRelationship(1, _viewer.XlWorktime,  _options.HasFlag(FlagsToSet.worktime),  _viewer.LblDuration.Text,  _viewer.Duration),
                new ControlRelationship(4, _viewer.XlOk,  true,  _viewer.OKButton.Text,  _viewer.OKButton),
                new ControlRelationship(4, _viewer.XlCancel,  true,  _viewer.Cancel_Button.Text,  _viewer.Cancel_Button),
                new ControlRelationship(1, _viewer.XlReminder,  _options.HasFlag(FlagsToSet.reminder),  _viewer.LblReminder.Text,  _viewer.DtReminder),
                new ControlRelationship(1, _viewer.XlDuedate,  _options.HasFlag(FlagsToSet.duedate),  _viewer.LblDuedate.Text,  _viewer.DtDuedate),
                new ControlRelationship(3, _viewer.XlScWaiting,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutWaitingFor.Text,  _viewer.ShortcutWaitingFor),
                new ControlRelationship(3, _viewer.XlScUnprocessed,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutUnprocessed.Text,  _viewer.ShortcutUnprocessed),
                new ControlRelationship(3, _viewer.XlScNews,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutNews.Text,  _viewer.ShortcutNews),
                new ControlRelationship(3, _viewer.XlScEmail,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutEmail.Text,  _viewer.ShortcutEmail),
                new ControlRelationship(3, _viewer.XlScReadingbusiness,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutReadingBusiness.Text,  _viewer.ShortcutReadingBusiness),
                new ControlRelationship(3, _viewer.XlScCalls,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutCalls.Text,  _viewer.ShortcutCalls),
                new ControlRelationship(3, _viewer.XlScInternet,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutInternet.Text,  _viewer.ShortcutInternet),
                new ControlRelationship(3, _viewer.XlScPreread,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutPreRead.Text,  _viewer.ShortcutPreRead),
                new ControlRelationship(3, _viewer.XlScMeeting,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutMeeting.Text,  _viewer.ShortcutMeeting),
                new ControlRelationship(3, _viewer.XlScPersonal,  _options.HasFlag(FlagsToSet.all),  _viewer.ShortcutPersonal.Text,  _viewer.ShortcutPersonal),
                new ControlRelationship(3, _viewer.XlScBullpin,  _options.HasFlag(FlagsToSet.all),  _viewer.CbxBullpin.Text,  _viewer.CbxBullpin),
                new ControlRelationship(3, _viewer.XlScToday,  _options.HasFlag(FlagsToSet.all),  _viewer.CbxToday.Text,  _viewer.CbxToday)
            };
            return list;
        }

        private struct ControlRelationship
        {
            public ControlRelationship() { }

            public ControlRelationship(int group, Label accelerator, bool active, string caption, Control control)
            {
                Group = group;
                Accelerator = accelerator;
                Active = active;
                Caption = caption;
                Control = control;
            }

            public int Group;
            public Label Accelerator;
            public bool Active;
            public string Caption;
            public Control Control;

        }

        private Dictionary<FlagsToSet, List<Control>> _optionsGroups;
        internal Dictionary<FlagsToSet, List<Control>> OptionsGroups
        {
            get
            {
                if (_optionsGroups is null)
                {
                    _optionsGroups = new()
                    {
                        { FlagsToSet.context, new List<Control> { _viewer.CategorySelection, _viewer.LblContext } },
                        { FlagsToSet.topics, new List<Control>{ _viewer.TopicSelection, _viewer.LblTopic } },
                        { FlagsToSet.projects, new List<Control>{ _viewer.ProjectSelection, _viewer.LblProject } },
                        { FlagsToSet.people, new List<Control>{ _viewer.PeopleSelection, _viewer.LblPeople } },
                        { FlagsToSet.taskname, new List<Control>{ _viewer.TaskName, _viewer.LblTaskname } },
                        { FlagsToSet.priority, new List<Control>{ _viewer.PriorityBox, _viewer.LblPriority } },
                        { FlagsToSet.kbf, new List<Control>{ _viewer.KbSelector, _viewer.LblKbf } },
                        { FlagsToSet.worktime, new List<Control>{ _viewer.Duration, _viewer.LblDuration } },
                        { FlagsToSet.reminder, new List<Control>{ _viewer.DtReminder, _viewer.LblReminder } },
                        { FlagsToSet.duedate, new List<Control>{ _viewer.DtDuedate, _viewer.LblDuedate } },
                        { FlagsToSet.all, new List<Control> 
                        { 
                            _viewer.ShortcutMeeting,_viewer.ShortcutCalls,_viewer.ShortcutPersonal,
                            _viewer.ShortcutEmail,_viewer.ShortcutInternet,_viewer.ShortcutReadingBusiness,
                            _viewer.ShortcutNews,_viewer.ShortcutUnprocessed,_viewer.ShortcutWaitingFor,
                            _viewer.ShortcutPreRead}}
                    };
                }
                return _optionsGroups;
            }
        }
        

        #endregion

    }
}