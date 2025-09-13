using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace Tags
{

    public interface IAutoAssign
    {

        IList<string> AutoFind(object objItem);

        Task<IList<string>> AutoFindAsync(object objItem);

        IList<string> AddChoicesToDict(MailItem olMail, IList<IPrefix> prefixes, string prefixKey, string currentUserEmail);

        Category AddColorCategory(IPrefix prefix, string categoryName);

        IList<string> FilterList { get; }

    }
}