using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;



namespace ToDoModel
{
    /// <summary>
/// Class converts color categories to flags relevant to People, Projects, Topics, Context, etc
/// </summary>
    public class FlagParser
    {

        private readonly FlagDetails _people = new FlagDetails(My.MySettingsProperty.Settings.Prefix_People);
        private readonly FlagDetails _projects = new FlagDetails(My.MySettingsProperty.Settings.Prefix_Project);
        private readonly FlagDetails _topics = new FlagDetails(My.MySettingsProperty.Settings.Prefix_Topic);
        private readonly FlagDetails _context = new FlagDetails(My.MySettingsProperty.Settings.Prefix_Context);
        private readonly FlagDetails _kb = new FlagDetails(My.MySettingsProperty.Settings.Prefix_KB);
        public string other = "";
        public bool today = false;
        public bool bullpin = false;

        /// <summary>
    /// Constructor for the FlagParser class accepts a comma delimited string containing 
    /// color categories and initializes
    /// </summary>
    /// <param name="strCats_All"></param>
    /// <param name="DeleteSearchSubString"></param>
        public FlagParser(ref string strCats_All, bool DeleteSearchSubString = false)
        {
            if (strCats_All is null)
                strCats_All = "";
            // Splitter(strCats_All, DeleteSearchSubString)
            InitFromString(ref strCats_All);
        }

        /// <summary>
    /// Function tests to see if a string begins with a prefix
    /// </summary>
    /// <param name="test_string"></param>
    /// <param name="prefix"></param>
    /// <returns>True if present. False if not present.</returns>
        private bool PrefixPresent(string test_string, string prefix)
        {
            return (Strings.Left(test_string, prefix.Length) ?? "") == (prefix ?? "");
        }

        private void InitFromString(ref string strCats_All)
        {
            var list_categories = SplitToList(strCats_All, ",");
            _people.List = FindMatches(list_categories, _people.prefix);
            _projects.List = FindMatches(list_categories, _projects.prefix);
            _topics.List = FindMatches(list_categories, _topics.prefix);
            _context.List = FindMatches(list_categories, _context.prefix);
            _kb.List = FindMatches(list_categories, _kb.prefix);

            list_categories = list_categories.Except(_people.ListWithPrefix).Except(_projects.ListWithPrefix).Except(_topics.ListWithPrefix).Except(_context.ListWithPrefix).Except(_kb.ListWithPrefix).ToList();





            if (list_categories.Contains(My.MySettingsProperty.Settings.Prefix_Today))
            {
                today = true;
                bool unused1 = list_categories.Remove(My.MySettingsProperty.Settings.Prefix_Today);
            }
            else
            {
                today = false;
            }

            if (list_categories.Contains(My.MySettingsProperty.Settings.Prefix_Bullpin))
            {
                bullpin = true;
                bool unused = list_categories.Remove(My.MySettingsProperty.Settings.Prefix_Bullpin);
            }
            else
            {
                bullpin = false;
            }

            other = list_categories.Count > 0 ? string.Join(", ", list_categories) : "";

        }

        public string get_KB(bool IncludePrefix = false)
        {
            return IncludePrefix ? _kb.WithPrefix : _kb.NoPrefix;
        }

        public void set_KB(bool IncludePrefix = false, string value = default)
        {
            _kb.List = SplitToList(value, ",", _kb.prefix);
        }

        /// <summary>
    /// Property accesses a private instance of FlagDetails. 
    /// SET splits a comma delimited String to a list excluding 
    /// the prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    /// <returns>A string containing a comma separated Context names</returns>
        public string get_Context(bool IncludePrefix = false)
        {
            return IncludePrefix ? _context.WithPrefix : _context.NoPrefix;
        }

