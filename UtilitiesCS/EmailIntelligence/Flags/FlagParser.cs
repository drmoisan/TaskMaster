using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UtilitiesCS;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UtilitiesCS.Extensions.Lazy;
using Microsoft.Office.Interop.Outlook;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UtilitiesCS.Threading;
using UtilitiesCS.EmailIntelligence.Flags;

namespace UtilitiesCS
{
    /// <summary>
    /// Class converts color categories to flags relevant to People, Projects, Topics, Context, etc
    /// </summary>
    public class FlagParser: INotifyCollectionChanged, ICloneable, INotifyPropertyChanged
    {
        #region ctor

        /// <summary>
        /// Constructor for the FlagParser class accepts a comma delimited string containing 
        /// color categories and initializes
        /// </summary>
        /// <param name="categoryString"></param>
        /// <param name="deleteSearchSubString"></param>
        public FlagParser(ref string categoryString, bool deleteSearchSubString = false)
        {
            //_wiring = new Lazy<Dictionary<FlagDetails, NotifyCollectionChangedEventHandler>>(GetWiring);
            if (categoryString is null)
                categoryString = "";

            var categories = categoryString.Split(separator: ',', trim: true).ToList();
            Initialize(categories);            
        }

        public FlagParser(IList<string> categories)
        {
            //_wiring = new Lazy<Dictionary<FlagDetails, NotifyCollectionChangedEventHandler>>(GetWiring);
            Initialize(categories);
        }

        internal void Initialize(IList<string> categories)
        {
            People.List = FindMatches(categories, People.Prefix);
            Projects.List = FindMatches(categories, Projects.Prefix);
            Program.List = FindMatches(categories, Program.Prefix);
            Topics.List = FindMatches(categories, Topics.Prefix);
            Context.List = FindMatches(categories, Context.Prefix);
            Kb.List = FindMatches(categories, Kb.Prefix);

            categories = categories.Except(_people.ListWithPrefix)
                                   .Except(_projects.ListWithPrefix)
                                   .Except(_topics.ListWithPrefix)
                                   .Except(_context.ListWithPrefix)
                                   .Except(_kb.ListWithPrefix)
                                   .ToList();

            Today = categories.Remove(Properties.Settings.Default.Prefix_Today);
            Bullpin = categories.Remove(Properties.Settings.Default.Prefix_Bullpin);
            Other = categories.Count > 0 ? string.Join(", ", categories) : "";    
            Combined = new(this);
            WireEvents();
        }
        
        private string identifier = "not set";
        public string Identifier 
        { 
            get => identifier;
            set 
            { 
                identifier = value; 
                Wiring.ForEach(x => x.Key.Identifier = value);
            }
        }

        #endregion ctor

        #region Flags By Type

        #region Context

        private FlagDetails _context = new(Properties.Settings.Default.Prefix_Context);
        internal FlagDetails Context => _context;

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="includePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Context names</returns>
        public string GetContext(bool includePrefix = false)
        {
            return includePrefix ? _context.WithPrefix : _context.NoPrefix;
        }

        public void SetContext(bool includePrefix = false, string value = default)
        {
            _context.List = SplitToList(value, ",", _context.Prefix);
        }

        public ObservableCollection<string> GetContextList(bool IncludePrefix = false) => IncludePrefix ? _context.ListWithPrefix : _context.List;
        public void SetContextList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _context.List = value;

        #endregion

        #region Projects

        private FlagDetails _projects = new(Properties.Settings.Default.Prefix_Project);
        internal FlagDetails Projects => _projects;

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="includePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Project names</returns>
        public string GetProjects(bool includePrefix = false)
        {
            return includePrefix ? _projects.WithPrefix : _projects.NoPrefix;
        }

        public void SetProjects(bool includePrefix = false, string value = default)
        {
            _projects.List = SplitToList(value, ",", _projects.Prefix);
        }

        public ObservableCollection<string> GetProjectList(bool IncludePrefix = false) => IncludePrefix ? _projects.ListWithPrefix : _projects.List;
        public void SetProjectList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _projects.List = value;

        #endregion

        #region Program

        private FlagDetails _program = new(Properties.Settings.Default.Prefix_Program);
        internal FlagDetails Program => _program;

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="includePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Project names</returns>
        public string GetProgram(bool includePrefix = false)
        {
            return includePrefix ? _program.WithPrefix : _program.NoPrefix;
        }

        public void SetProgram(bool includePrefix = false, string value = default)
        {
            _program.List = SplitToList(value, ",", _program.Prefix);
        }

        public ObservableCollection<string> GetProgramList(bool IncludePrefix = false) => IncludePrefix ? _program.ListWithPrefix : _program.List;
        public void SetProgramList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _program.List = value;

        #endregion Program

        #region Topics

        private FlagDetails _topics = new(Properties.Settings.Default.Prefix_Topic);
        internal FlagDetails Topics => _topics;

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Topic names</returns>
        public string GetTopics(bool IncludePrefix = false)
        {
            return IncludePrefix ? _topics.WithPrefix : _topics.NoPrefix;
        }

