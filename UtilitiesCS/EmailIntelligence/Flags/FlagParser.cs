using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using UtilitiesCS;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace UtilitiesCS
{
    /// <summary>
    /// Class converts color categories to flags relevant to People, Projects, Topics, Context, etc
    /// </summary>
    public class FlagParser: INotifyCollectionChanged//, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor for the FlagParser class accepts a comma delimited string containing 
        /// color categories and initializes
        /// </summary>
        /// <param name="categoryString"></param>
        /// <param name="deleteSearchSubString"></param>
        public FlagParser(ref string categoryString, bool deleteSearchSubString = false)
        {
            if (categoryString is null)
                categoryString = "";

            var categories = categoryString.Split(separator: ',', trim: true).ToList();
            Initialize(categories);
        }

        public FlagParser(IList<string> categories)
        {
            Initialize(categories);
        }

        internal void Initialize(IList<string> categories)
        {
            _people.List = FindMatches(categories, _people.Prefix);
            _projects.List = FindMatches(categories, _projects.Prefix);
            _topics.List = FindMatches(categories, _topics.Prefix);
            _context.List = FindMatches(categories, _context.Prefix);
            _kb.List = FindMatches(categories, _kb.Prefix);

            categories = categories.Except(_people.ListWithPrefix)
                                       .Except(_projects.ListWithPrefix)
                                       .Except(_topics.ListWithPrefix)
                                       .Except(_context.ListWithPrefix)
                                       .Except(_kb.ListWithPrefix).ToList();

            Today = categories.Remove(Properties.Settings.Default.Prefix_Today);
            Bullpin = categories.Remove(Properties.Settings.Default.Prefix_Bullpin);
            Other = categories.Count > 0 ? string.Join(", ", categories) : "";
            WireEvents();
        }

        #region Context

        private readonly FlagDetails _context = new FlagDetails(Properties.Settings.Default.Prefix_Context);

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

        private readonly FlagDetails _projects = new FlagDetails(Properties.Settings.Default.Prefix_Project);

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

        #region Topics

        private readonly FlagDetails _topics = new FlagDetails(Properties.Settings.Default.Prefix_Topic);

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

        public ObservableCollection<string> GetTopicList(bool IncludePrefix = false) => IncludePrefix ? _topics.ListWithPrefix : _context.List;
        public void SetTopicList(bool IncludePrefix = false, ObservableCollection<string> value = default) => _topics.List = value;

        #endregion

        #region People

        private readonly FlagDetails _people = new FlagDetails(Properties.Settings.Default.Prefix_People);

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

        #endregion

        #region Other Public Methods and Properties
        
        /// <summary>
    /// Function recombines flag settings in one comma delimited string representing color categories
    /// </summary>
    /// <returns>A string containing color categories</returns>
        public string Combine(bool wtag = true)
        {
            string string_return = "";
            string_return = AppendDetails(string_return, _people, wtag);
            string_return = AppendDetails(string_return, _projects, wtag);
            string_return = AppendDetails(string_return, _topics, wtag);
            string_return = AppendDetails(string_return, _context, wtag);
            string_return = AppendDetails(string_return, _kb, wtag);

            if (Today)
                string_return = string_return + ", " + "Tag A Top Priority Today";
            if (Bullpin)
                string_return = string_return + ", " + "Tag Bullpin Priorities";


            if (string_return.Length > 2)
            {
                string_return = string_return.Substring(2);
            }

            return string_return;
        }
        
        private readonly FlagDetails _kb = new FlagDetails(Properties.Settings.Default.Prefix_KB);
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

        private bool _today = false;
        public bool Today { get => _today; set => _today = value; }

        private bool _bullpin = false;
        public bool Bullpin { get => _bullpin; set => _bullpin = value; }

        private string _other = "";
        public string Other { get => _other; set => _other = value; }

        #endregion

        #region INotifyCollectionChanged Implementation

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        //public event PropertyChangedEventHandler PropertyChanged;

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            CollectionChanged?.Invoke(sender, e);
        }

        public void WireEvents()
        {
            var list = new List<FlagDetails> { _people, _projects, _topics, _context, _kb };
            list.ForEach(x => x.CollectionChanged += List_CollectionChanged);
        }

        #endregion

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

        #endregion
    
    }


}