        public void set_Context(bool IncludePrefix = false, string value = default)
        {
            _context.List = SplitToList(value, ",", _context.prefix);
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
    /// the prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    /// <returns>A string containing a comma separated Project names</returns>
        public string get_Projects(bool IncludePrefix = false)
        {
            return IncludePrefix ? _projects.WithPrefix : _projects.NoPrefix;
        }

        public void set_Projects(bool IncludePrefix = false, string value = default)
        {
            _projects.List = SplitToList(value, ",", _projects.prefix);
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
    /// the prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    /// <returns>A string containing a comma separated Topic names</returns>
        public string get_Topics(bool IncludePrefix = false)
        {
            return IncludePrefix ? _topics.WithPrefix : _topics.NoPrefix;
        }

        public void set_Topics(bool IncludePrefix = false, string value = default)
        {
            _topics.List = SplitToList(value, ",", _topics.prefix);
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
    /// the prefix which is passed to the FlagDetails class.
    /// </summary>
    /// <param name="IncludePrefix">Determines whether GET includes the category prefix</param>
    /// <returns>A string containing a comma separated Topic names</returns>
        public string get_People(bool IncludePrefix = false)
        {
            return IncludePrefix ? _people.WithPrefix : _people.NoPrefix;
        }

        public void set_People(bool IncludePrefix = false, string value = default)
        {
            _people.List = SplitToList(value, ",", _people.prefix);
        }

        public List<string> PeopleList
        {
            get
            {
                return _people.List;
            }
        }

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

            if (today)
                string_return = string_return + ", " + "Tag A Top Priority Today";
            if (bullpin)
                string_return = string_return + ", " + "Tag Bullpin Priorities";


            if (string_return.Length > 2)
            {
                string_return = Strings.Right(string_return, string_return.Length - 2);
            }

            return string_return;
        }

        /// <summary>
    /// Subroutine extracts flag settings from color categories and loads to internal variables
    /// </summary>
    /// <param name="strCats_All">String containing comma delimited color categories</param>
    /// <param name="DeleteSearchSubString"></param>
        public void Splitter(ref string strCats_All, bool DeleteSearchSubString = false)
        {
            _people.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", DeleteSearchSubString: DeleteSearchSubString);
            other = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PPL "), ", ", true);

            _projects.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag PROJECT "), ", ", DeleteSearchSubString: DeleteSearchSubString);
            other = SubStr_w_Delimeter(other, AddWildcards("Tag PROJECT "), ", ", true);

            string strTemp = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag Bullpin Priorities"), ", ", DeleteSearchSubString: false);
            other = SubStr_w_Delimeter(other, AddWildcards("Tag Bullpin Priorities"), ", ", true);
            bullpin = !string.IsNullOrEmpty(strTemp);

            strTemp = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag A Top Priority Today"), ", ", DeleteSearchSubString: false);
            other = SubStr_w_Delimeter(other, AddWildcards("Tag A Top Priority Today"), ", ", true);
            today = !string.IsNullOrEmpty(strTemp);

            _topics.WithPrefix = SubStr_w_Delimeter(strCats_All, AddWildcards("Tag TOPIC "), ", ", DeleteSearchSubString: DeleteSearchSubString);
            other = SubStr_w_Delimeter(other, AddWildcards("Tag TOPIC "), ", ", true);

            set_KB(value: SubStr_w_Delimeter(strCats_All, AddWildcards("Tag KB "), ", ", DeleteSearchSubString: DeleteSearchSubString));
            other = SubStr_w_Delimeter(other, AddWildcards("Tag KB "), ", ", true);

            set_Context(value: other);

        }

        /// <summary>
    /// Function adds wildcards to a seach string
    /// </summary>
    /// <param name="strOriginal">A search string</param>
    /// <param name="b_Leading">If true, a wildcard is added at the beginning</param>
    /// <param name="b_Trailing">If true, a wildcard is added at the end</param>
    /// <param name="charWC">Character representing wildcard. Default is *</param>
    /// <returns>A search string with wildcards added</returns>
        public string AddWildcards(string strOriginal, bool b_Leading = true, bool b_Trailing = true, string charWC = "*")
        {
            string AddWildcardsRet = default;

            string strTemp;
            strTemp = strOriginal;
            if (b_Leading)
                strTemp = charWC + strTemp;
            if (b_Trailing)
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
                list_return = MainString.Split(Conversions.ToChar(Delimiter)).Select(x => x.Replace(ReplaceString, "").Trim()).ToList();


            }
            return list_return;
        }

        private List<string> FindMatches(List<string> source, string substring, bool return_nonmatches = false)
        {

            var list_return = return_nonmatches ? source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) == -1).Select(x => x).ToList() : source.Where(x => x.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1).Select(x => x.Replace(substring, "")).ToList();
            return list_return;

        }