        public void SetTopics(bool includePrefix = false, string value = default)
        {
            _topics.List = SplitToList(value, ",", _topics.Prefix);
        }

        public ObservableCollection<string> GetTopicList(bool IncludePrefix = false) => IncludePrefix ? _topics.ListWithPrefix : _topics.List;
        public void SetTopicList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _topics.List = value;

        #endregion Topics

        #region People

        private FlagDetails _people = new FlagDetails(Properties.Settings.Default.Prefix_People);
        internal FlagDetails People => _people;

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Topic names</returns>
        public string GetPeople(bool IncludePrefix = false) => IncludePrefix ? _people.WithPrefix : _people.NoPrefix;
        
        public void SetPeople(bool IncludePrefix = false, string value = default) => _people.List = SplitToList(value, ",", _people.Prefix);
        
        public ObservableCollection<string> GetPeopleList(bool IncludePrefix = false) => IncludePrefix ? _people.ListWithPrefix : _people.List;
        public void SetPeopleList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _people.List = value;

        #endregion People

        #region Kanban

        private FlagDetails _kb = new FlagDetails(Properties.Settings.Default.Prefix_KB);
        internal FlagDetails Kb => _kb;

        public string GetKb(bool includePrefix = false)
        {
            return includePrefix ? _kb.WithPrefix : _kb.NoPrefix;
        }
        public void SetKb(bool includePrefix = false, string value = default)
        {
            _kb.List = SplitToList(value, ",", _kb.Prefix);
        }

        public ObservableCollection<string> GetKbList(bool IncludePrefix = false) => IncludePrefix ? _kb.ListWithPrefix : _kb.List;
        public void SetKbList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _kb.List = value;

        #endregion Kanban

        private bool _today = false;
        public bool Today
        {
            get => _today;
            set
            {
                if (_today != value)
                {
                    _today = value;
                    Notify();
                }
            }
        }

        private bool _bullpin = false;
        public bool Bullpin
        {
            get => _bullpin;
            set
            {
                if (_bullpin != value)
                {
                    _bullpin = value;
                    Notify();
                }
            }
        }

        private string _other = "";
        public string Other
        {
            get => _other;
            set
            {
                if (_other != value)
                {
                    _other = value;
                    Notify();
                }
            }
        }

        private void Update() => Combined.RequestUpdate();//updated = false;
        //protected bool updated;
        
        public FlagConsolidator Combined { get; protected set; }

        #region commented out
        ///// <summary>
        ///// Function recombines flag settings in one comma delimited string representing color categories
        ///// </summary>
        ///// <returns>A string containing color categories</returns>
        //public string Combine(bool wtag = true)
        //{
        //    string string_return = "";
        //    string_return = AppendDetails(string_return, _people, wtag);
        //    string_return = AppendDetails(string_return, _projects, wtag);
        //    string_return = AppendDetails(string_return, _topics, wtag);
        //    string_return = AppendDetails(string_return, _context, wtag);
        //    string_return = AppendDetails(string_return, _kb, wtag);
        //    string_return += $", {Other}";

        //    if (Today)
        //        string_return = string_return + ", " + "Tag A Top Priority Today";
        //    if (Bullpin)
        //        string_return = string_return + ", " + "Tag Bullpin Priorities";


        //    if (string_return.Length > 2)
        //    {
        //        string_return = string_return.Substring(2);
        //    }

        //    return string_return;
        //}
        #endregion commented out

        #endregion Flags By Type

        #region INotifyCollectionChanged and INotifyPropertyChanged Implementations

        public event NotifyCollectionChangedEventHandler CollectionChanged { add { } remove { } }
        public event NotifyCollectionChangedEventHandler PeopleChanged;
        public event NotifyCollectionChangedEventHandler ProjectsChanged;
        public event NotifyCollectionChangedEventHandler ProgramChanged;
        public event NotifyCollectionChangedEventHandler TopicsChanged;
        public event NotifyCollectionChangedEventHandler ContextChanged;
        public event NotifyCollectionChangedEventHandler KbChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { PeopleChanged?.Invoke(sender, e); Update(); }
        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { ProjectsChanged?.Invoke(sender, e); Update(); }
        private void Program_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { ProgramChanged?.Invoke(sender, e); Update(); }
        private void Topics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { TopicsChanged?.Invoke(sender, e); Update(); }
        private void Context_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { ContextChanged?.Invoke(sender, e); Update(); }
        private void Kb_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { KbChanged?.Invoke(sender, e); Update(); }
        private void PropertyChanged_CollectionChanged(object sender, PropertyChangedEventArgs e) { Update(); }

