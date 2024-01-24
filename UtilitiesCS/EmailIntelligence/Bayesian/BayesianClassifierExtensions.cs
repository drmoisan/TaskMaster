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
        #region BayesianClassifierShared

        public static async Task<BayesianClassifierShared> ToClassifierAsync(
            this BayesianClassifierGroup parent,
            string tag,
            IEnumerable<string> matchTokens,
            int emailCount,
            bool addToSharedTokens,
            CancellationToken token)
        {
            return await BayesianClassifierShared.FromTokenBaseAsync(parent, tag, 
                matchTokens.GroupAndCount(), emailCount, addToSharedTokens, token);
        }

        public static async Task<BayesianClassifierShared> ToClassifierAsync(
            this BayesianClassifierGroup parent,
            string tag,
            IDictionary<string, int> matchFrequency,
            int emailCount,
            bool addToSharedTokens,
            CancellationToken token)
        {
            return await BayesianClassifierShared.FromTokenBaseAsync(parent, tag,
                matchFrequency, emailCount, addToSharedTokens, token);
        }

        public static Dictionary<string, int> GroupAndCount(this IEnumerable<string> items)
        {
            return items.GroupBy(item => item)
            .ToDictionary(group => group.Key, group => group.Count());
        }

        public static async Task<Dictionary<string, int>> GroupAndCountAsync(
            this IEnumerable<string> items) 
        { 
            return await Task.Run(() => items.GroupBy(item => item)
                                             .ToDictionary(
                                                 group => group.Key, 
                                                 group => group.Count()));
        }

        #endregion BayesianClassifierShared

        #region OriginalBayesianClassifier
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
            IEnumerable<string> matchTokens,
            CancellationToken token)
        {
            return await BayesianClassifier.FromTokenBaseAsync(parent, tag, matchTokens, token);
        }

        #endregion OriginalBayesianClassifier

    }
}
