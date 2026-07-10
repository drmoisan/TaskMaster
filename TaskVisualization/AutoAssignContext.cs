using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Tags;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;

namespace TaskVisualization
{
    public class AutoAssignContext : IAutoAssign
    {
        private readonly IApplicationGlobals _globals;

        // MailItemHelper construction seam. Production default builds the helper
        // from a live MailItem; tests inject a stub (for example returning null) so
        // the host-neutral early-return branch of AutoFindAsync is measured.
        private readonly Func<object, Task<MailItemHelper>> _toHelper;

        public AutoAssignContext(
            IApplicationGlobals globals,
            Func<object, Task<MailItemHelper>> toHelper = null
        )
        {
            _globals = globals;
            _toHelper = toHelper ?? DefaultToHelper;
        }

        public IList<string> FilterList => _globals.TD.CategoryFilters;

        public IList<string> AddChoicesToDict(
            MailItem olMail,
            IList<IPrefix> prefixes,
            string prefixKey,
            string currentUserEmail
        )
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
            var helper = await _toHelper(objItem);
            if (helper is null)
            {
                return [];
            }

            return await RunContextClassifierAsync(helper);
        }

        // Classifier-engine invocation: runs the live Context category classifier.
        // Not unit-testable without the classifier engine / live data.
        [ExcludeFromCodeCoverage]
        private async Task<IList<string>> RunContextClassifierAsync(MailItemHelper helper)
        {
            var project = await CategoryClassifierGroup
                .CreateEngineAsync(_globals, "Context", default)
                .ConfigureAwait(true);
            project.ProbabilityThreshold = 0.2;
            var results = project.GetMatchingCategories(helper).ToList();
            return results;
        }

        // Outlook-bound default: constructs a MailItemHelper from a live MailItem.
        // Not unit-testable without a running Outlook process.
        [ExcludeFromCodeCoverage]
        private async Task<MailItemHelper> DefaultToHelper(object objItem)
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
                    helper = await MailItemHelper
                        .FromMailItemAsync(mailItem, _globals, default, false)
                        .ConfigureAwait(true);
                }
            }
            else if (objItem is MailItem mailItem)
            {
                helper = await MailItemHelper
                    .FromMailItemAsync(mailItem, _globals, default, false)
                    .ConfigureAwait(true);
            }

            if (helper is null)
            {
                return null;
            }
            else
            {
                await Task.Run(() => _ = helper.Tokens).ConfigureAwait(true);
                return helper;
            }
        }
    }
}
