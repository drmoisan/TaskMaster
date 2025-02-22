using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Tags;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace ToDoModel.Data_Model.People
{
    public class PeopleScoDictionaryNew : ScoDictionaryNew<string, string>, IPeopleScoDictionaryNew
    {
        //private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
        //    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public PeopleScoDictionaryNew() : base() { }
        public PeopleScoDictionaryNew(IApplicationGlobals globals) : base() { Globals = globals; }
        //public PeopleScoDictionaryNew(IEnumerable<KeyValuePair<string, string>> collection) : base(collection) { }
        //public PeopleScoDictionaryNew(IEqualityComparer<string> comparer) : base(comparer) { }
        //public PeopleScoDictionaryNew(IEnumerable<KeyValuePair<string, string>> collection, IEqualityComparer<string> comparer) : base(collection, comparer) { }
        //public PeopleScoDictionaryNew(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { }
        //public PeopleScoDictionaryNew(int concurrencyLevel, IEnumerable<KeyValuePair<string, string>> collection, IEqualityComparer<string> comparer) : base(concurrencyLevel, collection, comparer) { }
        //public PeopleScoDictionaryNew(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer, IApplicationGlobals globals) : base(concurrencyLevel, capacity, comparer) { Globals = globals; }
        //public PeopleScoDictionaryNew(ScoDictionaryNew<string, string> dictionary, IApplicationGlobals globals) : base(dictionary) { Globals = globals; }

        #endregion Constructors

        [JsonProperty]
        internal IApplicationGlobals Globals { get; set; }

        [JsonIgnore]
        private IPrefix _prefix;
        public IPrefix Prefix { get => _prefix; set => _prefix = value; }

        public bool IsPeopleCategory(string test)
        {
            return (test is not null) && (test.Length >= _prefix.Value.Length) && (test.Substring(0, _prefix.Value.Length) == _prefix.Value);
        }

        public List<string> GetPeopleCatNames()
        {
            return [.. Globals.ThrowIfNull().Ol.App.Session.Categories.Cast<Outlook.Category>().Where(cat => IsPeopleCategory(cat.Name)).Select(cat => cat.Name)];
        }

        public bool CategoryExists(string category)
        {
            return Globals.ThrowIfNull().Ol.App.Session.Categories.Cast<Outlook.Category>().Any(cat => cat.Name == category);
        }

        public IList<string> AddMissingEntries(Outlook.MailItem olMail)
        {
            Globals.ThrowIfNull();
            var addressList = olMail.GetEmailAddresses(Globals.Ol.InboxPath,
                                                       Globals.TD.DictRemap,
                                                       Globals.Ol.UserEmailAddress)
                                                       .Where(x => !this.ContainsKey(x))
                                                       .Select(x => x)
                                                       .ToList();
            IList<string> newPeople = [];

            foreach (var address in addressList)
            {
                var response = MyBox.ShowDialog($"Add mapping for {address}?", "Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (response == DialogResult.Cancel) { break; }
                if (response == DialogResult.Yes)
                {
                    var entry = AddMissingEntry(address);
                    if (entry is not null) { newPeople.Add(entry); }
                }
            }
            if (!newPeople.IsNullOrEmpty()) { this.Serialize(); }
            return newPeople;
        }

        public string AddMissingEntry(string address) //internal
        {
            var newPerson = SplitAddressToFirstLastName(address);
            var existingPeople = GetPeopleCatNames();
            var matchResult = MatchToExisting(existingPeople, newPerson);
            if (matchResult.IsNullOrEmpty())
            {
                newPerson = RefineValidateCategory(newPerson, _prefix);
                if (newPerson is null) { return null; }
                AddColorCategory(newPerson);
                this.AddOrUpdate(address, newPerson);
                return newPerson;
            }
            else
            {
                this.AddOrUpdate(address, matchResult);
                //this.Add(address, matchResult);
                return matchResult;
            }
        }

        public string AddPrefix(string seed, string prefix)
        {
            if (seed is null) { throw new ArgumentNullException(nameof(seed)); }
            if (prefix is null) { throw new ArgumentNullException(nameof(prefix)); }
            if (seed.StartsWith(prefix)) { return seed; }
            else { return $"{prefix}{seed}"; }
        }

        public string RefineValidateCategory(string newPerson, IPrefix prefix)
        {
            bool continueAsking = true;
            while (continueAsking)
            {
                newPerson = InputBox.ShowDialog("The following category name will be added:", "Add Category Dialog", DefaultResponse: newPerson);
                // if user cancels, return null
                if (newPerson is null) { continueAsking = false; }
                // if user leaves blank, continue asking
                else if (newPerson == "") { continueAsking = true; }
                // else check if input is valid
                else
                {
                    // Add prefix if not already there
                    newPerson = AddPrefix(newPerson, prefix.Value);
                    // if category already exists, tell the user and continue asking
                    if (CategoryExists(newPerson))
                    {
                        MessageBox.Show($"Category {newPerson} already exists. Please choose another name.", "Category Exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continueAsking = true;
                    }
                    // else accept the category name and stop asking
                    else { continueAsking = false; }
                }
            }
            return newPerson;
        }

        public void AddColorCategory(string newPerson)
        {
            Globals.ThrowIfNull().Ol.NamespaceMAPI.Categories.Add(newPerson, _prefix.Color, OlCategoryShortcutKey.olCategoryShortcutKeyNone);
        }

        public string SplitAddressToFirstLastName(string address)
        {
            var regex = new Regex(@"([a-zA-Z\d]+)[\._]([a-zA-Z]+)[\._]*([a-zA-z]*)\d*@([a-zA-z\d]+)(\.(com|ca|au|mx))+", RegexOptions.Multiline);
            var groups = regex.GetRegexGroups(address);
            StringBuilder sb = new StringBuilder();
            if (groups.IsNullOrEmpty() || groups[1].IsNullOrEmpty()) 
            {
                regex = new Regex(@"([a-zA-Z\d])([a-zA-Z\d]+)\d*(@)([a-zA-z\d]+)(\.(com|ca|au|mx))+", RegexOptions.Multiline);
                groups = regex.GetRegexGroups(address);
                if (groups.IsNullOrEmpty()) { return address; }
            }
            else if (!groups[2].IsNullOrEmpty())
            {
                groups[1] += $" {groups[2]}";
            }
            
            if (!groups[0].IsNullOrEmpty())
            {
                sb.Append(groups[0]);
                if (!groups[1].IsNullOrEmpty())
                {
                    sb.Append(" ");
                }
            }
            sb.Append(groups[1]);
            if (!groups[3].IsNullOrEmpty())
            {
                sb.Append(" -> ");
                sb.Append(groups[3]);
            }
            //string newPplTag = regex.Replace(address, ("$1 $2")).Trim();
            var newPplTag = sb.ToString();
            if (!newPplTag.IsNullOrEmpty())
            {
                newPplTag = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(newPplTag);
            }
            return newPplTag;
        }

        public string MatchToExisting(List<string> existingPeople, string newPerson)
        {
            var regex = new Regex(@" -> .+");
            var searchString = regex.Replace(newPerson, "");
            searchString = searchString.Replace(" ", "*");
            return Globals.TD.FindMatchingTag(existingPeople, _prefix, Globals.Ol.UserEmailAddress, searchString);
            //var launcher = new TagLauncher(existingPeople, _prefix, Globals.Ol.UserEmailAddress);
            //return launcher.FindMatch(searchString);
        }

        
    }
}
