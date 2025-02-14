using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tags;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;

namespace TaskVisualization
{
    public class AutoAssignContext: IAutoAssign
    {
        private readonly IApplicationGlobals _globals;
        public AutoAssignContext(IApplicationGlobals globals)
        {
            _globals = globals;
        }
        public IList<string> FilterList => _globals.TD.CategoryFilters;
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
        public async Task<IList<string>> AutoFindAsync(object objItem)
        {
            MailItem mailItem = null;
            if (objItem is OutlookItem olItem) { mailItem = olItem.InnerObject as MailItem; }
            else if (objItem is MailItem mail) { mailItem = mail; }
            if (mailItem is null) { return null; }
            var project = await CategoryClassifierGroup.CreateEngineAsync(_globals, "Context", default).ConfigureAwait(true);
            project.ProbabilityThreshold = 0.2;
            var helper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, default, true).ConfigureAwait(true);
            var results = project.GetMatchingCategories(helper).ToList();
            return results;

            
        }
    }
    
}
