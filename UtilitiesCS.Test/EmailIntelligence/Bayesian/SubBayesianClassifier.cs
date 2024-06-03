using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    public class SubBayesianClassifier : BayesianClassifierShared, ICloneable
    {
        public SubBayesianClassifier() { }
        public SubBayesianClassifier(ConcurrentDictionary<string, double> prob)
        {
            base._prob = prob;
        }
        public SubBayesianClassifier(BayesianClassifierShared classifier) : base(classifier) { }

        public new ConcurrentDictionary<string, double> Prob { get => base._prob; set => base._prob = value; }

        public new SubClassifierGroup Parent
        {
            get => base._parent as SubClassifierGroup;
            set => base._parent = value;
        }

        public new SubCorpus Match { get => (base._match).ToCorpusSub(); set => base._match = value; }

        public object Clone()
        {
            var result = this.MemberwiseClone() as SubBayesianClassifier;
            result.Match = (SubCorpus)this.Match.Clone();
            result.Prob = new ConcurrentDictionary<string, double>(this.Prob ?? new ConcurrentDictionary<string, double>());
            result.Parent = (SubClassifierGroup)this.Parent.Clone();
            return result;
        }
    }
}
