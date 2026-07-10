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

        // MAPI category-creation seam. Production default calls the live
        // CreateCategoryModule against the running Outlook namespace; tests inject a
        // stub so the forwarding of prefix/categoryName is measured without a live
        // MAPI process.
        private readonly Func<IPrefix, string, Category> _createCategory;

        public AutoAssignPeople(
            IApplicationGlobals globals,
            Func<object, MailItemHelper> toHelper = null,
            Func<IPrefix, string, Category> createCategory = null
        )
        {
            _globals = globals;
            _toHelper = toHelper ?? DefaultToHelper;
            _createCategory = createCategory ?? DefaultCreateCategory;
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

        public IList<string> AddChoicesToDict(
            MailItem olMail,
            IList<IPrefix> prefixes,
            string prefixKey,
            string currentUserEmail
        )
        {
            return _globals.TD.People.AddMissingEntries(olMail);
        }

        public Category AddColorCategory(IPrefix prefix, string categoryName)
        {
            return _createCategory(prefix, categoryName);
        }

        // MAPI-bound default: creates a live Outlook category. Not unit-testable
        // without a running Outlook process.
        [ExcludeFromCodeCoverage]
        private Category DefaultCreateCategory(IPrefix prefix, string categoryName)
        {
            return CreateCategoryModule.CreateCategory(
                olNS: _globals.Ol.NamespaceMAPI,
                prefix: prefix,
                newCatName: categoryName
            );
        }
    }
}
