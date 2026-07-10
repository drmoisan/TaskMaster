using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace TaskVisualization
{
    internal class AutoAssignPeople : IAutoAssign
    {
        private readonly IApplicationGlobals _globals;

        // Synchronous MailItemHelper construction seam. Production default builds
        // the helper from a live MailItem; tests inject a stub so the host-neutral
        // branch selection in AutoFind is measured without a live Outlook process.
        private readonly Func<object, MailItemHelper> _toHelper;

        public AutoAssignPeople(
            IApplicationGlobals globals,
            Func<object, MailItemHelper> toHelper = null
        )
        {
            _globals = globals;
            _toHelper = toHelper ?? DefaultToHelper;
        }

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
            MailItemHelper helper = null;
            if (objItem is null)
            {
                return [];
            }
            else if (objItem is MailItemHelper)
            {
                helper = objItem as MailItemHelper;
            }
            else if (
                objItem is IOutlookItem olItem
                && olItem.GetOlItemType() == OlItemType.olMailItem
            )
            {
                helper = _toHelper(olItem.InnerObject);
            }
            else if (objItem is MailItem olMail)
            {
                helper = _toHelper(olMail);
            }
            else
            {
                return [];
            }

            return RunPeopleClassifier(helper);
        }

        // Recipient-matching over a live MailItemHelper; enumerates COM recipient
        // collections and may show a "missing recipients" dialog. Not unit-testable
        // without live Outlook data or a form.
        [ExcludeFromCodeCoverage]
        private IList<string> RunPeopleClassifier(MailItemHelper helper)
        {
            return AutoFile.AutoFindPeople(helper, _globals.TD.People, true, false);

            //return AutoFile.AutoFindPeople(
            //        objItem: objItem,
            //        ppl_dict: _globals.TD.People,
            //        emailRootFolder: _globals.Ol.InboxPath,
            //        dictRemap: _globals.TD.DictRemap,
            //        userAddress: _globals.Ol.UserEmailAddress,
            //        blExcludeFlagged: false);
        }

        // Outlook-bound default: constructs a MailItemHelper from a live MailItem.
        // Not unit-testable without a running Outlook process.
        [ExcludeFromCodeCoverage]
        private MailItemHelper DefaultToHelper(object mailItem)
        {
            return new MailItemHelper(mailItem as MailItem, _globals);
        }

        // Outlook-bound: reads recipients from a live MailItem to add missing
        // people entries. Not unit-testable without a running Outlook process.
        [ExcludeFromCodeCoverage]
        public IList<string> AddChoicesToDict(
            MailItem olMail,
            IList<IPrefix> prefixes,
            string prefixKey,
            string currentUserEmail
        )
        {
            return _globals.TD.People.AddMissingEntries(olMail);
        }

        // MAPI-bound: creates a live Outlook category. Not unit-testable without a
        // running Outlook process.
        [ExcludeFromCodeCoverage]
        public Category AddColorCategory(IPrefix prefix, string categoryName)
        {
            return CreateCategoryModule.CreateCategory(
                olNS: _globals.Ol.NamespaceMAPI,
                prefix: prefix,
                newCatName: categoryName
            );
        }
    }
}