        protected virtual void Notify([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private Lazy<Dictionary<FlagDetails, NotifyCollectionChangedEventHandler>> _wiring;
        //internal Dictionary<FlagDetails, NotifyCollectionChangedEventHandler> Wiring { get => _wiring.Value; set => _wiring = value.ToLazy(); }
        internal Dictionary<FlagDetails, NotifyCollectionChangedEventHandler> Wiring { get => GetWiring(); }
        private Dictionary<FlagDetails, NotifyCollectionChangedEventHandler> GetWiring()
        {
            return new()
            {
                { People,  People_CollectionChanged },
                { Projects , Projects_CollectionChanged },
                { Program , Program_CollectionChanged   },
                { Topics , Topics_CollectionChanged },
                { Context , Context_CollectionChanged },
                { Kb , Kb_CollectionChanged }
            };
        }
        
        public void WireEvents()
        {
            _people.CollectionChanged += People_CollectionChanged;
            _projects.CollectionChanged += Projects_CollectionChanged;
            _program.CollectionChanged += Program_CollectionChanged;
            _topics.CollectionChanged += Topics_CollectionChanged;
            _context.CollectionChanged += Context_CollectionChanged;
            _kb.CollectionChanged += Kb_CollectionChanged;
        }

        public void UnWireEvents() => Wiring.ForEach(x => UnWireFlagParserEvent(x.Key, x.Value));        

        public void UnWireFlagParserEvent(FlagDetails flagDetails,NotifyCollectionChangedEventHandler handler)
        {
            if (flagDetails is not null)
            {
                flagDetails.CollectionChanged -= handler;
            }
        }

        #endregion INotifyCollectionChanged and INotifyPropertyChanged Implementations

        #region IClonable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public FlagParser DeepCopy()
        {
            lock (this)
            {
                return DeepCopyInternal();
            }
        }

        private FlagParser DeepCopyInternal()
        {
            UnWireEvents();
            var clone = (FlagParser)this.MemberwiseClone();
            clone._context = _context.DeepCopy();
            clone._people = _people.DeepCopy();
            clone._projects = _projects.DeepCopy();
            clone._program = _program.DeepCopy();
            clone._topics = _topics.DeepCopy();
            clone._kb = _kb.DeepCopy();
            WireEvents();
            clone.WireEvents();
            return clone;
        }

        #endregion IClonable

        #region Helper Methods

        private string AppendDetails(string @base, FlagDetails details, bool wtag)
        {
            return details.WithPrefix.Length == 0 ? @base : wtag ? @base + ", " + details.WithPrefix : @base + ", " + details.NoPrefix;
        }

        /// <summary>
        /// Function adds wildcards to a seach string
        /// </summary>
        /// <param name="sourceString">A search string</param>
        /// <param name="leading">If true, a wildcard is added at the beginning</param>
        /// <param name="trailing">If true, a wildcard is added at the end</param>
        /// <param name="charWC">Character representing wildcard. Default is *</param>
        /// <returns>A search string with wildcards added</returns>
        public string AddWildcards(string sourceString, bool leading = true, bool trailing = true, string charWC = "*")
        {
            string AddWildcardsRet = default;

            string strTemp;
            strTemp = sourceString;
            if (leading)
                strTemp = charWC + strTemp;
            if (trailing)
                strTemp += charWC;

            AddWildcardsRet = strTemp;
            return AddWildcardsRet;

        }

        private ObservableCollection<string> SplitToList(string MainString, string Delimiter, string ReplaceString = "XXXXX")
        {
            ObservableCollection<string> list_return;
            if (MainString is null)
            {
                list_return = new ObservableCollection<string>();
            }
            else if (string.IsNullOrEmpty(MainString))
            {
                list_return = new ObservableCollection<string>();
            }
            else
            {
                list_return = new ObservableCollection<string>(MainString.Split(Delimiter[0]).Select(x => x.Replace(ReplaceString, "").Trim()));
            }
            return list_return;
        }

        private ObservableCollection<string> FindMatches(IList<string> source, string substring, bool return_nonmatches = false)
        {
            if (return_nonmatches)
            {
                return new ObservableCollection<string>(source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) == -1));
            }
            else
            {
                return new ObservableCollection<string>(source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1).Select(x => x.Replace(substring, "")).ToList());
            }
            //var list_return = return_nonmatches ? source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) == -1).Select(x => x).ToList() : source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1).Select(x => x.Replace(substring, "")).ToList();
            //return list_return;

        }

        #endregion Helper Methods

        #region Comparison
        
        public bool AreEquivalentTo(string other) 
        {
            if (Combined.AsStringWithPrefix.IsNullOrEmpty())
            {
                if (other.IsNullOrEmpty()) { return true; }
                else { return false; }
            }
            else if (other.IsNullOrEmpty()) { return false; }                
            else if (Combined.AsStringWithPrefix == other) { return true; }            
            else 
            {
                var otherList = other.Split(separator: ',', trim: true).OrderBy(x => x).ToList();
                return Combined.AsListWithPrefix.SequenceEqual(otherList);
            }
        }

        public bool AreEquivalentTo(IList<string> other)
        {
            if (Combined.AsListWithPrefix.IsNullOrEmpty())
            {
                if (other.IsNullOrEmpty()) { return true; }
                else { return false; }
            }
            else if (other.IsNullOrEmpty()) { return false; }
            else
            {
                other = other.OrderBy(x => x).ToList();
                return Combined.AsListWithPrefix.SequenceEqual(other);
            }
        }

        #endregion Comparison

    }


}
