using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tags;
using UtilitiesCS;

namespace TaskVisualization
{
    internal class AutoCreateProject(IApplicationGlobals globals) : IAutoAssign
    {
        private readonly IApplicationGlobals _globals = globals;

        public IList<string> FilterList => throw new NotImplementedException();

        public IList<string> AddChoicesToDict(MailItem olMail, IList<IPrefix> prefixes, string prefixKey, string currentUserEmail)
        {            
            throw new NotImplementedException();
        }

        public Category AddColorCategory(IPrefix prefix, string categoryName)
        {
            
            throw new NotImplementedException();
        }

        public IList<string> AutoFind(object objItem)
        {
            throw new NotImplementedException();
        }
    }
}
