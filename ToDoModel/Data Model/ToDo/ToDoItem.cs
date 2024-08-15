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
        #region Constructors

        private ToDoItem() { }

        public ToDoItem(OutlookItem outlookItem)
        {
            FlaggableItem = new OutlookItemFlaggable(outlookItem);
            InitializeOutlookItem(_olItem);
            string argstrCats_All = outlookItem.Categories;
            Flags = new FlagParser(ref argstrCats_All);
            outlookItem.Categories = argstrCats_All;
            InitializeCustomFields(FlaggableItem);
        }

        public ToDoItem(OutlookItem outlookItem, bool onDemand)
        {
            FlaggableItem = new OutlookItemFlaggable(outlookItem);
            if (!onDemand)
            {
                InitializeOutlookItem(_olItem);
                string argstrCats_All = outlookItem.Categories;
                Flags = new FlagParser(ref argstrCats_All);
                outlookItem.Categories = argstrCats_All;
                InitializeCustomFields(FlaggableItem);
            }
        }

        public ToDoItem(string strID)
        {
            _toDoID = strID;
        }

        #region Obsolete Constructors
        //TODO: Simplify Implementation by Leveraging new OutlookItem Class
        [Obsolete("Use new ToDoItem(OutlookItem) instead")]
        public ToDoItem(MailItem OlMail)
        {
            FlaggableItem = new OutlookItemFlaggable(OlMail);
            this.InitializeMail(OlMail);
            string strCategories = OlMail.Categories;
            this.Flags = new FlagParser(ref strCategories);
            OlMail.Categories = strCategories;
            this.InitializeCustomFields(FlaggableItem);

        }

        [Obsolete("Use new ToDoItem(OutlookItem) instead")]
        public ToDoItem(MailItem OlMail, bool OnDemand)
        {
            FlaggableItem = new OutlookItemFlaggable(OlMail);

            if (OnDemand == false)
            {
                InitializeMail(OlMail);
                string argstrCats_All = OlMail.Categories;
                Flags = new FlagParser(ref argstrCats_All);
                OlMail.Categories = argstrCats_All;
                InitializeCustomFields(FlaggableItem);
            }
        }

        [Obsolete("Use new ToDoItem(OutlookItem) instead")]
        public ToDoItem(TaskItem OlTask)
        {
            FlaggableItem = new OutlookItemFlaggable(OlTask);
            InitializeTask(OlTask);
            string argstrCats_All = OlTask.Categories;
            Flags = new FlagParser(ref argstrCats_All);
            OlTask.Categories = argstrCats_All;
            InitializeCustomFields(FlaggableItem);

        }

        [Obsolete("Use new ToDoItem(OutlookItem) instead")]
        public ToDoItem(TaskItem OlTask, bool OnDemand)
        {
            FlaggableItem = new OutlookItemFlaggable(OlTask);

            if (OnDemand == false)
            {
                InitializeTask(OlTask);
                string argstrCats_All = OlTask.Categories;
                Flags = new FlagParser(ref argstrCats_All);
                OlTask.Categories = argstrCats_All;
                InitializeCustomFields(FlaggableItem);
            }
        }

        [Obsolete("Use new ToDoItem(OutlookItem) instead")]
        public ToDoItem(object item, bool onDemand)
        {

            FlaggableItem = new OutlookItemFlaggable(item);
            string argstrCats_All = FlaggableItem.Categories;
            Flags = new FlagParser(ref argstrCats_All);
            FlaggableItem.Categories = argstrCats_All;
            if (onDemand == false)
            {
                MessageBox.Show("Coding Error: New ToDoItem() is overloaded. Only supply the OnDemand variable if you want to load values on demand");
            }
        }

        #endregion Obsolete Constructors

        #endregion Constructors

        #region Private Variables

        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";
        private string _metaTaskSubject = "";
        private string _metaTaskLvl = "";
        private string _tagProgram = "";
        private bool? _activeBranch = null;
        private string _expandChildren = "";

        private List<IPrefix> _prefixes = new ToDoDefaults().PrefixList;
        internal List<IPrefix> Prefixes => _prefixes;

        private Func<string, string> _projectsToPrograms;
        public Func<string, string> ProjectsToPrograms { get => _projectsToPrograms; set => _projectsToPrograms = value; }

        #endregion Private Variables

        #region Initializers

        private void InitializeOutlookItem(OutlookItemFlaggable olItem)
        {
            _taskSubject = olItem.TaskSubject;
            _priority = olItem.Importance;
            _taskCreateDate = olItem.CreationTime;
            _startDate = olItem.TaskStartDate;
        }

        #region Obsolete Initializers

        [Obsolete("Use InitializeOutlookItem instead")]
        private void InitializeMail(MailItem olMail)
        {
            _taskSubject = olMail.TaskSubject.IsNullOrEmpty() ? olMail.Subject : olMail.TaskSubject;
            _priority = olMail.Importance;
            _taskCreateDate = olMail.CreationTime;
            _startDate = olMail.TaskStartDate;
            _complete = (olMail.FlagStatus == OlFlagStatus.olFlagComplete);
            _totalWork = get_PA_FieldExists(PA_TOTAL_WORK) ? (int)olMail.PropertyAccessor.GetProperty(PA_TOTAL_WORK) : 0;
        }

        [Obsolete("Use InitializeOutlookItem instead")]
        private void InitializeTask(TaskItem OlTask)
        {
            _taskSubject = OlTask.Subject;
            _priority = OlTask.Importance;
            _taskCreateDate = OlTask.CreationTime;
            _startDate = OlTask.StartDate;
            _complete = OlTask.Complete;
            _totalWork = OlTask.TotalWork;
        }

        private void InitializeCustomFields(object item)
        {
            _tagProgram = FlaggableItem.GetUdfString("TagProgram");
            _activeBranch = (bool)(FlaggableItem.GetUdfValue("AB", OlUserPropertyType.olYesNo));
            //EC2 = (bool)(FlaggableItem.GetUdfValue("EC2", OlUserPropertyType.olYesNo));
            _ec2 = (bool)(FlaggableItem.GetUdfValue("EC2", OlUserPropertyType.olYesNo));
            _expandChildren = FlaggableItem.GetUdfString("EC");
            _expandChildrenState = FlaggableItem.GetUdfString("EcState");
        }

        #endregion Obsolete Initializers

        #endregion Initializers

        #region IClonable
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ToDoItem DeepCopy()
        {
            var clone = (ToDoItem)MemberwiseClone();
            clone._flags = Flags.DeepCopy();
            return clone;
        }

        #endregion IClonable
                
        /// <summary>
        /// Saves all internal variables to the [Object]
        /// </summary>
        public void ForceSave()
        {
            // Save the current state of the read only flag
            bool tmpReadOnly_state = ReadOnly;

            // Activate saving
            ReadOnly = false;

            WriteFlagsBatch();
            ToDoID = _toDoID;
            TaskSubject = _taskSubject;
            MetaTaskSubject = _metaTaskSubject;
            MetaTaskLvl = _metaTaskLvl;
            Priority = (OlImportance)_priority;
            StartDate = (DateTime)_startDate;
            Complete = (bool)_complete;
            TotalWork = (int)_totalWork;
            ActiveBranch = _activeBranch ?? false;
            ExpandChildren = _expandChildren;
            ExpandChildrenState = _expandChildrenState;
            EC2 = _ec2;
            VisibleTreeState = (int)_visibleTreeState;
            FlaggableItem.FlagAsTask = FlagAsTask;
            FlaggableItem.Save();

            // Return read only variable to its original state
            ReadOnly = tmpReadOnly_state;
        }

        public void WriteFlagsBatch()
        {
            FlaggableItem.Categories = Flags.Combine();

            FlaggableItem.TrySetUdf("TagContext", Context.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf("TagPeople", People.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any _children
            FlaggableItem.TrySetUdf("TagProject", Projects.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf("TagProgram", Program.AsStringNoPrefix, OlUserPropertyType.olText);
            FlaggableItem.TrySetUdf("TagTopic", Topics.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf("KB", Flags.GetKb(false));
        }

        public void WriteFlagsBatch(Enums.FlagsToSet flagsToSet)
        {
            FlaggableItem.Categories = Flags.Combine();

            if (flagsToSet.HasFlag(Enums.FlagsToSet.Context))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.Context);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, Context.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            }
            if (flagsToSet.HasFlag(Enums.FlagsToSet.People))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.People);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, People.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            }
            // TODO: Assign ToDoID if project assignment changes
            // TODO: If ID exists and project reassigned, move any _children
            if (flagsToSet.HasFlag(Enums.FlagsToSet.Projects))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.Project);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, Projects.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            }
            if (flagsToSet.HasFlag(Enums.FlagsToSet.Program))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.Program);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, Program.AsStringNoPrefix, OlUserPropertyType.olText);
            }
            if (flagsToSet.HasFlag(Enums.FlagsToSet.Topics))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.Topic);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, Topics.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            }
            if (flagsToSet.HasFlag(Enums.FlagsToSet.Kbf))
            {
                var prefix = Prefixes.Find(x => x.PrefixType == PrefixTypeEnum.KB);
                FlaggableItem.SetUdf(prefix.OlUserFieldName, Flags.GetKb(false));
            }


        }

        #region Events

        public void FlagDetails_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) { WriteFlagsBatch(); }
        }

        public void People_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            WriteFlagsBatch(Enums.FlagsToSet.People);
        }

        public void Projects_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ProjectsToPrograms is not null)
            {
                var programNames = ProjectsToPrograms(Projects.AsStringNoPrefix);
                Program.AsStringNoPrefix = programNames;
            }
            WriteFlagsBatch(Enums.FlagsToSet.Projects);
        }

        public void Program_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            WriteFlagsBatch(Enums.FlagsToSet.Program);
        }

        public void Context_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            WriteFlagsBatch(Enums.FlagsToSet.Context);
        }

        public void Topics_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            WriteFlagsBatch(Enums.FlagsToSet.Topics);
        }

        public void KB_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            WriteFlagsBatch(Enums.FlagsToSet.Kbf);
        }

        #endregion Events

        #region Public Properties

        public OutlookItem OlItem => _olItem;
        private OutlookItemFlaggable _olItem;
        internal OutlookItemFlaggable FlaggableItem { get => _olItem; set => _olItem = value; }

        /// <summary>
        /// Gets and Sets a flag that when true, prevents saving changes to the underlying [object]
        /// </summary>
        /// <returns>Boolean</returns>
        public bool ReadOnly { get => _readonly; set => _readonly = value; }
        private bool _readonly = false;
        internal bool IsReadOnly() { return _readonly; }

        public FlagParser Flags
        {
            get => GetOrLoad(ref _flags, () => _flags = FlagsLoader());
            internal set
            {
                if (_flags is not null)
                {
                    _flags.CollectionChanged -= FlagDetails_Changed;
                    _flags.ProjectsChanged -= Projects_Changed;
                    _flags.PeopleChanged -= People_Changed;
                    _flags.ContextChanged -= Context_Changed;
                    _flags.TopicsChanged -= Topics_Changed;
                    _flags.KbChanged -= KB_Changed;
                }
                _flags = value;
                _flags.CollectionChanged += FlagDetails_Changed;
                _flags.ProjectsChanged += Projects_Changed;
                _flags.PeopleChanged += People_Changed;
                _flags.ContextChanged += Context_Changed;
                _flags.TopicsChanged += Topics_Changed;
                _flags.KbChanged += KB_Changed;
            }
        }
        internal FlagParser _flags;
        private FlagParser FlagsLoader()
        {
            if (FlaggableItem is null)
            {
                var callerName = new StackTrace().GetFrame(1).GetMethod().Name;
                throw new ArgumentNullException("Cannot get property " + callerName + " if both _flags AND olObject are Null");
            }
            var categories = FlaggableItem.Categories;
            var flags = new FlagParser(ref categories);

            if (FlaggableItem.Categories != categories)
            {
                //Question: Is this next line correct? Shouldn't it be FlaggableItem.Categories = flags.Combine???
                FlaggableItem.Categories = categories;
                FlaggableItem.Save();
            };
            return flags;
        }

        public bool FlagAsTask
        {
            get => (bool)GetOrLoad(ref _flagAsTask, () => FlaggableItem.FlagAsTask, FlaggableItem);
            set => SetAndSave(ref _flagAsTask, value, (x) => FlaggableItem.FlagAsTask = (bool)x);
        }
        private bool? _flagAsTask = null;

        public DateTime TaskCreateDate
        {
            get => (DateTime)GetOrLoad(ref _taskCreateDate, () => FlaggableItem.CreationTime, FlaggableItem);
            set => _taskCreateDate = value;
        }
        private DateTime? _taskCreateDate = null;

        //Convert Bullpin
        public bool Bullpin
        {
            get
            {
                return Flags.Bullpin;
            }
            set
            {
                Flags.Bullpin = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {
                        FlaggableItem.Categories = Flags.Combine();
                        FlaggableItem.Save();
                    }
                }
            }
        }

        //Convert Today Field
        public bool Today
        {
            get
            {
                return Flags.Today;
            }
            set
            {
                Flags.Today = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {

                        FlaggableItem.Categories = Flags.Combine();
                        FlaggableItem.Save();
                    }
                }
            }
        }

        public DateTime ReminderTime
        {
            get => (DateTime)GetOrLoad(ref _reminderTime, () => FlaggableItem.ReminderTime, FlaggableItem);
            set => _reminderTime = value;
        }
        private DateTime? _reminderTime = null;

        public DateTime DueDate
        {
            get => (DateTime)GetOrLoad(ref _dueDate, DateTime.Parse("1/1/4501"), () => FlaggableItem.DueDate, FlaggableItem);
            set => SetAndSave(ref _dueDate, value, (x) => FlaggableItem.DueDate = (DateTime)x);
        }
        private DateTime? _dueDate = null;

        public DateTime StartDate
        {
            get => (DateTime)GetOrLoad(ref _startDate, TaskCreateDate, () => FlaggableItem.TaskStartDate, FlaggableItem);
            set => SetAndSave(ref _dueDate, value, (x) => FlaggableItem.TaskStartDate = (DateTime)x);
        }
        private DateTime? _startDate = null;

        public OlImportance Priority
        {
            get => (OlImportance)GetOrLoad(ref _priority, OlImportance.olImportanceNormal, () => FlaggableItem.Importance, FlaggableItem);
            set => SetAndSave(ref _priority, value, (x) => FlaggableItem.Importance = (OlImportance)x);
        }
        private OlImportance? _priority = null;

        public bool Complete
        {
            get => (bool)GetOrLoad(ref _complete, () => FlaggableItem.Complete, FlaggableItem);
            set => SetAndSave(ref _complete, value, (x) => FlaggableItem.Complete = (bool)x);
        }
        private bool? _complete = null;

        public string TaskSubject
        {
            get => GetOrLoad(ref _taskSubject, () => FlaggableItem.TaskSubject, FlaggableItem);
            set => SetAndSave(ref _taskSubject, value, (x) => FlaggableItem.TaskSubject = x);
        }
        private string _taskSubject = null;

        internal string Categories
        {
            get => Load(() => FlaggableItem.Categories, FlaggableItem);
            set => SetAndSave(value, (x) => FlaggableItem.Categories = x);
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
        //        if (!ReadOnly)
        //            SaveCatsToObj("TagPeople", _flags.GetPeople(false));
        //    } 
        //}

        //TODO: Convert People Property to use FlagTranslator

        #region People

        private FlagTranslator _people;
        public FlagTranslator People
        {
            get => GetOrLoad(ref _people, () => LoadPeople(), Flags);
            //set => SetAndSave(ref _people, value, (x) => UdfCategorySetter("TagPeople", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadPeople() => new(Flags.GetPeople, Flags.SetPeople, Flags.GetPeopleList, Flags.SetPeopleList);
        async private Task LoadPeopleAsync() => await Task.Run(() => _people = LoadPeople());

        #endregion People

        #region Projects

        private FlagTranslator _projects;
        public FlagTranslator Projects
        {
            get => GetOrLoad(ref _projects, LoadProjects, Flags);
            //set => SetAndSave(ref _projects, value, (x) => UdfCategorySetter("TagProject", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadProjects() => new(Flags.GetProjects, Flags.SetProjects, Flags.GetProjectList, Flags.SetProjectList);
        async private Task LoadProjectAsync() => await Task.Run(() => _projects = LoadProjects());

        #endregion Projects

        #region Program

        private FlagTranslator _program;
        public FlagTranslator Program => GetOrLoad(ref _program, LoadProgram, Flags);
        private FlagTranslator LoadProgram() => new(Flags.GetProgram, Flags.SetProgram, Flags.GetProgramList, Flags.SetProgramList);
        async private Task LoadProgramAsync() => await Task.Run(() => _program = LoadProgram());

        #endregion Program

        #region Context

        private FlagTranslator _context;
        public FlagTranslator Context
        {
            get => GetOrLoad(ref _context, LoadContext, Flags);
            //set => SetAndSave(ref _context, value, (x) => UdfCategorySetter("TagContext", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadContext() => new(Flags.GetContext, Flags.SetContext, Flags.GetContextList, Flags.SetContextList);
        async private Task LoadContextAsync() => await Task.Run(() => _context = LoadContext());

        #endregion Context

        #region Topic

        private FlagTranslator _topic;
        public FlagTranslator Topics
        {
            get => GetOrLoad(ref _topic, LoadTopic, Flags);
            //set => SetAndSave(ref _topic, value, (x) => UdfCategorySetter("TagTopic", x.AsStringNoPrefix));
        }
        private FlagTranslator LoadTopic() => new(Flags.GetTopics, Flags.SetTopics, Flags.GetTopicList, Flags.SetTopicList);
        async private Task LoadTopicAsync() => await Task.Run(() => _topic = LoadTopic());

        #endregion Topic

        #region KB

        private FlagTranslator _kb;
        public FlagTranslator KB
        {
            get => GetOrLoad(ref _kb, LoadKb, Flags);
        }
        private FlagTranslator LoadKb() => new(Flags.GetKb, Flags.SetKb, Flags.GetKbList, Flags.SetKbList);
        async private Task LoadKbAsync() => await Task.Run(() => _kb = LoadKb());

        public string get_KB(bool IncludePrefix = false)
        {
            EnsureInitialized(callerName: "KB");
            return Flags.GetKb(IncludePrefix);
            // Set Context and sanitize value
        }
        [Obsolete("Use KB instead")]
        public string KBSimple
        {
            get
            {
                EnsureInitialized(callerName: "KB");
                return Flags.GetKb(false);
            }
            set
            {
                Flags.SetKb(value: value);
                if (!ReadOnly)
                    UdfCategorySetter("KB", Flags.GetKb(false));
            }
        }

        [Obsolete("Use KB instead")]
        public void SetKB(bool IncludePrefix = false, string value = default)
        {
            Flags.SetKb(value: value);
            if (!ReadOnly)
                UdfCategorySetter("KB", Flags.GetKb(false));
        }

        #endregion KB

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        
        private void ThrowIfNull(object obj, string property)
        {
            if (obj == null)
                throw new ArgumentNullException($"Cannot get {property}. Item is null.");
        }

        private int? _totalWork = null;
        public int TotalWork
        {
            get => (int)GetOrLoad(ref _totalWork, () => FlaggableItem.TotalWork, FlaggableItem);
            set => SetAndSave(ref _totalWork, value, (x) => FlaggableItem.TotalWork = (int)x);
        }

        private string _toDoID = null;
        public string ToDoID
        {
            get => GetOrLoad(ref _toDoID, () => FlaggableItem.GetUdfString("ToDoID"), FlaggableItem);
            set => SetAndSave(ref _toDoID, value, (x) => { FlaggableItem.TrySetUdf("ToDoID", x); SplitID(); });
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
            get => (int)GetOrLoad(ref _visibleTreeState, 63, () => FlaggableItem.GetUdfValue<int>("VTS"), (x) => VisibleTreeSetAndSaver((int)x), FlaggableItem);
            set => VisibleTreeSetAndSaver(value);
        }
        private void VisibleTreeSetAndSaver(int value)
        {
            SetAndSave(ref _visibleTreeState, value, (x) => { FlaggableItem.TrySetUdf("VTS", x, OlUserPropertyType.olInteger); SplitID(); });
        }

        public bool ActiveBranch
        {
            get
            {
                if (_activeBranch != null) { return (bool)_activeBranch; }
                else if (FlaggableItem is null) { return false; }
                else
                {
                    if (FlaggableItem.UdfExists("AB"))
                    {
                        _activeBranch = (bool)FlaggableItem.GetUdfValue("AB", OlUserPropertyType.olYesNo);
                    }
                    else
                    {
                        FlaggableItem.TrySetUdf("AB", true, OlUserPropertyType.olYesNo);
                        _activeBranch = true;
                    }

                    return (bool)_activeBranch;
                }
            }
            set
            {
                _activeBranch = value;
                if (!ReadOnly)
                {
                    FlaggableItem?.TrySetUdf("AB", value, OlUserPropertyType.olYesNo);
                }
            }
        }

        private bool _ec2;
        //internal bool EC2 { get => _ec2; set => _ec2 = value; }
        public bool EC3
        {
            //internal T GetOrLoad<T>(ref T value, T defaultValue, Func<T> loader, Action<T> defaultSetAndSaver, params object[] dependencies)
            get => GetOrLoad(value: ref _ec2, defaultValue: false, loader: () => FlaggableItem.GetUdfValue<bool>("EC2"), EC2SetAndSaver, FlaggableItem);
            set => EC2SetAndSaver(value);
        }
        private void EC2SetAndSaver(bool value)
        {
            SetAndSave(ref _ec2, value, (x) =>
            {
                if (!ReadOnly) { FlaggableItem.TrySetUdf("EC2", value, OlUserPropertyType.olYesNo); }
                var ecSymbolMeaning = ExpandChildren == "+";
                if (value != (ExpandChildren == "+"))
                {
                    ExpandChildren = _ec2 ? "+" : "-";
                }
                _expandChildrenState = "";
            });
        }

        public bool EC2
        {
            get
            {
                if (FlaggableItem.UdfExists("EC2"))
                {
                    _ec2 = (bool)FlaggableItem.GetUdfValue("EC2");

                    if (_ec2 == true)
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
                return _ec2;
            }
            set
            {
                _ec2 = value;
                if (!ReadOnly)
                    FlaggableItem.TrySetUdf("EC2", value, OlUserPropertyType.olYesNo);
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
                else if (FlaggableItem is null)
                {
                    return "";
                }
                else
                {
                    _expandChildren = FlaggableItem.GetUdfString("EC");
                    return _expandChildren;
                }
            }
            set
            {
                _expandChildren = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {
                        FlaggableItem.TrySetUdf("EC", value);
                    }
                }
            }
        }

        private string _expandChildrenState = "";
        public string ExpandChildrenState
        {
            get
            {
                if (_expandChildrenState.Length != 0)
                {
                    return _expandChildrenState;
                }
                else if (FlaggableItem is null)
                {
                    return "";
                }
                else
                {
                    _expandChildrenState = FlaggableItem.GetUdfString("EcState");
                    return _expandChildrenState;
                }
            }
            set
            {
                _expandChildrenState = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {
                        FlaggableItem.TrySetUdf("EcState", value);
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
                        if (!ReadOnly)
                            FlaggableItem.TrySetUdf(strField, strFieldValue);
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
                if (_metaTaskLvl?.Length != 0)
                {
                    return _metaTaskLvl;
                }
                else if (FlaggableItem is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskLvl = FlaggableItem.GetUdfString("Meta Task Level");
                    return _metaTaskLvl;
                }
            }
            set
            {
                _metaTaskLvl = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {
                        FlaggableItem.TrySetUdf("Meta Task Level", value);
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
                else if (FlaggableItem is null)
                {
                    return "";
                }
                else
                {
                    _metaTaskSubject = FlaggableItem.GetUdfString("Meta Task Subject");
                    return _metaTaskSubject;
                }
            }
            set
            {
                _metaTaskSubject = value;
                if (!ReadOnly)
                {
                    if (FlaggableItem is not null)
                    {
                        FlaggableItem.TrySetUdf("Meta Task Subject", value);
                    }
                }
            }
        }

        #endregion Public Properties

        public void SwapIDPrefix(object strPrefixOld, object strPrefixNew)
        {
            NotImplementedDialog.StopAtNotImplemented("SwapIDPrefix");
        }

        public object GetItem()
        {
            return FlaggableItem;
        }

        public string InFolder
        {
            get
            {
                // Dim Prefix As String = Globals.ThisAddIn._OlNS.DefaultStore.GetRootFolder.FolderPath & "\"
                // Return Replace(_olObject.Parent.FolderPath, Prefix, "")
                dynamic olItem = FlaggableItem;
                string[] ary = olItem.Parent.FolderPath.ToString().Split('\\');
                return ary[ary.Length - 1];
            }
        }

        public bool get_PA_FieldExists(string PA_Schema)
        {
            try
            {
                dynamic olItem = FlaggableItem;
                PropertyAccessor OlPA = (PropertyAccessor)olItem.PropertyAccessor;
                var OlProperty = OlPA.GetProperty(PA_Schema);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Get<T> and Set<T>

        private void EnsureInitialized(string callerName)
        {
            if (Flags is null)
            {
                if (FlaggableItem is null)
                {
                    throw new ArgumentNullException("Cannot get property " + callerName + " if both _flags AND olObject are Null");
                }
                dynamic olItem = FlaggableItem;
                string argstrCats_All = olItem.Categories;
                Flags = new FlagParser(ref argstrCats_All);
                olItem.Categories = argstrCats_All;
            }
        }

        private void UdfCategorySetter(string udfName, string udfValue)
        {
            if (FlaggableItem is not null)
            {
                FlaggableItem.TrySetUdf(udfName, udfValue, OlUserPropertyType.olKeywords);
                FlaggableItem.Categories = Flags.Combine();
                FlaggableItem.Save();
            }
        }

        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)
        {
            SetAndSave(ref variable, value, objectSetter, () => FlaggableItem.Save());
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
            if (!ReadOnly)
            {
                if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null"); }
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
            SetAndSave(value, objectSetter, () => FlaggableItem.Save());
        }

        internal void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver)
        {
            if (!ReadOnly)
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
                catch (System.Exception)
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

        #endregion Get<T> and Set<T>

    }
}