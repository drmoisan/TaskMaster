using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public static class BayesianClassifierExtensions
    {
        public static BayesianClassifier ToClassifier(
            this ClassifierGroup parent, 
            string tag,
            IEnumerable<string> positiveTokens)
        {
            return BayesianClassifier.FromTokenBase(parent, tag, positiveTokens);
        }

        public static async Task<BayesianClassifier> ToClassifierAsync(
            this ClassifierGroup parent,
            string tag,
            IEnumerable<string> positiveTokens,
            CancellationToken token)
        {
            return await BayesianClassifier.FromTokenBaseAsync(parent, tag, positiveTokens, token);
        }
    }
}
