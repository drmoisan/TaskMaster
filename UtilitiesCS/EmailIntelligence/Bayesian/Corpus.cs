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

        public Corpus() { _tokenFrequency = [];  }
        public Corpus(IEnumerable<string> tokens) { _tokenFrequency = new(); AddOrIncrementTokens(tokens); }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection) { _tokenFrequency = new(collection); }
        public Corpus(IEqualityComparer<string> comparer) { _tokenFrequency = new ConcurrentDictionary<string, int>(comparer); }
        public Corpus(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { _tokenFrequency = new ConcurrentDictionary<string, int>(collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity) { _tokenFrequency = new ConcurrentDictionary<string, int>(concurrencyLevel, capacity); }
        public Corpus(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, collection, comparer); }
        public Corpus(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, capacity, comparer); }

        #endregion Constructors

        #region Public Properties and Methods

        public enum ActionEnum 
        { 
            Failed = 0, 
            ValueUpdated = 1, 
            ItemAdded = 2, 
            ItemRemoved = 3 
        }
        
        public ConcurrentDictionary<string, int> TokenFrequency { get => _tokenFrequency; protected set => _tokenFrequency = value; }
        private ConcurrentDictionary<string, int> _tokenFrequency;

        public int TokenCount { get => _tokenCount; protected set => _tokenCount = value; }
        private int _tokenCount;

        public Enums.Corpus Indicator { get => _indicator; set => _indicator = value; }
        private Enums.Corpus _indicator;

        public int AddTokenCount(int increment)
        {
            return Interlocked.Add(ref _tokenCount, increment);
        }

        public void AddOrIncrementToken(string token) 
        { 
            TokenFrequency.AddOrUpdate(token, 1, (key, count) => ++count); 
        }

        public void AddOrIncrementTokens(IEnumerable<string> tokens) => tokens.ForEach(AddOrIncrementToken);

        public bool DecrementOrRemoveToken(string token)
        {
            if (_tokenFrequency.TryGetValue(token, out int count))
            {
                Interlocked.Decrement(ref _tokenCount);
                if (Interlocked.Decrement(ref count) == 0)
                {
                    _tokenFrequency.TryRemove(token, out _);
                    return false;
                }
                else
                {
                    _tokenFrequency[token] = count;
                    return true;
                }
            }
            else { return false; }            
        }

        public void AddTokenOrSumValues(IEnumerable<KeyValuePair<string, int>> tokenFrequency)
        {
            foreach (var kvp in tokenFrequency)
            {
                this.AddOrSumTokenValue(kvp.Key, kvp.Value);
            }
        }

        public int AddOrSumTokenValue(string token, int value)
        {
            return TokenFrequency.AddOrUpdate(token, value, (key, count) => count + value);
        }

        public void SubtractOrRemoveValues(IEnumerable<KeyValuePair<string, int>> tokenFrequency)
        {
            foreach (var kvp in tokenFrequency)
            {
                SubtractOrRemoveValue(kvp.Key, kvp.Value);
            }
        }

        public void SubtractOrRemoveValue(string token, int value)
        {
            _tokenFrequency.UpdateOrRemove(
                token,
                (key, oldValue) => oldValue - value <= 0,
                (key, oldValue) => oldValue - value,
                out _);
        }

        public object Clone()
        {
            var result = this.MemberwiseClone() as Corpus;
            result.TokenFrequency = new ConcurrentDictionary<string, int>(this.TokenFrequency);
            return result;
        }

        #endregion Public Properties and Methods

        #region Operator Overloads

        public static Corpus operator +(Corpus c1, Corpus c2)
        {
            var result = c1.Clone() as Corpus;
            
            foreach (var kvp in c2.TokenFrequency)
            {
                result.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value, (key, count) => count + kvp.Value);
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
            var chunkSize = (int)Math.Round((double)c2.TokenFrequency.Count() / (double)processors, 0);
            if (chunkSize == 0)
                return result;
            chunkSize = Math.Min(Math.Max(chunkSize, 50),c2.TokenFrequency.Count());
            var chunks = c2.TokenFrequency.Chunk(chunkSize);
            sw.LogDuration("chunk positive tokens");

            var tasks = chunks.Select(chunk => Task.Run(() => chunk.ForEach(x => 
            { 
                if (result.TokenFrequency.TryGetValue(x.Key, out int count))
                {
                    if (count > x.Value)
                    {
                        result.TokenFrequency.TryUpdate(x.Key, count - x.Value, count);
                    }
                    else
                    {
                        result.TokenFrequency.TryRemove(x.Key, out _);
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
            foreach (var kvp in c2.TokenFrequency)
            {
                if (result.TokenFrequency.TryGetValue(kvp.Key, out int count))
                {
                    if (count > kvp.Value)
                    {
                        result.TokenFrequency.TryUpdate(kvp.Key, count - kvp.Value, count);
                    }
                    else
                    {
                        result.TokenFrequency.TryRemove(kvp.Key, out _);
                    }
                }
            }
            return result;
        }

        public static (Corpus NotMatchFiltered, Corpus MatchFiltered) SubtractFilter(Corpus all, Corpus match, int negTokenWt, int minCt)
        {
            var result = all.Clone() as Corpus;
            var matchClone = match.Clone() as Corpus;

            result.TokenFrequency = new ConcurrentDictionary<string, int>(result.TokenFrequency.Where(x => x.Value * negTokenWt >= minCt));
            foreach (var kvp in matchClone.TokenFrequency)
            {
                if (result.TokenFrequency.TryGetValue(kvp.Key, out int count))
                {
                    if (count > kvp.Value)
                    {
                        result.TokenFrequency.TryUpdate(kvp.Key, count - kvp.Value, count);
                        if (result.TokenFrequency[kvp.Key] * negTokenWt + kvp.Value < minCt)
                        {
                            result.TokenFrequency.TryRemove(kvp.Key, out _);        
                            matchClone.TokenFrequency[kvp.Key] = 0;
                        }
                    }
                    else
                    {
                        result.TokenFrequency.TryRemove(kvp.Key, out _);
                    }
                }
            }
            matchClone.TokenFrequency = new ConcurrentDictionary<string, int>(matchClone.TokenFrequency.Where(x => x.Value > 0));
            return (result, matchClone);
            
        }



        #endregion Operator Overloads

    }
}
