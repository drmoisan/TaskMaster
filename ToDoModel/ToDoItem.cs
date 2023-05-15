using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;


using UtilitiesCS.OutlookExtensions;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoItem : ICloneable
    {

        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";

        private readonly object _olObject;
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
        private bool? _ActiveBranch = null;
        private string _ExpandChildren = "";
        private string _ExpandChildrenState = "";
        private bool _EC2;
        private int _VisibleTreeState;
        private bool _readonly = false;
        private FlagParser _flags;
        private bool _flagAsTask = true;


        public object Clone()
        {
            var cloned_todo = new ToDoItem(_olObject, true);
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
            ActiveBranch = _ActiveBranch ?? false;
            ExpandChildren = _ExpandChildren;
            ExpandChildrenState = _ExpandChildrenState;
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

        public ToDoItem(MailItem OlMail)
        {
            _olObject = OlMail;

            InitializeMail(OlMail);
            string argstrCats_All = OlMail.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            OlMail.Categories = argstrCats_All;
            InitializeCustomFields(_olObject);

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
            _ToDoID = strID;
        }

        private void InitializeMail(MailItem OlMail)
        {
            _TaskSubject = OlMail.TaskSubject.Length != 0 ? OlMail.TaskSubject : OlMail.Subject;
            _Priority = OlMail.Importance;
            _TaskCreateDate = OlMail.CreationTime;
            _StartDate = OlMail.TaskStartDate;
            _Complete = (OlMail.FlagStatus == OlFlagStatus.olFlagComplete);
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
            _TagProgram = (string)(_olObject.GetUdf("TagProgram"));
            _ActiveBranch = (bool)(_olObject.GetUdf("AB", OlUserPropertyType.olYesNo));
            _EC2 = (bool)(_olObject.GetUdf("EC2", OlUserPropertyType.olYesNo));
            _ExpandChildren = (string)(_olObject.GetUdf("EC"));
            _ExpandChildrenState = (string)(_olObject.GetUdf("EcState"));
        }

        public void WriteFlagsBatch()
        {
            dynamic olTemp = _olObject;
            olTemp.Categories = _flags.Combine();
            olTemp.Save();
            _olObject.SetUdf("TagContext", _flags.get_Context(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("TagPeople", _flags.get_People(false),OlUserPropertyType.olKeywords);
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any children
            _olObject.SetUdf("TagProject", _flags.get_Projects(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("TagTopic", _flags.get_Topics(false), OlUserPropertyType.olKeywords);
            _olObject.SetUdf("KB", _flags.get_KB(false));
        }

        public object olItem
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
                return _flags.today;
            }
            set
            {
                _flags.today = value;
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
                    _Complete = false;
                }
                else if (_olObject is MailItem)
                {
                    MailItem OlMail = (MailItem)_olObject;
                    _Complete = OlMail.FlagStatus == OlFlagStatus.olFlagComplete;
                }
                else if (_olObject is TaskItem)
                {
                    TaskItem OlTask = (TaskItem)_olObject;
                    _Complete = OlTask.Complete;
                }
                return _Complete;
            }
            set
            {
                _Complete = value;
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
                if (_TaskSubject.Length == 0)
                {
                    if (_olObject is null)
                    {
                        _TaskSubject = "";
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        _TaskSubject = OlMail.TaskSubject;
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
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
                    if (_olObject is null)
                    {
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        OlMail.TaskSubject = _TaskSubject;
                        OlMail.Save();
                    }
                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
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
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _TagProgram = (string)(_olObject.GetUdf("TagProgram", OlUserPropertyType.olKeywords));
                    return _TagProgram;
                }

            }
            set
            {
                _TagProgram = value;
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

        public int TotalWork
        {
            get
            {
                if (_TotalWork == 0)
                {
                    if (_olObject is null)
                    {
                        _TotalWork = 0;
                    }
                    else if (_olObject is MailItem)
                    {
                        MailItem OlMail = (MailItem)_olObject;
                        _TotalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
                    }

                    else if (_olObject is TaskItem)
                    {
                        TaskItem OlTask = (TaskItem)_olObject;
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
                if (_ToDoID.Length != 0)
                {
                    return _ToDoID;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _ToDoID = (string)(_olObject.GetUdf("ToDoID"));
                    return _ToDoID;
                }
            }
            set
            {
                _ToDoID = value;
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
                        _VisibleTreeState = (int)(_olObject.GetUdf("VTS", OlUserPropertyType.olInteger));
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
                if (_ActiveBranch != null) { return (bool)_ActiveBranch; }
                else if (_olObject is null) { return false; }
                else
                {
                    if (_olObject.UdfExists("AB"))
                    {
                        _ActiveBranch = (bool)_olObject.GetUdf("AB", OlUserPropertyType.olYesNo);
                    }
                    else
                    {
                        _olObject.SetUdf("AB", true, OlUserPropertyType.olYesNo);
                        _ActiveBranch = true;
                    }

                    return (bool)_ActiveBranch;
                }
            }
            set
            {
                _ActiveBranch = value;
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
                    _EC2 = (bool)_olObject.GetUdf("EC2");

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
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _ExpandChildren = (string)(_olObject.GetUdf("EC"));
                    return _ExpandChildren;
                }
            }
            set
            {
                _ExpandChildren = value;
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
                if (_ExpandChildrenState.Length != 0)
                {
                    return _ExpandChildrenState;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _ExpandChildrenState = (string)(_olObject.GetUdf("EcState"));
                    return _ExpandChildrenState;
                }
            }
            set
            {
                _ExpandChildrenState = value;
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
                            _olObject.SetUdf(strField, strFieldValue);
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
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _MetaTaskLvl = (string)(_olObject.GetUdf("Meta Task Level"));
                    return _MetaTaskLvl;
                }
            }
            set
            {
                _MetaTaskLvl = value;
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
                if (_MetaTaskSubject.Length != 0)
                {
                    return _MetaTaskSubject;
                }
                else if (_olObject is null)
                {
                    return "";
                }
                else
                {
                    _MetaTaskSubject = (string)(_olObject.GetUdf("Meta Task Subject"));
                    return _MetaTaskSubject;
                }
            }
            set
            {
                _MetaTaskSubject = value;
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
                // Dim prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
                // Return Replace(_olObject.Parent.FolderPath, prefix, "")
                string[] ary = _olObject.Parent.FolderPath.ToString().Split('\\');
                return ary[Information.UBound(ary)];
            }
        }

        public bool get_PA_FieldExists(string PA_Schema)
        {
            try
            {
                PropertyAccessor OlPA = (PropertyAccessor)_olObject.PropertyAccessor;
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
            var value = _olObject.GetUdf(fieldName, olFieldType);
            if (value is Array) { value = FlattenArry((object[])value); }
            
            switch (olFieldType)
            {
                case OlUserPropertyType.olInteger:
                    return (object)0;
                case OlUserPropertyType.olYesNo:
                    return (object)false;
                default:
                    return (object)"";
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