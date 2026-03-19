using System.Collections.Generic;
using System.Windows.Input;
using ConcurrentObservableCollection.ConcurrentObservableDictionary;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    //public interface IPeopleScoDictionaryNew : ISmartSerializable<ScoDictionaryNew<string, string>>, IConcurrentObservableDictionary<string, string>
    //IScoDictionaryNew<TKey, TValue>
    public interface IPeopleScoDictionaryNew : IScoDictionaryNew<string, string>
    {
        #region IPeopleScoDictionaryNew Specific

        IPrefix Prefix { get; set; }
        void AddColorCategory(string newPerson);
        IList<string> AddMissingEntries(MailItem olMail);
        string AddMissingEntry(string address);
        string AddPrefix(string seed, string prefix);
        bool CategoryExists(string category);
        List<string> GetPeopleCatNames();
        bool IsPeopleCategory(string test);
        string MatchToExisting(List<string> existingPeople, string newPerson);
        string RefineValidateCategory(string newPerson, IPrefix prefix);
        string SplitAddressToFirstLastName(string address);

        #endregion IPeopleScoDictionaryNew Specific
    }
}
