using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Flags
{
    public class FlagConsolidator
    {
        public FlagConsolidator(FlagParser parser)
        {            
            Parser = parser.ThrowIfNull();
            ResetAll();
        }

        #region Consolidation Permutations

        public IList<string> AsListWithPrefix { get => Refreshable(_asListWithPrefix); set => _asListWithPrefix.ToLazy(); }
        protected Lazy<IList<string>> _asListWithPrefix;        
        internal virtual void ResetLazyListWithPrefix()
        {
            _asListWithPrefix = new Lazy<IList<string>>(() => CombineLists(true));
        }
                        
        public IList<string> AsListNoPrefix { get => Refreshable(_asListNoPrefix); set => _asListNoPrefix = value.ToLazy(); }
        protected Lazy<IList<string>> _asListNoPrefix;
        internal virtual void ResetLazyListNoPrefix()
        {
            _asListNoPrefix = new Lazy<IList<string>>(() => CombineLists(false));
        }

        public string AsStringWithPrefix { get => Refreshable(_asStringWithPrefix); set => _asStringWithPrefix = value.ToLazy(); }
        protected Lazy<string> _asStringWithPrefix;
        internal virtual void ResetStringWithPrefix()
        {
            _asStringWithPrefix = new Lazy<string>(() => string.Join(", ", AsListWithPrefix));
        }

        public string AsStringNoPrefix { get => Refreshable(_asStringNoPrefix); set => _asStringNoPrefix = value.ToLazy(); }
        protected Lazy<string> _asStringNoPrefix;
        internal virtual void ResetStringNoPrefix()
        {
            _asStringNoPrefix = new Lazy<string>(() => string.Join(", ", AsListNoPrefix));
        }

        private T Refreshable<T>(Lazy<T> lazy)
        {
            T value = lazy.Value;
            _guard = new();
            return value;
        }

        #endregion Consolidation Permutations

        public void RequestUpdate()
        {
            if (_guard.CheckAndSetFirstCall)
            {
                ResetAll();
            }
        }

        #region Internal Helpers

        internal FlagParser Parser { get; set; }
        internal virtual List<string> CombineLists(bool wtag = true)
        {
            List<IList<string>> list = [Parser.People.ListWithPrefix, Parser.Projects.ListWithPrefix, 
                Parser.Topics.ListWithPrefix, Parser.Context.ListWithPrefix, Parser.Kb.ListWithPrefix];

            if (!Parser.Other.IsNullOrEmpty()) { list.Add([.. Parser.Other.Split(separator: ',', trim: true)]); }
            if (Parser.Today) { list.Add([Properties.Settings.Default.Prefix_Today]); }
            if (Parser.Bullpin) { list.Add([Properties.Settings.Default.Prefix_Bullpin]); }

            var flat = list.SelectMany(x => x).OrderBy(x => x).ToList();
            _guard = new();
            return flat;
        }
        internal virtual void ResetAll()
        {
            ResetLazyListWithPrefix();
            ResetLazyListNoPrefix();
            ResetStringWithPrefix();
            ResetStringNoPrefix();
        }

        protected ThreadSafeSingleShotGuard _guard = new ThreadSafeSingleShotGuard();

        #endregion Internal Helpers
    }
}