        public List<string> SubStr_MatchList_w_Delimiter(string MainString, string SubString, string Delimiter, bool bNotSearchStr = false, bool DeleteSearchSubString = true)
        {

            string[] str_array = MainString.Split(Conversions.ToChar(Delimiter));
            object argvarStrArry = str_array;
            var filtered_array = SearchArry4Str(ref argvarStrArry, SubString, bNotSearchStr, DeleteSearchSubString: DeleteSearchSubString);
            str_array = (string[])argvarStrArry;
            List<string> match_list = filtered_array as List<string>;
            return match_list;
        }

        /// <summary>
    /// Extract: Function accepts a comma delimited string and converts to an array of strings
    /// Transform: Function selects members of the array that match the substring
    /// LoadFromFile: Function returns a comma delimited string containing matching elements
    /// </summary>
    /// <param name="strMainString">A comma delimited string that will be searched</param>
    /// <param name="strSubString">Target substring to find</param>
    /// <param name="strDelimiter">String used as delimiter</param>
    /// <param name="bNotSearchStr">Boolean flag that inverts the search to return 
    /// elements that don't match</param>
    /// <param name="DeleteSearchSubString">Boolean that determines if return value 
    /// eliminates substring from each match</param>
    /// <returns></returns>
        public string SubStr_w_Delimeter(string strMainString, string strSubString, string strDelimiter, bool bNotSearchStr = false, bool DeleteSearchSubString = false)
        {
            string SubStr_w_DelimeterRet = default;
            object varTempStrAry;
            object varFiltStrAry;
            string strTempStr;

            varTempStrAry = strMainString.Split(Conversions.ToChar(strDelimiter));
            varFiltStrAry = SearchArry4Str(ref varTempStrAry, strSubString, bNotSearchStr, DeleteSearchSubString: DeleteSearchSubString);
            strTempStr = Condense_Variant_To_Str(varFiltStrAry);

            SubStr_w_DelimeterRet = strTempStr;
            return SubStr_w_DelimeterRet;

        }

        /// <summary>
    /// Function accepts a pointer to a string array and searches for a substring.
    /// It returns a pointer to a new string array containing matches 
    /// </summary>
    /// <param name="varStrArry">Pointer to the string array to search</param>
    /// <param name="SearchStr$">Target substring to search</param>
    /// <param name="bNotSearchStr">Boolean flag that inverts the search to return 
    /// any element that doesn't match</param>
    /// <param name="DeleteSearchSubString">Boolean that removes </param>
    /// <returns>Pointer to a string array with elements that match the criteria</returns>
        public object SearchArry4Str(ref object varStrArry, string SearchStr = "", bool bNotSearchStr = false, bool DeleteSearchSubString = false)
        {
            object SearchArry4StrRet = default;
            string m_Find;
            bool m_Wildcard;

            string[] strCats;
            int i;
            int intFoundCt;
            bool boolFound;
            string strTemp;
            string strSearchNoWC;

            if (Strings.Len(Strings.Trim(SearchStr)) != 0)
            {

                strCats = new string[1];
                m_Find = SearchStr;

                // Make lower case
                m_Find = Strings.LCase(m_Find);

                // Standardize characters used as wildcards
                m_Find = Strings.Replace(m_Find, "%", "*");

                // Determine if wildcards are present in search string
                m_Wildcard = Conversions.ToBoolean(Strings.InStr(m_Find, "*"));

                intFoundCt = 0;

                // Remove wildcards from the string
                strSearchNoWC = Strings.Replace(SearchStr, "*", "");

                // Loop through the array to find substring
                var loopTo = Information.UBound((Array)varStrArry);
                for (i = Information.LBound((Array)varStrArry); i <= loopTo; i++)
                {
                    boolFound = false;

                    // Skip over blank entries
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(varStrArry((object)i), "", false)))
                    {
                        boolFound = m_Wildcard ? bNotSearchStr == false ? (bool)LikeOperator.LikeObject(LCase(varStrArry((object)i)), m_Find, CompareMethod.Binary) : (bool)!LikeOperator.LikeObject(LCase(varStrArry((object)i)), m_Find, CompareMethod.Binary) : bNotSearchStr == false ? Operators.ConditionalCompareObjectEqual(LCase(varStrArry((object)i)), m_Find, false) : !Operators.ConditionalCompareObjectEqual(LCase(varStrArry((object)i)), m_Find, false);
                    }

