using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using System.Text.RegularExpressions;
using UtilitiesCS;


namespace ToDoModel
{
    /// <summary>
    /// Class converts color categories to flags relevant to People, Projects, Topics, Context, etc
    /// </summary>
    public class FlagParser
    {
        /// <summary>
        /// Constructor for the FlagParser class accepts a comma delimited string containing 
        /// color categories and initializes
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="deleteSearchSubString"></param>
        public FlagParser(ref string categories, bool deleteSearchSubString = false)
        {
            if (categories is null)
                categories = "";

            ArrayExtensions.SearchOptions options = ArrayExtensions.SearchOptions.Standard;
            
            if (deleteSearchSubString)
                options = ArrayExtensions.SearchOptions.DeleteFromMatches;
            
            var categoryList = categories.Split(separator: ',', trim: true).ToList();
            _people.List = FindMatches(categoryList, _people.Prefix);
            _projects.List = FindMatches(categoryList, _projects.Prefix);
            _topics.List = FindMatches(categoryList, _topics.Prefix);
            _context.List = FindMatches(categoryList, _context.Prefix);
            _kb.List = FindMatches(categoryList, _kb.Prefix);

            categoryList = categoryList.Except(_people.ListWithPrefix)
                                       .Except(_projects.ListWithPrefix)
                                       .Except(_topics.ListWithPrefix)
                                       .Except(_context.ListWithPrefix)
                                       .Except(_kb.ListWithPrefix).ToList();

            Today = categoryList.Remove(Properties.Settings.Default.Prefix_Today);
            Bullpin = categoryList.Remove(Properties.Settings.Default.Prefix_Bullpin);
            Other = categoryList.Count > 0 ? string.Join(", ", categoryList) : "";

        }

        private readonly FlagDetails _people = new FlagDetails(Properties.Settings.Default.Prefix_People);
        private readonly FlagDetails _projects = new FlagDetails(Properties.Settings.Default.Prefix_Project);
        private readonly FlagDetails _topics = new FlagDetails(Properties.Settings.Default.Prefix_Topic);
        private readonly FlagDetails _context = new FlagDetails(Properties.Settings.Default.Prefix_Context);
        private readonly FlagDetails _kb = new FlagDetails(Properties.Settings.Default.Prefix_KB);
        private string _other = "";
        private bool _today = false;
        private bool _bullpin = false;

        public string get_KB(bool IncludePrefix = false)
        {
            return IncludePrefix ? _kb.WithPrefix : _kb.NoPrefix;
        }

        public void set_KB(bool IncludePrefix = false, string value = default)
        {
            _kb.List = SplitToList(value, ",", _kb.Prefix);
        }

        /// <summary>
    /// Property accesses a private instance of FlagDetails. 
    /// SET splits a comma delimited String to a list excluding 
    /// the Prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
    /// <returns>A string containing a comma separated Context names</returns>
        public string get_Context(bool IncludePrefix = false)
        {
            return IncludePrefix ? _context.WithPrefix : _context.NoPrefix;
        }

        public void set_Context(bool IncludePrefix = false, string value = default)
        {
            _context.List = SplitToList(value, ",", _context.Prefix);
        }

        public List<string> ContextList
        {
            get
            {
                return _context.List;
            }
        }

        /// <summary>
    /// Property accesses a private instance of FlagDetails. 
    /// SET splits a comma delimited String to a list excluding 
    /// the Prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
    /// <returns>A string containing a comma separated Project names</returns>
        public string get_Projects(bool IncludePrefix = false)
        {
            return IncludePrefix ? _projects.WithPrefix : _projects.NoPrefix;
        }

        public void set_Projects(bool IncludePrefix = false, string value = default)
        {
            _projects.List = SplitToList(value, ",", _projects.Prefix);
        }

        public List<string> ProjectList
        {
            get
            {
                return _projects.List;
            }
        }

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Topic names</returns>
        public string get_Topics(bool IncludePrefix = false)
        {
            return IncludePrefix ? _topics.WithPrefix : _topics.NoPrefix;
        }

        public void set_Topics(bool IncludePrefix = false, string value = default)
        {
            _topics.List = SplitToList(value, ",", _topics.Prefix);
        }

        public List<string> TopicList
        {
            get
            {
                return _topics.List;
            }
        }

        /// <summary>
        /// Property accesses a private instance of FlagDetails. 
        /// SET splits a comma delimited String to a list excluding 
        /// the Prefix which is passed to the FlagDetails class.
        /// </summary>
        /// <param name="IncludePrefix">Determines whether GET includes the category Prefix</param>
        /// <returns>A string containing a comma separated Topic names</returns>
        public string get_People(bool IncludePrefix = false)
        {
            return IncludePrefix ? _people.WithPrefix : _people.NoPrefix;
        }

        public void set_People(bool IncludePrefix = false, string value = default)
        {
            _people.List = SplitToList(value, ",", _people.Prefix);
        }

        public List<string> PeopleList
        {
            get
            {
                return _people.List;
            }
        }

        public bool Today { get => _today; set => _today = value; }
        public bool Bullpin { get => _bullpin; set => _bullpin = value; }
        public string Other { get => _other; set => _other = value; }

        private string AppendDetails(string @base, FlagDetails details, bool wtag)
        {
            return details.WithPrefix.Length == 0 ? @base : wtag ? @base + ", " + details.WithPrefix : @base + ", " + details.NoPrefix;
        }

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

        private List<string> SplitToList(string MainString, string Delimiter, string ReplaceString = "XXXXX")
        {
            List<string> list_return;
            if (MainString is null)
            {
                list_return = new List<string>();
            }
            else if (string.IsNullOrEmpty(MainString))
            {
                list_return = new List<string>();
            }
            else
            {
                list_return = MainString.Split(Delimiter[0]).Select(x => x.Replace(ReplaceString, "").Trim()).ToList();
            }
            return list_return;
        }

        private List<string> FindMatches(List<string> source, string substring, bool return_nonmatches = false)
        {

            var list_return = return_nonmatches ? source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) == -1).Select(x => x).ToList() : source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1).Select(x => x.Replace(substring, "")).ToList();
            return list_return;

        }
                
    }

    
}