using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace TaskVisualization
{
    public partial class TaskController
    {
        /// <summary>
        /// Constructor initializes the controller for the TaskViewer
        /// </summary>
        /// <param name="formInstance">Instance of TaskViewer</param>
        /// <param name="olCategories"></param>
        /// <param name="toDoSelection">List of ToDoItems</param>
        /// <param name="defaults"></param>
        /// <param name="autoAssign">Class implementing <seealso cref="IAutoAssign"/> interface</param>
        /// <param name="userEmailAddress">Email address of user to avoid auto-tagging everything with user tag</param>
        /// <param name="flagOptions">Enumeration of fields to activate</param>
        public TaskController(
            ITaskViewer formInstance,
            Categories olCategories,
            List<ToDoItem> toDoSelection,
            ToDoDefaults defaults,
            IAutoAssign autoAssign,
            string userEmailAddress,
            Enums.FlagsToSet flagOptions = Enums.FlagsToSet.All,
            ITagPromptService tagPromptService = null,
            Action<string> showWarning = null,
            Func<MailItem, Task<MailItemHelper>> mailItemHelperFactory = null
        )
        {
            //TODO: Add description of olCategories and defaults in documentation
            // Save parameters to internal variables
            _viewer = formInstance;
            _todo_list = toDoSelection;
            _options = flagOptions;
            _defaults = defaults;
            _autoAssign = autoAssign;
            _userEmailAddress = userEmailAddress;
            InitializeSeams(tagPromptService, showWarning, mailItemHelperFactory);

            // Activate this controller within the viewer. The accept/cancel button
            // wiring is performed inside TaskViewer.SetController (relocated there so the
            // controller depends only on ITaskViewer, not the concrete buttons).
            formInstance.SetController(this);

            // First ToDoItem in list is cloned to _active and set to readonly
            _active = _todo_list[0].DeepCopy();
            _active.ReadOnly = true;
            _active.Identifier = "ReadOnly Clone From Task Controller";
            _todo_list[0].Identifier = "Original ToDoItem passed into Task Controller";

            // All color categories in Outlook.Namespace are loaded to a sorted dictionary
            _dict_categories = new SortedDictionary<string, bool>();
            foreach (Category cat in olCategories)
                _dict_categories.Add(cat.Name, false);

            _xlCtrlLookup = GetControlLookup();
            _xlCtrlOptions = GetOptionsLookup();
            _xlCtrlCaptions = GetCaptionLookup();
            _xlCtrlsNav = (
                from controlCaption in GetCaptionLookup(0)
                where GetOptionsLookup(0)[controlCaption.Key]
                select controlCaption
            ).ToDictionary(
                controlCaption => controlCaption.Key,
                controlCaption => controlCaption.Value[0]
            );
        }

        public TaskController(
            ITaskViewer formInstance,
            Categories olCategories,
            List<ToDoItem> toDoSelection,
            ToDoDefaults defaults,
            IAutoAssign autoAssign,
            IAutoAssign projectAssign,
            IAutoAssign contextAssign,
            Func<string, string> projectsToPrograms,
            string userEmailAddress,
            IApplicationGlobals globals,
            Enums.FlagsToSet flagOptions = Enums.FlagsToSet.All,
            ITagPromptService tagPromptService = null,
            Action<string> showWarning = null,
            Func<MailItem, Task<MailItemHelper>> mailItemHelperFactory = null
        )
        {
            _viewer = formInstance;
            _todo_list = toDoSelection;
            _options = flagOptions;
            _defaults = defaults;
            _autoAssign = autoAssign;
            _userEmailAddress = userEmailAddress;
            Globals = globals;
            InitializeSeams(tagPromptService, showWarning, mailItemHelperFactory);

            // Activate this controller within the viewer. The accept/cancel button
            // wiring is performed inside TaskViewer.SetController (relocated there so the
            // controller depends only on ITaskViewer, not the concrete buttons).
            formInstance.SetController(this);

            // First ToDoItem in list is cloned to _active and set to readonly
            _active = _todo_list[0].DeepCopy();
            _active.ReadOnly = true;
            _active.Identifier = "ReadOnly Clone From Task Controller";
            _todo_list[0].Identifier = "Original ToDoItem passed into Task Controller";

            // All color categories in Outlook.Namespace are loaded to a sorted dictionary
            _dict_categories = new SortedDictionary<string, bool>();
            foreach (Category cat in olCategories)
                _dict_categories.Add(cat.Name, false);

            _xlCtrlLookup = GetControlLookup();
            _xlCtrlOptions = GetOptionsLookup();
            _xlCtrlCaptions = GetCaptionLookup();
            _xlCtrlsNav = (
                from controlCaption in GetCaptionLookup(0)
                where GetOptionsLookup(0)[controlCaption.Key]
                select controlCaption
            ).ToDictionary(
                controlCaption => controlCaption.Key,
                controlCaption => controlCaption.Value[0]
            );

            ProjectAssign = projectAssign;
            ProjectsToPrograms = projectsToPrograms;
            ContextAssign = contextAssign;
        }

        /// <summary>
        /// Assigns the three seam fields, applying production defaults when a caller passes
        /// null. Invoked from both constructors so the seams initialize on every construction
        /// path. The <c>MailItemHelper</c> factory reads <see cref="Globals"/> lazily at
        /// invocation time (matching the former inline call), so its default is valid on the
        /// 11-parameter path where <see cref="Globals"/> is set.
        /// </summary>
        private void InitializeSeams(
            ITagPromptService tagPromptService,
            Action<string> showWarning,
            Func<MailItem, Task<MailItemHelper>> mailItemHelperFactory
        )
        {
            _tagPromptService = tagPromptService ?? new TagPromptService();
            _showWarning = showWarning ?? (m => MessageBox.Show(m));
            _mailItemHelperFactory =
                mailItemHelperFactory
                ?? (m => MailItemHelper.FromMailItemAsync(m, Globals, default, false));
        }

        /// <summary>
        /// Prepares the task viewer by loading data values into the facade and activating the
        /// desired accelerator controls. Split into the host-neutral data writes
        /// (<see cref="InitializeData"/>) and the accelerator initialization
        /// (<see cref="InitializeAccelerators"/>).
        /// </summary>
        public void Initialize()
        {
            InitializeData();
            InitializeAccelerators();
        }

        /// <summary>
        /// Loads the model values into the primitive <see cref="ITaskViewer"/> facade.
        /// Host-neutral (no control identity), so it is exercised through a mocked viewer.
        /// </summary>
        internal void InitializeData()
        {
            _viewer.TaskNameText = _active.TaskSubject;
            if (!_active.Context.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ContextText = _active.Context.AsStringNoPrefix;
            if (!_active.People.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.PeopleText = _active.People.AsStringNoPrefix;
            if (!_active.Projects.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.ProjectText = _active.Projects.AsStringNoPrefix;
            if (!_active.Topics.AsStringNoPrefix.IsNullOrEmpty())
                _viewer.TopicText = _active.Topics.AsStringNoPrefix;

            _viewer.PrioritySelectedItem = TaskPriorityMapper.ToDisplayString(_active.Priority);

            _viewer.KbSelectedItem = _active.KB.AsStringNoPrefix.IsNullOrEmpty()
                ? "Backlog"
                : _active.KB.AsStringNoPrefix;

            if (_active.TotalWork == 0)
                _active.TotalWork = _defaults.DefaultTaskLength;
            _viewer.DurationText = _active.TotalWork.ToString();

            if (_active.ReminderTime != new DateTime(4501, 1, 1))
            {
                _viewer.ReminderValue = _active.ReminderTime;
                _viewer.ReminderChecked = true;
            }
            if (_active.DueDate != new DateTime(4501, 1, 1))
            {
                _viewer.DueDateValue = _active.DueDate;
                _viewer.DueDateChecked = true;
            }
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

        internal IApplicationGlobals Globals { get; set; }

        private Enums.FlagsToSet _options;

        /// <summary>
        /// Sets options for which controls / fields to activate using Enums.FlagsToSet enumeration
        /// </summary>
        /// <returns></returns>
        public Enums.FlagsToSet Options
        {
            get => _options;
            set
            {
                _options = value;
                ActivateOptions();
            }
        }

        internal Enums.FlagsToSet ChangedFlags { get; set; }

        private ITaskViewer _viewer;

        /// <summary>
        /// Concrete-form accessor confined to the irreducible live-window-handle residue
        /// (<c>.Handle</c> / <c>PostMessage</c>). All other WinForms access goes through
        /// <see cref="ITaskViewer"/> (primitive data facade) or <see cref="ViewerControls"/>
        /// (control identity), so the testable core sees only interfaces.
        /// </summary>
        private TaskViewer Form => (TaskViewer)_viewer;

        /// <summary>
        /// Control-identity accessor. Carries the measurable control-identity reads used by
        /// the control-map / accelerator regions so dedicated STA tests can supply real,
        /// never-shown, in-memory controls through <see cref="ITaskViewerControls"/>.
        /// </summary>
        private ITaskViewerControls ViewerControls => (ITaskViewerControls)_viewer;

        // Seam fields, initialized on both constructor paths via InitializeSeams.
        private ITagPromptService _tagPromptService;
        private Action<string> _showWarning;
        private Func<MailItem, Task<MailItemHelper>> _mailItemHelperFactory;

        private readonly List<ToDoItem> _todo_list;
        private readonly ToDoItem _active;

        /// <summary>
        /// The read-only working clone of the first selected item, exposed to unit tests so
        /// they can assert model-state effects of the controller's actions.
        /// </summary>
        internal ToDoItem Active => _active;

        private readonly SortedDictionary<string, bool> _dict_categories;

        //private string _exit_type = "Cancel";
        private readonly Dictionary<Label, string> _xlCtrlCaptions;
        private readonly Dictionary<Label, Control> _xlCtrlLookup;
        private readonly Dictionary<Label, bool> _xlCtrlOptions;
        private Dictionary<Label, char> _xlCtrlsActive;
        private Dictionary<Label, char> _xlCtrlsNav;
        private int _activeNavGroup = -1;
        private bool _altActive = false;
        private int _altLevel = 0;

        //private readonly string _keyCapture = "";
        private readonly ToDoDefaults _defaults;
        private readonly IAutoAssign _autoAssign;
        private string _userEmailAddress;

        private IAutoAssign _projectAssign;
        public IAutoAssign ProjectAssign
        {
            get => _projectAssign;
            set => _projectAssign = value;
        }

        public IAutoAssign ContextAssign { get; set; }

        private Func<string, string> _projectsToPrograms;
        internal Func<string, string> ProjectsToPrograms
        {
            get => _projectsToPrograms;
            private protected set => _projectsToPrograms = value;
        }
    }
}
