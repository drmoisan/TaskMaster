//using Microsoft.Office.Interop.Outlook;
////using Microsoft.TeamFoundation.Common;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Tags;
//using UtilitiesCS;
//using UtilitiesCS.ReusableTypeClasses;
//using Outlook = Microsoft.Office.Interop.Outlook;

//namespace ToDoModel
//{
//    public class PeopleScoDictionary : ScoDictionary<string, string>, IPeopleScoDictionary
//    {
//        #region Constructors and Initializers

//        //public PeopleScoDictionary(IApplicationGlobals appGlobals, IPrefix prefix) : base()
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(IDictionary<string, string> source, IApplicationGlobals appGlobals, IPrefix prefix) : base(source)
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(IEqualityComparer<string> equalityComparer,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(equalityComparer)
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(int capactity,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(capactity)
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(IDictionary<string, string> source,
//        //                           IEqualityComparer<string> equalityComparer,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(source, equalityComparer)
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(int capacity,
//        //                           IEqualityComparer<string> equalityComparer,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(capacity, equalityComparer)
//        //{ Initialize(appGlobals, prefix); }

//        public PeopleScoDictionary(string filename,
//                                   string folderpath,
//                                   IApplicationGlobals appGlobals,
//                                   IPrefix prefix) : base(filename, folderpath)
//        { Initialize(appGlobals, prefix); }

//        /// <summary>
//        /// Creates a new serializable concurrent observable dictionary from an existing dictionary and filepath
//        /// </summary>
//        /// <param name="dictionary">Existing dictionary</param>
//        /// <param name="filename">Name of json file to house the PeopleScoDictionary</param>
//        /// <param name="folderpath">Location of json file</param>
//        /// <param name="appGlobals">Reference to global variables</param>
//        /// <param name="prefix">Reference to class implementing <seealso cref="IPrefix"/> interface</param>
//        //public PeopleScoDictionary(IDictionary<string, string> dictionary,
//        //                           string filename,
//        //                           string folderpath,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(dictionary, filename, folderpath)
//        //{ Initialize(appGlobals, prefix); }

//        //public PeopleScoDictionary(string filename,
//        //                           string folderpath,
//        //                           IScoDictionary<string, string>.AltLoader backupLoader,
//        //                           string backupFilepath,
//        //                           bool askUserOnError,
//        //                           IApplicationGlobals appGlobals,
//        //                           IPrefix prefix) : base(filename, folderpath, backupLoader, backupFilepath, askUserOnError)
//        //{ Initialize(appGlobals, prefix); }

//        internal void Initialize(IApplicationGlobals appGlobals, IPrefix prefix)
//        {
//            _globals = appGlobals;
//            _prefix = prefix;
//        }

//        #endregion

//        private IApplicationGlobals _globals;

//        [JsonIgnore]
//        private IPrefix _prefix;
//        public IPrefix Prefix { get => _prefix; set => _prefix = value; }

//        public bool IsPeopleCategory(string test)
//        {
//            return (test is not null) && (test.Length >= _prefix.Value.Length) && (test.Substring(0, _prefix.Value.Length) == _prefix.Value);
//        }

//        public List<string> GetPeopleCatNames()
//        {
//            return _globals.Ol.App.Session.Categories.Cast<Outlook.Category>().Where(cat => IsPeopleCategory(cat.Name)).Select(cat => cat.Name).ToList();
//        }

//        public bool CategoryExists(string category)
//        {
//            return _globals.Ol.App.Session.Categories.Cast<Outlook.Category>().Any(cat => cat.Name == category);
//        }

//        public IList<string> AddMissingEntries(Outlook.MailItem olMail)
//        {
//            var addressList = olMail.GetEmailAddresses(_globals.Ol.EmailRootPath,
//                                                       _globals.TD.DictRemap,
//                                                       _globals.Ol.UserEmailAddress)
//                                                       .Where(x => !this.ContainsKey(x))
//                                                       .Select(x => x)
//                                                       .ToList();
//            IList<string> newPeople = new List<string>();
            
//            foreach (var address in addressList)
//            {
//                var response = MessageBox.Show($"Add mapping for {address}?", "Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
//                if (response == DialogResult.Cancel) { break; }
//                if (response == DialogResult.Yes)
//                {
//                    var entry = AddMissingEntry(address);
//                    if (entry is not null) { newPeople.Add(entry); }
//                }
//            }
//            if (!newPeople.IsNullOrEmpty()) { this.Serialize(); }
//            return newPeople;            
//        }

//        public string AddMissingEntry(string address) //internal
//        {
//            var newPerson = SplitAddressToFirstLastName(address);
//            var existingPeople = GetPeopleCatNames();
//            var matchResult = MatchToExisting(existingPeople, newPerson);
//            if (matchResult.IsNullOrEmpty())
//            {
//                newPerson = RefineValidateCategory(newPerson, _prefix);
//                if (newPerson is null) { return null; }
//                AddColorCategory(newPerson);
//                return newPerson;
//            }
//            else
//            {
//                this.Add(address, matchResult);
//                return matchResult;
//            }
//        }

//        public string AddPrefix(string seed, string prefix)
//        {
//            if (seed is null) { throw new ArgumentNullException(nameof(seed)); }
//            if (prefix is null) { throw new ArgumentNullException(nameof(prefix)); }
//            if (seed.StartsWith(prefix)) { return seed; }
//            else { return $"{prefix}{seed}"; }
//        }
        
//        public string RefineValidateCategory(string newPerson, IPrefix prefix) 
//        {
//            bool continueAsking = true;
//            while (continueAsking)
//            {
//                newPerson = InputBox.ShowDialog("The following category name will be added:", "Add Category Dialog", DefaultResponse: newPerson);
//                // if user cancels, return null
//                if (newPerson is null) { continueAsking = false; }
//                // if user leaves blank, continue asking
//                else if (newPerson == "") { continueAsking = true; }
//                // else check if input is valid
//                else 
//                {
//                    // Add prefix if not already there
//                    newPerson = AddPrefix(newPerson, prefix.Value);
//                    // if category already exists, tell the user and continue asking
//                    if (CategoryExists(newPerson)) 
//                    { 
//                        MessageBox.Show($"Category {newPerson} already exists. Please choose another name.", "Category Exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                        continueAsking = true;
//                    }
//                    // else accept the category name and stop asking
//                    else { continueAsking = false; }
//                }
//            }
//            return newPerson;
//        }

//        public void AddColorCategory(string newPerson) 
//        { 
//            _globals.Ol.NamespaceMAPI.Categories.Add(newPerson, _prefix.Color, OlCategoryShortcutKey.olCategoryShortcutKeyNone);
//        }

//        public string SplitAddressToFirstLastName(string address)
//        {
//            var regex = new Regex(@"([a-zA-z\d]+)\.([a-zA-z]+)\d*@([a-zA-z\d]+)\.com", RegexOptions.Multiline);
//            string newPplTag = regex.Replace(address, ("$1 $2")).Trim();
//            if (!newPplTag.IsNullOrEmpty())
//            {
//                newPplTag = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(newPplTag);
//            }
//            return newPplTag;
//        }

//        public string MatchToExisting(List<string> existingPeople, string newPerson)
//        {
//            var searchString = newPerson.Replace(" ", "*");
//            var launcher = new TagLauncher(existingPeople, _prefix, _globals.Ol.UserEmailAddress);
//            return launcher.FindMatch(searchString);
//        }
//    }
//}
