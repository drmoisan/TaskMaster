using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// Pure delegate-wiring implementation of <see cref="IAutoAssign"/>, extracted from
    /// <see cref="TagLauncher"/> so its pass-through logic is unit-testable without a live form or
    /// Outlook host. Each member forwards to its injected <see cref="Func{T, TResult}"/>. This type
    /// is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is testable and must meet the
    /// coverage floor.
    /// </summary>
    internal class LauncherAutoAssign : IAutoAssign
    {
        public LauncherAutoAssign() { }

        public LauncherAutoAssign(
            IList<string> filterList,
            Func<MailItem, IList<string>> addChoicesToDictDelegate,
            Func<IPrefix, string, Category> addColorCategoryDelegate,
            Func<object, IList<string>> autoFindDelegate
        )
        {
            _filterList = filterList;
            _addChoicesToDictDelegate = addChoicesToDictDelegate;
            _addColorCategoryDelegate = addColorCategoryDelegate;
            _autoFindDelegate = autoFindDelegate;
        }

        private IList<string> _filterList;
        public IList<string> FilterList
        {
            get => _filterList;
            set => _filterList = value;
        }

        private Func<MailItem, IList<string>> _addChoicesToDictDelegate;
        public Func<MailItem, IList<string>> AddChoicesToDictDelegate
        {
            get => _addChoicesToDictDelegate;
            set => _addChoicesToDictDelegate = value;
        }

        public IList<string> AddChoicesToDict(
            MailItem olMail,
            IList<IPrefix> prefixes,
            string prefixKey,
            string currentUserEmail
        )
        {
            return _addChoicesToDictDelegate(olMail);
        }

        private Func<IPrefix, string, Category> _addColorCategoryDelegate;
        public Func<IPrefix, string, Category> AddColorCategoryDelegate
        {
            get => _addColorCategoryDelegate;
            set => _addColorCategoryDelegate = value;
        }

        public Category AddColorCategory(IPrefix prefix, string categoryName)
        {
            return _addColorCategoryDelegate(prefix, categoryName);
        }

        private Func<object, IList<string>> _autoFindDelegate;
        public Func<object, IList<string>> AutoFindDelegate
        {
            get => _autoFindDelegate;
            set => _autoFindDelegate = value;
        }

        public IList<string> AutoFind(object objItem)
        {
            return _autoFindDelegate(objItem);
        }

        public Task<IList<string>> AutoFindAsync(object objItem)
        {
            try
            {
                return Task.Run(() => AutoFind(objItem));
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Pure factory that builds a <see cref="LauncherAutoAssign"/> from a filter list and the
        /// three assignment delegates. Extracted from <see cref="TagLauncher"/> (register-testable).
        /// </summary>
        public static IAutoAssign GetAutoAssign(
            IList<string> filterList,
            Func<MailItem, IList<string>> addChoicesToDictDelegate,
            Func<IPrefix, string, Category> addColorCategoryDelegate,
            Func<object, IList<string>> autoFindDelegate
        )
        {
            return new LauncherAutoAssign(
                filterList,
                addChoicesToDictDelegate,
                addColorCategoryDelegate,
                autoFindDelegate
            );
        }
    }
}
