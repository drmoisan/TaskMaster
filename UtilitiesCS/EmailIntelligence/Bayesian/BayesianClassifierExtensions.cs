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
            this Corpus tokenBase, 
            string tag,
            IEnumerable<string> positiveTokens)
        {
            return BayesianClassifier.FromTokenBase(tokenBase, tag, positiveTokens);
        }

        public static async Task<BayesianClassifier> ToClassifierAsync(
            this Corpus tokenBase,
            string tag,
            IEnumerable<string> positiveTokens,
            CancellationToken token)
        {
            return await BayesianClassifier.FromTokenBaseAsync(tokenBase, tag, positiveTokens, token);
        }
    }
}
