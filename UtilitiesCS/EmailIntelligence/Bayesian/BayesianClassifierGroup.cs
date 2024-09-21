using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianClassifierGroup: NewSmartSerializable<BayesianClassifierGroup>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public BayesianClassifierGroup()
        {
            _classifiers = [];
        }

        #endregion Constructors

        #region Public Properties

        public ConcurrentDictionary<string, BayesianClassifierShared> Classifiers { get => _classifiers; protected set => _classifiers = value; }
        protected ConcurrentDictionary<string, BayesianClassifierShared> _classifiers;

        [JsonProperty(Order = -2)]
        public Corpus SharedTokenBase { get => _sharedTokenBase; set => _sharedTokenBase = value; }
        protected Corpus _sharedTokenBase = new();

        [JsonProperty(Order = -1)]
        public int TotalEmailCount { get => _totalEmailCount; set => _totalEmailCount = value; }
        protected int _totalEmailCount;

        public IApplicationGlobals AppGlobals { get; set; }

        [JsonIgnore]
        public Func<object, IApplicationGlobals, IEnumerable<string>> Tokenize { get => _tokenize; set => _tokenize = value; }
        private Func<object, IApplicationGlobals, IEnumerable<string>> _tokenize = new EmailTokenizer().Tokenize;

        [JsonIgnore]
        public Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> TokenizeAsync { get => _tokenizeAsync; set => _tokenizeAsync = value; }
        private Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> _tokenizeAsync = new EmailTokenizer().TokenizeAsync;

        public double MinimumProbability { get => _minimumProbability; set => _minimumProbability = value; }
        protected double _minimumProbability = 0.0;

        #endregion Public Properties

        #region Public Model Training Methods

        public void UnTrain(string tag, IEnumerable<string> matchTokens, int emailCount)
        {
            if(_classifiers.TryGetValue(tag, out var classifier))
            {
                if (classifier is not null) 
                {
                    var matchFrequency = matchTokens.GroupAndCount();
                    classifier.UnTrain(matchFrequency, emailCount);
                    if (classifier.MatchEmailCount <= 0)
                    {
                        _classifiers.TryRemove(tag, out _);
                    }
                }
            }
            
        }

        public void AddOrUpdateClassifier(string tag, IEnumerable<string> matchTokens, int emailCount)
        {
            var classifier = _classifiers.GetOrAdd(tag, CreateNewClassifier(tag, this));
            var matchFrequency = matchTokens.GroupAndCount();
            classifier.Train(matchFrequency, emailCount);
        }

        private BayesianClassifierShared CreateNewClassifier(string tag, BayesianClassifierGroup instance) => new BayesianClassifierShared(tag, instance);

        public void AddToEmailCount(int count)
        {
            Interlocked.Add(ref _totalEmailCount, count);
        }

        public async Task RebuildClassifier(string tag, IDictionary<string, int> matchTokens, int matchEmailCount, CancellationToken cancel)
        {
            _classifiers[tag] = await BayesianClassifierShared.FromTokenBaseAsync(
                this, tag, matchTokens, matchEmailCount, false, cancel);
        }

        #endregion Public Model Training Methods

        #region Public Classification Prediction Methods

        public OrderedParallelQuery<Prediction<string>> Classify(object source)
        {
            var tokens = _tokenize(source, AppGlobals);
            var tokenIncidence = tokens.GroupAndCount();
            var result = this.Classify(tokenIncidence).OrderByDescending(x => x.Probability);
            var sl = new SortedList<int, Prediction<string>>();
            return result;
        }

        public OrderedParallelQuery<Prediction<string>> Classify(string[] tokens)
        {
            var tokenIncidence = tokens.GroupAndCount();
            return this.Classify(tokenIncidence);
        }

        public OrderedParallelQuery<Prediction<string>> Classify(
            IDictionary<string, int> tokenIncidence)
        {
            var results = Classifiers.AsParallel()
                .Select(classifier => new Prediction<string>(
                    classifier.Key,
                    //classifier.Value.GetMatchProbability(tokenIncidence)))
                    classifier.Value.Chi2SpamProb(tokenIncidence)))
                .Where(x => x.Probability >= MinimumProbability)
                .OrderByDescending(x => x.Probability);
            return results;
        }

        public async ValueTask<Prediction<string>[]> ClassifyAsync(object source, CancellationToken cancel)
        {
            var tokens = await TokenizeAsync(source, AppGlobals, cancel);
            var tokenIncidence = await tokens.GroupAndCountAsync();
            var result = await ClassifyAsync(tokenIncidence, cancel).ToArrayAsync();
            return result;
        }

        public async ValueTask<Prediction<string>[]> ClassifyAsync(string[] tokens, CancellationToken cancel)
        {
            var tokenIncidence = tokens.GroupAndCount();
            return await ClassifyAsync(tokenIncidence, cancel).ToArrayAsync();
        }


        public IOrderedAsyncEnumerable<Prediction<string>> ClassifyAsync(
            IDictionary<string, int> tokenIncidence, CancellationToken cancel)
        {
            var results = Classifiers.ToAsyncEnumerable()
                .SelectAwait(async(classifier) => new Prediction<string>(
                    classifier.Key, 
                    await classifier.Value.Chi2SpamProbAsync(tokenIncidence.Keys.ToArray())))
                //await classifier.Value.GetMatchProbabilityAsync(tokenIncidence, cancel)))
                .Where(x => x.Probability >= MinimumProbability)
                .OrderByDescending(prediction => prediction.Probability);
            return results;
        }

        #endregion Public Classification Prediction Methods

        #region Debug Methods

        //public void LogMetrics()
        //{
        //    var metrics = Classifiers.Select(x => new
        //    {
        //        Descriptor = x.Value?.Tag ?? "",
        //        Match = x.Value?.Match?.TokenFrequency?.Keys?.Count() ?? 0,
        //        //NotMatch = x.Value?.NotMatch?.TokenFrequency?.Keys?.Count() ?? 0,
        //        Probability = x.Value?.Prob?.Keys?.Count() ?? 0,
        //        Total = x.Value?.Match?.TokenFrequency?.Keys?.Count() ?? 0 +
        //                x.Value?.NotMatch?.TokenFrequency?.Keys?.Count() ?? 0 +
        //                x.Value?.Prob?.Keys?.Count() ?? 0
        //    }).ToList();
        //    metrics.Insert(0, new
        //    {
        //        Descriptor = "Dedicated",
        //        Match = this.DedicatedTokens.Count(),
        //        NotMatch = 0,
        //        Probability = 0,
        //        Total = (int)((double)this.DedicatedTokens.Count() * 6)
        //    });
        //    metrics.Insert(1, new
        //    {
        //        Descriptor = "TokenBase",
        //        Match = this.SharedTokenBase.TokenFrequency.Keys.Count(),
        //        NotMatch = 0,
        //        Probability = 0,
        //        Total = this.SharedTokenBase.TokenFrequency.Keys.Count()
        //    });
        //    metrics.Add(new
        //    {
        //        Descriptor = "Total",
        //        Match = metrics.Select(x => x.Match).Sum(),
        //        NotMatch = metrics.Select(x => x.NotMatch).Sum(),
        //        Probability = metrics.Select(x => x.Probability).Sum(),
        //        Total = metrics.Select(x => x.Total).Sum()
        //    });

        //    var jagged = metrics.Select(x => new[] { x.Descriptor, x.Match.ToString("N0"), x.NotMatch.ToString("N0"), x.Probability.ToString("N0"), x.Total.ToString("N0") }).ToArray();
        //    //var jagged = metrics.Select(x => new object[] { x.Descriptor, x.Match, x.NotMatch, x.Probability, x.Total }).ToArray();

        //    //logger.Debug($"\n{jagged.ToFormattedText(
        //            ["Descriptor", "Matches", "Not Match", "Probability", "Total Lines"],
        //            [Enums.Justification.Left, Enums.Justification.Right,
        //                Enums.Justification.Right, Enums.Justification.Right,
        //                Enums.Justification.Right],
        //            "Classifier Manager Metrics".ToUpper())}");
        //}

        //public void LogState()
        //{
        //    //logger.Debug($"\n{Classifiers
        //        .Select(x => new[]
        //            {
        //                x.Value.Tag,
        //                (x.Value.Parent is not null).ToString(),
        //                (x.Value.Parent.SharedTokenBase is not null).ToString(),
        //                (x.Value.NotMatch is not null).ToString(),
        //                (x.Value.Match is not null).ToString()
        //            })
        //        .ToArray()
        //        .ToFormattedText(
        //            ["Classifier", "Parent", "TokenBase", "Positive", "Negative"],
        //            [Enums.Justification.Center, Enums.Justification.Center,
        //                Enums.Justification.Center, Enums.Justification.Center,
        //                Enums.Justification.Center],
        //            "Classifier Manager State".ToUpper())}");
        //}

        #endregion Debug Methods

        #region Serialization

        //[OnDeserialized]
        //internal void OnDeserializedMethod(StreamingContext context)
        //{
        //    //IdleActionQueue.AddEntry(async () => await AfterDeserialize(AppGlobals.AF.CancelLoad));
        //    //LogMetrics();
        //}

        internal string GetReportMessage(int completed, int count, SegmentStopWatch sw, string header = "Completed")
        {
            string message;
            if (completed > 0)
            {
                var speed = sw.Elapsed.TotalSeconds / (double)completed;
                var remaining = TimeSpan.FromSeconds((count - completed) * speed);
                message = $"{header} {completed} of {count} @ {speed:N2} per sec ({remaining:mm\\:ss} remaining)";
            }
            else
            {
                message = $"{header} {completed} of {count}";
            }

            return message;
        }

        #endregion Serialization

        #region obsolete

        [Obsolete]
        public void AddOrUpdateClassifier_2(string tag, IEnumerable<string> matchTokens)
        {
            // Saved logic from when DedicatedTokens was a ConcurrentDictionary<string, DedicatedToken>

            //var classifier = _classifiers.GetOrAdd(tag, new BayesianClassifierShared(tag));

            //var matchFrequency = GroupAndCount(matchTokens);

            //foreach (var kvp in matchFrequency)
            //{
            //    DedicatedTokens.AddOrUpdate(kvp.Key, new DedicatedToken { FolderPath = tag, Count = kvp.Value }, (key, existingVal) =>
            //    {
            //        if (existingVal.FolderPath == tag)
            //        {
            //            existingVal.Count += kvp.Value;
            //        }
            //        return existingVal;
            //    });

            //    SharedTokenBase.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value, (key, existingVal) =>
            //    {
            //        return existingVal + kvp.Value;
            //    });
            //}

            //// Update other match probabilities for new total counts
            //throw new NotImplementedException();
        }

        [Obsolete]
        public void UpdateSharedDictionaries2(string key, int count, string tag)
        {
            //// Check whether the KeyValuePair<string, int> named kvp has a matching key
            //// in DedicatedTokens and get its value in a variable named dedicatedToken
            //DedicatedToken dedicatedToken = null;
            //bool moveDedicatedToShared = false;

            //lock (_dedicatedTokens3)
            //{
            //    // Does The Token Exist in DedicatedTokens?
            //    if (_dedicatedTokens3.TryGetValue(key, out dedicatedToken))
            //    {
            //        // Does the FolderPath match the tag?
            //        if (dedicatedToken.FolderPath == tag)
            //        {
            //            // If So, Add the count to the dedicated token and return
            //            Interlocked.Add(ref dedicatedToken.Count, count);
            //            return;
            //        }
            //        else
            //        {
            //            // If Not, it means it has become a shared token. 
            //            // Remove the token and mark it for migration to shared tokens
            //            moveDedicatedToShared = _dedicatedTokens3.Remove(key);
            //        }
            //    }
            //    // If the token is not in DedicatedTokens, try to add to a shared token
            //    else if (this.SharedTokenBase.TokenFrequency.TryAddValues(key, count))
            //    {
            //        // If successful return and release lock
            //        return;
            //    }
            //    else
            //    {
            //        // Token is new. Add to dedicated tokens 
            //        _dedicatedTokens3.Add(key, new DedicatedToken 
            //            { Token = key, FolderPath = tag, Count = count });
            //        return;
            //    }
            //}

            //if (this.SharedTokenBase.TokenFrequency.TryGetValue(kvp.Key, out var st))
            //{
            //    // Threadsafe update the value in the shared token base
            //}
            //else
            //{
            //    // Add to dedicated tokens
            //}
            //    // Add to the bayesian clasifier and update the probability

        }

        [Obsolete]
        public void UpdateSharedDictionaries(string key, int value, string tag)
        {
            //Enums.DictionaryResult result = UpdateOrRemoveDedicated(
            //    key, value, tag, out var dedicatedToken);

            //// Exit if dedicated token value updated successfully
            //if (result.HasFlag(Enums.DictionaryResult.ValueChanged))
            //    return;

            //// Else if the dedicated token should be migrated, add or update shared tokens
            //else if (result.HasFlag(Enums.DictionaryResult.KeysChanged) &&
            //    !result.HasFlag(Enums.DictionaryResult.KeyExists))
            //{
            //    int migratedValue = dedicatedToken.Count + value;
            //    SharedTokenBase.TokenFrequency.AddOrUpdate(key, migratedValue,
            //        (sharedKey, existingValue) => existingValue + migratedValue);
            //    return;
            //}
            //// Else it add to dedicated tokens
            //else
            //{
            //    _dedicatedTokens.TryAdd(key, new DedicatedToken
            //    { Token = key, FolderPath = tag, Count = value });
            //}
        }


        #endregion obsolete
    }
}
