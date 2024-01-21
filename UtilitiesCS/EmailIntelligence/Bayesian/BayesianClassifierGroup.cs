using ExCSS;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class BayesianClassifierGroup
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

        [JsonProperty(Order = -3)]
        public ConcurrentDictionary<string, DedicatedToken> DedicatedTokens { get => _dedicatedTokens; set => _dedicatedTokens = value; }
        protected ConcurrentDictionary<string, DedicatedToken> _dedicatedTokens = new();

        protected Dictionary<string, DedicatedToken> _dedicatedTokens3 = new();
        

        [JsonProperty(Order = -2)]
        public Corpus SharedTokenBase { get => _sharedTokenBase; set => _sharedTokenBase = value; }
        protected Corpus _sharedTokenBase = new();

        [JsonProperty(Order = -1)]
        public int TotalTokenCount { get => _totalTokenCount; set => _totalTokenCount = value; }
        protected int _totalTokenCount;

        public IApplicationGlobals AppGlobals { get; set; }

        //[JsonIgnore]
        public Func<object, IEnumerable<string>> Tokenizer { get => _tokenizer; set => _tokenizer = value; }
        private Func<object, IEnumerable<string>> _tokenizer = new EmailTokenizer().tokenize;

        #endregion Public Properties

        #region Public Methods

        
        public void ForceClassifierUpdate(string tag, IEnumerable<string> matchTokens)
        {
            _classifiers[tag] = BayesianClassifierShared.FromTokenBase(this, tag, matchTokens);
        }

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

        public void UpdateSharedDictionaries(string key, int value, string tag)
        {
            Enums.DictionaryResult result = UpdateOrRemoveDedicated(
                key, value, tag, out var dedicatedToken);

            // Exit if dedicated token value updated successfully
            if (result.HasFlag(Enums.DictionaryResult.ValueChanged))
                return;

            // Else if the dedicated token should be migrated, add or update shared tokens
            else if (result.HasFlag(Enums.DictionaryResult.KeysChanged) &&
                !result.HasFlag(Enums.DictionaryResult.KeyExists))
            {
                int migratedValue = dedicatedToken.Count + value;
                SharedTokenBase.TokenFrequency.AddOrUpdate(key, migratedValue, 
                    (sharedKey, existingValue) => existingValue + migratedValue);
                return;    
            }
            // Else it add to dedicated tokens
            else
            {
                _dedicatedTokens.TryAdd(key, new DedicatedToken
                    { Token = key, FolderPath = tag, Count = value });
            }
        }

        private Enums.DictionaryResult UpdateOrRemoveDedicated(
            string key, int value, string tag, out DedicatedToken dedicatedToken)
        {
            return _dedicatedTokens.UpdateOrRemove(
                key: key,
                removeCondition: (key, oldValue) => oldValue.FolderPath == tag,
                updateValueFactory: (key, existingValue) =>
                {
                    if (existingValue.FolderPath != tag)
                    {
                        throw new ArgumentException($"New Tag [{tag}] does not match " +
                            $"existing [{existingValue.FolderPath}]. Should have been removed" +
                            $"already by removal condition");
                    }

                    existingValue.Count += value;
                    return existingValue;
                },
                value: out dedicatedToken);
        }

        public void AddOrUpdateClassifier(string tag, IEnumerable<string> matchTokens)
        {
            // This whole method is not threadsafe if I am filing multiple emails at once
            var classifier = _classifiers.GetOrAdd(tag, new BayesianClassifierShared(tag));
            var matchFrequency = GroupAndCount(matchTokens);
            foreach (var kvp in matchFrequency)
            {
                UpdateSharedDictionaries(kvp.Key, kvp.Value, tag);
            }
            classifier.AddToMatches(matchFrequency);

            //// Make threadsafe
            //var (notMatchFiltered, matchFiltered) = Corpus.SubtractFilter(
            //            SharedTokenBase,
            //            classifier.Match,
            //            classifier.Knobs.NotMatchTokenWeight,
            //            classifier.Knobs.MinCountForInclusion);
            
            //classifier.NotMatchCount = notMatchFiltered.TokenFrequency.Values.Sum();
            //classifier.MatchCount = matchFiltered.TokenFrequency.Values.Sum();


            // Update other match probabilities for new total counts
            
        }
        
        public static Dictionary<string, int> GroupAndCount(IEnumerable<string> items)
        {
            return items.GroupBy(item => item)
            .ToDictionary(group => group.Key, group => group.Count());
        }
        public IOrderedEnumerable<Prediction<string>> Classify(object source)
        {
            return this.Classify(_tokenizer(source));
        }
        public IOrderedEnumerable<Prediction<string>> Classify(IEnumerable<string> tokens)
        {
            var results = Classifiers.Select(
            classifier => new Prediction<string>(
            classifier.Key, classifier.Value.GetMatchProbability(tokens))).OrderBy(x => x);
            return results;
        }

        #endregion Public Methods

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

        //    logger.Debug($"\n{jagged.ToFormattedText(
        //            ["Descriptor", "Matches", "Not Match", "Probability", "Total Lines"],
        //            [Enums.Justification.Left, Enums.Justification.Right,
        //                Enums.Justification.Right, Enums.Justification.Right,
        //                Enums.Justification.Right],
        //            "Classifier Manager Metrics".ToUpper())}");
        //}

        //public void LogState()
        //{
        //    logger.Debug($"\n{Classifiers
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

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //IdleActionQueue.AddEntry(async () => await AfterDeserialize(AppGlobals.AF.CancelLoad));
            //LogMetrics();
        }

        public async Task AfterDeserialize(CancellationToken token)
        {
            var sw = new SegmentStopWatch().Start();

            AppGlobals.AF.ProgressPane.Visible = true;
            logger.Debug("Starting Classifier Probability Calculation");
            AppGlobals.AF.ProgressTracker.Report(0, "Starting Classifier Probability Calculation");
            sw.LogDuration("Initialized AfterDeserialize");

            //await Classifiers.First().Value.AfterDeserialize(AppGlobals.AF.CancelLoad);
            //Interlocked.Increment(ref completed);
            //AppGlobals.AF.ProgressTracker.Report((int)(completed / count * 100), GetReportMessage(completed, count, sw));

            //await AfterDeserialized_HeavyParallelizationAsync(token, sw);
            await OptimizeUpdate(sw);

            sw.Stop().GroupByActionName(inplace: true);
            sw.WriteToLog();

            AppGlobals.AF.ProgressTracker.Report(100, $"Completed in {sw.Elapsed:mm\\:ss}");

        }

        //internal async Task AfterDeserialized_HeavyParallelizationAsync(
        //    CancellationToken token, SegmentStopWatch sw)
        //{
        //    await Task.Run(async () =>
        //    {
        //        // Memory issue with infer negative. 
        //        await InferNegative(token);
        //        sw.LogDuration("InferNegative Tokens");

        //        await RecalcNullProbs(token);
        //        sw.LogDuration("Update Probabilities");
        //    }, token);
        //}

        internal async Task RecalcNullProbs(CancellationToken token)
        {
            if (Classifiers.Values.Any(x => x.Prob is null))
            {
                AppGlobals.AF.ProgressTracker.Report(
                    0, "Starting to Recalculate Probabilities");
                var count = Classifiers.Count;
                int completed = 0;
                var sw = new SegmentStopWatch().Start();

                await Classifiers.Values.ToAsyncEnumerable()
                        .ForEachAsync(async (classifier) =>
                        {
                            await classifier.RecalcProbsAsync(token);
                            Interlocked.Increment(ref completed);
                            AppGlobals.AF.ProgressTracker.Report((int)((double)completed / (double)count * 100),
                                GetReportMessage(completed, count, sw, "Recalc Probabilities: Completed"));
                        });

            }

        }

        //// Parallelization made this slower because memory usage was too high
        //internal async Task InferNegative(CancellationToken token)
        //{
        //    AppGlobals.AF.ProgressTracker.Report(
        //        0, "Starting Negative Token Inference");
        //    var count = Classifiers.Count;
        //    int completed = 0;

        //    var processors = Math.Max(Environment.ProcessorCount - 2, 1);
        //    var chunkSize = (int)Math.Round((double)count / (double)processors, 0);
        //    var chunks = Classifiers.Values.Chunk(chunkSize);

        //    var sw = new SegmentStopWatch();
        //    // Start the chunked tasks to multiprocess async
        //    var tasks = chunks.Select(
        //        chunk => Task.Run(async () => await
        //        chunk.ToAsyncEnumerable()
        //        .ForEachAsync(async (classifier) =>
        //        {
        //            await classifier.InferNegativeTokensAsync(token);
        //            Interlocked.Increment(ref completed);
        //            AppGlobals.AF.ProgressTracker.Report(
        //                (int)((double)completed / (double)count * 100),
        //                GetReportMessage(completed, count, sw, "Infer Negative Tokens: Completed"));
        //        })));

        //    sw.Start();

        //    try
        //    {
        //        await Task.WhenAll(tasks);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        logger.Debug("Loading Canceled by User");
        //    }
        //}

        internal async Task OptimizeUpdate(SegmentStopWatch sw)
        {
            var count = Classifiers.Count;
            int completed = 0;

            try
            {
                foreach (var classifier in Classifiers)
                {
                    AppGlobals.AF.CancelLoad.ThrowIfCancellationRequested();
                    await classifier.Value.AfterDeserialize(AppGlobals.AF.CancelLoad, sw);
                    Interlocked.Increment(ref completed);
                    AppGlobals.AF.ProgressTracker.Report((int)((double)completed / (double)count * 100),
                        GetReportMessage(completed, count, sw));
                }
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Loading Canceled by User");
            }
        }

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
    }
}
