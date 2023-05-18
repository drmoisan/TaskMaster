using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using UtilitiesVB;

namespace Tags
{

    public interface IAutoAssign
    {

        IList<string> AutoFind(object objItem);

        IList<string> AddChoicesToDict(MailItem olMail, List<IPrefix> prefixes, string prefixKey, string currentUserEmail);

        Category AddColorCategory(IPrefix prefix, string categoryName);

        List<string> FilterList { get; }

    }
}