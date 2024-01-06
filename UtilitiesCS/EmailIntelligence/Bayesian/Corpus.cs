using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    [Serializable]
    public class Corpus: ICloneable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public Corpus() { _tokenCounts = [];  }
        public Corpus(IEnumerable<string> tokens) { _tokenCounts = new(); AddOrIncrementTokens(tokens); }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection) { _tokenCounts = new(collection); }
        public Corpus(IEqualityComparer<string> comparer) { _tokenCounts = new ConcurrentDictionary<string, int>(comparer); }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { _tokenCounts = new ConcurrentDictionary<string, int>(collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity) { _tokenCounts = new ConcurrentDictionary<string, int>(concurrencyLevel, capacity); }
        public Corpus(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, capacity, comparer); }

        #endregion Constructors

        #region Public Properties and Methods

        public ConcurrentDictionary<string, int> TokenCounts { get => _tokenCounts; protected set => _tokenCounts = value; }
        private ConcurrentDictionary<string, int> _tokenCounts;
        
        public Enums.Corpus Indicator { get => _indicator; set => _indicator = value; }
        private Enums.Corpus _indicator;

        public void AddOrIncrementToken(string token) => TokenCounts.AddOrUpdate(token, 1, (key, count) => ++count);

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

        public object Clone()
        {
            var result = this.MemberwiseClone() as Corpus;
            result.TokenCounts = new ConcurrentDictionary<string, int>(this.TokenCounts);
            return result;
        }

        #endregion Public Properties and Methods

        #region Operator Overloads

        public static Corpus operator +(Corpus c1, Corpus c2)
        {
            var result = c1.Clone() as Corpus;
            
            foreach (var kvp in c2.TokenCounts)
            {
                result.TokenCounts.AddOrUpdate(kvp.Key, kvp.Value, (key, count) => count + kvp.Value);
            }
            return result;
        }

        public static async Task<Corpus> SubtractAsync(
            Corpus c1, 
            Corpus c2, 
            CancellationToken token, 
            SegmentStopWatch sw = null)
        {
            sw ??= new SegmentStopWatch().Start();
            sw.LogDuration("SubtractAsync Start");

            var result = c1.Clone() as Corpus;
            sw.LogDuration("clone universe");

            var processors = Math.Max(Environment.ProcessorCount - 2, 1);
            var chunkSize = (int)Math.Round((double)c2.TokenCounts.Count() / (double)processors, 0);
            if (chunkSize == 0)
                return result;
            chunkSize = Math.Min(Math.Max(chunkSize, 50),c2.TokenCounts.Count());
            var chunks = c2.TokenCounts.Chunk(chunkSize);
            sw.LogDuration("chunk positive tokens");

            var tasks = chunks.Select(chunk => Task.Run(() => chunk.ForEach(x => 
            { 
                if (result.TokenCounts.TryGetValue(x.Key, out int count))
                {
                    if (count > x.Value)
                    {
                        result.TokenCounts.TryUpdate(x.Key, count - x.Value, count);
                    }
                    else
                    {
                        result.TokenCounts.TryRemove(x.Key, out _);
                    }
                }
            }), 
            token));

            await Task.WhenAll(tasks);
            sw.LogDuration("subtract positive tokens");

            return result;
        }

        public static Corpus operator -(Corpus c1, Corpus c2)
        {
            var result = c1.Clone() as Corpus;
            foreach (var kvp in c2.TokenCounts)
            {
                if (result.TokenCounts.TryGetValue(kvp.Key, out int count))
                {
                    if (count > kvp.Value)
                    {
                        result.TokenCounts.TryUpdate(kvp.Key, count - kvp.Value, count);
                    }
                    else
                    {
                        result.TokenCounts.TryRemove(kvp.Key, out _);
                    }
                }
            }
            return result;
        }

        #endregion Operator Overloads

    }
}
