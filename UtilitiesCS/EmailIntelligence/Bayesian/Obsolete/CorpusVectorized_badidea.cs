// Idea was to use a vector to store each token's count of match and not match.
// But it is a bad idea because it is hard to update the concurrent dictionary
// when one part of the vector could be changed by another thread.

//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using UtilitiesCS.HelperClasses;

//namespace UtilitiesCS.EmailIntelligence.Bayesian
//{
//    [Serializable]
//    public class CorpusVectorized : ICloneable
//    {
//        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
//            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//        #region Constructors

//        public CorpusVectorized() { _tokenFrequency = []; }
//        //public CorpusVectorized(IEnumerable<string> tokens) { _tokenFrequency = new(); AddOrIncrementTokens(tokens); }
//        //public CorpusVectorized(IEnumerable<KeyValuePair<string, int>> collection) { _tokenFrequency = new(collection); }
//        //public CorpusVectorized(IEqualityComparer<string> comparer) { _tokenFrequency = new ConcurrentDictionary<string, int>(comparer); }
//        //public CorpusVectorized(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { _tokenFrequency = new ConcurrentDictionary<string, int>(collection, comparer); }
//        //public CorpusVectorized(int concurrencyLevel, int capacity) { _tokenFrequency = new ConcurrentDictionary<string, int>(concurrencyLevel, capacity); }
//        //public CorpusVectorized(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, collection, comparer); }
//        //public CorpusVectorized(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) { new ConcurrentDictionary<string, int>(concurrencyLevel, capacity, comparer); }

//        #endregion Constructors

//        #region Public Properties and Methods

//        public ConcurrentDictionary<string, Vector2> TokenFrequency { get => _tokenFrequency; protected set => _tokenFrequency = value; }
//        private ConcurrentDictionary<string, Vector2> _tokenFrequency;

//        public int TokenCount { get => _tokenCount; protected set => _tokenCount = value; }
//        private int _tokenCount;

//        public Enums.Corpus Indicator { get => _indicator; set => _indicator = value; }
//        private Enums.Corpus _indicator;

//        //public void AddOrIncrementToken(string token)
//        //{
//        //    TokenFrequency.AddOrUpdate(token, 1, (key, count) => ++count);
//        //}

//        //public void AddOrIncrementTokens(IEnumerable<string> tokens) => tokens.ForEach(AddOrIncrementToken);

//        //public bool DecrementOrRemoveToken(string token)
//        //{
//        //    if (_tokenFrequency.TryGetValue(token, out Vector2 count))
//        //    {
//        //        Interlocked.Decrement(ref _tokenCount);
//        //        if (Interlocked.Decrement(ref count) == 0)
//        //        {
//        //            _tokenFrequency.TryRemove(token, out _);
//        //            return false;
//        //        }
//        //        else
//        //        {
//        //            _tokenFrequency[token] = count;
//        //            return true;
//        //        }
//        //    }
//        //    else { return false; }
//        //}

//        public void AddTokenOrSumValues(IEnumerable<KeyValuePair<string, int>> tokenFrequency)
//        {
//            foreach (var kvp in tokenFrequency)
//            {
//                this.AddOrSumTokenValue(kvp.Key, kvp.Value);
//            }
//        }

//        public void AddOrSumTokenValue(string token, Vector2 value)
//        {
//            TokenFrequency.AddOrUpdate(token, value, (key, count) => count + value);
//        }

//        public void SubtractOrRemoveValues(IEnumerable<KeyValuePair<string, int>> tokenFrequency)
//        {
//            foreach (var kvp in tokenFrequency)
//            {
//                SubtractOrRemoveValue(kvp.Key, kvp.Value);
//            }
//        }

//        public void SubtractOrRemoveValue(string token, int value)
//        {
//            _tokenFrequency.UpdateOrRemove(
//                token,
//                (key, oldValue) => oldValue - value <= 0,
//                (key, oldValue) => oldValue - value,
//                out _);
//        }

//        public object Clone()
//        {
//            var result = this.MemberwiseClone() as CorpusVectorized;
//            result.TokenFrequency = new ConcurrentDictionary<string, Vector2>(this.TokenFrequency);
//            return result;
//        }

//        #endregion Public Properties and Methods

//        #region Operator Overloads

