using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoItem : ICloneable
    {
        public ToDoItem(MailItem OlMail)
        {
            this._olObject = OlMail;
            this.InitializeMail(OlMail);
            string strCategories = OlMail.Categories;
            this._flags = new FlagParser(ref strCategories);
            OlMail.Categories = strCategories;
            this.InitializeCustomFields(_olObject);

        }

        public ToDoItem(MailItem OlMail, bool OnDemand)
        {
            _olObject = OlMail;

            if (OnDemand == false)
            {
                InitializeMail(OlMail);
                string argstrCats_All = OlMail.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlMail.Categories = argstrCats_All;
                InitializeCustomFields(_olObject);
            }
        }

        public ToDoItem(TaskItem OlTask)
        {
            _olObject = OlTask;

            InitializeTask(OlTask);
            string argstrCats_All = OlTask.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            OlTask.Categories = argstrCats_All;
            InitializeCustomFields(_olObject);

        }

        public ToDoItem(TaskItem OlTask, bool OnDemand)
        {
            _olObject = OlTask;

            if (OnDemand == false)
            {
                InitializeTask(OlTask);
                string argstrCats_All = OlTask.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlTask.Categories = argstrCats_All;
                InitializeCustomFields(_olObject);
            }
        }

        public ToDoItem(object Item, bool OnDemand)
        {

            _olObject = Item;
            dynamic olItem = Item;
            string argstrCats_All = olItem.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            olItem.Categories = argstrCats_All;
            if (OnDemand == false)
            {
                MessageBox.Show("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand");
            }
        }

        public ToDoItem(string strID)
        {
            _toDoID = strID;
        }

        
        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";
        private readonly object _olObject;
        private string _toDoID = "";
        private string _taskSubject = "";
        private string _metaTaskSubject = "";
        private string _metaTaskLvl = "";
        private string _tagProgram = "";
        private OlImportance _Priority;
        private DateTime _taskCreateDate;
        private DateTime _startDate;
        private bool _complete;
        private int _totalWork = 0;
        private bool? _activeBranch = null;
        private string _expandChildren = "";
        private string _expandChildrenState = "";
        private bool _EC2;
        private int _VisibleTreeState;
        private bool _readonly = false;
        private FlagParser _flags;
        private bool _flagAsTask = true;

        private void InitializeMail(MailItem OlMail)
        {
            _taskSubject = OlMail.TaskSubject.Length != 0 ? OlMail.TaskSubject : OlMail.Subject;
            _Priority = OlMail.Importance;
            _taskCreateDate = OlMail.CreationTime;
            _startDate = OlMail.TaskStartDate;
            _complete = (OlMail.FlagStatus == OlFlagStatus.olFlagComplete);
            _totalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
        }

        private void InitializeTask(TaskItem OlTask)
        {
            _taskSubject = OlTask.Subject;
            _Priority = OlTask.Importance;
            _taskCreateDate = OlTask.CreationTime;
            _startDate = OlTask.StartDate;
            _complete = OlTask.Complete;
            _totalWork = OlTask.TotalWork;
        }

        private void InitializeCustomFields(object Item)
        {
            _tagProgram = _olObject.GetUdfString("TagProgram");
            _activeBranch = (bool)(_olObject.GetUdfValue("AB", OlUserPropertyType.olYesNo));
            _EC2 = (bool)(_olObject.GetUdfValue("EC2", OlUserPropertyType.olYesNo));
            _expandChildren = _olObject.GetUdfString("EC");
            _expandChildrenState = _olObject.GetUdfString("EcState");
        }

        public object Clone()
        {
            var cloned_todo = new ToDoItem(_olObject, true);
            cloned_todo._toDoID = _toDoID;
            cloned_todo._taskSubject = _taskSubject;
            cloned_todo._metaTaskSubject = _metaTaskSubject;
            cloned_todo._metaTaskLvl = _metaTaskLvl;
            cloned_todo._tagProgram = _tagProgram;
            cloned_todo._Priority = _Priority;
            cloned_todo._startDate = _startDate;
            cloned_todo._complete = _complete;
            cloned_todo._totalWork = _totalWork;
            cloned_todo._activeBranch = _activeBranch;
            cloned_todo._expandChildren = _expandChildren;
            cloned_todo._expandChildrenState = _expandChildrenState;
            cloned_todo._EC2 = _EC2;
            cloned_todo._VisibleTreeState = _VisibleTreeState;
            cloned_todo._readonly = _readonly;
            return cloned_todo;
        }

        /// <summary>
    /// Gets and Sets a flag that when true, prevents saving changes to the underlying [object]
    /// </summary>
    /// <returns>Boolean</returns>
        public bool IsReadOnly {get => _readonly; set => _readonly = value; }

        /// <summary>
    /// Saves all internal variables to the [Object]
    /// </summary>
        public void ForceSave()
        {
            // Save the current state of the read only flag
            bool tmp_readonly_state = _readonly;

            // Activate saving
            _readonly = false;

            WriteFlagsBatch();
            ToDoID = _toDoID;
            TaskSubject = _taskSubject;
            MetaTaskSubject = _metaTaskSubject;
            MetaTaskLvl = _metaTaskLvl;
            TagProgram = _tagProgram;
            Priority = _Priority;
            StartDate = _startDate;
            Complete = _complete;
            TotalWork = _totalWork;
            ActiveBranch = _activeBranch ?? false;
            ExpandChildren = _expandChildren;
            ExpandChildrenState = _expandChildrenState;
            EC2 = _EC2;
            VisibleTreeState = _VisibleTreeState;

            if (_olObject is MailItem)
            {
                MailItem OlMail = (MailItem)_olObject;
                if (OlMail.FlagStatus == OlFlagStatus.olNoFlag & _flagAsTask)
                {
                    OlMail.MarkAsTask(OlMarkInterval.olMarkNoDate);
                }
                else if (OlMail.FlagStatus == OlFlagStatus.olFlagMarked & !_flagAsTask)
                {
                    OlMail.ClearTaskFlag();
                }
                OlMail.Save();
            }

            // Return read only variable to its original state
            _readonly = tmp_readonly_state;
        }
                
        public void WriteFlagsBatch()
        {
            _olObject.SetCategories(_flags.Combine());
            _olObject.SetUdf("TagContext", _flags.get_Context(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("TagPeople", _flags.get_People(false),OlUserPropertyType.olKeywords);
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any _children
            _olObject.SetUdf("TagProject", _flags.get_Projects(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("TagTopic", _flags.get_Topics(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("KB", _flags.get_KB(false));
        }

        public object OlItem
        {
            get
            {
                return _olObject;
            }
        }

        public bool FlagAsTask
        {
            get
            {
                return _flagAsTask;
            }
            set
            {
                if (_olObject is not null)
                {
                    if (_olObject is MailItem)
                    {
                        _flagAsTask = value;
                        if (!_readonly)
                        {
                            MailItem OlMail = (MailItem)_olObject;
                            if (OlMail.FlagStatus == OlFlagStatus.olNoFlag & value)
                            {
                                OlMail.MarkAsTask(OlMarkInterval.olMarkNoDate);
                            }
                            else if (OlMail.FlagStatus == OlFlagStatus.olFlagMarked & !value)
                            {
                                OlMail.ClearTaskFlag();
                            }
                            OlMail.Save();
                        }
                    }
                    else if (_olObject is TaskItem)
                    {
                        _flagAsTask = true;
                    }
                    else
                    {
                        _flagAsTask = false;
                    }
                }
            }

        }

        public DateTime TaskCreateDate
        {
            get
            {
                DateTime TaskCreateDateRet = default;
                TaskCreateDateRet = _taskCreateDate;
                return TaskCreateDateRet;
            }
        }

        public bool Bullpin
        {
            get
            {
                return _flags.Bullpin;
            }
            set
            {
                _flags.Bullpin = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        dynamic olItem = _olObject;
                        olItem.Categories = _flags.Combine();
                        olItem.Save();
                    }
                }
            }
        }

        public bool Today
        {
            get
            {
                return _flags.Today;
            }
            set
            {
                _flags.Today = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        dynamic olItem = _olObject;
                        olItem.Categories = _flags.Combine();
                        olItem.Save();
                    }
                }
            }
        }

        public DateTime ReminderTime
        {
            get
            {
                dynamic olItem = _olObject;
                return olItem.ReminderTime;
            }
            set
            {
                if (!_readonly)
                {
                    dynamic olItem = _olObject;
                    olItem.ReminderTime = (object)value;
                    var unused = olItem.Save();
                }
            }
        }

        public DateTime DueDate
        {
            get
            {
                if (_olObject is MailItem)
                {
                    MailItem OlMail = (MailItem)_olObject;
                    return OlMail.TaskDueDate;
                }
                else if (_olObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)_olObject;
                    return OlTask.DueDate;
                }
                else
                {
                    return DateTime.Parse("1/1/4501");
                }
            }
            set
            {
                if (!_readonly)
                {
                    if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.TaskDueDate = value;
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        OlTask.DueDate = value;
                        OlTask.Save();
                    }
                }
            }
        }

        public DateTime StartDate
        {
            get
            {
                return _taskCreateDate;
            }
            set
            {
                _taskCreateDate = value;
            }
        }

        public OlImportance Priority
        {
            get
            {

                if (_olObject is null)
                {
                    _Priority = OlImportance.olImportanceNormal;
                }
                else if (_olObject is MailItem)
                {
                    MailItem OlMail = (MailItem)_olObject;
                    _Priority = OlMail.Importance;
                }
                else if (_olObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)_olObject;
                    _Priority = OlTask.Importance;
                }
                return _Priority;
            }
            set
            {
                _Priority = value;
                if (!_readonly)
                {
                    if (_olObject is null)
                    {
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.Importance = _Priority;
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        OlTask.Importance = _Priority;
                        OlTask.Save();
                    }
                }
            }
        }

        public bool Complete
        {
            get
            {
                if (_olObject is null)
                {
                    _complete = false;
                }
                else if (_olObject is MailItem)
                {
                    MailItem OlMail = (MailItem)_olObject;
                    _complete = OlMail.FlagStatus == OlFlagStatus.olFlagComplete;
                }
                else if (_olObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)_olObject;
                    _complete = OlTask.Complete;
                }
                return _complete;
            }
            set
            {
                _complete = value;
                if (!_readonly)
                {
                    if (_olObject is null)
                    {
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.FlagStatus = value == true ? OlFlagStatus.olFlagComplete : OlFlagStatus.olFlagMarked;
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        OlTask.Complete = value;
                        OlTask.Save();
                    }
                }
            }
        }

        public string TaskSubject
        {
            get
            {
                if (_taskSubject.Length == 0)
                {
                    if (_olObject is null)
                    {
                        _taskSubject = "";
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        _taskSubject = OlMail.TaskSubject;
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        _taskSubject = OlTask.Subject;
                    }
                    else
                    {
                        _taskSubject = "";
                    }
                }
                return _taskSubject;
            }
            set
            {
                _taskSubject = value;
                if (!_readonly)
                {
                    if (_olObject is null)
                    {
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.TaskSubject = _taskSubject;
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        OlTask.Subject = _taskSubject;
                        OlTask.Save();
                    }
                }
            }
        }

        public string get_People(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "People");
            return _flags.get_People(IncludePrefix);
            // Set People and sanitize value
        }

        public string People 
        {
            get 
            {
                EnsureInitialized(CallerName: "People");
                return _flags.get_People(false);
            }
            set 
            {
                _flags.set_People(value: value);
                if (!_readonly)
                    SaveCatsToObj("TagPeople", _flags.get_People(false));
            } 
        }

        public void set_People(bool IncludePrefix = false, string value = default)
        {
            _flags.set_People(value: value);
            if (!_readonly)
                SaveCatsToObj("TagPeople", _flags.get_People(false));
        }

        internal string Categories 
        {
            get 
            {
                ThrowIfNull(_olObject, nameof(Categories));
                return _olObject.GetCategories();
            }
            set => _olObject.SetCategories(value);
        }

        public string get_Project(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "Project");
            return _flags.get_Projects(IncludePrefix);
            // Set Projects and sanitize value
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any _children 
        }

        public string Project 
        {
            get 
            {
                EnsureInitialized(CallerName: "Project");
                return _flags.get_Projects(false);
            }
            set 
            {
                _flags.set_Projects(value: value);
                if (!_readonly)
                    SaveCatsToObj("TagProject", _flags.get_Projects(false));
            } 
        }

        public void set_Project(bool IncludePrefix = false, string value = default)
        {
            _flags.set_Projects(value: value);
            if (!_readonly)
                SaveCatsToObj("TagProject", _flags.get_Projects(false));
        }

        public string TagProgram
        {
            get
            {
                if (_tagProgram.Length != 0)
                {
                    return _tagProgram;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _tagProgram = _olObject.GetUdfString("TagProgram");
                    return _tagProgram;
                }

            }
            set
            {
                _tagProgram = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("TagProgram", value, OlUserPropertyType.olKeywords);
                    }
                }
            }
        }
        
        public string get_Context(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "Context");
            return _flags.get_Context(IncludePrefix);
            // Set Context and sanitize value
        }

        public string Context
        {
            get => _flags.get_Context(false);
            set
            {
                _flags.set_Context(value: value);
                if (!_readonly)
                    SaveCatsToObj("TagContext", _flags.get_Context(false));
            }
        }

        public void set_Context(bool IncludePrefix = false, string value = default)
        {
            _flags.set_Context(value: value);
            if (!_readonly)
                SaveCatsToObj("TagContext", _flags.get_Context(false));
        }

        public string get_Topic(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "Topic");
            return _flags.get_Topics(IncludePrefix);
            // Set Context and sanitize value
        }

        public string Topic 
        { 
            get
            {
                EnsureInitialized(CallerName: "Topic");
                return _flags.get_Topics(false);
            } 
            set
            {
                _flags.set_Topics(value: value);
                if (!_readonly)
                    SaveCatsToObj("TagTopic", _flags.get_Topics(false));
            } 
        }
        
        public void set_Topic(bool IncludePrefix = false, string value = default)
        {
            _flags.set_Topics(value: value);
            if (!_readonly)
                SaveCatsToObj("TagTopic", _flags.get_Topics(false));
        }

        public string get_KB(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "KB");
            return _flags.get_KB(IncludePrefix);
            // Set Context and sanitize value
        }

        public string KB
        {
            get
            {
                EnsureInitialized(CallerName: "KB");
                return _flags.get_KB(false);
            }
            set
            {
                _flags.set_KB(value: value);
                if (!_readonly)
                    SaveCatsToObj("KB", _flags.get_KB(false));
            }
        }

        public void set_KB(bool IncludePrefix = false, string value = default)
        {
            _flags.set_KB(value: value);
            if (!_readonly)
                SaveCatsToObj("KB", _flags.get_KB(false));
        }

        private void SaveCatsToObj(string fieldName, string fieldValue)
        {
            if (_olObject is not null)
            {
                _olObject.SetUdf(fieldName, fieldValue, OlUserPropertyType.olKeywords);
                dynamic olTemp = _olObject;
                olTemp.Categories = _flags.Combine();
                olTemp.Save();
            }
        }

        private void EnsureInitialized(string CallerName)
        {
            if (_flags is null)
            {
                if (_olObject is null)
                    throw new ArgumentNullException("Cannot get property " + CallerName + " if both _flags AND olObject are Null");
                dynamic olItem = _olObject;
                string argstrCats_All = olItem.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                olItem.Categories = argstrCats_All;
            }
        }

        private void ThrowIfNull(object obj, string property)
        {
            if (obj == null)
                throw new ArgumentNullException($"Cannot get {property}. Item is null.");
        }

        public int TotalWork
        {
            get
            {
                if (_totalWork == 0)
                {
                    if (_olObject is null)
                    {
                        _totalWork = 0;
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        _totalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
                    }

                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        _totalWork = OlTask.TotalWork;
                    }

                    else
                    {
                        _totalWork = 0;
                    }
                }
                return _totalWork;

            }

            set
            {
                _totalWork = value;
                if (!_readonly)
                {
                    if (_olObject is null)
                    {
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.PropertyAccessor.SetProperty(PA_TOTAL_WORK, value);
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
                        OlTask.TotalWork = value;
                        OlTask.Save();
                    }
                }
            }
        }

        public string ToDoID
        {
            get
            {
                if (_toDoID.Length != 0)
                {
                    return _toDoID;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _toDoID = _olObject.GetUdfString("ToDoID");
                    return _toDoID;
                }
            }
            set
            {
                _toDoID = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("ToDoID", value);
                        SplitID();
                    }
                }
            }
        }
        // _VisibleTreeState
        public bool get_VisibleTreeStateLVL(int Lvl)
        {
            return ((int)Math.Pow(2d, Lvl - 1) & VisibleTreeState) > 0;
        }

        public void set_VisibleTreeStateLVL(int Lvl, bool value)
        {
            if (value == true)
            {
                VisibleTreeState = (int)((long)VisibleTreeState | (long)Math.Round(Math.Pow(2d, Lvl - 1)));
            }
            else
            {
                VisibleTreeState = (int)(VisibleTreeState - (VisibleTreeState & (long)Math.Round(Math.Pow(2d, Lvl - 1))));
            }
        }
        public int VisibleTreeState
        {
            get
            {
                if (_VisibleTreeState != 0)
                {
                    return _VisibleTreeState;
                }
                else if (_olObject is null)
                {
                    return -1;
                }
                else
                {
                    if (_olObject.UdfExists("VTS"))
                    {
                        _olObject.SetUdf("VTS", 63, OlUserPropertyType.olInteger); // Binary 111111 for 6 levels
                        _VisibleTreeState = 63;
                    }
                    else
                    {
                        _VisibleTreeState = (int)(_olObject.GetUdfValue("VTS", OlUserPropertyType.olInteger));
                    }
                    return _VisibleTreeState;

                }
            }
            set
            {
                if (_olObject is not null)
                {
                    _VisibleTreeState = value;
                    if (!_readonly)
                        _olObject.SetUdf("VTS", value, OlUserPropertyType.olInteger);
                }
            }
        }

        public bool ActiveBranch
        {
            get
            {
                if (_activeBranch != null) { return (bool)_activeBranch; }
                else if (_olObject is null) { return false; }
                else
                {
                    if (_olObject.UdfExists("AB"))
                    {
                        _activeBranch = (bool)_olObject.GetUdfValue("AB", OlUserPropertyType.olYesNo);
                    }
                    else
                    {
                        _olObject.SetUdf("AB", true, OlUserPropertyType.olYesNo);
                        _activeBranch = true;
                    }

                    return (bool)_activeBranch;
                }
            }
            set
            {
                _activeBranch = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("AB", value, OlUserPropertyType.olYesNo);
                    }
                }
            }
        }

        public bool EC2
        {
            get
            {
                if (_olObject.UdfExists("EC2"))
                {
                    _EC2 = (bool)_olObject.GetUdfValue("EC2");

                    if (_EC2 == true)
                    {
                        if (ExpandChildren == "+")
                        {
                            ExpandChildren = "-";
                        }
                    }
                    else if (ExpandChildren == "-")
                    {
                        ExpandChildren = "+";
                    }
                }
                return _EC2;
            }
            set
            {
                _EC2 = value;
                if (!_readonly)
                    _olObject.SetUdf("EC2", value, OlUserPropertyType.olYesNo);
                _expandChildren = "";
                _expandChildrenState = "";
            }
        }

        public bool EC_Change
        {
            get
            {
                if (ExpandChildren.Length == 0)
                {
                    ExpandChildren = "-";
                }

                return (ExpandChildrenState ?? "") != (ExpandChildren ?? "");
            }
            set
            {
                if (value == false)
                {
                    ExpandChildrenState = ExpandChildren;
                }
            }
        }
        public string ExpandChildren
        {
            get
            {
                if (_expandChildren.Length != 0)
                {
                    return _expandChildren;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _expandChildren = _olObject.GetUdfString("EC");
                    return _expandChildren;
                }
            }
            set
            {
                _expandChildren = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("EC", value);
                    }
                }
            }
        }

        public string ExpandChildrenState
        {
            get
            {
                if (_expandChildrenState.Length != 0)
                {
                    return _expandChildrenState;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _expandChildrenState = _olObject.GetUdfString("EcState");
                    return _expandChildrenState;
                }
            }
            set
            {
                _expandChildrenState = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("EcState", value);
                    }
                }
            }
        }

        public void SplitID()
        {
            string strField = "";
            string strFieldValue = "";
            try
            {
                string strToDoID = ToDoID;
                int strToDoID_Len = strToDoID.Length;
                if (strToDoID_Len > 0L)
                {
                    int maxlen = Properties.Settings.Default.MaxIDLength;

                    for (int i = 2, loopTo = maxlen; i <= loopTo; i += 2)
                    {
                        strField = "ToDoIdLvl" + i / 2d;
                        strFieldValue = "00";
                        if (i <= strToDoID_Len)
                        {
                            strFieldValue = strToDoID.Substring(i - 1, 2);
                        }
                        if (!_readonly)
                            _olObject.SetUdf(strField, strFieldValue);
                    }
                }
            }
            catch (System.Exception e) 
            {
                Debug.WriteLine("Error in Split_ToDoID");
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Field Name is " + strField);
                Debug.WriteLine("Field Value is " + strFieldValue);
                Debug.WriteLine(e.StackTrace);
                Debugger.Break();
            }
        }

        public string MetaTaskLvl
        {
            get
            {
                if (_metaTaskLvl.Length != 0)
                {
                    return _metaTaskLvl;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskLvl = _olObject.GetUdfString("Meta Task Level");
                    return _metaTaskLvl;
                }
            }
            set
            {
                _metaTaskLvl = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("Meta Task Level", value);
                    }
                }
            }
        }

        public string MetaTaskSubject
        {
            get
            {
                if (_metaTaskSubject.Length != 0)
                {
                    return _metaTaskSubject;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskSubject = _olObject.GetUdfString("Meta Task Subject");
                    return _metaTaskSubject;
                }
            }
            set
            {
                _metaTaskSubject = value;
                if (!_readonly)
                {
                    if (_olObject is not null)
                    {
                        _olObject.SetUdf("Meta Task Subject", value);
                    }
                }
            }
        }

        public void SwapIDPrefix(object strPrefixOld, object strPrefixNew)
        {

        }

        public object GetItem()
        {
            return _olObject;
        }

        public string InFolder
        {
            get
            {
                // Dim Prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
                // Return Replace(_olObject.Parent.FolderPath, Prefix, "")
                dynamic olItem = _olObject;
                string[] ary = olItem.Parent.FolderPath.ToString().Split('\\');
                return ary[ary.Length -1];
            }
        }

        public bool get_PA_FieldExists(string PA_Schema)
        {
            try
            {
                dynamic olItem = _olObject;
                PropertyAccessor OlPA = (PropertyAccessor)olItem.PropertyAccessor;
                var OlProperty = OlPA.GetProperty(PA_Schema);
                return true;
            }
            catch
            {
                return false;
            }
        }
                
        public object GetCustomField(string fieldName, OlUserPropertyType olFieldType = OlUserPropertyType.olText)
        {
            return _olObject.GetUdfValue(fieldName, olFieldType);
        }
                
        

    }
}