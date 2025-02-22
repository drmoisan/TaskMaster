using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace TaskVisualization
{
    internal class AutoAssignPeople(IApplicationGlobals globals) : IAutoAssign
    {
        private readonly IApplicationGlobals _globals = globals;

        public IList<string> FilterList
        {
            get => [.. _globals.TD.CategoryFilters];
        }

        public async Task<IList<string>> AutoFindAsync(object objItem)
        {
            try
            {
                return await Task.Run(() => AutoFind(objItem)).ConfigureAwait(true);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public IList<string> AutoFind(object objItem)
        {
            if (objItem is IOutlookItem olItem && olItem.GetOlItemType() == OlItemType.olMailItem)
            {
                objItem = olItem.InnerObject;                
            }
            else if (objItem is not MailItem) { return []; }
            
            return AutoFile.AutoFindPeople(
                    objItem: objItem,
                    ppl_dict: _globals.TD.People,
                    emailRootFolder: _globals.Ol.InboxPath,
                    dictRemap: _globals.TD.DictRemap,
                    userAddress: _globals.Ol.UserEmailAddress,
                    blExcludeFlagged: false);            

        }

        public IList<string> AddChoicesToDict(MailItem olMail, IList<IPrefix> prefixes, string prefixKey, string currentUserEmail)
        {
            return _globals.TD.People.AddMissingEntries(olMail);
        }

        public Category AddColorCategory(IPrefix prefix, string categoryName)
        {
            return CreateCategoryModule.CreateCategory(olNS: _globals.Ol.NamespaceMAPI, prefix: prefix, newCatName: categoryName);
        }
    }

}
