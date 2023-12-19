using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class Corpus
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public Corpus() { _tokenCounts = [];  }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection) { _tokenCounts = new ConcurrentDictionary<string, int>(collection); }
        public Corpus(IEqualityComparer<string> comparer) { _tokenCounts = new ConcurrentDictionary<string, int>(comparer); }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { _tokenCounts = new ConcurrentDictionary<string, int>(collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity) { _tokenCounts = new ConcurrentDictionary<string, int>(concurrencyLevel, capacity); }
        public Corpus(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, capacity, comparer); }

        #endregion Constructors

        public ConcurrentDictionary<string, int> TokenCounts { get => _tokenCounts; protected set => _tokenCounts = value; }
        private ConcurrentDictionary<string, int> _tokenCounts;

        #region Public Properties and Methods

        //private string _id;
        //public string Id { get => _id; set => _id = value; }

        public Enums.Corpus Indicator { get => _indicator; set => _indicator = value; }
        private Enums.Corpus _indicator;

        public void AddOrIncrementToken(string token) => TokenCounts.AddOrUpdate(token, 1, (key, count) => count++);

        public void AddOrIncrementTokens(IEnumerable<string> tokens) => tokens.ForEach(AddOrIncrementToken);

        public void DecrementOrRemoveToken(string token)
        {
            lock (_tokenCounts)
            {
                if (_tokenCounts.TryGetValue(token, out int count))
                {
                    if (--count == 0)
                    {
                        _tokenCounts.TryRemove(token, out _);
                    }
                    else
                    {
                        _tokenCounts[token] = count;
                    }
                }
            }
        }

        #endregion Public Properties and Methods

    }
}