                    if (boolFound)
                    {
                        intFoundCt += 1;
                        Array.Resize(ref strCats, intFoundCt + 1);
                        strTemp = Conversions.ToString(varStrArry((object)i));
                        if (DeleteSearchSubString)
                            strTemp = Strings.Replace(strTemp, strSearchNoWC, "", Compare: Constants.vbTextCompare);
                        strCats[intFoundCt] = strTemp;
                    }
                }

                SearchArry4StrRet = intFoundCt == 0 ? "" : strCats;
            }

            else
            {
                SearchArry4StrRet = varStrArry;
            }

            return SearchArry4StrRet;


        }

        /// <summary>
    /// Function accepts a pointer to a string array and collapses into a comma delimited string
    /// </summary>
    /// <param name="varAry">Pointer to string array</param>
    /// <returns>A comma delimited string</returns>
        public string Condense_Variant_To_Str(object varAry)
        {
            string Condense_Variant_To_StrRet = default;
            string strTempStr = "";
            int i;

            if (varAry is Array)
            {
                var loopTo = Information.UBound((Array)varAry);
                for (i = 1; i <= loopTo; i++)
                    strTempStr = Conversions.ToString(Operators.ConcatenateObject(strTempStr + ", ", varAry((object)i)));
                if (!string.IsNullOrEmpty(strTempStr))
                    strTempStr = Strings.Right(strTempStr, Strings.Len(strTempStr) - 2);
            }
            else
            {
                strTempStr = Conversions.ToString(varAry);
            }

            Condense_Variant_To_StrRet = strTempStr;
            return Condense_Variant_To_StrRet;

        }

    }


    public class FlagDetails
    {
        private RestrictedList<string> _list;
        public string prefix;

        public FlagDetails()
        {
        }

        public FlagDetails(string prefix)
        {
            this.prefix = prefix;
        }

        public List<string> List
        {
            get
            {
                return _list;
            }
            set
            {
                List<string> TmpList;
                if (value is null)
                {
                    TmpList = new List<string>();
                }
                else if (value.Count == 0)
                {
                    TmpList = value;
                }
                else if ((Strings.Left(value[0], prefix.Length) ?? "") == (prefix ?? ""))
                {
                    TmpList = value.Select(x => x.Replace(prefix, "")).ToList();
                }
                else
                {
                    TmpList = value;
                }
                _list = new RestrictedList<string>(TmpList, this);
                ListChange_Refresh();
            }
        }

        public List<string> ListWithPrefix
        {
            get
            {
                return _list.Select(x => prefix + x).ToList();
            }
        }

        private void ListChange_Refresh()
        {
            WithPrefix = string.Join(", ", _list.Select(x => prefix + x));
            NoPrefix = string.Join(", ", _list);
        }

        public string WithPrefix { get; set; }

        public string NoPrefix { get; set; }

        private sealed class RestrictedList<T> : List<T>
        {
            // Implements ICloneable

            private readonly FlagDetails outer;

            public RestrictedList(List<T> wrapped_list, FlagDetails outer) : base(wrapped_list)
            {
                if (wrapped_list is null)
                {
                    throw new ArgumentNullException("wrapped_list");
                }
                this.outer = outer;
            }

            public new void Add(T item)
            {
                base.Add(item);
                outer.ListChange_Refresh();
            }

            public new void Remove(T item)
            {
                bool unused = base.Remove(item);
                outer.ListChange_Refresh();
            }

            // Public Function ToClonedList() As List(Of T)
            // Dim ClonedList As List(Of T) = TryCast(Me.Clone(), List(Of T))
            // Return ClonedList
            // End Function

            // Private Function Clone() As Object Implements ICloneable.Clone
            // Return MyBase.MemberwiseClone()
            // End Function
        }

    }
}