using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class ClassifierGroup
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ClassifierGroup()
        {
            _classifiers = [];
        }

        public ConcurrentDictionary<string, BayesianClassifier> Classifiers { get => _classifiers; protected set => _classifiers = value; }
        private ConcurrentDictionary<string, BayesianClassifier> _classifiers;

        [JsonProperty(Order = -2)]
        public Corpus TokenBase { get => _tokenBase; set => _tokenBase = value; }
        private Corpus _tokenBase = new ();
        
        public IEnumerable<(string Token, string FolderPath, int Count)> DedicatedTokens { get => _dedicatedTokens; set => _dedicatedTokens = value; }
        private IEnumerable<(string Token, string FolderPath, int Count)> _dedicatedTokens;

        public IApplicationGlobals AppGlobals { get; set; }

        public void ForceClassifierUpdate(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _classifiers[tag] = new BayesianClassifier(tag, positiveTokens, negativeTokens);
            Interlocked.CompareExchange(ref _classifiers[tag].TokenBase, _tokenBase, null);   
        }

        public void AddOrUpdateClassifier(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            var classifier = _classifiers.GetOrAdd(tag, new BayesianClassifier(tag));
            classifier.AddTokens(positiveTokens, negativeTokens);
            
        }

        public IOrderedEnumerable<KeyValuePair<string, double>> Classify(object source)
        {
            return this.Classify(_tokenizer(source));
        }

        public IOrderedEnumerable<KeyValuePair<string, double>> Classify(IEnumerable<string> tokens)
        {
            var results = Classifiers.Select(
                classifier => new KeyValuePair<string, double>(
                    classifier.Key, classifier.Value.CalculateProbability(tokens)))
                .OrderByDescending(x => x.Value);
            return results;
        }

        public void LogState() 
        {
            logger.Debug($"\n{
                Classifiers
                .Select(x => new[]
                    {
                        x.Value.Tag,
                        (x.Value.TokenBase is not null).ToString(),
                        (x.Value.NotMatch is not null).ToString(),
                        (x.Value.Match is not null).ToString()
                    })
                .ToArray()
                .ToFormattedText(
                    ["Classifier", "TokenBase", "Positive", "Negative"],
                    "Classifier Manager State".ToUpper())}");
        }

        //[JsonIgnore]
        public Func<object, IEnumerable<string>> Tokenizer { get => _tokenizer; set => _tokenizer = value; }
        private Func<object, IEnumerable<string>> _tokenizer;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            IdleActionQueue.AddEntry(async () => await AfterDeserialize(AppGlobals.AF.CancelLoad));
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
    }
}
