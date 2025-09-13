using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic.ApplicationServices;
using ToDoModel.Data_Model.ToDo;
using UtilitiesCS;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;

[assembly: InternalsVisibleTo("ToDoModel.Test")]
namespace ToDoModel
{
    [Serializable()]
    public class ToDoItem : ICloneable, IToDoItem
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        private ToDoItem() { }

        public ToDoItem(IOutlookItem outlookItem): this(new OutlookItemFlaggable(outlookItem)) 
        {            
            //FlaggableItem = new OutlookItemFlaggable(outlookItem);
            //Loader = new ToDoLoader(() => FlaggableItem.Save(), IsReadOnly);
            //InitializeOutlookItem(_flaggableItem);
            //string argstrCats_All = outlookItem.Categories;
            //Flags = new FlagParser(ref argstrCats_All);
            //outlookItem.Categories = argstrCats_All;
            //InitializeCustomFields(FlaggableItem);
        }

        public ToDoItem(IOutlookItemFlaggable flaggableItem)
        {
            FlaggableItem = flaggableItem;
            Loader = new ToDoLoader(() => FlaggableItem.Save(), IsReadOnly);
            InitializeOutlookItem(FlaggableItem);
            string argstrCats_All = FlaggableItem.Categories;
            Flags = new FlagParser(ref argstrCats_All);
            
            if (!Flags.AreEquivalentTo(FlaggableItem.Categories))
            {
                FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;
                FlaggableItem.Save();
            }
            InitializeCustomFields(FlaggableItem);
        }

        public ToDoItem(IOutlookItem outlookItem, bool onDemand)
        {
            FlaggableItem = new OutlookItemFlaggable(outlookItem);
            Loader = new ToDoLoader(() => FlaggableItem.Save(), IsReadOnly);
            if (!onDemand)
            {
                InitializeOutlookItem(FlaggableItem);
                string argstrCats_All = outlookItem.Categories;
                Flags = new FlagParser(ref argstrCats_All);
                if (!Flags.AreEquivalentTo(FlaggableItem.Categories))
                {
                    FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;
                    FlaggableItem.Save();
                }
                InitializeCustomFields(FlaggableItem);
            }
        }

        public ToDoItem(string strID)
        {
            _toDoID = strID;
            Loader = new ToDoLoader(() => FlaggableItem.Save(), IsReadOnly);
        }
                
        #endregion Constructors

        #region Private Variables

        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";
        private string _metaTaskSubject = "";
        private string _metaTaskLvl = "";
        //private string _tagProgram = "";
        private bool? _activeBranch = null;
        private string _expandChildren = "";
        
        #endregion Private Variables

        #region Initializers

        private void InitializeOutlookItem(IOutlookItemFlaggable olItem)
        {
            _taskSubject = olItem.TaskSubject;
            _priority = olItem.Importance;
            _taskCreateDate = olItem.CreationTime;
            _startDate = olItem.TaskStartDate;
        }

        private void InitializeCustomFields(object item)
        {
            //_tagProgram = FlaggableItem.GetUdfString("TagProgram");
            _activeBranch = (bool)(FlaggableItem.GetUdfValue("AB", OlUserPropertyType.olYesNo));
            _ec2 = (bool)(FlaggableItem.GetUdfValue("EC2", OlUserPropertyType.olYesNo));
            _expandChildren = FlaggableItem.GetUdfString("EC");
            _expandChildrenState = FlaggableItem.GetUdfString("EcState");
        }

        #endregion Initializers

        #region IClonable / Serialization

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ToDoItem DeepCopy()
        {
            UnWireFlagParser();
            var clone = (ToDoItem)MemberwiseClone();
            clone._flags = _flags.DeepCopy();
            clone.WireFlagParser();
            clone.ReloadFlagTranslators();
            WireFlagParser();
            clone.Identifier = Identifier == "not set" ? "Clone" : "Clone of " + Identifier;
            return clone;
        }

        public string Identifier
        {
            get => _identifier;
            set
            {
                _identifier = value;
                _flags.Identifier = value;
                GetFlagTranslators().ForEach(x => x.Identifier = value);
            }
        }
        private string _identifier = "not set";

