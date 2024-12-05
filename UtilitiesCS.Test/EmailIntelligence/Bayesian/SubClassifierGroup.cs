using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    public class SubClassifierGroup : BayesianClassifierGroup, ICloneable
    {
        public SubClassifierGroup() { }

        [Obsolete("This method is not used in the current implementation of the Bayesian Classifier.")]
        public SubClassifierGroup(
            ConcurrentDictionary<string, DedicatedToken> dedicated,
            Corpus sharedTokenBase)
        {
            base._sharedTokenBase = sharedTokenBase;
            base._totalEmailCount = sharedTokenBase.TokenCount + dedicated.Sum(x => x.Value.Count);
        }

        public new virtual SubCorpus SharedTokenBase { get => (SubCorpus)base._sharedTokenBase; set => base._sharedTokenBase = value; }

        public object Clone()
        {
            var result = this.MemberwiseClone() as SubClassifierGroup;
            result.SharedTokenBase = (SubCorpus)this.SharedTokenBase.Clone();
            return result;
        }
    }
}
