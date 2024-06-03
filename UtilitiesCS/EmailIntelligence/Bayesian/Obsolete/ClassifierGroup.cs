using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    [Obsolete("Use BayesianClassifierGroup instead")]
    public class ClassifierGroup
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ClassifierGroup()
        {
            _classifiers = [];
        }

        #endregion Constructors

        #region Public Properties

        public ConcurrentDictionary<string, BayesianClassifier> Classifiers { get => _classifiers; protected set => _classifiers = value; }
        protected ConcurrentDictionary<string, BayesianClassifier> _classifiers;

        [JsonProperty(Order = -3)]
        public ConcurrentDictionary<string, DedicatedToken> DedicatedTokens { get => _dedicatedTokens; set => _dedicatedTokens = value; }
        protected ConcurrentDictionary<string, DedicatedToken> _dedicatedTokens = new();

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

        public void ForceClassifierUpdate(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _classifiers[tag] = new BayesianClassifier(tag, positiveTokens, negativeTokens);
            _classifiers[tag].Parent ??= this;
        }

        public void AddOrUpdateClassifier(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            var classifier = _classifiers.GetOrAdd(tag, new BayesianClassifier(tag));
            classifier.AddTokens(positiveTokens, negativeTokens);

        }

        public IOrderedEnumerable<Prediction<string>> Classify(object source)
        {
            return this.Classify(_tokenizer(source));
        }

        public IOrderedEnumerable<Prediction<string>> Classify(IEnumerable<string> tokens)
        {
            var results = Classifiers.Select(
                classifier => new Prediction<string>(
                    classifier.Key, classifier.Value.GetMatchProbability(tokens))).OrderBy(x=>x);
            return results;
        }

        #endregion Public Methods

        #region Debug Methods

        public void LogMetrics() 
        {
            var metrics = Classifiers.Select(x => new
            {
                Descriptor = x.Value?.Tag ?? "",
                Match = x.Value?.Match?.TokenFrequency?.Keys?.Count() ?? 0,
                NotMatch = x.Value?.NotMatch?.TokenFrequency?.Keys?.Count() ?? 0,
                Probability = x.Value?.Prob?.Keys?.Count() ?? 0,
                Total = x.Value?.Match?.TokenFrequency?.Keys?.Count() ?? 0 + 
                        x.Value?.NotMatch?.TokenFrequency?.Keys?.Count() ?? 0 + 
                        x.Value?.Prob?.Keys?.Count() ?? 0
            }).ToList();
            metrics.Insert(0, new
            {
                Descriptor = "Dedicated",
                Match = this.DedicatedTokens.Count(),
                NotMatch = 0,
                Probability = 0,
                Total = (int)((double)this.DedicatedTokens.Count() * 6)
            });
            metrics.Insert(1, new
            {
                Descriptor = "TokenBase",
                Match = this.SharedTokenBase.TokenFrequency.Keys.Count(),
                NotMatch = 0,
                Probability = 0,
                Total = this.SharedTokenBase.TokenFrequency.Keys.Count()
            });
            metrics.Add(new
            {
                Descriptor = "Total",
                Match = metrics.Select(x=>x.Match).Sum(),
                NotMatch = metrics.Select(x => x.NotMatch).Sum(),
                Probability = metrics.Select(x => x.Probability).Sum(),
                Total = metrics.Select(x => x.Total).Sum()
            });

            var jagged = metrics.Select(x => new[] { x.Descriptor, x.Match.ToString("N0"), x.NotMatch.ToString("N0"), x.Probability.ToString("N0"), x.Total.ToString("N0") }).ToArray();
            //var jagged = metrics.Select(x => new object[] { x.Descriptor, x.Match, x.NotMatch, x.Probability, x.Total }).ToArray();

            logger.Debug($"\n{jagged.ToFormattedText(
                    ["Descriptor", "Matches", "Not Match", "Probability", "Total Lines"],
                    [Enums.Justification.Left, Enums.Justification.Right, 
                        Enums.Justification.Right, Enums.Justification.Right, 
                        Enums.Justification.Right],
                    "Classifier Manager Metrics".ToUpper())}");
        }

        public void LogState() 
        {
            logger.Debug($"\n{
                Classifiers
                .Select(x => new[]
                    {
                        x.Value.Tag,
                        (x.Value.Parent is not null).ToString(),
                        (x.Value.Parent.SharedTokenBase is not null).ToString(),
                        (x.Value.NotMatch is not null).ToString(),
                        (x.Value.Match is not null).ToString()
                    })
                .ToArray()
                .ToFormattedText(
                    ["Classifier", "Parent", "TokenBase", "Positive", "Negative"],
                    [Enums.Justification.Center, Enums.Justification.Center,
                        Enums.Justification.Center, Enums.Justification.Center, 
                        Enums.Justification.Center],
                    "Classifier Manager State".ToUpper())}");
        }

        #endregion Debug Methods
        
        #region Serialization

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //IdleActionQueue.AddEntry(async () => await AfterDeserialize(AppGlobals.AF.CancelLoad));
            LogMetrics();
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

        internal async Task AfterDeserialized_HeavyParallelizationAsync(
            CancellationToken token, SegmentStopWatch sw)
        {
            await Task.Run(async () =>
            {
                // Memory issue with infer negative. 
                await InferNegative(token);
                sw.LogDuration("InferNegative Tokens");

                await RecalcNullProbs(token);
                sw.LogDuration("Update Probabilities");
            }, token);
        }

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
        
        // Parallelization made this slower because memory usage was too high
        internal async Task InferNegative(CancellationToken token) 
        {
            AppGlobals.AF.ProgressTracker.Report(
                0, "Starting Negative Token Inference");
            var count = Classifiers.Count;
            int completed = 0;

            var processors = Math.Max(Environment.ProcessorCount - 2, 1);
            var chunkSize = (int)Math.Round((double)count / (double)processors, 0);
            var chunks = Classifiers.Values.Chunk(chunkSize);

            var sw = new SegmentStopWatch();
            // Start the chunked tasks to multiprocess async
            var tasks = chunks.Select(
                chunk => Task.Run(async () => await 
                chunk.ToAsyncEnumerable()
                .ForEachAsync(async (classifier) => 
                {
                    await classifier.InferNegativeTokensAsync(token);
                    Interlocked.Increment(ref completed);
                    AppGlobals.AF.ProgressTracker.Report(
                        (int)((double)completed / (double)count * 100),
                        GetReportMessage(completed, count, sw, "Infer Negative Tokens: Completed"));
                })));

            sw.Start();
            
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Loading Canceled by User");
            }
        }

        internal async Task OptimizeUpdate(SegmentStopWatch sw)
        {
            var count = Classifiers.Count;
            int completed = 0;

            try
            {
                foreach (var classifier in Classifiers)
                {
                    AppGlobals.AF.CancelToken.ThrowIfCancellationRequested();
                    await classifier.Value.AfterDeserialize(AppGlobals.AF.CancelToken, sw);
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