        /// <summary>
        /// Saves all internal variables to the [Object]
        /// </summary>
        public async Task ForceSave()
        {
            ToDoEvents.Editing.AddOrUpdate(OlItem.EntryID, 1, (key, existing) => existing + 1);

            // Save the current state of the read only flag
            bool tmpReadOnly_state = ReadOnly;

            // Activate saving
            ReadOnly = false;

            await WriteFlagsBatch();
            ToDoID = _toDoID;
            TaskSubject = _taskSubject;
            MetaTaskSubject = _metaTaskSubject;
            MetaTaskLvl = _metaTaskLvl;
            Priority = (OlImportance)_priority;
            StartDate = (DateTime)_startDate;
            Complete = _complete ?? false;
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

            ToDoEvents.Editing.UpdateOrRemove(OlItem.EntryID, (key, existing) => existing == 1, (key, existing) => existing - 1, out _);
        }

        public async Task WriteFlagsBatch()
        {
            ToDoEvents.Editing.AddOrUpdate(OlItem.EntryID, 1, (key, existing) => existing + 1);

            var ro = ReadOnly;
            ReadOnly = false;

            FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;

            FlaggableItem.TrySetUdf(GetUdfName(PrefixTypeEnum.Context), Context.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf(GetUdfName(PrefixTypeEnum.People), People.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf(GetUdfName(PrefixTypeEnum.Topic), Topics.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
            FlaggableItem.TrySetUdf(GetUdfName(PrefixTypeEnum.KB), Flags.GetKb(false));
            FlaggableItem.TrySetUdf(GetUdfName(PrefixTypeEnum.Program), Program.AsStringNoPrefix, OlUserPropertyType.olText);

            var projField = GetUdfName(PrefixTypeEnum.Project);
            if (FlaggableItem.GetUdfValue(projField) as string != Projects.AsStringNoPrefix)
            {
                FlaggableItem.TrySetUdf(projField, Projects.AsListNoPrefix.ToArray(), OlUserPropertyType.olKeywords);
                if (IdAutoCoding) { await Task.Run(AutoCodeIdAsync); }
            }

            ReadOnly = ro;
            ToDoEvents.Editing.UpdateOrRemove(OlItem.EntryID, (key, existing) => existing == 1, (key, existing) => existing - 1, out _);
        }

        private string GetUdfName(PrefixTypeEnum type) => Prefixes.Find(x => x.PrefixType == type).OlUserFieldName;

        public async Task WriteFlagsBatchAsync(Enums.FlagsToSet flagsToSet)
        {
            await Task.Run(() => WriteFlagsBatch(flagsToSet));
        }

        public void WriteFlagsBatch(Enums.FlagsToSet flagsToSet)
        {
            ToDoEvents.Editing.AddOrUpdate(OlItem.EntryID, 1, (key, existing) => existing + 1);
            
            if (flagsToSet.HasAnyFlags([Enums.FlagsToSet.Context, Enums.FlagsToSet.People, Enums.FlagsToSet.Projects, 
                Enums.FlagsToSet.Topics, Enums.FlagsToSet.Kbf, Enums.FlagsToSet.Today, Enums.FlagsToSet.Bullpin]))
            {
                FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;
            }

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

            ToDoEvents.Editing.UpdateOrRemove(OlItem.EntryID, (key, existing) => existing == 1, (key, existing) => existing - 1, out _);
        }

        #endregion IClonable / Serialization

        #region Events

        public async void FlagDetails_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (!ReadOnly)
                    await WriteFlagsBatch();
            }
        }

        public void People_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReadOnly)
                WriteFlagsBatch(Enums.FlagsToSet.People);
        }

        public async void Projects_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ProjectsToPrograms is not null)
            {
                var programNames = ProjectsToPrograms(Projects.AsStringNoPrefix);
                Program.AsStringNoPrefix = programNames;
            }
            if (!ReadOnly)
            {
                if (IdAutoCoding) { await Task.Run(AutoCodeIdAsync); }
                WriteFlagsBatch(Enums.FlagsToSet.Projects);
            }
        }

        public void Program_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReadOnly)
                WriteFlagsBatch(Enums.FlagsToSet.Program);
        }

        public void Context_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReadOnly)
                WriteFlagsBatch(Enums.FlagsToSet.Context);
        }

        public void Topics_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReadOnly)
                WriteFlagsBatch(Enums.FlagsToSet.Topics);
        }

        public void KB_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReadOnly)
                WriteFlagsBatch(Enums.FlagsToSet.Kbf);
        }

        #endregion Events

        #region Public Properties

        internal List<IPrefix> Prefixes { get; } = new ToDoDefaults().PrefixList;

        public Func<string, string> ProjectsToPrograms { get; set; }
        
        public IProjectData ProjectData { get; set; }

        internal ToDoLoader Loader { get; private set; }

        public bool IdAutoCoding { get; set; }

        public IOutlookItem OlItem => FlaggableItem;
        internal IOutlookItemFlaggable FlaggableItem { get; set; }

        public IIDList IdList { get; set; }

        /// <summary>
        /// Gets and Sets a flag that when true, prevents saving changes to the underlying [object]
        /// </summary>
        /// <returns>Boolean</returns>
        public bool ReadOnly { get; set; } = false;       
        internal bool IsReadOnly() { return ReadOnly; }
                
        public bool FlagAsTask
        {
            get => (bool)Loader.GetOrLoad(ref _flagAsTask, () => FlaggableItem.Try().FlagAsTask, FlaggableItem);
            set => Loader.SetAndSave(ref _flagAsTask, value, (x) => FlaggableItem.Try().FlagAsTask = (bool)x);
        }
        private bool? _flagAsTask = null;

        public DateTime TaskCreateDate
        {
            get => (DateTime)Loader.GetOrLoad(ref _taskCreateDate, () => FlaggableItem.Try().CreationTime, FlaggableItem);
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
                if (!ReadOnly && FlaggableItem is not null)
                {
                    FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;
                    FlaggableItem.Save();                    
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
                if (!ReadOnly && FlaggableItem is not null)
                {
                    FlaggableItem.Categories = Flags.Combined.AsStringWithPrefix;
                    FlaggableItem.Save();                    
                }
            }
        }

        public DateTime ReminderTime
        {
            get => (DateTime)Loader.GetOrLoad(ref _reminderTime, () => FlaggableItem.Try().ReminderTime, FlaggableItem);
            set => _reminderTime = value;
        }
        private DateTime? _reminderTime = null;

        public DateTime DueDate
        {
            get => (DateTime)Loader.GetOrLoad(ref _dueDate, DateTime.Parse("1/1/4501"), () => FlaggableItem.Try().DueDate, FlaggableItem);
            set => Loader.SetAndSave(ref _dueDate, value, (x) => FlaggableItem.DueDate = (DateTime)x);
        }
        private DateTime? _dueDate = null;

        public DateTime StartDate
        {
            get => (DateTime)Loader.GetOrLoad(ref _startDate, TaskCreateDate, () => FlaggableItem.Try().TaskStartDate, FlaggableItem);
            set => Loader.SetAndSave(ref _dueDate, value, (x) => FlaggableItem.TaskStartDate = (DateTime)x);
        }
        private DateTime? _startDate = null;

        public OlImportance Priority
        {
            get => (OlImportance)Loader.GetOrLoad(ref _priority, OlImportance.olImportanceNormal, () => FlaggableItem.Try().Importance, FlaggableItem);
            set => Loader.SetAndSave(ref _priority, value, (x) => FlaggableItem.Importance = (OlImportance)x);
        }
        private OlImportance? _priority = null;

        public bool Complete
        {
            get => (bool)Loader.GetOrLoad(ref _complete, () => FlaggableItem.Try().Complete, FlaggableItem);
            set => Loader.SetAndSave(ref _complete, value, (x) => FlaggableItem.Complete = (bool)x);
        }
        private bool? _complete = null;

        public string TaskSubject
        {
            get => Loader.GetOrLoad(ref _taskSubject, () => FlaggableItem.Try().TaskSubject, FlaggableItem);
            set => Loader.SetAndSave(ref _taskSubject, value, (x) => FlaggableItem.TaskSubject = x);
        }
        private string _taskSubject = null;

        internal string Categories
        {
            get => Loader.Load(() => FlaggableItem.Categories, FlaggableItem);
            set => Loader.SetAndSave(value, (x) => FlaggableItem.Categories = x);
        }

        #region Flag Parser
        public FlagParser Flags
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => Loader.GetOrLoad(ref _flags, () => _flags = FlagsLoader());
            [MethodImpl(MethodImplOptions.Synchronized)]
            internal set
            {
                if (_flags is not null) { UnWireFlagParser(); }
                _flags = value;
                WireFlagParser();
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
            }
            ;
            return flags;
        }
        private void WireFlagParser()
        {
            _flags.CollectionChanged += FlagDetails_Changed;
            _flags.ProjectsChanged += Projects_Changed;
            _flags.ProgramChanged += Program_Changed;
            _flags.PeopleChanged += People_Changed;
            _flags.ContextChanged += Context_Changed;
            _flags.TopicsChanged += Topics_Changed;
            _flags.KbChanged += KB_Changed;
        }
        private void UnWireFlagParser()
        {
            _flags.CollectionChanged -= FlagDetails_Changed;
            _flags.ProjectsChanged -= Projects_Changed;
            _flags.ProgramChanged -= Program_Changed;
            _flags.PeopleChanged -= People_Changed;
            _flags.ContextChanged -= Context_Changed;
            _flags.TopicsChanged -= Topics_Changed;
            _flags.KbChanged -= KB_Changed;
        }
        #endregion Flag Parser

        #region Flag Translators
        internal FlagTranslator[] GetFlagTranslators()
        {
            return [People, Projects, Program, Context, Topics, KB];
        }
        protected async Task ReloadFlagTranslatorsAsync()
        {
            await Task.WhenAll(
                LoadPeopleAsync(),
                LoadProjectAsync(),
                LoadProgramAsync(),
                LoadContextAsync(),
                LoadTopicAsync(),
                LoadKbAsync()
            );
        }
        protected void ReloadFlagTranslators()
        {
            _people = LoadPeople();
            _projects = LoadProjects();
            _program = LoadProgram();
            _context = LoadContext();
            _topic = LoadTopic();
            _kb = LoadKb();
        }
        #endregion Flag Translators

        #region People

        private FlagTranslator _people;
        public FlagTranslator People
        {
            get => Loader.GetOrLoad(ref _people, () => LoadPeople(), Flags);
            //set => SetAndSave(ref _people, value, (x) => UdfCategorySetter("TagPeople", x.AsStringNoPrefix));
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadPeople() => new(Flags.GetPeople, Flags.SetPeople, Flags.GetPeopleList, Flags.SetPeopleList);
        async protected Task LoadPeopleAsync() => await Task.Run(() => _people = LoadPeople());

        #endregion People

        #region Projects

        private FlagTranslator _projects;
        public FlagTranslator Projects
        {
            get => Loader.GetOrLoad(ref _projects, LoadProjects, Flags);
            //set => SetAndSave(ref _projects, value, (x) => UdfCategorySetter("TagProject", x.AsStringNoPrefix));
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadProjects() => new(Flags.GetProjects, Flags.SetProjects, Flags.GetProjectList, Flags.SetProjectList);
        async protected Task LoadProjectAsync() => await Task.Run(() => _projects = LoadProjects());

        #endregion Projects

        #region Program

        private FlagTranslator _program;
        public FlagTranslator Program => Loader.GetOrLoad(ref _program, LoadProgram, Flags);
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadProgram() => new(Flags.GetProgram, Flags.SetProgram, Flags.GetProgramList, Flags.SetProgramList);
        async protected Task LoadProgramAsync() => await Task.Run(() => _program = LoadProgram());

        #endregion Program

        #region Context

        private FlagTranslator _context;
        public FlagTranslator Context
        {
            get => Loader.GetOrLoad(ref _context, LoadContext, Flags);
            //set => SetAndSave(ref _context, value, (x) => UdfCategorySetter("TagContext", x.AsStringNoPrefix));
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadContext() => new(Flags.GetContext, Flags.SetContext, Flags.GetContextList, Flags.SetContextList);
        async protected Task LoadContextAsync() => await Task.Run(() => _context = LoadContext());

        #endregion Context

        #region Topic

        private FlagTranslator _topic;
        public FlagTranslator Topics
        {
            get => Loader.GetOrLoad(ref _topic, LoadTopic, Flags);
            //set => SetAndSave(ref _topic, value, (x) => UdfCategorySetter("TagTopic", x.AsStringNoPrefix));
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadTopic() => new(Flags.GetTopics, Flags.SetTopics, Flags.GetTopicList, Flags.SetTopicList);
        async private Task LoadTopicAsync() => await Task.Run(() => _topic = LoadTopic());

        #endregion Topic

        #region KB

        private FlagTranslator _kb;
        public FlagTranslator KB
        {
            get => Loader.GetOrLoad(ref _kb, LoadKb, Flags);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private FlagTranslator LoadKb() => new(Flags.GetKb, Flags.SetKb, Flags.GetKbList, Flags.SetKbList);
        async private Task LoadKbAsync() => await Task.Run(() => _kb = LoadKb());

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
            get => (int)Loader.GetOrLoad(ref _totalWork, () => FlaggableItem.Try().TotalWork, FlaggableItem);
            set => Loader.SetAndSave(ref _totalWork, value, (x) => FlaggableItem.TotalWork = (int)x);
        }

        private string _toDoID = null;
        public string ToDoID
        {
            get => Loader.GetOrLoad(ref _toDoID, () => FlaggableItem.Try().GetUdfString("ToDoID"), FlaggableItem);
            set => Loader.SetAndSave(ref _toDoID, value, (x) =>
            {
                if (!ReadOnly && FlaggableItem is not null)
                {
                    FlaggableItem.TrySetUdf("ToDoID", x);
                    if (!x.IsNullOrEmpty() && x.Length > 0) { SplitID(); }
                }
            });
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
            get => (int)Loader.GetOrLoad(ref _visibleTreeState, 63, () => FlaggableItem.GetUdfValue<int>("VTS"), (x) => VisibleTreeSetAndSaver((int)x), FlaggableItem);
            set => VisibleTreeSetAndSaver(value);
        }
        private void VisibleTreeSetAndSaver(int value)
        {
            Loader.SetAndSave(ref _visibleTreeState, value, (x) => { FlaggableItem.TrySetUdf("VTS", x, OlUserPropertyType.olInteger); SplitID(); });
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
            get => Loader.GetOrLoad(value: ref _ec2, defaultValue: false, loader: () => FlaggableItem.GetUdfValue<bool>("EC2"), EC2SetAndSaver, FlaggableItem);
            set => EC2SetAndSaver(value);
        }
        private void EC2SetAndSaver(bool value)
        {
            Loader.SetAndSave(ref _ec2, value, (x) =>
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
                    _ec2 = (bool)FlaggableItem.GetUdfValue("EC2", OlUserPropertyType.olYesNo);

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
                if (ExpandChildren?.Length == 0)
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
                    _expandChildren = FlaggableItem.Try().GetUdfString("EC");
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
                    _expandChildrenState = FlaggableItem.Try().GetUdfString("EcState");
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
                    _metaTaskLvl = FlaggableItem.Try().GetUdfString("Meta Task Level");
                    return _metaTaskLvl;
                }
            }
            set
            {
                _metaTaskLvl = value;
                if (!ReadOnly)
                {
                    FlaggableItem?.TrySetUdf("Meta Task Level", value);
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
                    _metaTaskSubject = FlaggableItem.Try().GetUdfString("Meta Task Subject");
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

        #region Other Methods

        internal async Task AutoCodeIdAsync()
        {
            var projects = Projects.AsListNoPrefix;
            if (projects is not null && projects.Count == 1)
            {
                var newProject = projects.First();
                var newRoot = ProjectData?.Find_ByProjectName(newProject)?.First()?.ProjectID;
                if (newRoot is not null)
                    await AutoCodeIdAsync(newRoot, newProject);
            }
        }

        // Overload might become obsolete based on revised logic
        internal async Task AutoCodeIdAsync(NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems?.Cast<string>()?.ToList();
            if (newItems is not null && newItems.Count == 1)
            {
                var newProject = newItems.First();
                var newRoot = ProjectData.Find_ByProjectName(newProject).First().ProjectID;
                await AutoCodeIdAsync(newRoot, newProject);
            }
        }

        internal async Task AutoCodeIdAsync(string newRoot, string newProject)
        {
            if (ParamArray.AnyNull(this.ProjectData, IdList)) { return; }
            if (ParamArray<string>.AnyNullOrEmpty(newProject, newRoot)) { return; }

            var oldId = ToDoID;
            switch (oldId)
            {
                case string s when s.IsNullOrEmpty():
                    // If the ToDoID is empty, we need to assign a new one and there should be no children
                    await Task.Run(() => AssignIdFromNewRoot(newRoot));
                    break;
                case string s when s.Length == 2:
                    break; // If the ToDoID is of length 2, it is a program ... no action yet
                case string s when s.Length == 4:
                    break; // If the ToDoID is of length 4, it is a project ID ... no action yet
                case string s when s.Substring(0, 4) == newRoot:
                    break; // If the ToDoID is already a child of the new ProjectId STOP processing
                default:
                    // If the ToDoID is of a length greater than 4, make the ToDo a child of the new ProjectId 
                    // and then use the newId and the oldId as the roots for changes in the children                    
                    await Task.Run(() => AssignIdFromNewRoot(newRoot));
                    await AutoCodeChildren(newProject, ToDoID, oldId);
                    break;
            }
        }

        internal void AssignIdFromNewRoot(string newRoot)
        {
            ToDoID = IdList.GetNextToDoID($"{newRoot}00");
            EC2 = true;
        }

        internal async Task AutoCodeChildren(string newProject, string newRoot, string oldRoot)
        {
            
            // Use the newId and the oldId as the roots for changes in the children
            var items = await IdList.GetItemsWithRootIdAsync(oldRoot).ToArrayAsync();
            
            foreach (var todo in items) 
            {
                ToDoEvents.Editing.AddOrUpdate(todo.OlItem.EntryID, 1, (key, existing) => existing + 1);
                todo.IdAutoCoding = false;
                var oldId = todo.ToDoID;
                todo.ToDoID = await IdList.SubstituteIdRootAsync(oldId, newRoot, oldRoot);
                todo.ProjectsToPrograms = ProjectsToPrograms;
                todo.Projects.AsStringNoPrefix = newProject;
                ToDoEvents.Editing.UpdateOrRemove(todo.OlItem.EntryID, (key, existing) => existing == 1, (key, existing) => existing - 1, out _);
            }

            //var items = IdList.GetItemsWithRootIdAsync(oldRoot);
            //var todos = await items.ToAsyncEnumerable().SelectAwait(async todo =>
            //{
            //    ToDoEvents.Editing.AddOrUpdate(todo.OlItem.InnerObject, 1, (key, existing) => existing + 1);
            //    todo.IdAutoCoding = false;
            //    var oldId = todo.ToDoID;
            //    todo.ToDoID = await IdList.SubstituteIdRootAsync(oldId, newRoot, oldRoot);
            //    todo.ProjectsToPrograms = ProjectsToPrograms;
            //    todo.Projects.AsStringNoPrefix = newProject;
            //    ToDoEvents.Editing.UpdateOrRemove(todo.OlItem.InnerObject, (key, existing) => existing == 1, (key, existing) => existing - 1, out _);
            //    return todo;
            //}).ToArrayAsync();
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

        #endregion Other Methods

    }
}