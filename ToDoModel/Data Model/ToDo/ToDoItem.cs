using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoItem : ICloneable
    {
        //TODO: Convert PEOPLE, PROJECTS, CONTEXTS, and TOPICS to FlagTranslator
        //TODO: Simplify Implementation by Leveraging new OutlookItem Class
        public ToDoItem(MailItem OlMail)
        {
            _olItem = new OutlookItemFlaggable(OlMail);
            this.InitializeMail(OlMail);
            string strCategories = OlMail.Categories;
            this._flags = new FlagParser(ref strCategories);
            OlMail.Categories = strCategories;
            this.InitializeCustomFields(_olItem);

        }

        public ToDoItem(MailItem OlMail, bool OnDemand)
        {
            _olItem = new OutlookItemFlaggable(OlMail);

            if (OnDemand == false)
            {
                InitializeMail(OlMail);
                string argstrCats_All = OlMail.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlMail.Categories = argstrCats_All;
                InitializeCustomFields(_olItem);
            }
        }

        public ToDoItem(TaskItem OlTask)
        {
            _olItem = new OutlookItemFlaggable(OlTask);
            InitializeTask(OlTask);
            string argstrCats_All = OlTask.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            OlTask.Categories = argstrCats_All;
            InitializeCustomFields(_olItem);

        }

        public ToDoItem(TaskItem OlTask, bool OnDemand)
        {
            _olItem = new OutlookItemFlaggable(OlTask);

            if (OnDemand == false)
            {
                InitializeTask(OlTask);
                string argstrCats_All = OlTask.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                OlTask.Categories = argstrCats_All;
                InitializeCustomFields(_olItem);
            }
        }

        public ToDoItem(object Item, bool OnDemand)
        {

            _olItem = new OutlookItemFlaggable(Item);
            string argstrCats_All = _olItem.Categories;
            _flags = new FlagParser(ref argstrCats_All);
            _olItem.Categories = argstrCats_All;
            if (OnDemand == false)
            {
                MessageBox.Show("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand");
            }
        }

        public ToDoItem(string strID)
        {
            _toDoID = strID;
        }


        private OutlookItemFlaggable _olItem;
        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";
        
        
        private string _metaTaskSubject = "";
        private string _metaTaskLvl = "";
        private string _tagProgram = "";
        
        
        
        
        
        private bool? _activeBranch = null;
        private string _expandChildren = "";
        private string _expandChildrenState = "";
        private bool _EC2;
        
        private bool _readonly = false;

        private void InitializeOutlookItem(OutlookItemFlaggable olItem) 
        {
            _taskSubject = olItem.TaskSubject;
            _priority = olItem.Importance;
            _taskCreateDate = olItem.CreationTime;
            _startDate = olItem.TaskStartDate;
        }
        
        private void InitializeMail(MailItem OlMail)
        {
            _taskSubject = OlMail.TaskSubject.Length != 0 ? OlMail.TaskSubject : OlMail.Subject;
            _priority = OlMail.Importance;
            _taskCreateDate = OlMail.CreationTime;
            _startDate = OlMail.TaskStartDate;
            _complete = (OlMail.FlagStatus == OlFlagStatus.olFlagComplete);
            _totalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)OlMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
        }

        private void InitializeTask(TaskItem OlTask)
        {
            _taskSubject = OlTask.Subject;
            _priority = OlTask.Importance;
            _taskCreateDate = OlTask.CreationTime;
            _startDate = OlTask.StartDate;
            _complete = OlTask.Complete;
            _totalWork = OlTask.TotalWork;
        }

        private void InitializeCustomFields(object Item)
        {
            _tagProgram = _olItem.GetUdfString("TagProgram");
            _activeBranch = (bool)(_olItem.GetUdfValue("AB", OlUserPropertyType.olYesNo));
            _EC2 = (bool)(_olItem.GetUdfValue("EC2", OlUserPropertyType.olYesNo));
            _expandChildren = _olItem.GetUdfString("EC");
            _expandChildrenState = _olItem.GetUdfString("EcState");
        }

        public object Clone()
        {
            var clonedTodo = new ToDoItem(_olItem.InnerObject, true);
            clonedTodo._toDoID = _toDoID;
            clonedTodo._taskSubject = _taskSubject;
            clonedTodo._metaTaskSubject = _metaTaskSubject;
            clonedTodo._metaTaskLvl = _metaTaskLvl;
            clonedTodo._tagProgram = _tagProgram;
            clonedTodo._priority = _priority;
            clonedTodo._startDate = _startDate;
            clonedTodo._complete = _complete;
            clonedTodo._totalWork = _totalWork;
            clonedTodo._activeBranch = _activeBranch;
            clonedTodo._expandChildren = _expandChildren;
            clonedTodo._expandChildrenState = _expandChildrenState;
            clonedTodo._EC2 = _EC2;
            clonedTodo._visibleTreeState = _visibleTreeState;
            clonedTodo._readonly = _readonly;
            return clonedTodo;
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
            Priority = (OlImportance)_priority;
            StartDate = (DateTime)_startDate;
            Complete = (bool)_complete;
            TotalWork = (int)_totalWork;
            ActiveBranch = _activeBranch ?? false;
            ExpandChildren = _expandChildren;
            ExpandChildrenState = _expandChildrenState;
            EC2 = _EC2;
            VisibleTreeState = (int)_visibleTreeState;
            _olItem.FlagAsTask = FlagAsTask;
            _olItem.Save();
            
            // Return read only variable to its original state
            _readonly = tmp_readonly_state;
        }
                
        public void WriteFlagsBatch()
        {
            _olItem.Categories = _flags.Combine();
            
            _olItem.SetUdf("TagContext", _flags.GetContext(false), OlUserPropertyType.olKeywords);
            _olItem.SetUdf("TagPeople", _flags.GetPeople(false),OlUserPropertyType.olKeywords);
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any _children
            _olItem.SetUdf("TagProject", _flags.GetProjects(false), OlUserPropertyType.olKeywords);
            _olItem.SetUdf("TagTopic", _flags.GetTopics(false), OlUserPropertyType.olKeywords);
            _olItem.SetUdf("KB", _flags.GetKb(false));
        }

        public OutlookItem OlItem => _olItem;

        internal FlagParser _flags;
        public FlagParser Flags => GetOrLoad(ref _flags, ()=> _flags = FlagsLoader());
        private FlagParser FlagsLoader()
        {
            if (_olItem is null)
            {
                var callerName = new StackTrace().GetFrame(1).GetMethod().Name;
                throw new ArgumentNullException("Cannot get property " + callerName + " if both _flags AND olObject are Null");
            }
            var categories = _olItem.Categories;
            var flags = new FlagParser(ref categories);

            if (_olItem.Categories != categories)
            {
                //Question: Is this next line correct? Shouldn't it be _olItem.Categories = flags.Combine???
                _olItem.Categories = categories; 
                _olItem.Save();
            };
            return flags;
        }

        private bool? _flagAsTask = null;
        public bool FlagAsTask 
        { 
            get => (bool)GetOrLoad(ref _flagAsTask, () => _olItem.FlagAsTask, _olItem); 
            set => SetAndSave(ref _flagAsTask, value, (x) => _olItem.FlagAsTask = (bool)x); 
        }
     
        private DateTime? _taskCreateDate = null;
        public DateTime TaskCreateDate 
        { 
            get => (DateTime)GetOrLoad(ref _taskCreateDate, () => _olItem.CreationTime, _olItem); 
            set => _taskCreateDate = value; 
        }
        
        //Convert Bullpin
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
                    if (_olItem is not null)
                    {
                        _olItem.Categories = _flags.Combine();
                        _olItem.Save();
                    }
                }
            }
        }

        //Convert Today Field
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
                    if (_olItem is not null)
                    {
                        
                        _olItem.Categories = _flags.Combine();
                        _olItem.Save();
                    }
                }
            }
        }

        private DateTime? _reminderTime = null;
        public DateTime ReminderTime 
        { 
            get => (DateTime)GetOrLoad(ref _reminderTime, () => _olItem.ReminderTime, _olItem); 
            set => _reminderTime = value; 
        }

        private DateTime? _dueDate = null;
        public DateTime DueDate 
        { 
            get => (DateTime)GetOrLoad(ref _dueDate, DateTime.Parse("1/1/4501"), () => _olItem.DueDate, _olItem); 
            set => SetAndSave(ref _dueDate, value, (x) => _olItem.DueDate = (DateTime)x); 
        }

        private DateTime? _startDate = null;
        public DateTime StartDate 
        { 
            get => (DateTime)GetOrLoad(ref _startDate, TaskCreateDate, () => _olItem.TaskStartDate, _olItem); 
            set => SetAndSave(ref _dueDate, value, (x) => _olItem.TaskStartDate = (DateTime)x); 
        }

        private OlImportance? _priority = null;
        public OlImportance Priority 
        { 
            get => (OlImportance)GetOrLoad(ref _priority, OlImportance.olImportanceNormal, () => _olItem.Importance, _olItem); 
            set => SetAndSave(ref _priority, value, (x) => _olItem.Importance = (OlImportance)x); 
        }

        private bool? _complete = null;
        public bool Complete
        {
            get => (bool)GetOrLoad(ref _complete, () => _olItem.Complete, _olItem);
            set => SetAndSave(ref _complete, value, (x) => _olItem.Complete = (bool)x);
        }

        private string _taskSubject = null;
        public string TaskSubject
        {
            get => GetOrLoad(ref _taskSubject, () => _olItem.TaskSubject, _olItem);
            set => SetAndSave(ref _taskSubject, value, (x) => _olItem.TaskSubject = x);
        }
        

        internal string Categories
        {
            get => Load(() => _olItem.Categories, _olItem);
            set => SetAndSave(value, (x) => _olItem.Categories = x);
        }

        //public string get_People(bool IncludePrefix = false)
        //{
        //    EnsureInitialized(CallerName: "People");
        //    return _flags.get_People(IncludePrefix);
        //    // Set People and sanitize value
        //}

        //public string People 
        //{
        //    get 
        //    {
        //        EnsureInitialized(CallerName: "People");
        //        return _flags.GetPeople(false);
        //    }
        //    set 
        //    {
        //        _flags.SetPeople(value: value);
        //        if (!_readonly)
        //            SaveCatsToObj("TagPeople", _flags.GetPeople(false));
        //    } 
        //}

        //TODO: Convert People Property to use FlagTranslator
        
        private FlagTranslator _people; 
        public FlagTranslator People 
        { 
            get => GetOrLoad(ref _people, () => LoadPeople(), Flags); 
            set => SetAndSave(ref _people, value, (x) => UdfCategorySetter("TagPeople", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadPeople() => new(_flags.GetPeople, _flags.SetPeople, _flags.GetPeopleList, _flags.SetPeopleList);
        async private Task LoadPeopleAsync() => await Task.Run(() => _people = LoadPeople());

        private FlagTranslator _projects;
        public FlagTranslator Projects
        {
            get => GetOrLoad(ref _projects, LoadProjects, Flags);
            set => SetAndSave(ref _projects, value, (x) => UdfCategorySetter("TagProject", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadProjects() => new(_flags.GetProjects, _flags.SetProjects, _flags.GetProjectList, _flags.SetProjectList);
        async private Task LoadProjectAsync() => await Task.Run(() => _projects = LoadProjects());

        public string TagProgram
        {
            get
            {
                if (_tagProgram.Length != 0)
                {
                    return _tagProgram;
                }
                else if (_olItem is null)
                {
                    return "";
                }
                else
                {
                    _tagProgram = _olItem.GetUdfString("TagProgram");
                    return _tagProgram;
                }

            }
            set
            {
                _tagProgram = value;
                if (!_readonly)
                {
                    if (_olItem is not null)
                    {
                        _olItem.SetUdf("TagProgram", value, OlUserPropertyType.olKeywords);
                    }
                }
            }
        }

        private FlagTranslator _context;
        public FlagTranslator Context
        {
            get => GetOrLoad(ref _context, LoadContext, Flags);
            set => SetAndSave(ref _context, value, (x) => UdfCategorySetter("TagContext", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadContext() => new(_flags.GetContext, _flags.SetContext, _flags.GetContextList, _flags.SetContextList);
        async private Task LoadContextAsync() => await Task.Run(() => _context = LoadContext());

        private FlagTranslator _topic;
        public FlagTranslator Topics
        {
            get => GetOrLoad(ref _topic, LoadTopic, Flags);
            set => SetAndSave(ref _topic, value, (x) => UdfCategorySetter("TagTopic", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadTopic() => new(_flags.GetTopics, _flags.SetTopics, _flags.GetTopicList, _flags.SetTopicList);
        async private Task LoadTopicAsync() => await Task.Run(() => _topic = LoadTopic());

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        //public void set_Topic(bool IncludePrefix = false, string value = default)
        //{
        //    _flags.set_Topics(value: value);
        //    if (!_readonly)
        //        SaveCatsToObj("TagTopic", _flags.get_Topics(false));
        //}

        public string get_KB(bool IncludePrefix = false)
        {
            EnsureInitialized(callerName: "KB");
            return _flags.GetKb(IncludePrefix);
            // Set Context and sanitize value
        }

        public string KB
        {
            get
            {
                EnsureInitialized(callerName: "KB");
                return _flags.GetKb(false);
            }
            set
            {
                _flags.SetKb(value: value);
                if (!_readonly)
                    UdfCategorySetter("KB", _flags.GetKb(false));
            }
        }

        public void SetKB(bool IncludePrefix = false, string value = default)
        {
            _flags.SetKb(value: value);
            if (!_readonly)
                UdfCategorySetter("KB", _flags.GetKb(false));
        }


        private void ThrowIfNull(object obj, string property)
        {
            if (obj == null)
                throw new ArgumentNullException($"Cannot get {property}. Item is null.");
        }

        private int? _totalWork = null;
        public int TotalWork
        {
            get => (int)GetOrLoad(ref _totalWork, () => _olItem.TotalWork, _olItem);
            set => SetAndSave(ref _totalWork, value, (x) => _olItem.TotalWork = (int)x);
        }

        private string _toDoID = null;
        public string ToDoID
        {
            get => GetOrLoad(ref _toDoID, () => _olItem.GetUdfString("ToDoID"), _olItem);
            set => SetAndSave(ref _toDoID, value, (x) => { _olItem.SetUdf("ToDoID", x); SplitID(); });
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

        private int? _visibleTreeState;
        public int VisibleTreeState
        {
            get => (int)GetOrLoad(ref _visibleTreeState, 63, () => _olItem.GetUdfValue<int>("VTS"), (x) => VisibleTreeSetAndSaver((int)x), _olItem);
            set => VisibleTreeSetAndSaver(value);
        }
        private void VisibleTreeSetAndSaver(int value)
        {
            SetAndSave(ref _visibleTreeState, value, (x) => { _olItem.SetUdf("VTS", x, OlUserPropertyType.olInteger); SplitID(); });
        }

        public bool ActiveBranch
        {
            get
            {
                if (_activeBranch != null) { return (bool)_activeBranch; }
                else if (_olItem is null) { return false; }
                else
                {
                    if (_olItem.UdfExists("AB"))
                    {
                        _activeBranch = (bool)_olItem.GetUdfValue("AB", OlUserPropertyType.olYesNo);
                    }
                    else
                    {
                        _olItem.SetUdf("AB", true, OlUserPropertyType.olYesNo);
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
                    _olItem?.SetUdf("AB", value, OlUserPropertyType.olYesNo);
                }
            }
        }

        public bool EC2
        {
            get
            {
                if (_olItem.UdfExists("EC2"))
                {
                    _EC2 = (bool)_olItem.GetUdfValue("EC2");

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
                    _olItem.SetUdf("EC2", value, OlUserPropertyType.olYesNo);
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
                else if (_olItem is null)
                {
                    return "";
                }
                else
                {
                    _expandChildren = _olItem.GetUdfString("EC");
                    return _expandChildren;
                }
            }
            set
            {
                _expandChildren = value;
                if (!_readonly)
                {
                    if (_olItem is not null)
                    {
                        _olItem.SetUdf("EC", value);
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
                else if (_olItem is null)
                {
                    return "";
                }
                else
                {
                    _expandChildrenState = _olItem.GetUdfString("EcState");
                    return _expandChildrenState;
                }
            }
            set
            {
                _expandChildrenState = value;
                if (!_readonly)
                {
                    if (_olItem is not null)
                    {
                        _olItem.SetUdf("EcState", value);
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
                    int maxlen = Properties.Settings.Default.MaxLengthOfID;

                    for (int i = 2, loopTo = maxlen; i <= loopTo; i += 2)
                    {
                        strField = "ToDoIdLvl" + i / 2d;
                        strFieldValue = "00";
                        if (i <= strToDoID_Len)
                        {
                            strFieldValue = strToDoID.Substring(i - 2, 2);
                        }
                        if (!_readonly)
                            _olItem.SetUdf(strField, strFieldValue);
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
                else if (_olItem is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskLvl = _olItem.GetUdfString("Meta Task Level");
                    return _metaTaskLvl;
                }
            }
            set
            {
                _metaTaskLvl = value;
                if (!_readonly)
                {
                    if (_olItem is not null)
                    {
                        _olItem.SetUdf("Meta Task Level", value);
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
                else if (_olItem is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskSubject = _olItem.GetUdfString("Meta Task Subject");
                    return _metaTaskSubject;
                }
            }
            set
            {
                _metaTaskSubject = value;
                if (!_readonly)
                {
                    if (_olItem is not null)
                    {
                        _olItem.SetUdf("Meta Task Subject", value);
                    }
                }
            }
        }

        public void SwapIDPrefix(object strPrefixOld, object strPrefixNew)
        {

        }

        public object GetItem()
        {
            return _olItem;
        }

        public string InFolder
        {
            get
            {
                // Dim Prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
                // Return Replace(_olObject.Parent.FolderPath, Prefix, "")
                dynamic olItem = _olItem;
                string[] ary = olItem.Parent.FolderPath.ToString().Split('\\');
                return ary[ary.Length -1];
            }
        }

        public bool get_PA_FieldExists(string PA_Schema)
        {
            try
            {
                dynamic olItem = _olItem;
                PropertyAccessor OlPA = (PropertyAccessor)olItem.PropertyAccessor;
                var OlProperty = OlPA.GetProperty(PA_Schema);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureInitialized(string callerName)
        {
            if (_flags is null)
            {
                if (_olItem is null)
                {
                    throw new ArgumentNullException("Cannot get property " + callerName + " if both _flags AND olObject are Null");
                }
                dynamic olItem = _olItem;
                string argstrCats_All = olItem.Categories;
                _flags = new FlagParser(ref argstrCats_All);
                olItem.Categories = argstrCats_All;
            }
        }
        
        private void UdfCategorySetter(string udfName, string udfValue)
        {
            if (_olItem is not null)
            {
                _olItem.SetUdf(udfName, udfValue, OlUserPropertyType.olKeywords);
                _olItem.Categories = _flags.Combine();
                _olItem.Save();
            }
        }

        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)
        {
            SetAndSave(ref variable, value, objectSetter, ()=>_olItem.Save());
        }
        
        /// <summary>
        /// Sets the value of a local private variable. If the item is not readonly, it also
        /// sets the value of the corresponding property in the <seealso cref="OutlookItem"/> object"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">Private variable caching the value</param>
        /// <param name="value">Value to be saved</param>
        /// <param name="objectSetter">Action that sets an object property to the value</param>
        /// <param name="objectSaver">Action to save the object</param>
        /// <exception cref="ArgumentNullException"></exception>
        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, System.Action objectSaver)
        {
            variable = value;
            if (!_readonly) 
            { 
                if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null");}
                objectSetter(value);
                if (objectSaver is not null) { objectSaver(); }
            }
        }

        /// <summary>
        /// Sets the value of an <seealso cref="OutlookItem"/> property using a delegate. 
        /// Value is not cached in a local variable in this overload. <seealso cref="OutlookItem.Save()"/> is called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to be saved</param>
        /// <param name="objectSetter">Action that sets an object property to the value</param>
        internal void SetAndSave<T>(T value, Action<T> objectSetter)
        {
            SetAndSave(value, objectSetter, ()=>_olItem.Save());
        }

        internal void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver)
        {
            if (!_readonly)
            {
                if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null"); }
                objectSetter(value);
                if (objectSaver is not null) { objectSaver(); }
            }
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
            return value;
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null) { throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} was passed as a null array"); }
            if (dependencies.Any(x => x is null)) 
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} contains a null value at position {errors}"); 
            }
            return GetOrLoad(ref value, loader);
        }

        internal T GetOrLoad<T>(ref T value, T defaultValue, Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null))
            {
                value = defaultValue;
                return value;
            }
            else 
            {
                try 
                {
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = defaultValue; }
                }
                catch(System.Exception)
                {
                    value = defaultValue;
                }

                return value;
            }
        }

        internal T GetOrLoad<T>(ref T value, T defaultValue, Func<T> loader, Action<T> defaultSetAndSaver, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null))
            {
                value = defaultValue;
                return value;
            }
            else
            {
                try
                {
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) 
                    { 
                        value = defaultValue; 
                        defaultSetAndSaver(value);
                    }
                }
                catch (System.Exception)
                {
                    value = defaultValue;
                    defaultSetAndSaver(value);
                }

                return value;
            }
        }

        internal T Load<T>(Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null) { throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} was passed as a null array"); }
            if (dependencies.Any(x => x is null))
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} contains a null value at position {errors}");
            }
            return loader();
        }

        internal T Load<T>(Func<T> loader, T defaultValue, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null)) { return defaultValue; }
            else { return loader(); }
        }

    }
}