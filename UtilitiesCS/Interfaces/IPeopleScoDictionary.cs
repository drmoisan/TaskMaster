using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public interface IPeopleScoDictionary: IScoDictionary<string, string>
    {
        IPrefix Prefix { get; set; }
        IList<string> AddMissingEntries(MailItem olMail);
        List<string> GetPeopleCatNames();
        bool IsPeopleCategory(string test);
        string MatchToExisting(List<string> existingPeople, string newPerson);
        string SplitAddressToFirstLastName(string address);
    }
}