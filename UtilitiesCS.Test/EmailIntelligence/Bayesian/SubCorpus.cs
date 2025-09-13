using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    public class SubCorpus : Corpus
    {
        public SubCorpus() { }
        public SubCorpus(Corpus corpus) : base(corpus) { }
        public SubCorpus(IEnumerable<KeyValuePair<string, int>> tb) : base(tb) { }
        public SubCorpus(ConcurrentDictionary<string, int> tb)
        {
            this.TokenFrequency = tb;
            this.TokenCount = tb.Sum(x => x.Value);
        }

        public void SetTokenBase(ConcurrentDictionary<string, int> tb)
        {
            this.TokenFrequency = tb;
        }

        public new virtual ConcurrentDictionary<string, int> TokenFrequency
        {
            get => base._tokenFrequency;
            set => base._tokenFrequency = value;
        }
    }
}
