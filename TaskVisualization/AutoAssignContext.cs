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
            var helper = await ToHelper(objItem);
            if (helper is null) { return []; }

            var project = await CategoryClassifierGroup.CreateEngineAsync(_globals, "Context", default).ConfigureAwait(true);
            project.ProbabilityThreshold = 0.2;            
            var results = project.GetMatchingCategories(helper).ToList();
            return results;
        }

        private async Task<MailItemHelper> ToHelper(object objItem)
        {
            MailItemHelper helper = null;
            if (objItem is MailItemHelper mailItemHelper)
            {
                helper = mailItemHelper;
            }
            else if (objItem is OutlookItem olItem)
            {
                if (olItem.InnerObject is MailItem mailItem)
                {
                    helper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, default, false).ConfigureAwait(true);                    
                }
            }
            else if (objItem is MailItem mailItem)
            {
                helper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, default, false).ConfigureAwait(true);                
            }

            if (helper is null) { return null; }
            else
            {
                await Task.Run(() => _ = helper.Tokens).ConfigureAwait(true);
                return helper;
            }
        }
    }
    
}