//        public static CorpusVectorized operator +(CorpusVectorized c1, CorpusVectorized c2)
//        {
//            var result = c1.Clone() as CorpusVectorized;

//            foreach (var kvp in c2.TokenFrequency)
//            {
//                result.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value, (key, count) => count + kvp.Value);
//            }
//            return result;
//        }
//        // Hard to vectorize this since "other"
//        //public static async Task<CorpusVectorized> SubtractAsync(
//        //    CorpusVectorized c1,
//        //    CorpusVectorized c2,
//        //    CancellationToken token,
//        //    SegmentStopWatch sw = null)
//        //{
//        //    sw ??= new SegmentStopWatch().Start();
//        //    sw.LogDuration("SubtractAsync Start");

//        //    var result = c1.Clone() as CorpusVectorized;
//        //    sw.LogDuration("clone universe");

//        //    var processors = Math.Max(Environment.ProcessorCount - 2, 1);
//        //    var chunkSize = (int)Math.Round((double)c2.TokenFrequency.Count() / (double)processors, 0);
//        //    if (chunkSize == 0)
//        //        return result;
//        //    chunkSize = Math.Min(Math.Max(chunkSize, 50), c2.TokenFrequency.Count());
//        //    var chunks = c2.TokenFrequency.Chunk(chunkSize);
//        //    sw.LogDuration("chunk positive tokens");

//        //    var tasks = chunks.Select(chunk => Task.Run(() => chunk.ForEach(x =>
//        //    {
                
//        //        if (result.TokenFrequency.TryGetValue(x.Key, out int count))
//        //        {
//        //            if (count > x.Value)
//        //            {
//        //                result.TokenFrequency.TryUpdate(x.Key, count - x.Value, count);
//        //            }
//        //            else
//        //            {
//        //                result.TokenFrequency.TryRemove(x.Key, out _);
//        //            }
//        //        }
//        //    }),
//        //    token));

//        //    await Task.WhenAll(tasks);
//        //    sw.LogDuration("subtract positive tokens");

//        //    return result;
//        //}

//        //public static CorpusVectorized operator -(CorpusVectorized c1, CorpusVectorized c2)
//        //{
//        //    var result = c1.Clone() as CorpusVectorized;
//        //    foreach (var kvp in c2.TokenFrequency)
//        //    {
//        //        if (result.TokenFrequency.TryGetValue(kvp.Key, out int count))
//        //        {
//        //            if (count > kvp.Value)
//        //            {
//        //                result.TokenFrequency.TryUpdate(kvp.Key, count - kvp.Value, count);
//        //            }
//        //            else
//        //            {
//        //                result.TokenFrequency.TryRemove(kvp.Key, out _);
//        //            }
//        //        }
//        //    }
//        //    return result;
//        //}

//        //public static (CorpusVectorized NotMatchFiltered, CorpusVectorized MatchFiltered) SubtractFilter(CorpusVectorized all, CorpusVectorized match, int negTokenWt, int minCt)
//        //{
//        //    var result = all.Clone() as CorpusVectorized;
//        //    var matchClone = match.Clone() as CorpusVectorized;

//        //    result.TokenFrequency = new ConcurrentDictionary<string, int>(result.TokenFrequency.Where(x => x.Value * negTokenWt >= minCt));
//        //    foreach (var kvp in matchClone.TokenFrequency)
//        //    {
//        //        if (result.TokenFrequency.TryGetValue(kvp.Key, out int count))
//        //        {
//        //            if (count > kvp.Value)
//        //            {
//        //                result.TokenFrequency.TryUpdate(kvp.Key, count - kvp.Value, count);
//        //                if (result.TokenFrequency[kvp.Key] * negTokenWt + kvp.Value < minCt)
//        //                {
//        //                    result.TokenFrequency.TryRemove(kvp.Key, out _);
//        //                    matchClone.TokenFrequency[kvp.Key] = 0;
//        //                }
//        //            }
//        //            else
//        //            {
//        //                result.TokenFrequency.TryRemove(kvp.Key, out _);
//        //            }
//        //        }
//        //    }
//        //    matchClone.TokenFrequency = new ConcurrentDictionary<string, int>(matchClone.TokenFrequency.Where(x => x.Value > 0));
//        //    return (result, matchClone);

//        //}



//        #endregion Operator Overloads

//    }

//}
