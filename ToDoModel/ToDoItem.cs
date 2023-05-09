using System;
using System.Diagnostics;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoItem : ICloneable
    {

        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";

        private readonly object OlObject;
        private string _ToDoID = "";
        public string _TaskSubject = "";
        public string _MetaTaskSubject = "";
        public string _MetaTaskLvl = "";
        private string _TagProgram = "";
        private OlImportance _Priority;
        private DateTime _TaskCreateDate;
        private DateTime _StartDate;
        private bool _Complete;
        private int _TotalWork = 0;
        private bool _ActiveBranch = false;
        private string _ExpandChildren = "";
        private string _ExpandChildrenState = "";
        private bool _EC2;
        private int _VisibleTreeState;
        private bool _readonly = false;
        private FlagParser _flags;
        private bool _flagAsTask = true;


        public object Clone()
        {
            var cloned_todo = new ToDoItem(OlObject, true);
            cloned_todo._ToDoID = _ToDoID;
            cloned_todo._TaskSubject = _TaskSubject;
            cloned_todo._MetaTaskSubject = _MetaTaskSubject;
            cloned_todo._MetaTaskLvl = _MetaTaskLvl;
            cloned_todo._TagProgram = _TagProgram;
            cloned_todo._Priority = _Priority;
            cloned_todo._StartDate = _StartDate;
            cloned_todo._Complete = _Complete;
            cloned_todo._TotalWork = _TotalWork;
            cloned_todo._ActiveBranch = _ActiveBranch;
            cloned_todo._ExpandChildren = _ExpandChildren;
            cloned_todo._ExpandChildrenState = _ExpandChildrenState;
            cloned_todo._EC2 = _EC2;
            cloned_todo._VisibleTreeState = _VisibleTreeState;
            cloned_todo._readonly = _readonly;
            return cloned_todo;
        }

        /// <summary>
    /// Gets and Sets a flag that when true, prevents saving changes to the underlying [object]
    /// </summary>
    /// <returns>Boolean</returns>
        public bool IsReadOnly
        {
            get
            {
                return _readonly;
            }
            set
            {
                _readonly = value;
            }
        }

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

            ToDoID = _ToDoID;
            TaskSubject = _TaskSubject;
            MetaTaskSubject = _MetaTaskSubject;
            MetaTaskLvl = _MetaTaskLvl;
            TagProgram = _TagProgram;
            Priority = _Priority;
            StartDate = _StartDate;
            Complete = _Complete;
            TotalWork = _TotalWork;
            ActiveBranch = _ActiveBranch;
            ExpandChildren = _ExpandChildren;
            ExpandChildrenState = _ExpandChildrenState;
            EC2 = _EC2;
            VisibleTreeState = _VisibleTreeState;

            if (OlObject is MailItem)
            {
                MailItem OlMail = (MailItem)OlObject;
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

        public ToDoItem(MailItem OlMail)
        {
            OlObject = OlMail;

            InitializeMail(OlMail);
            string argstrCats_All = OlMail.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            OlMail.Categories = argstrCats_All;
            InitializeCustomFields(OlObject);

        }

        public ToDoItem(MailItem OlMail, bool OnDemand)
        {
            OlObject = OlMail;

            if (OnDemand == false)
            {
                InitializeMail(OlMail);
                string argstrCats_All = OlMail.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlMail.Categories = argstrCats_All;
                InitializeCustomFields(OlObject);
            }
        }

        public ToDoItem(TaskItem OlTask)
        {
            OlObject = OlTask;

            InitializeTask(OlTask);
            string argstrCats_All = OlTask.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            OlTask.Categories = argstrCats_All;
            InitializeCustomFields(OlObject);

        }

        public ToDoItem(TaskItem OlTask, bool OnDemand)
        {
            OlObject = OlTask;

            if (OnDemand == false)
            {
                InitializeTask(OlTask);
                string argstrCats_All = OlTask.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlTask.Categories = argstrCats_All;
                InitializeCustomFields(OlObject);
            }
        }

        public ToDoItem(object Item, bool OnDemand)
        {

            OlObject = Item;
            string argstrCats_All = Conversions.ToString(Item.Categories);
            _flags = new FlagParser(ref argstrCats_All);
            Item.Categories = argstrCats_All;
            if (OnDemand == false)
            {
                var unused = Interaction.MsgBox("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand");
            }
        }

        public ToDoItem(string strID)
        {
            _ToDoID = strID;
        }

        private void InitializeMail(MailItem OlMail)
        {
            _TaskSubject = OlMail.TaskSubject.Length != 0 ? OlMail.TaskSubject : OlMail.Subject;
            _Priority = OlMail.Importance;
            _TaskCreateDate = OlMail.CreationTime;
            _StartDate = OlMail.TaskStartDate;
            _Complete = Conversions.ToBoolean(OlMail.FlagStatus);
            _TotalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
        }

        private void InitializeTask(TaskItem OlTask)
        {
            _TaskSubject = OlTask.Subject;
            _Priority = OlTask.Importance;
            _TaskCreateDate = OlTask.CreationTime;
            _StartDate = OlTask.StartDate;
            _Complete = OlTask.Complete;
            _TotalWork = OlTask.TotalWork;
        }

        private void InitializeCustomFields(object Item)
        {
            _TagProgram = Conversions.ToString(get_CustomField("TagProgram"));
            _ActiveBranch = Conversions.ToBoolean(get_CustomField("AB", OlUserPropertyType.olYesNo));
            _EC2 = Conversions.ToBoolean(get_CustomField("EC2", OlUserPropertyType.olYesNo));
            _ExpandChildren = Conversions.ToString(get_CustomField("EC"));
            _ExpandChildrenState = Conversions.ToString(get_CustomField("EcState"));
        }

        public void WriteFlagsBatch()
        {
            OlObject.Categories = _flags.Combine();
            var unused = OlObject.Save();
            this.set_CustomField("TagContext", OlUserPropertyType.olKeywords, _flags.get_Context(false));
            this.set_CustomField("TagPeople", OlUserPropertyType.olKeywords, _flags.get_People(false));
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any children
            this.set_CustomField("TagProject", OlUserPropertyType.olKeywords, _flags.get_Projects(false));
            this.set_CustomField("TagTopic", OlUserPropertyType.olKeywords, _flags.get_Topics(false));
            this.set_CustomField("KB", value: _flags.get_KB(false));
        }

        public object object_item
        {
            get
            {
                return OlObject;
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
                if (OlObject is not null)
                {
                    if (OlObject is MailItem)
                    {
                        _flagAsTask = value;
                        if (!_readonly)
                        {
                            MailItem OlMail = (MailItem)OlObject;
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
                    else if (OlObject is TaskItem)
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
                TaskCreateDateRet = _TaskCreateDate;
                return TaskCreateDateRet;
            }
        }

        public bool Bullpin
        {
            get
            {
                return _flags.bullpin;
            }
            set
            {
                _flags.bullpin = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        OlObject.Categories = _flags.Combine();
                        var unused = OlObject.Save;
                    }
                }
            }
        }

        public bool Today
        {
            get
            {
                return _flags.today;
            }
            set
            {
                _flags.today = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        OlObject.Categories = _flags.Combine();
                        var unused = OlObject.Save;
                    }
                }
            }
        }

        public DateTime ReminderTime
        {
            get
            {
                return Conversions.ToDate(OlObject.ReminderTime);
            }
            set
            {
                if (!_readonly)
                {
                    OlObject.ReminderTime = (object)value;
                    var unused = OlObject.Save();
                }
            }
        }

        public DateTime DueDate
        {
            get
            {
                if (OlObject is MailItem)
                {
                    MailItem OlMail = (MailItem)OlObject;
                    return OlMail.TaskDueDate;
                }
                else if (OlObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)OlObject;
                    return OlTask.DueDate;
                }
                else
                {
                    return DateAndTime.DateValue("1/1/4501");
                }
            }
            set
            {
                if (!_readonly)
                {
                    if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        OlMail.TaskDueDate = value;
                        OlMail.Save();
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
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
                return _TaskCreateDate;
            }
            set
            {
                _TaskCreateDate = value;
            }
        }

        public OlImportance Priority
        {
            get
            {

                if (OlObject is null)
                {
                    _Priority = OlImportance.olImportanceNormal;
                }
                else if (OlObject is MailItem)
                {
                    MailItem OlMail = (MailItem)OlObject;
                    _Priority = OlMail.Importance;
                }
                else if (OlObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)OlObject;
                    _Priority = OlTask.Importance;
                }
                return _Priority;
            }
            set
            {
                _Priority = value;
                if (!_readonly)
                {
                    if (OlObject is null)
                    {
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        OlMail.Importance = _Priority;
                        OlMail.Save();
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
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
                if (OlObject is null)
                {
                    _Complete = false;
                }
                else if (OlObject is MailItem)
                {
                    MailItem OlMail = (MailItem)OlObject;
                    _Complete = OlMail.FlagStatus == OlFlagStatus.olFlagComplete;
                }
                else if (OlObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)OlObject;
                    _Complete = OlTask.Complete;
                }
                return _Complete;
            }
            set
            {
                _Complete = value;
                if (!_readonly)
                {
                    if (OlObject is null)
                    {
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        OlMail.FlagStatus = value == true ? OlFlagStatus.olFlagComplete : OlFlagStatus.olFlagMarked;
                        OlMail.Save();
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
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
                if (_TaskSubject.Length == 0)
                {
                    if (OlObject is null)
                    {
                        _TaskSubject = "";
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        _TaskSubject = OlMail.TaskSubject;
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
                        _TaskSubject = OlTask.Subject;
                    }
                    else
                    {
                        _TaskSubject = "";
                    }
                }
                return _TaskSubject;
            }
            set
            {
                _TaskSubject = value;
                if (!_readonly)
                {
                    if (OlObject is null)
                    {
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        OlMail.TaskSubject = _TaskSubject;
                        OlMail.Save();
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
                        OlTask.Subject = _TaskSubject;
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

        public void set_People(bool IncludePrefix = false, string value = default)
        {
            _flags.set_People(value: value);
            if (!_readonly)
                SaveCatsToObj("TagPeople", _flags.get_People(false));
        }



        public string get_Project(bool IncludePrefix = false)
        {
            EnsureInitialized(CallerName: "Project");
            return _flags.get_Projects(IncludePrefix);
            // Set Projects and sanitize value
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any children 
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
                if (_TagProgram.Length != 0)
                {
                    return _TagProgram;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _TagProgram = Conversions.ToString(get_CustomField("TagProgram", OlUserPropertyType.olKeywords));
                    return _TagProgram;
                }

            }
            set
            {
                _TagProgram = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("TagProgram", OlUserPropertyType.olKeywords, value);
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

        public void set_KB(bool IncludePrefix = false, string value = default)
        {
            _flags.set_KB(value: value);
            if (!_readonly)
                SaveCatsToObj("KB", _flags.get_KB(false));
        }

        private void SaveCatsToObj(string FieldName, string FieldValue)
        {
            if (OlObject is not null)
            {
                this.set_CustomField(FieldName, OlUserPropertyType.olKeywords, FieldValue);
                OlObject.Categories = _flags.Combine();
                var unused = OlObject.Save;
            }
        }

        private void EnsureInitialized(string CallerName)
        {
            if (_flags is null)
            {
                if (OlObject is null)
                    throw new ArgumentNullException("Cannot get property " + CallerName + " if both _flags AND olObject are Null");
                string argstrCats_All = Conversions.ToString(OlObject.Categories);
                _flags = new FlagParser(ref argstrCats_All);
                OlObject.Categories = argstrCats_All;
            }
        }

        public int TotalWork
        {
            get
            {
                if (_TotalWork == 0)
                {
                    if (OlObject is null)
                    {
                        _TotalWork = 0;
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        _TotalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
                    }

                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
                        _TotalWork = OlTask.TotalWork;
                    }

                    else
                    {
                        _TotalWork = 0;
                    }
                }
                return _TotalWork;

            }

            set
            {
                _TotalWork = value;
                if (!_readonly)
                {
                    if (OlObject is null)
                    {
                    }
                    else if (OlObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)OlObject;
                        OlMail.PropertyAccessor.SetProperty(PA_TOTAL_WORK, value);
                        OlMail.Save();
                    }
                    else if (OlObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)OlObject;
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
                if (_ToDoID.Length != 0)
                {
                    return _ToDoID;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _ToDoID = Conversions.ToString(get_CustomField("ToDoID"));
                    return _ToDoID;
                }
            }
            set
            {
                _ToDoID = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("ToDoID", value: value);
                        SplitID();
                    }
                }
            }
        }
        // _VisibleTreeState
        public bool get_VisibleTreeStateLVL(int Lvl)
        {
            return Conversions.ToDouble(Math.Pow(2d, Lvl - 1).ToString() + VisibleTreeState) > 0d;
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
                else if (OlObject is null)
                {
                    return -1;
                }
                else
                {
                    UserProperty objProperty = (UserProperty)OlObject.UserProperties.Find("VTS");
                    if (objProperty is null)
                    {
                        this.set_CustomField("VTS", OlUserPropertyType.olInteger, (object)63); // Binary 111111 for 6 levels
                        _VisibleTreeState = 63;
                    }
                    else
                    {
                        _VisibleTreeState = Conversions.ToInteger(get_CustomField("VTS", OlUserPropertyType.olInteger));
                    }
                    return _VisibleTreeState;

                }
            }
            set
            {
                if (OlObject is not null)
                {
                    _VisibleTreeState = value;
                    if (!_readonly)
                        this.set_CustomField("VTS", OlUserPropertyType.olInteger, (object)value);
                }
            }
        }

        public bool ActiveBranch
        {
            get
            {
                if (_ActiveBranch == true)
                {
                    return true;
                }
                else if (OlObject is null)
                {
                    return false;
                }
                else
                {
                    if (get_CustomFieldExists("AB"))
                    {
                        _ActiveBranch = Conversions.ToBoolean(get_CustomField("AB", OlUserPropertyType.olYesNo));
                    }
                    else
                    {
                        this.set_CustomField("AB", OlUserPropertyType.olYesNo, (object)true);
                        _ActiveBranch = true;
                    }

                    return _ActiveBranch;
                }
            }
            set
            {
                _ActiveBranch = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("AB", OlUserPropertyType.olYesNo, (object)value);
                    }
                }
            }
        }

        public bool EC2
        {
            get
            {
                if (get_CustomFieldExists("EC2"))
                {
                    _EC2 = Conversions.ToBoolean(get_CustomField("EC2"));

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
                    this.set_CustomField("EC2", OlUserPropertyType.olYesNo, (object)value);
                _ExpandChildren = "";
                _ExpandChildrenState = "";
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
                if (_ExpandChildren.Length != 0)
                {
                    return _ExpandChildren;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _ExpandChildren = Conversions.ToString(get_CustomField("EC"));
                    return _ExpandChildren;
                }
            }
            set
            {
                _ExpandChildren = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("EC", value: value);
                    }
                }
            }
        }

        public string ExpandChildrenState
        {
            get
            {
                if (_ExpandChildrenState.Length != 0)
                {
                    return _ExpandChildrenState;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _ExpandChildrenState = Conversions.ToString(get_CustomField("EcState"));
                    return _ExpandChildrenState;
                }
            }
            set
            {
                _ExpandChildrenState = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("EcState", value: value);
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
                long strToDoID_Len = strToDoID.Length;
                if (strToDoID_Len > 0L)
                {
                    long maxlen = My.MySettingsProperty.Settings.MaxIDLength;

                    for (long i = 2L, loopTo = maxlen; i <= loopTo; i += 2L)
                    {
                        strField = "ToDoIdLvl" + i / 2d;
                        strFieldValue = "00";
                        if (i <= strToDoID_Len)
                        {
                            strFieldValue = Strings.Mid(strToDoID, (int)(i - 1L), 2);
                        }
                        if (!_readonly)
                            this.set_CustomField(strField, value: strFieldValue);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error in Split_ToDoID");
                Debug.WriteLine(Information.Err().Description);
                Debug.WriteLine("Field Name is " + strField);
                Debug.WriteLine("Field Value is " + strFieldValue);
                Debugger.Break();
            }
        }

        public string MetaTaskLvl
        {
            get
            {
                if (_MetaTaskLvl.Length != 0)
                {
                    return _MetaTaskLvl;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _MetaTaskLvl = Conversions.ToString(get_CustomField("Meta Task Level"));
                    return _MetaTaskLvl;
                }
            }
            set
            {
                _MetaTaskLvl = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        this.set_CustomField("Meta Task Level", value: value);
                    }
                }
            }
        }

        public string MetaTaskSubject
        {
            get
            {
                if (_MetaTaskSubject.Length != 0)
                {
                    return _MetaTaskSubject;
                }
                else if (OlObject is null)
                {
                    return "";
                }
                else
                {
                    _MetaTaskSubject = Conversions.ToString(get_CustomField("Meta Task Subject"));
                    return _MetaTaskSubject;
                }
            }
            set
            {
                _MetaTaskSubject = value;
                if (!_readonly)
                {
                    if (OlObject is not null)
                    {
                        // SetUdf("Meta Task Subject", strID, SpecificItem:=OlObject)
                        this.set_CustomField("Meta Task Subject", value: value);
                    }
                }
            }
        }

        public void SwapIDPrefix(object strPrefixOld, object strPrefixNew)
        {

        }

        public object GetItem()
        {
            return OlObject;
        }

        public string InFolder
        {
            get
            {
                // Dim prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
                // Return Replace(OlObject.Parent.FolderPath, prefix, "")
                string[] ary = OlObject.Parent.FolderPath.ToString().Split('\\');
                return ary[Information.UBound(ary)];
            }
        }

        public bool get_PA_FieldExists(string PA_Schema)
        {
            try
            {
                PropertyAccessor OlPA = (PropertyAccessor)OlObject.PropertyAccessor;
                var OlProperty = OlPA.GetProperty(PA_Schema);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool get_CustomFieldExists(string FieldName)
        {
            UserProperty objProperty = (UserProperty)OlObject.UserProperties.Find(FieldName);
            return objProperty is not null;
        }
        public void get_CustomField(string FieldName, OlUserPropertyType OlFieldType = OlUserPropertyType.olText)
        {
            UserProperty objProperty = (UserProperty)OlObject.UserProperties.Find(FieldName);
            if (objProperty is null)
            {
                if (OlFieldType == OlUserPropertyType.olInteger)
                {
                    return (object)0;
                }
                else if (OlFieldType == OlUserPropertyType.olYesNo)
                {
                    return (object)false;
                }
                else
                {
                    return "";
                }
            }

            else
            {
                return objProperty is Array ? FlattenArry((object[])objProperty) : objProperty;
            }

        }

        public void set_CustomField(string FieldName, OlUserPropertyType OlFieldType = OlUserPropertyType.olText, void value = default)
        {
            UserProperty objProperty = (UserProperty)OlObject.UserProperties.Find(FieldName);
            if (objProperty is null)
            {
                try
                {
                    objProperty = (UserProperty)OlObject.UserProperties.Add(FieldName, OlFieldType);
                    objProperty = value;
                    var unused1 = OlObject.Save();
                }
                catch (System.Exception e)
                {
                    Debug.WriteLine("Exception in Set User Property: " + FieldName);
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.Source);
                    Debug.WriteLine(e.StackTrace);
                }
            }
            else
            {
                objProperty = value;
                var unused = OlObject.Save();
            }
        }

        private string FlattenArry(object[] varBranch)
        {
            string FlattenArryRet = default;
            int i;
            string strTemp;

            strTemp = "";

            var loopTo = Information.UBound(varBranch);
            for (i = 0; i <= loopTo; i++)
                strTemp = varBranch[i] is Array ? strTemp + ", " + FlattenArry((object[])varBranch[i]) : (string)Operators.ConcatenateObject(strTemp + ", ", varBranch[i]);
            if (strTemp.Length != 0)
                strTemp = Strings.Right(strTemp, Strings.Len(strTemp) - 2);
            FlattenArryRet = strTemp;
            return FlattenArryRet;
        }

    }
}